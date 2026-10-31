using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ICGNS.Tests;

internal static class ConfigMenuChecks
{
    internal static void Run(string gamePath, Action<bool, string> check)
    {
        ModConfig config = new();
        bool saved = false;
        int changed = 0;
        ConfigMenuIntegration.Register(null, null!, () => config, () => config = new(), () => saved = true, key => key);
        check(!saved && config.EnableSunrise, "GMCM must remain optional.");
        FakeApi api = new();
        ConfigMenuIntegration.Register(api, null!, () => config, () => config = new(), () => saved = true,
            key => "translated " + key, () => changed++);
        check(!api.TitleScreenOnly && api.Options.Select(option => option.Id).SequenceEqual(new[] { "EnableMod", "EnableSunrise", "SellAt6AM", "ShowFatigueBar" }),
            "The master toggle must remain first; the personal fatigue display option must also be available in game.");
        var enabled = api.Options[0];
        var sunrise = api.Options[1];
        var shipping = api.Options[2];
        var fatigueBar = api.Options[3];
        check(enabled.Get() && sunrise.Get() && !shipping.Get() && fatigueBar.Get(), "The menu must default to mod/sunrise/fatigue bar on and shipping off.");
        foreach (var option in api.Options)
            check(option.Name().StartsWith("translated config.") && option.Tooltip().StartsWith("translated config."),
                "Every checkbox must use translated names and tooltips.");
        enabled.Set(false);
        shipping.Set(true);
        check(!config.EnableMod && config.SellAt6AM && changed == 2, "Gameplay changes must update config and publish host settings.");
        fatigueBar.Set(false);
        check(!config.ShowFatigueBar && changed == 2, "The fatigue bar must be a personal display option, without publishing host gameplay settings.");
        enabled.Set(true);
        sunrise.Set(false);
        check(!config.EnableSunrise && !sunrise.Get(), "Unchecking sunrise must change the active config immediately.");
        int originalTime = Game1.timeOfDay;
        Color originalLight = Game1.outdoorLight;
        try
        {
            Game1.timeOfDay = 2900;
            Game1.outdoorLight = new Color(65, 40, 30);
            Sunrise.UpdateLighting(config);
            check(Game1.outdoorLight == new Color(65, 40, 30), "Disabled sunrise must leave the current lighting untouched.");
        }
        finally
        {
            Game1.timeOfDay = originalTime;
            Game1.outdoorLight = originalLight;
        }
        api.Save!();
        check(saved, "GMCM Save must invoke configuration persistence.");
        api.Reset!();
        check(config.EnableMod && sunrise.Get() && !shipping.Get() && fatigueBar.Get(), "Reset must restore defaults through the replacement config instance.");
        sunrise.Set(false);
        shipping.Set(true);
        fatigueBar.Set(false);
        check(!config.EnableSunrise && config.SellAt6AM && !config.ShowFatigueBar, "All callbacks must still edit the live config after Reset.");

        string path = Path.Combine(gamePath, "Mods", "GenericModConfigMenu", "GenericModConfigMenu.dll");
        if (!File.Exists(path))
            return;
        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        Type actualApi = assembly.GetType("GenericModConfigMenu.IGenericModConfigMenuApi", throwOnError: true)!;
        foreach (MethodInfo method in typeof(IGenericModConfigMenuApi).GetMethods())
        {
            MethodInfo? actual = actualApi.GetMethod(method.Name, method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
            check(actual?.ReturnType == method.ReturnType, $"Installed GMCM must support the exact {method.Name} API signature.");
        }
    }

    private sealed record Option(Func<bool> Get, Action<bool> Set, Func<string> Name, Func<string> Tooltip, string Id);

    private sealed class FakeApi : IGenericModConfigMenuApi
    {
        internal Action? Reset, Save;
        internal List<Option> Options = new();
        internal bool TitleScreenOnly;

        public void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false)
            => (Reset, Save, TitleScreenOnly) = (reset, save, titleScreenOnly);

        public void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue,
            Func<string> name, Func<string>? tooltip = null, string? fieldId = null)
            => Options.Add(new Option(getValue, setValue, name, tooltip!, fieldId!));
    }
}
