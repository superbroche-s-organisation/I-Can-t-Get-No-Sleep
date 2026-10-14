using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace ICGNS;

public sealed class ModEntry : Mod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal PerScreen<MorningState> State { get; } = new(() => new MorningState());

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Harmony harmony = new(ModManifest.UniqueID);
        try
        {
            harmony.Patch(GamePatches.FindClockBody(), transpiler: Patch(nameof(GamePatches.ExtendClock)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.UpdateOther)), transpiler: Patch(nameof(GamePatches.RemoveTimedPassOut)));
            harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.Update)), transpiler: Patch(nameof(GamePatches.KeepCombatDefeat)));
            harmony.Patch(AccessTools.Method(typeof(FishingRod), nameof(FishingRod.DoFunction)), transpiler: Patch(nameof(GamePatches.KeepPondFishing)));
            harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.passOutFromTired)), prefix: Patch(nameof(GamePatches.RecordPassOut)));
            harmony.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(Game1), "_newDayAfterFade")),
                transpiler: Patch(nameof(GamePatches.ChangeSettlement)));
            harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.dayupdate)),
                prefix: Patch(nameof(GamePatches.PreserveEnergyBeforeDayUpdate)), postfix: Patch(nameof(GamePatches.PreserveEnergyAfterDayUpdate)));
            harmony.Patch(AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanWakeUpHere)), prefix: Patch(nameof(GamePatches.AllowAwakeLocation)));
            harmony.Patch(AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.ApplyWakeUpPosition)), postfix: Patch(nameof(GamePatches.AfterWakeUp)));
            harmony.Patch(AccessTools.Method(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), new[] { typeof(SpriteBatch) }),
                prefix: Patch(nameof(GamePatches.BeforeClockDraw)), transpiler: Patch(nameof(GamePatches.NormalClockColor)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.UpdateGameClock)),
                postfix: new HarmonyMethod(typeof(Sunrise), nameof(Sunrise.AfterClockUpdate)));
            harmony.Patch(AccessTools.Method(typeof(Game1), "_draw"), prefix: Patch(nameof(GamePatches.DrawAwakeTransition)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer)),
                postfix: Patch(nameof(GamePatches.KeepDawnOnBuffer)));
        }
        catch (Exception exception)
        {
            harmony.UnpatchAll(ModManifest.UniqueID);
            Monitor.Log(helper.Translation.Get("patch-error"), LogLevel.Error);
            Monitor.Log(exception.ToString(), LogLevel.Error);
            return;
        }

        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.GameLoop.Saving += OnSaving;
        helper.Events.GameLoop.Saved += OnSaved;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.GameLoop.GameLaunched += (_, _) => IntegrateTimeDisplay(harmony);
        helper.Events.GameLoop.ReturnedToTitle += (_, _) =>
        {
            foreach (var screen in State.GetActiveValues())
                screen.Value.Presentation.Dispose();
            State.ResetAllScreens();
        };
        helper.Events.Multiplayer.PeerConnected += (_, _) => Monitor.Log(helper.Translation.Get("multiplayer"), LogLevel.Info);
    }

    private static HarmonyMethod Patch(string name) => new(typeof(GamePatches), name);

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        // DayStarted can occur when the save menu closes, before the native morning fade finishes.
        // Keep the scene visible until the fade callback has completed to avoid a final black flash.
        if (State.Value.DayHasStarted && State.Value.CanFinishPresentation(Game1.globalFade, Game1.fadeToBlackAlpha, Game1.newDay))
        {
            State.Value.Presentation.Dispose();
            State.Value = new MorningState();
        }
        if (!Context.IsWorldReady || Game1.timeOfDay < 3000 || Game1.newDay || State.Value.DawnStarted
            || Game1.player.passedOut || Game1.player.timeWentToBed.Value > 0 || Game1.eventUp
            || Game1.locationRequest is not null || Game1.currentMinigame is not null || Game1.killScreen
            || Game1.player.FarmerSprite.isPassingOut())
            return;

        Farmer farmer = Game1.player;
        try
        {
            State.Value.Presentation.Capture(Game1.game1);
        }
        catch (Exception exception)
        {
            Monitor.Log($"Impossible de conserver la scène à 6 h ; transition native utilisée. {exception.Message}", LogLevel.Warn);
        }
        State.Value.DawnStarted = true;
        MorningState.RememberPosition(farmer);
        farmer.mount?.dismount();
        if (farmer.IsSitting())
            farmer.StopSitting(animate: false);
        farmer.completelyStopAnimatingOrDoingAction();
        if (Game1.activeClickableMenu is not null)
        {
            Game1.activeClickableMenu.emergencyShutDown();
            Game1.exitActiveMenu();
        }
        farmer.lastSleepLocation.Value = farmer.currentLocation.NameOrUniqueName;
        farmer.lastSleepPoint.Value = farmer.TilePoint;
        farmer.CanMove = false;
        // This flag activates the native day/save fade without invoking the costly pass-out/rescue routines.
        farmer.passedOut = true;
        Monitor.Log("6 h : passage au lendemain sans sommeil.", LogLevel.Trace);
        if (Game1.IsMultiplayer)
            Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: false, _ => Game1.NewDay(0f));
        else
            Game1.NewDay(0f);
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        if (Context.IsMainPlayer)
            ShippingStorage.StoreBeforeSave();
    }

    private void OnSaved(object? sender, SavedEventArgs e)
    {
        if (Context.IsMainPlayer)
            ShippingStorage.RestoreAfterSaveOrLoad();
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        SaveDataMigration.MigrateFarmerData(Game1.player);
        Game1.player.modData.Remove(SalePolicy.PassOutDayKey);
        if (Context.IsMainPlayer)
            ShippingStorage.RestoreAfterSaveOrLoad();
        MorningState.ApplyPosition(Game1.player);
        if (Context.IsMultiplayer)
            Monitor.Log(Helper.Translation.Get("multiplayer"), LogLevel.Info);
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        Game1.player.modData.Remove(SalePolicy.PassOutDayKey);
        if (Context.IsMainPlayer)
            ShippingStorage.RestoreAfterSaveOrLoad();
        if (State.Value.DawnStarted)
        {
            Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("dawn"), HUDMessage.newQuest_type));
            State.Value.DayHasStarted = true;
        }
        else
        {
            State.Value.Presentation.Dispose();
            State.Value = new MorningState();
        }
    }

    private void IntegrateTimeDisplay(Harmony harmony)
    {
        // ItsStardewTime redraws the clock after vanilla, so apply the same color rule to its renderer.
        const string name = "ItsStardewTime.Patches.TimeDisplayPatches.DayTimeMoneyBoxPatches";
        Type? type = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(name)).FirstOrDefault(type => type is not null);
        if (type is null)
            return;
        try
        {
            harmony.Patch(AccessTools.Method(type, "HandleClock"), transpiler: Patch(nameof(GamePatches.NormalClockColor)));
        }
        catch (Exception exception)
        {
            Monitor.Log($"Impossible d'adapter l'affichage de l'horloge ItsStardewTime : {exception.Message}", LogLevel.Warn);
        }
    }
}
