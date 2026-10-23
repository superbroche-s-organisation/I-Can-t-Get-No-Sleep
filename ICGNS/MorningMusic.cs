using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace ICGNS;

internal static class MorningMusic
{
    internal static bool BeforePlayMorningSong()
    {
        // DayStarted/fade cleanup may already have disposed MorningState when
        // doMorningStuff runs. The dated marker survives both save-display modes.
        if (!GameplaySettings.Enabled || Game1.game1 is null || Game1.eventUp || Game1.farmEvent is not null
            || Game1.timeOfDay is < 600 or >= 1200 || Game1.player is not { } farmer
            || Game1.currentLocation is not { } location
            || !MorningState.TryGetPosition(farmer, out _, out _, out _))
            return true;

        // Vanilla already honors explicit map music and outdoor weather /
        // regional themes. Keep those rules, including custom indoor contexts.
        if ((location is not MineShaft && location.IsOutdoors) || location.GetLocationSpecificMusic() is not null
            || (location is not MineShaft && location.GetLocationContextId() != "Default"
                && location.GetLocationContext().DefaultMusic is not null))
            return true;

        // A delayed seasonal theme must not overwrite the location's next check.
        if (Game1.morningSongPlayAction is { } pending)
        {
            Game1.delayedActions?.Remove(pending);
            Game1.morningSongPlayAction = null;
        }
        if (Game1.IsPlayingMorningSong || Game1.IsPlayingTownMusic || Game1.IsPlayingOutdoorsAmbience
            || Game1.IsPlayingNightAmbience || Game1.getMusicTrackName() is "silence" or "rain" or "nightTime")
            Game1.changeMusicTrack("none", track_interruptable: true);

        // The mine has its own ambience, fog and silent-floor rules. Calling the
        // virtual location handler also lets other indoor maps remain silent.
        location.checkForMusic(new GameTime());
        return false;
    }
}
