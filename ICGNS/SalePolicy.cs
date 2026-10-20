using System.Globalization;
using StardewValley;

namespace ICGNS;

internal static class SalePolicy
{
    internal const string PassOutDayKey = SaveDataMigration.CurrentId + "/PassOutDay";

    internal static bool SellsAtDawn(bool enabled, bool sellAt6AM, int time)
        => enabled && sellAt6AM && time >= 3000;

    internal static bool HasSlept(Farmer farmer, int day)
    {
        // The native passedOut flag also drives our awake 6 AM fade, and is reset before settlement.
        // Only the real exhaustion routine records this synchronized, dated marker.
        return farmer.timeWentToBed.Value > 0
            || farmer.modData.TryGetValue(PassOutDayKey, out string? recordedDay)
                && recordedDay == day.ToString(CultureInfo.InvariantCulture);
    }

    // Includes both accepted sleep and genuine exhaustion; cancelling sleep resets timeWentToBed.
    internal static bool CanSell(bool separateWallets, long farmerId, IReadOnlyDictionary<long, bool> sleeping)
    {
        return separateWallets
            ? sleeping.TryGetValue(farmerId, out bool slept) && slept
            : sleeping.Count > 0 && sleeping.Values.All(slept => slept);
    }
}
