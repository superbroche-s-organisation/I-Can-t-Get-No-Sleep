# ICGNS releases

Every new public release must have these three downloads, matching v1.0.0:

1. **ICGNS-&lt;version&gt;.zip**: the compiled, installable mod, also suitable for Nexus Mods.
2. **Source code (zip)**: generated automatically by GitHub.
3. **Source code (tar.gz)**: generated automatically by GitHub.

Do not attach a custom source ZIP instead of the installable mod. The release workflow stops before publishing if the compiled package is missing or invalid.

## Prepare a version

On the Windows PC with Stardew Valley and SMAPI installed, install a .NET SDK that targets .NET 6 and run from the project directory:

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

This compiles the mod, runs its functional checks, creates `artifacts/ICGNS-<version>.zip`, and validates its contents and assembly version. Copy the verified ZIP to `releases/ICGNS-<version>.zip`.

Update the changelog and `docs/RELEASE-<version>.md`, keeping the project, manifest and tag versions consistent. Commit the sources, notes and compiled ZIP before pushing the new `v<version>` tag. The workflow uploads only the installable mod ZIP; GitHub adds the two source downloads automatically.

The package validator can also be run separately:

```powershell
.\scripts\Test-ReleasePackage.ps1 -PackagePath .\artifacts\ICGNS-1.1.0.zip
```

The ZIP must contain only the `ICGNS` folder with its compiled DLL, manifest, translations, English player guide and changelog. Game libraries, source files and development tools must not be bundled.

## Publish or repair an existing tag

Build the selected tag with the real game and SMAPI libraries, and validate the ZIP against those tagged sources. Do not move an existing tag. Current release tools and notes can be updated on `main` without changing the tagged runtime code.

Commit the verified `releases/ICGNS-<version>.zip` to `main`. In GitHub Actions, run **Publish GitHub release** on `main` with `tag` set to the existing tag, for example `v1.1.0`.

The workflow validates the ZIP against the tagged source files, attaches it, updates the release title and notes, and then removes any extra custom source ZIP from the release. GitHub's automatic source archives remain available. A different existing mod ZIP is never overwritten.

## Current v1.3.0 package

`ICGNS-1.3.0.zip` adds the master switch, optional 6AM shipping with native fades/screens, and targeted late-night performance mitigations. It passed 508 checks against the actual Windows game and SMAPI libraries, including GPU rendering, and is validated before publication. The reported player's stutter still needs live confirmation; the exact cause was not identified from the report alone.

## Existing v1.2.0 package

`ICGNS-1.2.0.zip` contains the mine rollover fix, optional GMCM sunrise checkbox and localized saving text. It passed 350 checks against the actual Windows game and SMAPI installation, including GPU rendering and the installed GMCM API signatures. The package validator checks the assembly version and packaged documents before publication.

## Existing v1.1.0 package

`ICGNS-1.1.0.zip` is the compiled Windows/SMAPI mod with the `EnableSunrise` setting. It was built from the preserved `v1.1.0` tag with the real game libraries and passed 331 checks, including both configuration modes and GPU rendering. Its packaged documentation and changelog match the tag.

## Existing v1.0.0 package

`ICGNS-1.0.0.zip` is the precompiled package. It does not include the `EnableSunrise` setting introduced in 1.1.0.
