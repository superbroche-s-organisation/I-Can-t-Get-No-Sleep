# ICGNS releases

## 1.1.0 source

`ICGNS-1.1.0-source.zip` contains the sources for the optional sunrise setting. It is not an installable mod package and should not be uploaded as the main Nexus Mods file.

Extract the source archive, install a .NET SDK that targets .NET 6, then run this command from the extracted project directory in PowerShell on the Windows PC that has Stardew Valley and SMAPI installed:

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

The script compiles the updated DLL, runs the functional checks, and writes **`artifacts/ICGNS-1.1.0.zip`**, the package to install or upload to Nexus Mods. Game libraries are required to build and are not included in either archive. The build and functional checks have not been completed in the cloud because those libraries are unavailable there.

## 1.0.0 precompiled package

`ICGNS-1.0.0.zip` is the ready-to-install package for Stardew Valley / SMAPI, also suitable for uploading to Nexus Mods.

This older package does not include the `EnableSunrise` setting.

Extract its **ICGNS** folder into the game's **Mods** directory. See the root README for requirements and update instructions.

This ZIP contains only the mod DLL, manifest, translations, player guide, and changelog.
