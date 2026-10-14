param(
    [string]$GamePath,
    [switch]$RunTests,
    [switch]$RunGraphicsTests
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not $GamePath) {
    $logPath = Join-Path $env:APPDATA 'StardewValley\ErrorLogs\SMAPI-latest.txt'
    if (Test-Path -LiteralPath $logPath) {
        $match = Select-String -LiteralPath $logPath -Pattern 'Mods go here: (.+)[\\/]Mods$' | Select-Object -First 1
        if ($match) { $GamePath = $match.Matches[0].Groups[1].Value }
    }
}
if (-not $GamePath -or -not (Test-Path -LiteralPath (Join-Path $GamePath 'StardewModdingAPI.dll'))) {
    throw 'Fournissez le chemin du jeu avec -GamePath. SMAPI doit être installé.'
}

$localDotnet = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { (Get-Command dotnet -ErrorAction Stop).Source }
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.tools\cli'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$config = Join-Path $projectRoot 'NuGet.Config'
$project = Join-Path $projectRoot 'ICGNS\ICGNS.csproj'

& $dotnet restore $project "-p:GamePath=$GamePath" --configfile $config --nologo
if ($LASTEXITCODE -ne 0) { throw 'Échec de la restauration du projet.' }
& $dotnet build $project --no-restore --configuration Release "-p:GamePath=$GamePath" "-p:PathMap=$projectRoot=/_/" -p:UseSharedCompilation=false --nologo
if ($LASTEXITCODE -ne 0) { throw 'Échec de la compilation du mod.' }

if ($RunTests -or $RunGraphicsTests) {
    $tests = Join-Path $projectRoot 'tests\ICGNS.Tests.csproj'
    & $dotnet restore $tests "-p:GamePath=$GamePath" --configfile $config --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Échec de la restauration des tests.' }
    $testArguments = @($GamePath)
    if ($RunGraphicsTests) { $testArguments += '--graphics' }
    & $dotnet run --project $tests --no-restore --configuration Release "-p:GamePath=$GamePath" "-p:PathMap=$projectRoot=/_/" -p:UseSharedCompilation=false -- @testArguments
    if ($LASTEXITCODE -ne 0) { throw 'Échec des vérifications.' }
}

$package = Join-Path $projectRoot 'artifacts\ICGNS'
if (Test-Path -LiteralPath $package) {
    $resolvedPackage = (Resolve-Path -LiteralPath $package).Path
    $expectedPackage = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts\ICGNS'))
    if ($resolvedPackage -ne $expectedPackage) { throw 'Le chemin du paquet de release est inattendu.' }
    Remove-Item -LiteralPath $resolvedPackage -Recurse -Force
}
New-Item -ItemType Directory -Path $package -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'ICGNS\bin\Release\net6.0\ICGNS.dll') -Destination $package -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'ICGNS\manifest.json') -Destination $package -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'ICGNS\i18n') -Destination $package -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\README.release.md') -Destination (Join-Path $package 'README.md') -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $package -Force
$manifest = Get-Content -LiteralPath (Join-Path $projectRoot 'ICGNS\manifest.json') -Raw | ConvertFrom-Json
if ($manifest.Name -ne "I Can't Get No Sleep" -or $manifest.UniqueID -ne 'Tom.ICGNS' -or $manifest.EntryDll -ne 'ICGNS.dll') {
    throw 'Les identifiants du manifest ne correspondent pas à la release ICGNS.'
}
$zip = Join-Path $projectRoot "artifacts\ICGNS-$($manifest.Version).zip"
Compress-Archive -LiteralPath $package -DestinationPath $zip -Force
Write-Output "Mod compilé : $zip"
