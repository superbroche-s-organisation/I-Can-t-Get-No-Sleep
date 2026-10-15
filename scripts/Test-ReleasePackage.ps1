param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
    throw "Paquet installable manquant : $PackagePath. Compilez avec scripts/Build.ps1 -RunTests avant de publier."
}
$manifest = Get-Content -LiteralPath (Join-Path $ProjectRoot 'ICGNS/manifest.json') -Raw | ConvertFrom-Json
[xml]$project = Get-Content -LiteralPath (Join-Path $ProjectRoot 'ICGNS/ICGNS.csproj') -Raw
if ($manifest.Version -notmatch '^\d+\.\d+\.\d+$' -or $project.Project.PropertyGroup.Version -ne $manifest.Version) {
    throw 'Les versions du manifest et du projet doivent correspondre.'
}
if ($manifest.UniqueID -ne 'Tom.ICGNS' -or $manifest.EntryDll -ne 'ICGNS.dll') {
    throw 'Le manifest ne correspond pas au mod ICGNS.'
}

$expected = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
$expected.Add('ICGNS/ICGNS.dll', '')
$expected.Add('ICGNS/manifest.json', (Join-Path $ProjectRoot 'ICGNS/manifest.json'))
$expected.Add('ICGNS/README.md', (Join-Path $ProjectRoot 'docs/README.release.md'))
$expected.Add('ICGNS/CHANGELOG.md', (Join-Path $ProjectRoot 'CHANGELOG.md'))
foreach ($translation in Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'ICGNS/i18n') -Filter '*.json' -File) {
    $expected.Add("ICGNS/i18n/$($translation.Name)", $translation.FullName)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = $null
$originalProcessDirectory = [System.IO.Directory]::GetCurrentDirectory()
$temporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
$temporary = Join-Path ([System.IO.Path]::GetTempPath()) ("icgns-package-" + [guid]::NewGuid().ToString('N'))
try {
    $archive = [System.IO.Compression.ZipFile]::OpenRead([System.IO.Path]::GetFullPath($PackagePath))
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    New-Item -ItemType Directory -Path $temporary | Out-Null
    foreach ($entry in $archive.Entries) {
        $name = $entry.FullName.Replace('\', '/')
        if ($name.EndsWith('/')) { continue }
        if (-not $expected.ContainsKey($name) -or -not $seen.Add($name)) {
            throw "Fichier inattendu ou dupliqué dans le paquet : $name"
        }
        if ($name -eq 'ICGNS/ICGNS.dll') {
            $dll = Join-Path $temporary 'ICGNS.dll'
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $dll)
            $assembly = [System.Reflection.AssemblyName]::GetAssemblyName($dll)
            if ($assembly.Name -ne 'ICGNS' -or $assembly.Version -ne [version]"$($manifest.Version).0") {
                throw 'La DLL compilée ne correspond pas à la version du manifest.'
            }
        }
        else {
            $stream = $entry.Open()
            $sha = [System.Security.Cryptography.SHA256]::Create()
            try {
                $actual = [System.BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '')
            }
            finally {
                $stream.Dispose()
                $sha.Dispose()
            }
            if ($actual -ne (Get-FileHash -LiteralPath $expected[$name] -Algorithm SHA256).Hash) {
                throw "Le fichier du paquet ne correspond pas aux sources sélectionnées : $name"
            }
        }
    }
    foreach ($name in $expected.Keys) {
        if (-not $seen.Contains($name)) { throw "Fichier manquant dans le paquet : $name" }
    }
}
finally {
    if ($null -ne $archive) { $archive.Dispose() }
    [System.IO.Directory]::SetCurrentDirectory($originalProcessDirectory)
    if (Test-Path -LiteralPath $temporary) {
        $resolvedTemporary = (Resolve-Path -LiteralPath $temporary).ProviderPath
        if ((Split-Path -Parent $resolvedTemporary) -ne $temporaryRoot -or (Split-Path -Leaf $resolvedTemporary) -notmatch '^icgns-package-[0-9a-f]{32}$') {
            throw 'Unexpected temporary package-validation directory.'
        }
        Remove-Item -LiteralPath $resolvedTemporary -Recurse -Force
    }
}
Write-Output "Paquet installable vérifié : $PackagePath (version $($manifest.Version))."
