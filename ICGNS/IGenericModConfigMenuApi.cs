using StardewModdingAPI;

namespace ICGNS;

/// <summary>The optional GMCM API methods used by ICGNS.</summary>
public interface IGenericModConfigMenuApi
{
    void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

    void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue,
        Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
}
