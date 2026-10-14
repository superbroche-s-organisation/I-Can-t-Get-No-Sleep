using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using StardewValley;

namespace ICGNS;

/// <summary>Import private-preview save data into the public mod's identifier.</summary>
internal static class SaveDataMigration
{
    internal const string CurrentId = "Tom.ICGNS";

    // Identify the exact private-preview namespace by its SHA-256 fingerprint.
    // This preserves existing saves without retaining previous branding in this release.
    private const string PreviewIdFingerprint = "BD1ABF27FD52DD19D9266BE6264095002A8E6D88DBBBFC88B631900DE6018307";

    internal static void MigrateInventories(FarmerTeam team)
    {
        foreach (string key in team.globalInventories.Keys.ToArray())
        {
            if (!TryPreviewSuffix(key, out string suffix) || !suffix.StartsWith("Shipping/", StringComparison.Ordinal))
                continue;
            string owner = suffix["Shipping/".Length..];
            if (owner != "Shared" && !long.TryParse(owner, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                continue;

            // Merge with any current inventory instead of replacing it; keep item instances
            // and their custom data, including the bins of disconnected farmhands.
            ShippingStorage.Move(team.globalInventories[key], team.GetOrCreateGlobalInventory(CurrentId + "/" + suffix));
            team.globalInventories.Remove(key);
        }
    }

    internal static void MigrateFarmerData(Farmer farmer)
    {
        foreach (string key in farmer.modData.Keys.ToArray())
        {
            if (!TryPreviewSuffix(key, out string suffix) || suffix is not ("AwakeMorning" or "PassOutDay"))
                continue;
            string currentKey = CurrentId + "/" + suffix;
            if (!farmer.modData.ContainsKey(currentKey))
                farmer.modData[currentKey] = farmer.modData[key];
            farmer.modData.Remove(key);
        }
    }

    private static bool TryPreviewSuffix(string key, out string suffix)
    {
        suffix = "";
        if (!key.StartsWith("Tom.", StringComparison.Ordinal))
            return false;
        int separator = key.IndexOf('/');
        if (separator < 0 || key[..separator] == CurrentId)
            return false;
        string fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key[..separator])));
        if (fingerprint != PreviewIdFingerprint)
            return false;
        suffix = key[(separator + 1)..];
        return true;
    }
}
