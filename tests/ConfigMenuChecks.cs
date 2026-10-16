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
        ConfigMenuIntegration.Register(null, null!, () => config, () => config = new(), () => saved = true, key => key);
        check(!saved && config.EnableSunrise, "GMCM must remain optional.");
        FakeApi api = new();
        ConfigMenuIntegration.Register(api, null!, () => config, () => config = new(), () => saved = true, key => "translated " + key);
        check(!api.TitleScreenOnly && api.Get!(), "The sunrise checkbox must be available in game and start checked.");
        check(api.FieldId == "EnableSunrise" && api.Name!() == "translated config.sunrise.name"
            && api.Tooltip!() == "translated config.sunrise.tooltip", "The option must have its stable ID and translated label/tooltip.");
        api.Set!(false);
        check(!config.EnableSunrise && !api.Get!(), "Unchecking the option must change the active config immediately.");
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
        check(config.EnableSunrise && api.Get!(), "Reset must update callbacks to the replacement config instance.");
        api.Set!(false);
        check(!config.EnableSunrise, "The checkbox must still edit the live config after Reset.");

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

    private sealed class FakeApi : IGenericModConfigMenuApi
    {
        internal Action? Reset, Save;
        internal Func<bool>? Get;
        internal Action<bool>? Set;
        internal Func<string>? Name, Tooltip;
        internal string? FieldId;
        internal bool TitleScreenOnly;

        public void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false)
            => (Reset, Save, TitleScreenOnly) = (reset, save, titleScreenOnly);

        public void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue,
            Func<string> name, Func<string>? tooltip = null, string? fieldId = null)
            => (Get, Set, Name, Tooltip, FieldId) = (getValue, setValue, name, tooltip, fieldId);
    }
}
