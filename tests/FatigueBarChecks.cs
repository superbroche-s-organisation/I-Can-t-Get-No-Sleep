using Microsoft.Xna.Framework;

namespace ICGNS.Tests;

internal static class FatigueBarChecks
{
    internal static void Run(Action<bool, string> check)
    {
        long dawn = FatigueProgress.Minute(42, 600);
        FatigueProgress first = FatigueProgress.FinishNight(null, true, false, true, dawn)!.Value;
        check(!FatigueBar.IsVisible(true, null) && !FatigueBar.IsVisible(true, first),
            "The fatigue gauge must stay hidden before the first fatigue threshold.");
        check(!FatigueBar.IsVisible(true, first.Advance(dawn + 710)), "The grace period must not reveal an almost-empty gauge.");
        FatigueProgress levelOne = first.Advance(dawn + 720);
        check(FatigueBar.IsVisible(true, levelOne) && Math.Abs(FatigueBar.Fill(levelOne) - 0.1f) < 0.00001f,
            "The first real fatigue threshold must reveal a ten-percent gauge.");
        check(!FatigueBar.IsVisible(false, levelOne) && levelOne.Level == 1 && levelOne.EnergyReductionPercent == 10,
            "Hiding the gauge must leave the fatigue level and penalties intact.");
        check(FatigueBar.Fill(first.Advance(dawn + 3600)) == 0.5f, "Level five must fill half the gauge.");
        check(FatigueBar.Fill(first.Advance(dawn + 7200)) == 1f && FatigueBar.Fill(first.Advance(dawn + 14400)) == 1f,
            "The gauge must cap at full after both penalties reach their maximum, without overflowing at later levels.");
        check(FatigueBar.Fill(first.Advance(dawn + 730)) > FatigueBar.Fill(levelOne),
            "The gauge must also show progress between twelve-hour thresholds.");
        check(!FatigueBar.IsVisible(true, FatigueProgress.FinishNight(levelOne, true, true, false, dawn + 1440)),
            "A real night's sleep must hide the gauge again.");

        Rectangle safe = new(0, 0, 1280, 720);
        Rectangle solo = FatigueBar.Bounds(safe, safe.Right - 56, false);
        Rectangle combat = FatigueBar.Bounds(safe, safe.Right - 56, true);
        check(solo.Right + 8 == safe.Right - 56 && combat.Right + 8 == solo.Left,
            "The gauge must reserve one native spacing when health is hidden, and two when it is visible.");
        check(solo.Bottom == safe.Bottom - 16 && combat.Bottom == solo.Bottom && safe.Contains(combat),
            "Fatigue must align with the native gauges' bottom and stay inside the safe viewport.");
        Rectangle splitSafe = new(640, 0, 640, 360);
        Rectangle split = FatigueBar.Bounds(splitSafe, splitSafe.Right - 56, true);
        check(splitSafe.Contains(split) && split.Bottom == 344, "An offset split-screen viewport must use its own safe area.");
        Rectangle shortSafe = new(0, 0, 640, 180);
        Rectangle shortBar = FatigueBar.Bounds(shortSafe, shortSafe.Right - 56, true);
        check(shortSafe.Contains(shortBar) && shortBar.Height == 148,
            "Short viewports must keep the gauge inside the screen.");
        check(FatigueBar.Bounds(new Rectangle(0, 0, 150, 720), 94, true).IsEmpty
            && FatigueBar.Bounds(new Rectangle(0, 0, 640, 120), 584, true).IsEmpty,
            "A viewport too small for three gauges must not overlap existing HUD bars.");
        Rectangle empty = FatigueBar.FillBounds(combat, 0f), full = FatigueBar.FillBounds(combat, 1f);
        check(empty.Height == 0 && full.Height == combat.Height - 56 && full.Top == combat.Top + 48
            && full.Bottom == combat.Bottom - 8 && combat.Contains(full), "Fill must grow upward inside the native frame.");
        check(FatigueBar.FillBounds(combat, 2f) == full && FatigueBar.FillBounds(combat, -1f).Height == 0,
            "Out-of-range values must not draw outside the gauge.");
    }
}
