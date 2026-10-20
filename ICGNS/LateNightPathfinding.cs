using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Pathfinding;

namespace ICGNS;

/// <summary>Bound repeated failed vanilla NPC path searches during the extended night.</summary>
internal static class LateNightPathfinding
{
    private static ConditionalWeakTable<Character, Failure> failures = new();
    private static readonly PathFindController.isAtEnd StandardEndpoint = PathFindController.isAtEndPoint;

    internal static void Reset() => failures = new();

    internal static bool BeforeFindPath(Point startPoint, Point endPoint, PathFindController.isAtEnd endPointFunction,
        GameLocation location, Character character, int limit, ref Stack<Point>? __result, out Attempt? __state)
    {
        __state = null;
        if (!GameplaySettings.Enabled || Game1.timeOfDay is < 2600 or >= 3000 || Game1.newDay || Game1.eventUp
            || character is not (NPC or FarmAnimal) || character is Monster || endPointFunction != StandardEndpoint)
            return true;

        Failure failure = failures.GetValue(character, _ => new Failure());
        long now = Stopwatch.GetTimestamp();
        int day = Game1.Date.TotalDays;
        if (failure.Matches(location, startPoint, endPoint, limit, day, Game1.timeOfDay, now))
        {
            __result = null;
            return false;
        }
        __state = new Attempt(failure, location, startPoint, endPoint, limit, day, Game1.timeOfDay);
        return true;
    }

    internal static void AfterFindPath(Stack<Point>? __result, Attempt? __state, bool __runOriginal)
    {
        if (!__runOriginal || __state is not { } attempt)
            return;
        if (__result is not null)
        {
            attempt.Failure.Expires = 0; // Never cache a path, successful or empty.
            return;
        }
        Failure failure = attempt.Failure;
        failure.Last = attempt;
        failure.Map = attempt.Location.map;
        failure.Expires = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
    }

    internal readonly record struct Attempt(Failure Failure, GameLocation Location, Point Start, Point End, int Limit, int Day, int Time);

    internal sealed class Failure
    {
        internal Attempt Last;
        internal object? Map;
        internal long Expires;

        internal bool Matches(GameLocation location, Point start, Point end, int limit, int day, int time, long now)
            => now < Expires && ReferenceEquals(Last.Location, location) && ReferenceEquals(Map, location.map)
                && Last.Start == start && Last.End == end && Last.Limit == limit && Last.Day == day && Last.Time == time;
    }
}
