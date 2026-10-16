using Microsoft.Xna.Framework;
using StardewValley;

namespace ICGNS;

internal static class Sunrise
{
    internal const int StartTime = 2800; // 4 AM, in the game's extended-day notation.
    internal const int EndTime = 3000;

    internal static float Progress(int time, float tickProgress)
    {
        float minutes = time / 100 * 60 + time % 100 + Math.Clamp(tickProgress, 0f, 1f) * 10f;
        float progress = Math.Clamp((minutes - 28 * 60) / 120f, 0f, 1f);
        return progress * progress * (3f - 2f * progress);
    }

    internal static Color Blend(Color night, Color morning, int time, float tickProgress)
        => Color.Lerp(night, morning, Progress(time, tickProgress));

    internal static void AfterClockUpdate()
        => UpdateLighting(ModEntry.Instance.Config);

    internal static void UpdateLighting(ModConfig config)
    {
        if (!config.EnableSunrise || Game1.timeOfDay < StartTime || Game1.timeOfDay > EndTime
            || Game1.currentLocation is not { IsOutdoors: true })
            return;
        int interval = Game1.realMilliSecondsPerGameTenMinutes + Game1.currentLocation.ExtraMillisecondsPerInGameMinute * 10;
        float tickProgress = interval > 0 ? (float)Game1.gameTimeInterval / interval : 0f;
        Color morning = MorningLight(Game1.IsRainingHere(), Game1.ambientLight);
        Game1.outdoorLight = Blend(Game1.outdoorLight, morning, Game1.timeOfDay, tickProgress);
    }

    internal static Color MorningLight(bool raining, Color ambient)
        // White tells the normal daytime renderer to skip lighting altogether. Inside a
        // sunrise blend, it is instead drawn into a subtractive lightmap and blacks out
        // the scene. Zero is neutral when that lightmap is actually being rendered.
        => raining ? ambient * 0.3f : Color.Transparent;
}
