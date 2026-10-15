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

## Repair the existing v1.1.0 release

Its installable ZIP has not been built in the cloud because the Stardew Valley/SMAPI libraries are unavailable there. The current extra source asset is not the mod package.

Build from the existing v1.1.0 tag with `-RunTests`, validate against those tagged sources, and commit the resulting `releases/ICGNS-1.1.0.zip` to `main`. Do not move the existing tag. In GitHub Actions, run **Publish GitHub release** on `main` with `tag` set to `v1.1.0`.

The workflow validates the ZIP against the tagged source files, attaches it, updates the release title and notes, and then removes `ICGNS-1.1.0-source.zip` from the release. GitHub's automatic source archives remain available. A different existing mod ZIP is never overwritten.

## Existing v1.0.0 package

`ICGNS-1.0.0.zip` is the precompiled package. It does not include the `EnableSunrise` setting introduced in 1.1.0.
