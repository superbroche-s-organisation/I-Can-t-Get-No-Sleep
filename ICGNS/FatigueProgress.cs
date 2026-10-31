using System.Globalization;

namespace ICGNS;

/// <summary>Saved, per-player game-time clock; independent of rendering and real time.</summary>
internal readonly record struct FatigueProgress(long AwakeMinutes, long LastMinute)
{
    internal const int MinutesPerLevel = 12 * 60;
    private const long MaximumMinutes = (long)int.MaxValue * MinutesPerLevel;

    internal int Level => (int)(AwakeMinutes / MinutesPerLevel);
    internal int EnergyReductionPercent => Math.Min(Level, 5) * 10;
    internal float SpeedPenalty => Math.Min(Math.Max(Level - 2, 0), 8) * 0.25f;

    internal int EnergyPenalty(float normalMaximum)
        => (int)Math.Floor(Math.Max(0, normalMaximum) * EnergyReductionPercent / 100d);

    // The extended night uses 2400..3000 on the outgoing date. In particular,
    // day N at 3000 and day N+1 at 0600 must denote the very same minute.
    internal static long Minute(int day, int time)
        => (long)day * 24 * 60 + time / 100 * 60 + time % 100;

    internal FatigueProgress Advance(long now)
    {
        long elapsed = Math.Max(0, now - LastMinute);
        return new(AwakeMinutes + Math.Min(elapsed, MaximumMinutes - AwakeMinutes), now);
    }

    // Loading a save or rejoining must not charge time spent disconnected.
    internal FatigueProgress Rebase(long now) => new(AwakeMinutes, now);

    internal static FatigueProgress? FinishNight(FatigueProgress? previous, bool enabled, bool slept,
        bool reachedDawn, long nextDawn)
    {
        if (!enabled || slept)
            return null;
        if (!reachedDawn)
            return previous;
        return previous?.Advance(nextDawn) ?? new FatigueProgress(0, nextDawn);
    }

    internal string Serialize()
        => string.Create(CultureInfo.InvariantCulture, $"{AwakeMinutes}|{LastMinute}");

    internal static FatigueProgress? Parse(string? value)
    {
        string[]? parts = value?.Split('|');
        if (parts?.Length != 2
            || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long minutes)
            || !long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out long last)
            || minutes > MaximumMinutes)
            return null;
        return new FatigueProgress(minutes, last);
    }
}
