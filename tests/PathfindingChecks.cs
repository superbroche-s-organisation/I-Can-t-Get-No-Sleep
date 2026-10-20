using System.Diagnostics;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.Pathfinding;
using xTile.Layers;

namespace ICGNS.Tests;

internal static class PathfindingChecks
{
    internal static void Run(Action<bool, string> check)
    {
        var previousWorld = Game1.netWorldState;
        int previousTime = Game1.timeOfDay;
        int previousYear = Game1.year;
        int previousDay = Game1.dayOfMonth;
        Harmony harmony = new("Tom.ICGNS.PathfindingChecks");
        try
        {
            Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
            Game1.year = 1;
            Game1.dayOfMonth = 1;
            LateNightPathfinding.Reset();
            SearchLocation location = (SearchLocation)RuntimeHelpers.GetUninitializedObject(typeof(SearchLocation));
            location.BlockEnd = true;
            location.map = new xTile.Map();
            location.map.AddLayer(new Layer("Back", location.map, new xTile.Dimensions.Size(32, 32), new xTile.Dimensions.Size(16, 16)));
            NPC npc = (NPC)RuntimeHelpers.GetUninitializedObject(typeof(NPC));
            Point start = new(2, 2), end = new(28, 28);
            harmony.Patch(AccessTools.Method(typeof(PathFindController), nameof(PathFindController.findPath)),
                prefix: new HarmonyMethod(typeof(LateNightPathfinding), nameof(LateNightPathfinding.BeforeFindPath)),
                postfix: new HarmonyMethod(typeof(LateNightPathfinding), nameof(LateNightPathfinding.AfterFindPath)));
            Stack<Point>? Search(Point target) => PathFindController.findPath(start, target, PathFindController.isAtEndPoint, location, npc, 10000);

            Game1.timeOfDay = 2500;
            check(Search(end) is null, "The real pathfinder must fail on the blocked destination fixture.");
            int cost = location.CollisionChecks;
            check(cost > 500, "The fixture must exercise an expensive native search, not a trivial mock.");
            Search(end);
            check(location.CollisionChecks == cost * 2, "Before 2AM the vanilla pathfinder must remain untouched.");
            Game1.timeOfDay = 2600;
            int before = location.CollisionChecks;
            for (int i = 0; i < 120; i++)
                check(Search(end) is null, "Throttling must preserve the native failed-path result.");
            check(location.CollisionChecks - before == cost, "120 repeated late-night failed requests must perform only one native search.");
            Search(new Point(27, 28));
            check(location.CollisionChecks > before + cost, "Changing the destination must invalidate the cached failure immediately.");
            before = location.CollisionChecks;
            Game1.timeOfDay = 2610;
            Search(end);
            check(location.CollisionChecks > before, "A clock change must invalidate the cached failure.");
            LateNightPathfinding.Reset();
            before = location.CollisionChecks;
            Search(end);
            check(location.CollisionChecks > before, "Changing locations or starting a new day must clear failed searches.");
            LateNightPathfinding.Reset();
            location.BlockEnd = false;
            before = location.CollisionChecks;
            Stack<Point>? path1 = Search(end);
            Stack<Point>? path2 = Search(end);
            check(path1 is { Count: > 0 } && path2 is { Count: > 0 } && !ReferenceEquals(path1, path2)
                && location.CollisionChecks > before, "Successful paths must remain fresh, independent native stacks.");
            LateNightPathfinding.Failure failure = new();
            failure.Last = new LateNightPathfinding.Attempt(failure, location, start, end, 10000, 1, 2600);
            failure.Map = location.map;
            long now = Stopwatch.GetTimestamp();
            failure.Expires = now + 100;
            check(failure.Matches(location, start, end, 10000, 1, 2600, now)
                && !failure.Matches(location, start, end, 10000, 1, 2600, now + 100),
                "Expired failures must retry, so removing an obstacle can unblock movement.");
            check(!failure.Matches(location, new Point(3, 2), end, 10000, 1, 2600, now)
                && !failure.Matches(location, start, end, 20000, 1, 2600, now),
                "Actor movement and path-search limits must invalidate cached failures.");
            Stack<Point>? ignored = null;
            PathFindController.isAtEnd customEndpoint = (_, _, _, _) => false;
            check(LateNightPathfinding.BeforeFindPath(start, end, customEndpoint, location, npc, 10000, ref ignored, out var customState)
                && customState is null, "Custom endpoint algorithms from other mods must never be cached.");
            Character monster = (Character)RuntimeHelpers.GetUninitializedObject(typeof(StardewValley.Monsters.GreenSlime));
            check(LateNightPathfinding.BeforeFindPath(start, end, PathFindController.isAtEndPoint, location, monster, 10000, ref ignored, out var monsterState)
                && monsterState is null, "Combat AI must never use the failed-NPC-path cache.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            LateNightPathfinding.Reset();
            Game1.netWorldState = previousWorld;
            Game1.timeOfDay = previousTime;
            Game1.year = previousYear;
            Game1.dayOfMonth = previousDay;
        }
    }

    private sealed class SearchLocation : GameLocation
    {
        internal int CollisionChecks;
        internal bool BlockEnd = true;

        public override bool isCollidingPosition(Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer,
            int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false,
            bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
        {
            CollisionChecks++;
            return BlockEnd && position.X / 64 >= 26 && position.Y / 64 >= 26;
        }
    }
}
