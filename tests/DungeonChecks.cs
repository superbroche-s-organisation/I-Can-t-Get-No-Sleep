using System.Globalization;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;

namespace ICGNS.Tests;

internal static class DungeonChecks
{
    internal static void Run(Action<bool, string> check)
    {
        Farmer? previousPlayer = Game1.player;
        Game1? previousGame = Game1.game1;
        var previousWorld = Game1.netWorldState;
        int previousDay = Game1.dayOfMonth;
        int previousYear = Game1.year;
        List<MineShaft> previousMines = MineShaft.activeMines.ToList();
        List<VolcanoDungeon> previousVolcano = VolcanoDungeon.activeLevels.ToList();
        try
        {
            Game1.game1 = (Game1)RuntimeHelpers.GetUninitializedObject(typeof(Game1));
            Game1.dayOfMonth = 1;
            Game1.year = 1;
            Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
            Farmer farmer = MakeFarmer(1);
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, farmer);
            MineShaft.activeMines.Clear();
            VolcanoDungeon.activeLevels.Clear();
            TrackedMine mine = MakeFloor<TrackedMine>("UndergroundMine37");
            TrackedMine skull = MakeFloor<TrackedMine>("UndergroundMine145");
            TrackedMine empty = MakeFloor<TrackedMine>("UndergroundMine22");
            TrackedVolcano volcano = MakeFloor<TrackedVolcano>("VolcanoDungeon3");
            MineShaft.activeMines.AddRange(new MineShaft[] { empty, mine, skull });
            VolcanoDungeon.activeLevels.Add(volcano);
            farmer.currentLocation = mine;
            Game1.currentLocation = mine;
            farmer.Position = new Vector2(640.5f, 704.25f);
            farmer.FacingDirection = 3;
            MorningState.RememberPosition(farmer);
            check(farmer.modData.ContainsKey(MorningState.PositionKey), "Generated mine floors must receive an awake morning marker.");
            check(!MorningState.TryGetPosition(farmer, out _, out _, out _), "A next-day marker must not apply on the previous date.");
            // Advance the real date used by the game's native new-day worker.
            Game1.dayOfMonth++;
            check(MorningState.TryGetPosition(farmer, out string name, out Vector2 position, out int facing)
                && name == mine.Name && position == farmer.Position && facing == 3,
                "The next day must recover the exact floor, sub-tile coordinates and direction.");
            Farmer remote = MakeFarmer(2);
            remote.modData[MorningState.PositionKey] = Marker(skull.Name);
            Farmer volcanoPlayer = MakeFarmer(3);
            volcanoPlayer.modData[MorningState.PositionKey] = Marker(volcano.Name);
            Farmer[] online = { farmer, remote, volcanoPlayer };
            DungeonContinuity.ClearUnoccupied(MineShaft.activeMines, online, MineShaft.clearActiveMines);
            DungeonContinuity.ClearUnoccupied(VolcanoDungeon.activeLevels, online, VolcanoDungeon.ClearAllLevels);
            check(empty.Removed && !mine.Removed && !skull.Removed && !volcano.Removed,
                "Vanilla cleanup must dispose empty floors while preserving all awake players' occupied floors.");
            check(MineShaft.activeMines.Count == 2 && MineShaft.GetMine(mine.Name) == mine
                && MineShaft.GetMine(skull.Name) == skull && VolcanoDungeon.GetLevel(volcano.Name) == volcano,
                "Native dungeon lookups must return the retained instances, including Skull Cavern and remote players.");
            farmer.Position = Vector2.Zero;
            check(!GamePatches.BeforeWakeUp(farmer) && farmer.currentLocation == mine && Game1.currentLocation == mine
                && farmer.Position == position && farmer.FacingDirection == 3,
                "Awake wake-up must bypass the home fallback and restore the exact mine position.");
            AccessTools.Field(typeof(MineShaft), "netTileBeneathLadder").SetValue(mine, new NetVector2(new Vector2(5, 8)));
            check(MorningState.ApplyPosition(farmer, loadingSave: true) && farmer.Position == new Vector2(320, 512),
                "A fresh save load must use the regenerated mine's entrance instead of a potentially blocked old tile.");
            volcano.startPosition = new Point(9, 4);
            farmer.modData[MorningState.PositionKey] = Marker(volcano.Name);
            Game1.currentLocation = volcano;
            check(MorningState.ApplyPosition(farmer, loadingSave: true) && farmer.currentLocation == volcano
                && farmer.Position == new Vector2(576, 256), "A fresh load in the volcano must use its regenerated entrance.");
            // Another consecutive all-nighter keeps the same live floor, without regeneration.
            farmer.currentLocation = mine;
            Game1.currentLocation = mine;
            MorningState.RememberPosition(farmer);
            Game1.dayOfMonth++;
            DungeonContinuity.ClearUnoccupied(MineShaft.activeMines, new[] { farmer }, MineShaft.clearActiveMines);
            check(MineShaft.activeMines.Single() == mine && !mine.Removed,
                "A second consecutive all-nighter must retain the same occupied mine instance.");
            // An ordinary sleeper and stale markers must never preserve old dungeon maps.
            farmer.modData.Remove(MorningState.PositionKey);
            remote.modData[MorningState.PositionKey] = "-1|" + skull.Name + "|64|64|2";
            volcanoPlayer.modData.Clear();
            check(GamePatches.BeforeWakeUp(farmer), "Ordinary sleep must use the native bed/rescue placement.");
            DungeonContinuity.ClearUnoccupied(MineShaft.activeMines, online, MineShaft.clearActiveMines);
            DungeonContinuity.ClearUnoccupied(VolcanoDungeon.activeLevels, online, VolcanoDungeon.ClearAllLevels);
            check(MineShaft.activeMines.Count == 0 && VolcanoDungeon.activeLevels.Count == 0
                && mine.Removed && skull.Removed && volcano.Removed, "Ordinary nights must still reset all dungeon floors.");
            MineShaft.activeMines.Add(mine);
            farmer.modData[MorningState.PositionKey] = Marker(mine.Name);
            try
            {
                DungeonContinuity.ClearUnoccupied(MineShaft.activeMines, online, () => throw new InvalidOperationException("fixture"));
            }
            catch (InvalidOperationException exception) when (exception.Message == "fixture") { }
            check(MineShaft.activeMines.Contains(mine), "An interrupted native cleanup must not lose the protected floor references.");
        }
        finally
        {
            MineShaft.activeMines.Clear();
            MineShaft.activeMines.AddRange(previousMines);
            VolcanoDungeon.activeLevels.Clear();
            VolcanoDungeon.activeLevels.AddRange(previousVolcano);
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, previousPlayer);
            Game1.game1 = previousGame;
            Game1.dayOfMonth = previousDay;
            Game1.year = previousYear;
            Game1.netWorldState = previousWorld;
        }
    }

    private static string Marker(string name)
        => Game1.Date.TotalDays.ToString(CultureInfo.InvariantCulture) + "|" + name + "|64|64|2";

    private static Farmer MakeFarmer(long id)
    {
        Farmer farmer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
        AccessTools.Field(typeof(Farmer), nameof(Farmer.uniqueMultiplayerID)).SetValue(farmer, new NetLong(id));
        AccessTools.Field(typeof(Character), "<modData>k__BackingField").SetValue(farmer, new StardewValley.Mods.ModDataDictionary());
        AccessTools.Field(typeof(Character), "currentLocationRef").SetValue(farmer, new NetLocationRef());
        AccessTools.Field(typeof(Character), nameof(Character.position)).SetValue(farmer, new NetPosition());
        AccessTools.Field(typeof(Character), nameof(Character.facingDirection)).SetValue(farmer, new NetDirection(2));
        AccessTools.Field(typeof(Character), nameof(Character.sprite)).SetValue(farmer, new NetRef<AnimatedSprite>());
        AccessTools.Field(typeof(Character), "simpleNonVillagerNPC").SetValue(farmer, new NetBool());
        return farmer;
    }

    private static T MakeFloor<T>(string name) where T : GameLocation
    {
        T floor = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        AccessTools.Field(typeof(GameLocation), nameof(GameLocation.name)).SetValue(floor, new NetString(name));
        AccessTools.Field(typeof(GameLocation), nameof(GameLocation.uniqueName)).SetValue(floor, new NetString());
        AccessTools.Field(typeof(GameLocation), nameof(GameLocation.isStructure)).SetValue(floor, new NetBool());
        return floor;
    }

    private sealed class TrackedMine : MineShaft
    {
        internal bool Removed;
        public override void OnRemoved() => Removed = true;
    }

    private sealed class TrackedVolcano : VolcanoDungeon
    {
        internal bool Removed;
        public override void OnRemoved() => Removed = true;
    }
}
