using StardewModdingAPI;

namespace ICGNS;

internal static class ConfigMenuIntegration
{
    internal static void Register(IGenericModConfigMenuApi? api, IManifest manifest, Func<ModConfig> config,
        Action reset, Action save, Func<string, string> translate)
    {
        if (api is null)
            return;
        api.Register(manifest, reset, save);
        api.AddBoolOption(manifest, () => config().EnableSunrise, value => config().EnableSunrise = value,
            () => translate("config.sunrise.name"), () => translate("config.sunrise.tooltip"), nameof(ModConfig.EnableSunrise));
    }
}
