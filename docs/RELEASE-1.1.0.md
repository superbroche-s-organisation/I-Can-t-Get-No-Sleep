## Optional sunrise lighting

ICGNS 1.1.0 adds `EnableSunrise` in `Mods/ICGNS/config.json`. It defaults to `true`, preserving the gradual outdoor sunrise from 4AM to 6AM.

To let Dynamic Dawn/Dusk, Golden Hours, or another lighting mod handle dawn, launch the game once with this version, close it, and set:

```json
{
  "EnableSunrise": false
}
```

Restart the game to apply the setting. Each player can choose it independently. The 6AM day change, automatic save, save icon, and shipping behavior remain active.

## Download and build status

This release provides **source code only**. `ICGNS-1.1.0-source.zip` is not an installable mod package and is not the main file to upload to Nexus Mods.

On a Windows PC with Stardew Valley 1.6.15 (build 24356), SMAPI 4.5.2 or later, and a .NET SDK that can target .NET 6, extract the source archive and run from its project directory:

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

After the build and tests succeed, the installable package will be `artifacts/ICGNS-1.1.0.zip`.

The cloud checks passed for C# syntax and both targeted configuration checks. Full compilation and functional tests remain blocked by the absence of the game/SMAPI libraries. Live compatibility with Dynamic Dawn/Dusk and Golden Hours has not yet been verified.

See [CHANGELOG.md](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/blob/main/CHANGELOG.md) for the complete version history.
