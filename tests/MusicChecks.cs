using System.Globalization;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using StardewValley.Locations;
using StardewValley.Network;

namespace ICGNS.Tests;

internal static class MusicChecks
{
    private static string track = "none";
    private static int vanillaCalls;
    private static readonly List<string> requests = new();

    internal static void Run(Action<bool, string> check)
    {
        ModEntry? previousMod = ModEntry.Instance;
        Game1? previousGame = Game1.game1;
        Farmer? previousPlayer = Game1.player;
        var previousWorld = Game1.netWorldState;
        var previousActions = Game1.delayedActions;
        var previousSongAction = Game1.morningSongPlayAction;
        int previousDay = Game1.dayOfMonth, previousYear = Game1.year, previousTime = Game1.timeOfDay;
        bool previousEvent = Game1.eventUp;
        Harmony harmony = new("Tom.ICGNS.MusicChecks");
        try
        {
            Game1.game1 = (Game1)RuntimeHelpers.GetUninitializedObject(typeof(Game1));
            Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
            Game1.dayOfMonth = 1;
            Game1.year = 1;
            Game1.timeOfDay = 600;
            Game1.eventUp = false;
            Game1.delayedActions = new List<DelayedAction>();
            ModEntry mod = new();
            AccessTools.Field(typeof(ModEntry), "<Instance>k__BackingField").SetValue(null, mod);
            Farmer farmer = (Farmer)RuntimeHelpers.GetUninitializedObject(typeof(Farmer));
            AccessTools.Field(typeof(Character), "<modData>k__BackingField").SetValue(farmer, new StardewValley.Mods.ModDataDictionary());
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, farmer);
            farmer.modData[MorningState.PositionKey] = Marker(Game1.Date.TotalDays);

            // Stub only audio submission and vanilla morning fall-through, which
            // require the live sound/content engine. Mine ambience selection below
            // runs the actual installed MineShaft.checkForMusic implementation.
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.getMusicTrackName)),
                prefix: new HarmonyMethod(typeof(MusicChecks), nameof(GetTrack)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.changeMusicTrack)),
                prefix: new HarmonyMethod(typeof(MusicChecks), nameof(ChangeTrack)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.playMorningSong)),
                prefix: new HarmonyMethod(typeof(MorningMusic), nameof(MorningMusic.BeforePlayMorningSong)));
            harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.playMorningSong)),
                prefix: new HarmonyMethod(typeof(MusicChecks), nameof(RecordVanilla)) { priority = Priority.Last });

            MusicMine mine = (MusicMine)RuntimeHelpers.GetUninitializedObject(typeof(MusicMine));
            AccessTools.Field(typeof(MineShaft), "netMineLevel").SetValue(mine, new NetInt());
            AccessTools.Field(typeof(MineShaft), nameof(MineShaft.isFogUp)).SetValue(mine, new NetBool());
            AccessTools.Field(typeof(MineShaft), "netIsQuarryArea").SetValue(mine, new NetBool());
            Game1.currentLocation = mine;
            foreach (bool shipping in new[] { false, true })
            {
                mod.State.Value = new MorningState { DawnStarted = true, UseShippingScreen = shipping };
                foreach ((int level, string expected) in new[] { (37, "Upper_Ambient"), (58, "Frost_Ambient"), (95, "Lava_Ambient"), (145, "Upper_Ambient") })
                {
                    mine.mineLevel = level;
                    ResetTrack("silence");
                    Game1.playMorningSong();
                    check(track == expected && vanillaCalls == 0,
                        $"Awake floor {level} must select its native ambience in shipping mode {shipping}.");
                }
            }
            mod.State.Value = new MorningState();
            mine.mineLevel = 58;
            ResetTrack("FrostMine");
            Game1.playMorningSong();
            check(track == "FrostMine" && requests.Count == 0 && vanillaCalls == 0,
                "The music guard must work after morning-state cleanup and preserve an already playing mine song.");
            foreach (string stale in new[] { "rain", "nightTime" })
            {
                ResetTrack(stale);
                Game1.playMorningSong();
                check(track == "Frost_Ambient" && vanillaCalls == 0,
                    "Rain or shipping-screen music must not prevent the current mine's native ambience from starting.");
            }
            ResetTrack("FrostMine");
            bool queuedPlayed = false;
            DelayedAction queued = new(500, () => queuedPlayed = true);
            DelayedAction unrelated = new(1000, () => { });
            Game1.morningSongPlayAction = queued;
            Game1.delayedActions.Add(queued);
            Game1.delayedActions.Add(unrelated);
            Game1.playMorningSong();
            check(Game1.morningSongPlayAction is null && !Game1.delayedActions.Contains(queued)
                && Game1.delayedActions.Contains(unrelated) && !queuedPlayed && track == "FrostMine",
                "Cancel only the pending general morning theme, leaving local music and unrelated morning actions intact.");
            mine.isFogUp.Value = true;
            ResetTrack("silence");
            Game1.playMorningSong();
            check(track == "none" && vanillaCalls == 0, "Fog must retain the mine's native no-ambience rule.");
            mine.isFogUp.Value = false;
            mine.mineLevel = 120;
            ResetTrack("silence");
            Game1.playMorningSong();
            check(track == "none" && vanillaCalls == 0, "The silent mine floor must not receive the village theme.");

            MusicLocation indoor = (MusicLocation)RuntimeHelpers.GetUninitializedObject(typeof(MusicLocation));
            AccessTools.Field(typeof(GameLocation), nameof(GameLocation.isOutdoors)).SetValue(indoor, new NetBool());
            Game1.currentLocation = indoor;
            ResetTrack("spring1");
            Game1.IsPlayingMorningSong = true;
            Game1.playMorningSong();
            check(track == "none" && indoor.Checks == 1 && vanillaCalls == 0,
                "Silent interiors must clear an outdoor morning theme and use their virtual music handler.");
            indoor.ExplicitMusic = "Saloon1";
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1 && indoor.Checks == 1, "Explicit indoor map music must use native morning selection.");
            indoor.ExplicitMusic = null;
            indoor.ContextMusic = "custom_region_theme";
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "A custom indoor regional theme must retain its native conditions and music context.");
            indoor.ContextMusic = null;
            indoor.IsOutdoors = true;
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "Outdoor morning, weather and seasonal selection must remain native.");

            Game1.currentLocation = mine;
            mine.mineLevel = 37;
            mine.ExplicitMusic = "custom_mine_theme";
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "Explicit music from a modded mine map must remain supported.");
            mine.ExplicitMusic = null;
            Game1.eventUp = true;
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "Event music must not be replaced by the awake-morning guard.");
            Game1.eventUp = false;
            Game1.timeOfDay = 1200;
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "The guard must only apply during the awake morning.");
            Game1.timeOfDay = 600;
            farmer.modData[MorningState.PositionKey] = Marker(Game1.Date.TotalDays - 1);
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "Previous-day markers must not affect ordinary wake-ups.");
            farmer.modData[MorningState.PositionKey] = Marker(Game1.Date.TotalDays);
            mod.Config.EnableMod = false;
            ResetTrack("none");
            Game1.playMorningSong();
            check(vanillaCalls == 1, "Disabling the mod must restore the original morning music behavior.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            AccessTools.Field(typeof(ModEntry), "<Instance>k__BackingField").SetValue(null, previousMod);
            AccessTools.Field(typeof(Game1), "_player").SetValue(null, previousPlayer);
            Game1.game1 = previousGame;
            Game1.netWorldState = previousWorld;
            Game1.delayedActions = previousActions;
            Game1.morningSongPlayAction = previousSongAction;
            Game1.dayOfMonth = previousDay;
            Game1.year = previousYear;
            Game1.timeOfDay = previousTime;
            Game1.eventUp = previousEvent;
        }
    }

    private static string Marker(int day) => day.ToString(CultureInfo.InvariantCulture) + "|UndergroundMine37|64|64|2";

    private static void ResetTrack(string value)
    {
        track = value;
        vanillaCalls = 0;
        requests.Clear();
        Game1.IsPlayingMorningSong = false;
        Game1.IsPlayingTownMusic = false;
    }

    private static bool GetTrack(ref string __result) { __result = track; return false; }

    private static bool ChangeTrack(string newTrackName)
    {
        track = newTrackName;
        requests.Add(newTrackName);
        Game1.IsPlayingMorningSong = false;
        Game1.IsPlayingTownMusic = false;
        return false;
    }

    private static bool RecordVanilla() { vanillaCalls++; return false; }

    private sealed class MusicMine : MineShaft
    {
        internal string? ExplicitMusic;
        public override string GetLocationSpecificMusic() => ExplicitMusic!;
    }

    private sealed class MusicLocation : GameLocation
    {
        internal string? ExplicitMusic, ContextMusic;
        internal int Checks;
        public override string GetLocationSpecificMusic() => ExplicitMusic!;
        public override string GetLocationContextId() => ContextMusic is null ? "Default" : "Custom";
        public override LocationContextData GetLocationContext() => new() { DefaultMusic = ContextMusic };
        public override void checkForMusic(GameTime time) => Checks++;
    }
}
