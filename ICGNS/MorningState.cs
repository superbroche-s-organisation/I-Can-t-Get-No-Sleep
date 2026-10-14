using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace ICGNS;

internal sealed class MorningState
{
    internal const string PositionKey = SaveDataMigration.CurrentId + "/AwakeMorning";

    internal bool DawnStarted { get; set; }
    internal bool DayHasStarted { get; set; }
    internal bool SettlementPrepared { get; set; }
    internal bool SeparateWallets { get; set; }
    internal Dictionary<long, bool> Sleeping { get; } = new();
    internal DawnPresentation Presentation { get; } = new();

    internal bool CanFinishPresentation(bool globalFade, float fadeAlpha, bool newDay)
        => DayHasStarted && !globalFade && fadeAlpha <= 0f && !newDay;

    internal static void RememberPosition(Farmer farmer)
    {
        GameLocation location = farmer.currentLocation;
        // Generated dungeon floors are recreated for the new day. Use the game's normal home fallback there.
        if (location is MineShaft or VolcanoDungeon || Game1.getLocationFromName(location.NameOrUniqueName) != location)
        {
            farmer.modData.Remove(PositionKey);
            return;
        }

        farmer.modData[PositionKey] = string.Join("|", (Game1.Date.TotalDays + 1).ToString(CultureInfo.InvariantCulture),
            location.NameOrUniqueName, farmer.Position.X.ToString(CultureInfo.InvariantCulture),
            farmer.Position.Y.ToString(CultureInfo.InvariantCulture), farmer.FacingDirection.ToString(CultureInfo.InvariantCulture));
    }

    internal static bool TryGetPosition(Farmer farmer, out string location, out Vector2 position, out int facing)
    {
        // The wake-up check happens before SaveLoaded, so import preview markers here too.
        SaveDataMigration.MigrateFarmerData(farmer);
        location = "";
        position = default;
        facing = 2;
        if (!farmer.modData.TryGetValue(PositionKey, out string? value))
            return false;
        string[] parts = value.Split('|');
        if (parts.Length != 5 || !int.TryParse(parts[0], out int day) || day != Game1.Date.TotalDays
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
            || !int.TryParse(parts[4], out facing) || !float.IsFinite(x) || !float.IsFinite(y))
            return false;
        location = parts[1];
        position = new Vector2(x, y);
        return true;
    }

    internal static void ApplyPosition(Farmer farmer)
    {
        if (!TryGetPosition(farmer, out string name, out Vector2 position, out int facing))
            return;
        GameLocation? location = Game1.getLocationFromName(name);
        if (location is null || !Game1.isLocationAccessible(name))
            return;
        farmer.currentLocation = location;
        farmer.Position = position;
        farmer.faceDirection(facing);
        if (farmer.IsLocalPlayer)
            Game1.currentLocation = location;
    }
}
