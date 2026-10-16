using StardewValley;
using StardewValley.Locations;

namespace ICGNS;

internal static class DungeonContinuity
{
    internal static void ClearMines()
        => ClearUnoccupied(MineShaft.activeMines, Game1.getOnlineFarmers(), MineShaft.clearActiveMines);

    internal static void ClearVolcano()
        => ClearUnoccupied(VolcanoDungeon.activeLevels, Game1.getOnlineFarmers(), VolcanoDungeon.ClearAllLevels);

    internal static void ClearUnoccupied<T>(List<T> floors, IEnumerable<Farmer> farmers, Action clear) where T : GameLocation
    {
        // These markers are synchronized before the native new-day barrier.
        // Protect every awake player's floor, including remote farmhands.
        HashSet<string> occupied = new(StringComparer.Ordinal);
        foreach (Farmer farmer in farmers)
        {
            if (MorningState.TryGetPosition(farmer, out string name, out _, out _))
                occupied.Add(name);
        }
        T[] retained = floors.Where(floor => occupied.Contains(floor.NameOrUniqueName)).ToArray();
        foreach (T floor in retained)
            floors.Remove(floor);
        try
        {
            // Let vanilla dispose unoccupied maps and collect their essential debris.
            clear();
        }
        finally
        {
            floors.AddRange(retained);
        }
    }
}
