using System.Globalization;
using ICGNS;

namespace ICGNS.Tests;

internal static class FatigueChecks
{
    internal static void Run(Action<bool, string> check)
    {
        long dawn = FatigueProgress.Minute(42, 600);
        FatigueProgress? before = null;
        check(FatigueProgress.FinishNight(before, true, true, false, dawn) is null,
            "Ordinary sleep before any all-nighter must not start fatigue.");
        check(FatigueProgress.FinishNight(before, true, false, false, dawn) is null,
            "Cancelling sleep or a non-dawn transition must not start fatigue.");
        FatigueProgress clock = FatigueProgress.FinishNight(before, true, false, true, dawn)!.Value;
        check(clock.Level == 0, "Reaching the first dawn must start a grace period, not an immediate penalty.");
        check(clock.Advance(FatigueProgress.Minute(42, 1750)).Level == 0,
            "The first penalty must wait until twelve full game hours after the first all-nighter.");
        clock = clock.Advance(FatigueProgress.Minute(42, 1800));
        check(clock.Level == 1 && clock.EnergyPenalty(270) == 27 && clock.SpeedPenalty == 0,
            "At 6 PM after the first all-nighter, lose 10% maximum energy and no speed.");
        check(clock.Advance(FatigueProgress.Minute(42, 1800)) == clock,
            "Repeated ticks and paused menus must not add fatigue.");
        clock = clock.Advance(FatigueProgress.Minute(42, 2950));
        check(clock.Level == 1, "At 5:50 AM the second twelve-hour interval isn't complete.");
        clock = FatigueProgress.FinishNight(clock, true, false, true, FatigueProgress.Minute(42, 3000))!.Value;
        check(clock.Level == 2 && clock.EnergyPenalty(270) == 54 && clock.SpeedPenalty == 0,
            "A second all-nighter must retain progress and reach level two at dawn.");
        check(clock.Advance(FatigueProgress.Minute(43, 600)) == clock,
            "The 3000-to-0600 date rollover must not double count a night.");
        clock = clock.Advance(FatigueProgress.Minute(43, 1800));
        check(clock.Level == 3 && clock.EnergyPenalty(270) == 81 && clock.SpeedPenalty == 0.25f,
            "The third level must introduce only a quarter-point speed reduction.");
        clock = clock.Advance(FatigueProgress.Minute(44, 600));
        check(clock.Level == 4 && clock.SpeedPenalty == 0.5f,
            "Movement must slow by another quarter point, not a full speed point.");
        clock = clock.Advance(FatigueProgress.Minute(44, 1800));
        check(clock.Level == 5 && clock.EnergyPenalty(270) == 135 && clock.SpeedPenalty == 0.75f,
            "Maximum energy must cap at a 50% reduction while speed progresses gently.");
        FatigueProgress late = clock.Advance(FatigueProgress.Minute(90, 600));
        check(late.Level > 10 && late.EnergyPenalty(270) == 135 && late.SpeedPenalty == 2f,
            "The counter keeps rising, but penalties must stop at 50% energy and two speed points.");
        check(late.EnergyPenalty(508) == 254 && late.EnergyPenalty(1) == 0,
            "Energy penalties must scale with the normal maximum without rounding a tiny capacity down to zero.");

        FatigueProgress slowClock = new(0, dawn);
        for (int i = 1; i <= 216; i++)
            slowClock = slowClock.Advance(dawn + i * 10);
        check(slowClock == new FatigueProgress(0, dawn).Advance(dawn + 2160),
            "Normal ticks and accelerated game time must produce identical fatigue.");
        check(FatigueProgress.Minute(42, 2400) - FatigueProgress.Minute(42, 2350) == 10,
            "Midnight must be ten minutes after 23:50, not a negative jump.");
        check(FatigueProgress.Minute(111, 3000) == FatigueProgress.Minute(112, 600),
            "The year boundary must follow the same monotonic total-day clock.");

        FatigueProgress restored = FatigueProgress.Parse(clock.Serialize())!.Value;
        check(restored == clock, "Saving and loading must retain the exact level and partial interval.");
        long rejoinTime = FatigueProgress.Minute(80, 600);
        restored = restored.Rebase(rejoinTime);
        check(restored.Level == clock.Level && restored.Advance(rejoinTime).Level == clock.Level,
            "An offline farmhand must not accumulate fatigue while disconnected.");
        check(restored.Advance(rejoinTime + 720).Level == clock.Level + 1,
            "After reconnecting, the next twelve hours must resume the saved progression.");
        check(clock.Advance(clock.LastMinute - 60).AwakeMinutes == clock.AwakeMinutes,
            "A clock rewind from another mod must not erase earned fatigue.");
        check(clock.Rebase(clock.LastMinute + 720).AwakeMinutes == clock.AwakeMinutes,
            "While disabled, rebasing the clock must pause progression.");

        check(FatigueProgress.FinishNight(clock, true, true, false, rejoinTime) is null,
            "Accepted sleep or genuine overnight exhaustion must clear the saved debt.");
        check(FatigueProgress.FinishNight(clock, true, true, true, rejoinTime) is null,
            "Real sleep must win even if a dawn flag is also present.");
        check(FatigueProgress.FinishNight(clock, false, false, true, rejoinTime) is null,
            "A completed night with the mod disabled must clear fatigue.");
        FatigueProgress rested = FatigueProgress.FinishNight(null, true, false, true, rejoinTime)!.Value;
        check(rested.Level == 0 && rested.Advance(rejoinTime + 710).Level == 0,
            "After recovery, another first all-nighter must grant a fresh twelve-hour grace period.");
        check(clock.Level == 5 && rested.Level == 0,
            "One player's sleep must not reset another player's progress.");

        foreach (string? invalid in new[] { null, "", "garbage", "1", "1|2|3", "-1|20", "20|-1", "9223372036854775807|0", "1|9223372036854775808" })
            check(FatigueProgress.Parse(invalid) is null, "Malformed saved progress must be ignored safely: " + invalid);
        FatigueProgress saturated = new((long)int.MaxValue * 720, dawn);
        check(saturated.Advance(dawn + 720).Level == int.MaxValue,
            "A very large saved counter must saturate without wrapping to negative fatigue.");
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            check(FatigueProgress.Parse(clock.Serialize()) == clock,
                "Saved progress must round-trip with French locale settings.");
        }
        finally { CultureInfo.CurrentCulture = original; }
    }
}
