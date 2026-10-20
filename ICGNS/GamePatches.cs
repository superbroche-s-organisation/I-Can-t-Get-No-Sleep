using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using HarmonyLib;
using StardewValley;
using StardewValley.Inventories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Locations;

namespace ICGNS;

internal static class GamePatches
{
    internal static MethodInfo FindClockBody()
    {
        // The game wraps its clock update in a delegate to expose its mod hook.
        return typeof(Game1).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .Single(method => method.Name.StartsWith("<performTenMinuteClockUpdate>", StringComparison.Ordinal)
                && method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
    }

    internal static IEnumerable<CodeInstruction> ExtendClock(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        ReplaceConstant(code, 2600, nameof(ClockLimit), 2);
        ReplaceConstant(code, 2800, nameof(EmergencyPassOutTime), 1);
        MethodInfo emote = AccessTools.Method(typeof(Farmer), nameof(Farmer.doEmote), new[] { typeof(int) });
        List<CodeInstruction> emotes = code.Where(instruction => instruction.Calls(emote)).ToList();
        if (emotes.Count != 2)
            throw new InvalidOperationException("Unexpected late-hour emote code.");
        foreach (CodeInstruction instruction in emotes)
        {
            instruction.opcode = OpCodes.Call;
            instruction.operand = AccessTools.Method(typeof(GamePatches), nameof(QuietClockEmote));
        }
        FieldInfo shake = AccessTools.Field(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.timeShakeTimer));
        int shakeCount = 0;
        int warningCount = 0;
        MethodInfo message = AccessTools.Method(typeof(Game1), nameof(Game1.showGlobalMessage));
        for (int i = 2; i < code.Count; i++)
        {
            if (code[i].StoresField(shake) && code[i - 1].LoadsConstant(2000))
            {
                code[i - 1].opcode = OpCodes.Call;
                code[i - 1].operand = AccessTools.Method(typeof(GamePatches), nameof(ClockShakeDuration));
                shakeCount++;
            }
            if (code[i].Calls(message) && code[i - 2].operand as string == "Strings\\StringsFromCSFiles:Game1.cs.2652")
            {
                code[i].opcode = OpCodes.Call;
                code[i].operand = AccessTools.Method(typeof(GamePatches), nameof(QuietBedtimeWarning));
                warningCount++;
            }
        }
        if (shakeCount != 3 || warningCount != 1)
            throw new InvalidOperationException("Unexpected late-hour warning code.");
        return code;
    }

    internal static void QuietClockEmote(Farmer farmer, int emote)
    {
        if (!GameplaySettings.Enabled || emote != 24)
            farmer.doEmote(emote);
    }

    internal static void QuietBedtimeWarning(string message)
    {
        if (!GameplaySettings.Enabled)
            Game1.showGlobalMessage(message);
    }

    internal static int ClockLimit() => GameplaySettings.Enabled ? 3000 : 2600;
    internal static int EmergencyPassOutTime() => GameplaySettings.Enabled ? int.MaxValue : 2800;
    internal static int TimedPassOutTime() => GameplaySettings.Enabled ? int.MaxValue : 2600;
    internal static int ClockShakeDuration() => GameplaySettings.Enabled ? 0 : 2000;

    internal static IEnumerable<CodeInstruction> NormalClockColor(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        MethodInfo red = AccessTools.PropertyGetter(typeof(Color), nameof(Color.Red));
        List<CodeInstruction> colors = code.Where(instruction => instruction.Calls(red)).ToList();
        if (colors.Count != 1)
            throw new InvalidOperationException("Unexpected clock color code.");
        colors[0].operand = AccessTools.Method(typeof(GamePatches), nameof(GetClockColor));
        return code;
    }

    internal static Color GetClockColor() => GameplaySettings.Enabled ? Game1.textColor : Color.Red;

    internal static void BeforeClockDraw(DayTimeMoneyBox __instance)
    {
        if (GameplaySettings.Enabled)
            __instance.timeShakeTimer = 0;
    }

    internal static void KeepDawnOnBuffer(ref bool __result)
    {
        // At 100% zoom vanilla draws directly to the backbuffer and leaves screen
        // stale. Render dawn to screen too so its capture always contains the scene.
        if (GameplaySettings.Enabled && NeedsSceneBuffer(Game1.timeOfDay, GameplaySettings.SellAt6AM, ModEntry.Instance.State.Value))
            __result = true;
    }

    internal static bool NeedsSceneBuffer(int time, bool sellAt6AM, MorningState state)
        => state.DawnStarted ? !state.UseShippingScreen : !sellAt6AM && time is >= 2950 and <= 3000;

    internal static bool DrawAwakeTransition(Game1 __instance, GameTime gameTime, RenderTarget2D target_screen)
    {
        MorningState state = ModEntry.Instance.State.Value;
        // Native overnight events keep their own rendering and input; ordinary days use the saved scene.
        return !state.DawnStarted || state.UseShippingScreen || Game1.farmEvent is not null || Game1.eventUp
            || !state.Presentation.Draw(__instance, gameTime, target_screen);
    }

    internal static IEnumerable<CodeInstruction> RemoveTimedPassOut(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        ReplaceConstant(code, 2600, nameof(TimedPassOutTime), 1); // Leave the independent stamina check intact.
        return code;
    }

    internal static IEnumerable<CodeInstruction> KeepCombatDefeat(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        ReplaceConstant(code, 2600, nameof(TimedPassOutTime), 1); // Combat defeat must still work during the extended night.
        return code;
    }

    internal static IEnumerable<CodeInstruction> KeepPondFishing(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        ReplaceConstant(code, 2600, nameof(TimedPassOutTime), 1);
        return code;
    }

    internal static IEnumerable<CodeInstruction> ChangeSettlement(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();
        MethodInfo shipping = AccessTools.Method(typeof(Farm), nameof(Farm.getShippingBin));
        MethodInfo minutes = AccessTools.Method(typeof(Utility), nameof(Utility.CalculateMinutesUntilMorning), new[] { typeof(int) });
        List<CodeInstruction> bins = code.Where(instruction => instruction.Calls(shipping)).ToList();
        List<CodeInstruction> elapsed = code.Where(instruction => instruction.Calls(minutes)).ToList();
        if (bins.Count != 4 || elapsed.Count != 1)
            throw new InvalidOperationException($"Unexpected new-day code: {bins.Count} shipping references and {elapsed.Count} time references.");

        elapsed[0].opcode = OpCodes.Call;
        elapsed[0].operand = AccessTools.Method(typeof(GamePatches), nameof(PrepareSettlement));
        // The first call transfers mini-bin contents into the actual bin. Always keep it.
        // The remaining calls calculate sales, handle offline farmers, and clear sold items.
        foreach (CodeInstruction instruction in bins.Skip(1))
        {
            instruction.opcode = OpCodes.Call;
            instruction.operand = AccessTools.Method(typeof(GamePatches), nameof(GetSettlementBin));
        }
        ReplaceCleanup(typeof(MineShaft), nameof(MineShaft.clearActiveMines), nameof(DungeonContinuity.ClearMines));
        ReplaceCleanup(typeof(VolcanoDungeon), nameof(VolcanoDungeon.ClearAllLevels), nameof(DungeonContinuity.ClearVolcano));
        return code;

        void ReplaceCleanup(Type type, string original, string replacement)
        {
            MethodInfo method = AccessTools.Method(type, original);
            List<CodeInstruction> calls = code.Where(instruction => instruction.Calls(method)).ToList();
            if (calls.Count != 1)
                throw new InvalidOperationException($"Unexpected new-day cleanup for {type.Name}: {calls.Count} calls.");
            calls[0].operand = AccessTools.Method(typeof(DungeonContinuity), replacement);
        }
    }

    internal static int PrepareSettlement(int time)
    {
        MorningState state = ModEntry.Instance.State.Value;
        state.SettlementPrepared = GameplaySettings.Enabled;
        state.SeparateWallets = Game1.player.team.useSeparateWallets.Value;
        state.Sleeping.Clear();
        foreach (Farmer farmer in Game1.getOnlineFarmers().Where(farmer => !farmer.IsDedicatedPlayer))
            state.Sleeping[farmer.UniqueMultiplayerID] = SalePolicy.HasSlept(farmer, Game1.Date.TotalDays)
                || SalePolicy.SellsAtDawn(GameplaySettings.Enabled, GameplaySettings.SellAt6AM, time);

        // Remove a previous morning marker when this player really chooses to sleep.
        if (!state.DawnStarted)
            Game1.player.modData.Remove(MorningState.PositionKey);

        // Vanilla assumes nights end by 2 AM. After that time, don't grant extra phantom processing hours.
        return GameplaySettings.Enabled && time > 2600
            ? Math.Max(0, 30 * 60 - Utility.ConvertTimeToMinutes(time)) : Utility.CalculateMinutesUntilMorning(time);
    }

    internal static void RecordPassOut(Farmer who)
    {
        if (GameplaySettings.Enabled && who.IsLocalPlayer)
            who.modData[SalePolicy.PassOutDayKey] = Game1.Date.TotalDays.ToString(CultureInfo.InvariantCulture);
    }

    internal static IInventory GetSettlementBin(Farm farm, Farmer farmer)
    {
        MorningState state = ModEntry.Instance.State.Value;
        return !state.SettlementPrepared || SalePolicy.CanSell(state.SeparateWallets, farmer.UniqueMultiplayerID, state.Sleeping)
            ? farm.getShippingBin(farmer)
            : new Inventory();
    }

    internal static void PreserveEnergyBeforeDayUpdate(Farmer __instance, out (float Stamina, int Health, bool Exhausted)? __state)
    {
        __state = __instance.IsLocalPlayer && ModEntry.Instance.State.Value.DawnStarted
            ? (__instance.Stamina, __instance.health, __instance.exhausted.Value)
            : null;
    }

    internal static void PreserveEnergyAfterDayUpdate(Farmer __instance, (float Stamina, int Health, bool Exhausted)? __state)
    {
        if (__state is not { } energy)
            return;
        __instance.Stamina = energy.Stamina;
        __instance.health = energy.Health;
        __instance.exhausted.Value = energy.Exhausted;
    }

    internal static bool AllowAwakeLocation(GameLocation __instance, Farmer who, ref bool __result)
    {
        if (!GameplaySettings.Enabled || !MorningState.TryGetPosition(who, out string name, out _, out _) || name != __instance.NameOrUniqueName)
            return true;
        __result = true;
        return false;
    }

    internal static bool BeforeWakeUp(Farmer who) => !GameplaySettings.Enabled || !MorningState.ApplyPosition(who);

    private static void ReplaceConstant(List<CodeInstruction> code, int oldValue, string getter, int expected)
    {
        List<CodeInstruction> matches = code.Where(instruction => instruction.LoadsConstant(oldValue)).ToList();
        if (matches.Count != expected)
            throw new InvalidOperationException($"Expected {expected} clock constants {oldValue}, found {matches.Count}.");
        foreach (CodeInstruction instruction in matches)
        {
            instruction.opcode = OpCodes.Call;
            instruction.operand = AccessTools.Method(typeof(GamePatches), getter);
        }
    }
}
