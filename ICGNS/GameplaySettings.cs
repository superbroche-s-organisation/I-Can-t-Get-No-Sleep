using StardewModdingAPI;
using StardewValley;

namespace ICGNS;

internal static class GameplaySettings
{
    private const string Prefix = SaveDataMigration.CurrentId + "/Settings/";
    private const string EnabledKey = Prefix + nameof(ModConfig.EnableMod);
    private const string ShippingKey = Prefix + nameof(ModConfig.SellAt6AM);

    // Finish an already-started transition before returning control to vanilla.
    internal static bool Enabled => ReadHostSetting(EnabledKey, ModEntry.Instance?.Config.EnableMod ?? true)
        || ModEntry.Instance?.State.Value.DawnStarted == true;

    internal static bool SellAt6AM => ReadHostSetting(ShippingKey, ModEntry.Instance?.Config.SellAt6AM ?? false);

    private static bool ReadHostSetting(string key, bool local)
    {
        if (!Context.IsWorldReady || !Context.IsMultiplayer || Context.IsMainPlayer)
            return local;
        return Game1.MasterPlayer.modData.TryGetValue(key, out string? value)
            && bool.TryParse(value, out bool host) ? host : local;
    }

    internal static void Publish(ModConfig config)
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer)
            return;
        // Native modData synchronization also covers joining and split-screen players.
        Game1.MasterPlayer.modData[EnabledKey] = config.EnableMod.ToString();
        Game1.MasterPlayer.modData[ShippingKey] = config.SellAt6AM.ToString();
    }
}
