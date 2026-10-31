using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;

namespace ICGNS;

internal sealed class Fatigue
{
    internal const string ProgressKey = SaveDataMigration.CurrentId + "/Fatigue";
    internal const string BuffId = SaveDataMigration.CurrentId + "/Fatigue";
    private int? displayedLevel;
    internal FatigueProgress? Progress { get; private set; }

    internal static FatigueProgress? Read(Farmer farmer)
        => FatigueProgress.Parse(farmer.modData.TryGetValue(ProgressKey, out string? value) ? value : null);

    private static void Write(Farmer farmer, FatigueProgress? progress)
    {
        if (progress is { } clock)
        {
            string value = clock.Serialize();
            if (!farmer.modData.TryGetValue(ProgressKey, out string? old) || old != value)
                farmer.modData[ProgressKey] = value;
        }
        else
            farmer.modData.Remove(ProgressKey);
    }

    internal static void FinishNight(Farmer farmer, bool reachedDawn)
    {
        // Run before vanilla clears sleep flags, changes the date and saves.
        // Shipping preferences and the synthetic 6 AM passedOut flag aren't sleep.
        Write(farmer, FatigueProgress.FinishNight(Read(farmer), GameplaySettings.Enabled,
            SalePolicy.HasSlept(farmer, Game1.Date.TotalDays), reachedDawn,
            FatigueProgress.Minute(Game1.Date.TotalDays, 3000)));
        // A real sleeper must recover energy against their normal maximum.
        if (Read(farmer) is null)
            farmer.buffs.Remove(BuffId);
    }

    internal void OnLoaded()
    {
        Farmer farmer = Game1.player;
        Write(farmer, Read(farmer)?.Rebase(FatigueProgress.Minute(Game1.Date.TotalDays, Game1.timeOfDay)));
        displayedLevel = null;
        Refresh();
    }

    internal void Refresh()
    {
        if (!Context.IsWorldReady || Game1.newDay
            || ModEntry.Instance.State.Value.DawnStarted && !ModEntry.Instance.State.Value.DayHasStarted)
            return;

        Farmer farmer = Game1.player;
        bool enabled = GameplaySettings.Enabled;
        long now = FatigueProgress.Minute(Game1.Date.TotalDays, Game1.timeOfDay);
        FatigueProgress? progress = Read(farmer);
        progress = enabled ? progress?.Advance(now) : progress?.Rebase(now);
        Write(farmer, progress);
        Progress = progress;
        int level = enabled ? progress?.Level ?? 0 : 0;
        if (level == 0 || progress is not { } clock)
        {
            farmer.buffs.Remove(BuffId);
            displayedLevel = 0;
            return;
        }

        farmer.buffs.AppliedBuffs.TryGetValue(BuffId, out Buff? current);
        // Add back only our own penalty before recalculating: repeated updates
        // must never compound it, and food/Stardrop bonuses must stay independent.
        float normalMaximum = farmer.MaxStamina - (current?.effects.MaxStamina.Value ?? 0);
        int energyPenalty = clock.EnergyPenalty(normalMaximum);
        float speedPenalty = clock.SpeedPenalty;
        string name = ModEntry.Instance.Helper.Translation.Get("fatigue.name", new { level });
        if (current is null || current.effects.MaxStamina.Value != -energyPenalty
            || current.effects.Speed.Value != -speedPenalty || current.displayName != name)
        {
            farmer.buffs.Remove(BuffId);
            farmer.applyBuff(new Buff(
                id: BuffId,
                displayName: name,
                description: ModEntry.Instance.Helper.Translation.Get("fatigue.description",
                    new { energyPercent = clock.EnergyReductionPercent, speed = speedPenalty }),
                duration: Buff.ENDLESS,
                iconTexture: Game1.buffsIcons,
                iconSheetIndex: 7,
                effects: new BuffEffects
                {
                    MaxStamina = { -energyPenalty },
                    Speed = { -speedPenalty }
                }));
        }

        // Lowering the cap cannot grant energy or erase negative exhaustion.
        if (farmer.Stamina > farmer.MaxStamina)
            farmer.Stamina = farmer.MaxStamina;
        if (displayedLevel is { } previous && level > previous)
            Game1.addHUDMessage(new HUDMessage(ModEntry.Instance.Helper.Translation.Get("fatigue.increased", new { level })));
        displayedLevel = level;
    }
}
