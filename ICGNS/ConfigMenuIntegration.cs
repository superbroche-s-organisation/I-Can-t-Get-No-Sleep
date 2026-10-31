using StardewModdingAPI;

namespace ICGNS;

internal static class ConfigMenuIntegration
{
    internal static void Register(IGenericModConfigMenuApi? api, IManifest manifest, Func<ModConfig> config,
        Action reset, Action save, Func<string, string> translate, Action? changed = null)
    {
        if (api is null)
            return;
        api.Register(manifest, reset, save);
        api.AddBoolOption(manifest, () => config().EnableMod, value => { config().EnableMod = value; changed?.Invoke(); },
            () => translate("config.enabled.name"), () => translate("config.enabled.tooltip"), nameof(ModConfig.EnableMod));
        api.AddBoolOption(manifest, () => config().EnableSunrise, value => config().EnableSunrise = value,
            () => translate("config.sunrise.name"), () => translate("config.sunrise.tooltip"), nameof(ModConfig.EnableSunrise));
        api.AddBoolOption(manifest, () => config().SellAt6AM, value => { config().SellAt6AM = value; changed?.Invoke(); },
            () => translate("config.shipping.name"), () => translate("config.shipping.tooltip"), nameof(ModConfig.SellAt6AM));
        api.AddBoolOption(manifest, () => config().ShowFatigueBar, value => config().ShowFatigueBar = value,
            () => translate("config.fatigue-bar.name"), () => translate("config.fatigue-bar.tooltip"), nameof(ModConfig.ShowFatigueBar));
    }
}
