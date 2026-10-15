# Release requirements

Use the existing checkout. Each cloud task is already isolated; do not create a Git worktree unless the user explicitly asks for one.

Every public GitHub release must follow the v1.0.0 layout:

- Attach the installable `ICGNS-<version>.zip` as the mod download.
- Let GitHub supply its automatic **Source code (zip)** and **Source code (tar.gz)** entries below it. Do not attach a separate source ZIP instead of the mod.
- Build using the real Stardew Valley/SMAPI libraries and run `scripts/Build.ps1 -RunTests`. Validate the result with `scripts/Test-ReleasePackage.ps1` before publishing.
- The ZIP must contain the compiled `ICGNS/ICGNS.dll`, matching manifest, translations, player guide and changelog. Do not bundle game libraries, development tools, or a DLL from a different version.
- Keep the tag, manifest, project and assembly versions consistent. Update the changelog and version-specific release notes.
- Commit the verified mod ZIP under `releases/` before pushing the version tag. The release workflow must fail before publication when the package is absent or invalid.

If game libraries or a verified compiled package are missing, report the blocker and finish independent preparation. Do not publish a source-only public release. Do not rename an old package to represent a new version. Do not rewrite an existing release tag to work around missing build inputs.

To repair an existing source-only release, first supply and verify its real installable ZIP, then run the release workflow for its existing tag. Remove the extra source asset only after the mod ZIP is successfully attached. Preserve the automatically generated source downloads.
