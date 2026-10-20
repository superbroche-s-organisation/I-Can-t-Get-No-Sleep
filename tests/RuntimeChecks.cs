using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using HarmonyLib;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.BellsAndWhistles;
using System.Runtime.CompilerServices;
using Netcode;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Runtime.Loader;
using System.IO.Compression;
using System.Text.Json;

namespace ICGNS.Tests;

internal static class RuntimeChecks
{
    private static int checks;

    internal static void Run(string gamePath, bool graphics)
    {
        ConfigurationDefaults();
        ShippingRules();
        PassOutShipping();
        ActualGamePatches();
        DungeonChecks.Run(Check);
        ConfigMenuChecks.Run(gamePath, Check);
        GameplayChecks.Run(Check);
        PathfindingChecks.Run(Check);
        ItemRetention();
        TeamInventoryPersistence();
        SunriseContinuity();
        NativeSaveDisplayGate();
        TimeDisplayCompatibility(gamePath);
        if (graphics)
            GraphicsChecks.Run(gamePath, Check);
        Console.WriteLine($"{checks} vérifications réussies (règles, patches, conservation, sérialisation et lumière{(graphics ? ", rendu GPU inclus" : "")}).");
    }

    private static void ConfigurationDefaults()
    {
        Check(JsonSerializer.Deserialize<ModConfig>("{}")!.EnableSunrise,
            "An absent sunrise setting must preserve the existing lighting behavior.");
        Check(!JsonSerializer.Deserialize<ModConfig>("{\"EnableSunrise\":false}")!.EnableSunrise,
            "The config must allow other lighting mods to take over sunrise.");
        ModConfig defaults = JsonSerializer.Deserialize<ModConfig>("{}")!;
        Check(defaults.EnableMod && !defaults.SellAt6AM, "The mod must default on and dawn shipping off for existing configs.");
    }

    private static void ShippingRules()
    {
        Dictionary<long, bool> solo = new() { [1] = false };
        Check(!SalePolicy.CanSell(false, 1, solo), "Solo all-nighter must retain items.");
        solo[1] = true;
        Check(SalePolicy.CanSell(false, 1, solo), "Voluntary solo sleep must sell.");
        Dictionary<long, bool> mixed = new() { [1] = true, [2] = false };
        Check(!SalePolicy.CanSell(false, 1, mixed), "Shared bin must wait when someone stays awake.");
        Check(SalePolicy.CanSell(true, 1, mixed), "Separate wallet: sleeper sells.");
        Check(!SalePolicy.CanSell(true, 2, mixed), "Separate wallet: awake player keeps items.");
        Check(!SalePolicy.CanSell(true, 3, mixed), "Offline player's items must be retained.");
        mixed[2] = true;
        Check(SalePolicy.CanSell(false, 2, mixed), "Shared bin sells when everyone sleeps.");
        mixed[1] = false;
        Check(!SalePolicy.CanSell(false, 2, mixed), "Cancelling sleep must prevent shared shipping.");
        Check(!SalePolicy.CanSell(false, 1, new Dictionary<long, bool>()), "Empty player list must not authorize sales.");
    }

    private static void PassOutShipping()
    {
        // Minimal real Farmer fields allow eligibility checks without loading a player save or graphics.
        Farmer farmer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
        AccessTools.Field(typeof(Farmer), nameof(Farmer.timeWentToBed)).SetValue(farmer, new NetInt());
        AccessTools.Field(typeof(Character), "<modData>k__BackingField").SetValue(farmer, new StardewValley.Mods.ModDataDictionary());
        const int day = 42;
        Check(!SalePolicy.HasSlept(farmer, day), "An awake player must not authorize shipping.");
        farmer.passedOut = true;
        Check(!SalePolicy.HasSlept(farmer, day), "The synthetic 6 AM passedOut flag must not authorize shipping.");
        farmer.modData[SalePolicy.PassOutDayKey] = day.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Check(SalePolicy.HasSlept(farmer, day), "A genuine pass-out this day must authorize shipping.");
        Dictionary<long, bool> sleeping = new() { [1] = SalePolicy.HasSlept(farmer, day) };
        Check(SalePolicy.CanSell(false, 1, sleeping), "Solo pass-out must sell the shared shipping bin.");
        sleeping[2] = true;
        Check(SalePolicy.CanSell(false, 1, sleeping), "A pass-out and a voluntary sleeper must allow shared shipping.");
        sleeping[2] = false;
        Check(!SalePolicy.CanSell(false, 1, sleeping), "Shared shipping must still wait if someone remains awake.");
        Check(SalePolicy.CanSell(true, 1, sleeping), "Separate wallets: an exhausted player must sell their own bin.");
        Check(!SalePolicy.HasSlept(farmer, day + 1), "A previous day's pass-out must not sell an awake player's next-day items.");
        farmer.timeWentToBed.Value = 2200;
        Check(SalePolicy.HasSlept(farmer, day + 1), "Voluntary sleep must still authorize shipping after an older pass-out.");
        farmer.timeWentToBed.Value = 0;
        Check(!SalePolicy.HasSlept(farmer, day + 1), "Cancelling sleep must not reuse an old pass-out marker.");
    }

    private static void ActualGamePatches()
    {
        MethodInfo clock = GamePatches.FindClockBody();
        List<CodeInstruction> patchedClock = GamePatches.ExtendClock(PatchProcessor.GetOriginalInstructions(clock)).ToList();
        Check(patchedClock.Count(instruction => instruction.Calls(AccessTools.Method(typeof(GamePatches), nameof(GamePatches.ClockLimit)))) == 2,
            "Clock and cleanup must use the runtime on/off setting.");
        Check(!patchedClock.Any(instruction => instruction.LoadsConstant(2800)), "No forced 4 AM pass-out.");
        Check(!patchedClock.Any(instruction => instruction.Calls(AccessTools.Method(typeof(Farmer), nameof(Farmer.doEmote), new[] { typeof(int) }))),
            "Hourly updates must not emit fatigue bubbles.");
        Check(patchedClock.Count(instruction => instruction.Calls(AccessTools.Method(typeof(GamePatches), nameof(GamePatches.QuietClockEmote)))) == 2,
            "Both midnight and 1 AM fatigue bubbles must be suppressed.");
        Check(patchedClock.Count(instruction => instruction.Calls(AccessTools.Method(typeof(GamePatches), nameof(GamePatches.QuietBedtimeWarning)))) == 1,
            "Only the midnight bedtime warning must be suppressed.");
        FieldInfo shake = AccessTools.Field(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.timeShakeTimer));
        Check(patchedClock.Select((instruction, index) => (instruction, index))
            .Where(entry => entry.instruction.StoresField(shake)).All(entry => patchedClock[entry.index - 1]
                .Calls(AccessTools.Method(typeof(GamePatches), nameof(GamePatches.ClockShakeDuration)))),
            "Late-hour clock shaking must follow the runtime on/off setting.");
        MethodInfo update = AccessTools.Method(typeof(Game1), nameof(Game1.UpdateOther));
        List<CodeInstruction> patchedUpdate = GamePatches.RemoveTimedPassOut(PatchProcessor.GetOriginalInstructions(update)).ToList();
        Check(!patchedUpdate.Any(instruction => instruction.LoadsConstant(2600)), "No 2 AM time check.");
        Check(patchedUpdate.Any(instruction => instruction.LoadsConstant(-15f)), "Exhaustion still applies.");
        MethodInfo farmerUpdate = AccessTools.Method(typeof(Farmer), nameof(Farmer.Update));
        List<CodeInstruction> combat = GamePatches.KeepCombatDefeat(PatchProcessor.GetOriginalInstructions(farmerUpdate)).ToList();
        Check(!combat.Any(instruction => instruction.LoadsConstant(2600)), "Combat defeat works after 2 AM.");
        MethodBase settlement = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(Game1), "_newDayAfterFade"));
        List<CodeInstruction> patchedSettlement = GamePatches.ChangeSettlement(PatchProcessor.GetOriginalInstructions(settlement)).ToList();
        MethodInfo getBin = AccessTools.Method(typeof(Farm), nameof(Farm.getShippingBin));
        Check(patchedSettlement.Count(instruction => instruction.Calls(getBin)) == 1, "Mini-bin transfer must use the real bin.");
        Check(patchedSettlement.Count(instruction => instruction.Calls(AccessTools.Method(typeof(GamePatches), nameof(GamePatches.GetSettlementBin)))) == 3,
            "Sales, offline sales and clearing must all use the filtered bin.");
        Check(patchedSettlement.Count(instruction => instruction.Calls(AccessTools.Method(typeof(DungeonContinuity), nameof(DungeonContinuity.ClearMines)))) == 1
            && patchedSettlement.Count(instruction => instruction.Calls(AccessTools.Method(typeof(DungeonContinuity), nameof(DungeonContinuity.ClearVolcano)))) == 1,
            "Native overnight cleanup must preserve occupied mine and volcano floors.");

        // Compile Harmony's actual replacement methods to validate stack balance and patch signatures.
        Harmony harmony = new("Tom.ICGNS.RuntimeChecks");
        try
        {
            ModEntry.ApplyPatches(harmony);
            HashSet<MethodBase> patched = harmony.GetPatchedMethods().ToHashSet();
            Check(patched.Count == 14, "All required patches must install.");
            Check(patched.Contains(AccessTools.Method(typeof(Game1), nameof(Game1.UpdateGameClock))),
                "The sunrise hook must remain installed for live checkbox changes.");
            Check(patched.Contains(clock) && patched.Contains(update) && patched.Contains(settlement),
                "Disabling sunrise must retain the extended clock, pass-out and shipping patches.");
            Check(patched.Contains(AccessTools.Method(typeof(Game1), "_draw"))
                && patched.Contains(AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer))),
                "The 6 AM save display must remain available in either lighting mode.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static void ItemRetention()
    {
        StardewValley.Object item = new() { ItemId = "24", Stack = 17, Quality = 2 };
        item.modData["Example.Custom/Data"] = "metadata must survive";
        Inventory source = new();
        source.Add(item);
        Inventory saved = new();
        ShippingStorage.Move(source, saved);
        Check(source.Count == 0 && saved.Count == 1 && ReferenceEquals(saved[0], item), "Moving items must preserve identity without duplication.");
        ShippingStorage.Move(saved, saved);
        Check(saved.Count == 1, "Moving to the same container must be harmless.");

        XmlSerializer serializer = new(typeof(Inventory));
        using StringWriter text = new();
        serializer.Serialize(text, saved);
        using StringReader reader = new(text.ToString());
        Inventory loaded = (Inventory)serializer.Deserialize(reader)!;
        Check(loaded.Count == 1 && loaded[0].ItemId == "24" && loaded[0].Stack == 17 && loaded[0].Quality == 2,
            "Game inventory serialization must preserve ID, quantity and quality.");
        Check(loaded[0].modData["Example.Custom/Data"] == "metadata must survive", "Custom item metadata must survive saving.");

        Inventory morning = new();
        ShippingStorage.Move(loaded, morning);
        ShippingStorage.Move(loaded, morning);
        Check(loaded.Count == 0 && morning.Count == 1, "Restoring twice must not duplicate retained items.");
        Inventory nextSave = new();
        ShippingStorage.Move(morning, nextSave);
        Check(morning.Count == 0 && nextSave.Count == 1, "Items must survive a second consecutive all-nighter.");
    }

    private static HarmonyMethod Patch(string name) => new(typeof(GamePatches), name);

    private static void TeamInventoryPersistence()
    {
        // FarmerTeam also loads multiplayer UI icons. Skip that visual-only setup in this headless process.
        Harmony visuals = new("Tom.ICGNS.HeadlessVisuals");
        try
        {
            visuals.Patch(AccessTools.Method(typeof(PlayerStatusList), nameof(PlayerStatusList.AddSpriteDefinition)),
                prefix: new HarmonyMethod(typeof(RuntimeChecks), nameof(SkipVisuals)));
            FarmerTeam team = new();
            // Deserializing native team flags calls Game1.player.team; supply only that fixture, without a game window.
            Farmer fixturePlayer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
            fixturePlayer.teamRoot = new NetRoot<FarmerTeam>(team);
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, fixturePlayer);
            Inventory vault = team.GetOrCreateGlobalInventory(ShippingStorage.SharedKey);
            StardewValley.Object item = new() { ItemId = "24", Stack = 9, Quality = 4 };
            item.modData["Example/Data"] = "shared-vault";
            vault.Add(item);
            XmlSerializer serializer = new(typeof(FarmerTeam));
            using StringWriter writer = new();
            serializer.Serialize(writer, team);
            using StringReader reader = new(writer.ToString());
            FarmerTeam loaded = (FarmerTeam)serializer.Deserialize(reader)!;
            Check(loaded.globalInventories.ContainsKey(ShippingStorage.SharedKey), "The game's team serializer must include the vault.");
            Inventory restored = loaded.GetOrCreateGlobalInventory(ShippingStorage.SharedKey);
            Check(restored.Count == 1 && restored[0].Stack == 9 && restored[0].Quality == 4
                && restored[0].modData["Example/Data"] == "shared-vault", "Team save/reload must preserve retained items completely.");
            PreviewSaveMigration(loaded);
        }
        finally
        {
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, null);
            visuals.UnpatchAll(visuals.Id);
        }
    }

    private static bool SkipVisuals() => false;

    private static void PreviewSaveMigration(FarmerTeam team)
    {
        // Read the real private-build ID from the preserved build archive. The public
        // source and assembly don't need to retain its former name as a string.
        string previewArchive = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", ".tools", "release-history", "preview-0.2.1.zip"));
        if (!File.Exists(previewArchive))
        {
            Console.WriteLine("Private-preview archive unavailable: import checks skipped.");
            return;
        }
        using ZipArchive archive = ZipFile.OpenRead(previewArchive);
        ZipArchiveEntry manifest = archive.Entries.Single(entry => entry.FullName.Replace('\\', '/').EndsWith("/manifest.json", StringComparison.Ordinal));
        using JsonDocument json = JsonDocument.Parse(manifest.Open());
        string previewId = json.RootElement.GetProperty("UniqueID").GetString()!;
        string sharedKey = previewId + "/Shipping/Shared";
        string personalKey = previewId + "/Shipping/42";
        string offlineKey = previewId + "/Shipping/-51";
        string unrelatedKey = "Tom.UnrelatedMod/Shipping/Shared";
        string unknownKey = previewId + "/Shipping/NotAFarmer";
        Inventory current = team.GetOrCreateGlobalInventory(ShippingStorage.SharedKey);
        int currentCount = current.Count;
        Inventory previous = team.GetOrCreateGlobalInventory(sharedKey);
        StardewValley.Object sharedItem = new() { ItemId = "24", Stack = 7, Quality = 2 };
        sharedItem.modData["Example/PreviewData"] = "preserve";
        previous.Add(sharedItem);
        team.GetOrCreateGlobalInventory(personalKey).Add(new StardewValley.Object { ItemId = "24", Stack = 12, Quality = 4 });
        team.GetOrCreateGlobalInventory(offlineKey).Add(new StardewValley.Object { ItemId = "24", Stack = 35 });
        Inventory unrelated = team.GetOrCreateGlobalInventory(unrelatedKey);
        unrelated.Add(new StardewValley.Object { ItemId = "24", Stack = 6 });
        Inventory unknown = team.GetOrCreateGlobalInventory(unknownKey);
        unknown.Add(new StardewValley.Object { ItemId = "24", Stack = 3 });

        SaveDataMigration.MigrateInventories(team);
        Check(!team.globalInventories.ContainsKey(sharedKey) && !team.globalInventories.ContainsKey(personalKey)
            && !team.globalInventories.ContainsKey(offlineKey), "Import must replace all recognized preview inventory keys.");
        Check(current.Count == currentCount + 1 && ReferenceEquals(current.Last(), sharedItem) && previous.Count == 0,
            "Import must merge into an existing ICGNS vault without replacing or cloning its items.");
        Check(sharedItem.Stack == 7 && sharedItem.Quality == 2 && sharedItem.modData["Example/PreviewData"] == "preserve",
            "Imported items must retain quantity, quality and custom data.");
        Check(team.GetOrCreateGlobalInventory(ShippingStorage.Prefix + "42").Single().Stack == 12,
            "Personal shipping must migrate to the same player's new inventory key.");
        Check(team.GetOrCreateGlobalInventory(ShippingStorage.Prefix + "-51").Single().Stack == 35,
            "Disconnected players' shipping must migrate without requiring an online farmer.");
        Check(team.globalInventories.ContainsKey(unrelatedKey) && unrelated.Single().Stack == 6,
            "The namespace fingerprint must leave other mods' inventories untouched.");
        Check(team.globalInventories.ContainsKey(unknownKey) && unknown.Single().Stack == 3,
            "Only recognized shipping owners may be migrated.");
        SaveDataMigration.MigrateInventories(team);
        Check(current.Count == currentCount + 1, "Repeated import must not duplicate retained items.");

        XmlSerializer serializer = new(typeof(FarmerTeam));
        using StringWriter writer = new();
        serializer.Serialize(writer, team);
        using StringReader reader = new(writer.ToString());
        FarmerTeam reloaded = (FarmerTeam)serializer.Deserialize(reader)!;
        Inventory migrated = reloaded.GetOrCreateGlobalInventory(ShippingStorage.SharedKey);
        Check(!reloaded.globalInventories.ContainsKey(sharedKey) && migrated.Count == currentCount + 1
            && migrated.Last().Stack == 7 && migrated.Last().modData["Example/PreviewData"] == "preserve",
            "Native save serialization must retain migrated items under the new ID.");

        Farmer farmer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
        AccessTools.Field(typeof(Character), "<modData>k__BackingField").SetValue(farmer, new StardewValley.Mods.ModDataDictionary());
        string previousPosition = previewId + "/AwakeMorning";
        string previousPassOut = previewId + "/PassOutDay";
        const string position = "43|Farm|128|192|2";
        farmer.modData[previousPosition] = position;
        farmer.modData[previousPassOut] = "42";
        farmer.modData["Tom.UnrelatedMod/AwakeMorning"] = "untouched";
        farmer.modData[previewId + "/UnknownField"] = "untouched";
        SaveDataMigration.MigrateFarmerData(farmer);
        Check(farmer.modData[MorningState.PositionKey] == position && farmer.modData[SalePolicy.PassOutDayKey] == "42",
            "Import must preserve both wake-up position and dated pass-out markers.");
        Check(!farmer.modData.ContainsKey(previousPosition) && !farmer.modData.ContainsKey(previousPassOut),
            "Import must remove obsolete recognized farmer keys.");
        Check(farmer.modData["Tom.UnrelatedMod/AwakeMorning"] == "untouched" && farmer.modData[previewId + "/UnknownField"] == "untouched",
            "Unrelated farmer metadata must not be changed.");
        farmer.modData[previousPosition] = "older position";
        SaveDataMigration.MigrateFarmerData(farmer);
        Check(farmer.modData[MorningState.PositionKey] == position && !farmer.modData.ContainsKey(previousPosition),
            "A current ICGNS marker must take precedence over a preview marker.");
        Check(PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(MorningState), nameof(MorningState.TryGetPosition)))
            .Any(instruction => instruction.Calls(AccessTools.Method(typeof(SaveDataMigration), nameof(SaveDataMigration.MigrateFarmerData)))),
            "Position import must be available during native wake-up, before SaveLoaded.");
    }

    private static void SunriseContinuity()
    {
        Check(Sunrise.Progress(2750, 1f) == 0f && Sunrise.Progress(2800, 0f) == 0f, "Sunrise must begin at 4 AM.");
        Check(Sunrise.Progress(2900, 0f) == 0.5f, "5 AM must be halfway through sunrise.");
        Check(Sunrise.Progress(3000, 0f) == 1f && Sunrise.Progress(3100, 0f) == 1f, "Sunrise must reach full daylight at 6 AM.");
        Check(Sunrise.Progress(2850, 1f) == Sunrise.Progress(2900, 0f), "Sunrise must be continuous across hour rollover.");
        Check(Sunrise.Progress(2950, 1f) == Sunrise.Progress(3000, 0f), "Sunrise must be continuous at 6 AM.");
        Check(Sunrise.Progress(2920, 0.5f) > Sunrise.Progress(2920, 0f), "Sunrise must interpolate between ten-minute ticks.");
        Color night = new(237, 237, 0, 237);
        Color morning = Sunrise.MorningLight(false, Color.White);
        Check(Sunrise.Blend(night, morning, 2800, 0f) == night, "Night lighting must remain unchanged at sunrise start.");
        Check(Sunrise.Blend(night, morning, 3000, 0f) == Color.Transparent, "Clear sunrise must reach a neutral subtractive lightmap, not the white skip-lighting sentinel.");
        Color rain = new Color(255, 200, 80) * 0.3f;
        Check(Sunrise.MorningLight(true, new Color(255, 200, 80)) == rain, "Rainy sunrise must preserve native morning weather lighting.");
        Check(Sunrise.Blend(night, rain, 3000, 0f) == rain, "Rainy-day sunrise must end at the rainy lighting color.");
    }

    private static void NativeSaveDisplayGate()
    {
        SaveGameMenu save = (SaveGameMenu)RuntimeHelpers.GetUninitializedObject(typeof(SaveGameMenu));
        Check(!save.hasDrawn, "Save display fixture must begin before its first draw.");
        DawnPresentation.EnableNativeSave(save);
        Check(save.hasDrawn, "A quiet save display must unlock the native save-start gate.");
        Check(PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(SaveGameMenu), nameof(SaveGameMenu.update)))
            .Any(instruction => instruction.LoadsField(AccessTools.Field(typeof(SaveGameMenu), nameof(SaveGameMenu.hasDrawn)))),
            "The installed game's save update must still use the verified display gate.");
        DawnPresentation.EnableNativeSave(null);
        using DawnPresentation presentation = new();
        Check(!presentation.IsCaptured, "An unavailable scene must allow the native display fallback.");
        MorningState state = new() { DawnStarted = true };
        Check(!state.CanFinishPresentation(false, 0f, false), "The scene must remain visible before the new day starts.");
        state.DayHasStarted = true;
        Check(!state.CanFinishPresentation(true, 0f, false), "The scene must remain visible until the native fade callback completes.");
        Check(!state.CanFinishPresentation(false, 1f, false), "The scene must cover any remaining black fade.");
        Check(!state.CanFinishPresentation(false, 0f, true), "The scene must remain stable until overnight updates finish.");
        Check(state.CanFinishPresentation(false, 0f, false), "The scene must release as soon as the morning is ready and clear.");
    }

    private static void TimeDisplayCompatibility(string gamePath)
    {
        string path = Path.Combine(gamePath, "Mods", "ItsStardewTime", "ItsStardewTime.dll");
        if (!File.Exists(path))
            return;
        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        Type type = assembly.GetType("ItsStardewTime.Patches.TimeDisplayPatches.DayTimeMoneyBoxPatches", throwOnError: true)!;
        MethodInfo clock = AccessTools.Method(type, "HandleClock");
        List<CodeInstruction> patched = GamePatches.NormalClockColor(PatchProcessor.GetOriginalInstructions(clock)).ToList();
        Check(!patched.Any(instruction => instruction.Calls(AccessTools.PropertyGetter(typeof(Color), nameof(Color.Red)))),
            "ItsStardewTime's redraw must not restore the red late-hour clock.");
        Harmony harmony = new("Tom.ICGNS.TimeDisplayChecks");
        try
        {
            harmony.Patch(clock, transpiler: Patch(nameof(GamePatches.NormalClockColor)));
            Check(harmony.GetPatchedMethods().Contains(clock), "The installed time mod's clock renderer must accept the compatibility patch.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
        checks++;
    }
}
