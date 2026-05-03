<#
    .SYNOPSIS
        Downloads the latest xray-core release zip into <InstallDir>
        and extracts it. Used by Inno Setup at install time and can be
        re-run by the user from the Start menu shortcut.

    .PARAMETER InstallDir
        Directory that will contain xray.exe after extraction.
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$InstallDir
)

$ErrorActionPreference = 'Stop'

function Get-Architecture {
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    switch ($arch) {
        'X64'   { return 'windows-64.zip' }
        'X86'   { return 'windows-32.zip' }
        'Arm64' { return 'windows-arm64-v8a.zip' }
        default { return 'windows-64.zip' }
    }
}

if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
}

$assetSuffix = Get-Architecture
Write-Host "Resolving xray-core latest release ($assetSuffix)..."

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$headers = @{
    'User-Agent' = 'VlessVpnClient-Installer'
    'Accept'     = 'application/vnd.github+json'
}

$release = Invoke-RestMethod -Uri 'https://api.github.com/repos/XTLS/Xray-core/releases/latest' -Headers $headers
$asset = $release.assets | Where-Object { $_.name -like "*$assetSuffix" } | Select-Object -First 1

if (-not $asset) {
    Write-Error "Could not find a release asset matching $assetSuffix"
    exit 1
}

$zipPath = Join-Path $InstallDir $asset.name
Write-Host "Downloading $($asset.name) from $($asset.browser_download_url)"
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -Headers $headers

Write-Host "Extracting to $InstallDir"
Expand-Archive -Path $zipPath -DestinationPath $InstallDir -Force
Remove-Item $zipPath -Force

if (-not (Test-Path (Join-Path $InstallDir 'xray.exe'))) {
    Write-Error "xray.exe not found after extraction"
    exit 1
}

Write-Host "xray-core installed to $InstallDir"
