using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using StardewValley.Inventories;

namespace ICGNS.Tests;

internal static class GameplayChecks
{
    private static bool interceptedBed, interceptedPassOut;

    internal static void Run(Action<bool, string> check)
    {
        ModEntry? previous = ModEntry.Instance;
        Farmer? previousPlayer = Game1.player;
        var previousContent = Game1.content;
        Harmony harmony = new("Tom.ICGNS.GameplayChecks");
        try
        {
            ModEntry mod = new();
            AccessTools.Field(typeof(ModEntry), "<Instance>k__BackingField").SetValue(null, mod);
            check(GamePatches.ClockLimit() == 3000 && GamePatches.TimedPassOutTime() == int.MaxValue
                && GamePatches.EmergencyPassOutTime() == int.MaxValue, "Enabled mode must keep all forced time pass-outs disabled until dawn.");
            mod.Config.EnableMod = false;
            check(GamePatches.ClockLimit() == 2600 && GamePatches.TimedPassOutTime() == 2600
                && GamePatches.EmergencyPassOutTime() == 2800, "Disabling the mod must restore vanilla clock and pass-out thresholds immediately.");
            check(GamePatches.GetClockColor() == Color.Red && GamePatches.ClockShakeDuration() == 2000,
                "Disabled mode must restore the native late-hour clock warnings.");
            Farmer farmer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
            AccessTools.Field(typeof(Farmer), nameof(Farmer.isInBed)).SetValue(farmer, new NetBool());
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, farmer);
            check(GamePatches.BeforeWakeUp(farmer), "Disabled mode must bypass awake-position overrides.");
            DayTimeMoneyBox clock = (DayTimeMoneyBox)RuntimeHelpers.GetUninitializedObject(typeof(DayTimeMoneyBox));
            clock.timeShakeTimer = 1234;
            GamePatches.BeforeClockDraw(clock);
            check(clock.timeShakeTimer == 1234, "Disabled mode must not clear native clock shaking.");
            mod.State.Value.DawnStarted = true;
            check(GameplaySettings.Enabled, "Disabling during a transition must let the existing save finish safely.");
            mod.State.Value.DawnStarted = false;
            check(!GameplaySettings.Enabled, "The master switch must take effect after the transition finishes.");
            mod.Config.EnableMod = true;
            GamePatches.BeforeClockDraw(clock);
            check(clock.timeShakeTimer == 0 && GamePatches.ClockShakeDuration() == 0, "Re-enabling must suppress clock warnings again.");

            check(!SalePolicy.SellsAtDawn(true, false, 3000) && SalePolicy.SellsAtDawn(true, true, 3000),
                "Only the enabled opt-in setting must authorize sales for an all-nighter.");
            check(!SalePolicy.SellsAtDawn(true, true, 2950) && !SalePolicy.SellsAtDawn(false, true, 3000),
                "The option must not sell early or override disabled mode.");
            Dictionary<long, bool> online = new() { [1] = SalePolicy.SellsAtDawn(true, true, 3000), [2] = true };
            check(SalePolicy.CanSell(false, 1, online) && SalePolicy.CanSell(true, 1, online)
                && !SalePolicy.CanSell(true, 3, online), "Dawn shipping must work for shared and personal bins while retaining offline players' bins.");
            harmony.Patch(AccessTools.Method(typeof(PlayerStatusList), nameof(PlayerStatusList.AddSpriteDefinition)),
                prefix: new HarmonyMethod(typeof(GameplayChecks), nameof(SkipVisuals)));
            farmer.teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());
            AccessTools.Field(typeof(Farmer), nameof(Farmer.uniqueMultiplayerID)).SetValue(farmer, new NetLong(1));
            AccessTools.Field(typeof(Farmer), nameof(Farmer.personalShippingBin)).SetValue(farmer, new NetRef<Inventory>(new Inventory()));
            Farm farm = (Farm)RuntimeHelpers.GetUninitializedObject(typeof(Farm));
            Inventory bin = new();
            bin.Add(new StardewValley.Object { ItemId = "24", Stack = 7 });
            // Farm's static initializer only loads a house texture. Supply an
            // asset-free manager for that visual field in this headless fixture.
            using (TexturelessContent content = new())
            {
                Game1.content = content;
                AccessTools.Field(typeof(Farm), "sharedShippingBin").SetValue(farm, new NetRef<Inventory>(bin));
                Game1.content = previousContent;
            }
            mod.State.Value.SettlementPrepared = true;
            mod.State.Value.Sleeping[1] = false;
            check(GamePatches.GetSettlementBin(farm, farmer).Count == 0 && bin.Count == 1,
                "The default all-nighter must hide items from settlement without deleting the real bin.");
            mod.State.Value.Sleeping[1] = true;
            check(ReferenceEquals(GamePatches.GetSettlementBin(farm, farmer), bin),
                "A dawn-authorized sale must expose the real bin to native pricing and shipping.");
            mod.State.Value.SettlementPrepared = false;
            mod.State.Value.Sleeping[1] = false;
            check(ReferenceEquals(GamePatches.GetSettlementBin(farm, farmer), bin),
                "Disabled mode must use the native shipping inventory regardless of awake markers.");

            MorningState state = new();
            check(!GamePatches.NeedsSceneBuffer(2600, false, state) && !GamePatches.NeedsSceneBuffer(2800, false, state),
                "The extended night must not force an extra world render at 2AM or throughout sunrise.");
            check(GamePatches.NeedsSceneBuffer(2950, false, state) && !GamePatches.NeedsSceneBuffer(2950, true, state),
                "Only the quiet save's final pre-dawn interval needs the capture buffer.");
            state.DawnStarted = true;
            check(GamePatches.NeedsSceneBuffer(600, false, state), "The quiet save must keep its buffer until the transition ends.");
            state.UseShippingScreen = true;
            check(!GamePatches.NeedsSceneBuffer(600, false, state), "Native shipping transitions must never force the quiet-save buffer.");
            mod.State.Value.DawnStarted = true;
            mod.State.Value.UseShippingScreen = true;
            check(GamePatches.DrawAwakeTransition(null!, new GameTime(), null!),
                "The quiet-save renderer must allow native shipping screens and fades to draw.");
            mod.State.Value.DawnStarted = false;

            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.NewDay)),
                prefix: new HarmonyMethod(typeof(GameplayChecks), nameof(InterceptNewDay)));
            farmer.passedOut = true;
            ModEntry.BeginAwakeNewDay(false);
            check(interceptedPassOut && !interceptedBed, "Default mode must retain the quiet dawn transition.");
            farmer.passedOut = true;
            ModEntry.BeginAwakeNewDay(true);
            check(!interceptedPassOut && interceptedBed && !farmer.isInBed.Value,
                "Shipping mode must request vanilla's gradual bed fade and restore the temporary bed flag immediately.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            AccessTools.Field(typeof(ModEntry), "<Instance>k__BackingField").SetValue(null, previous);
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, previousPlayer);
            Game1.content = previousContent;
        }
    }

    private static bool InterceptNewDay()
    {
        interceptedBed = Game1.player.isInBed.Value;
        interceptedPassOut = Game1.player.passedOut;
        return false;
    }

    private static bool SkipVisuals() => false;

    private sealed class TexturelessContent : LocalizedContentManager
    {
        internal TexturelessContent() : base(new EmptyServices(), "Content") { }
        public override T Load<T>(string assetName) => default!;
    }

    private sealed class EmptyServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
