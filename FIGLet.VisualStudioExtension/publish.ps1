#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and publishes the FIGLet Visual Studio Extension to the VS Marketplace.
.DESCRIPTION
    This script builds the VSIX package and optionally publishes it to the
    Visual Studio Marketplace using VsixPublisher.exe.
.PARAMETER Major
    Switch to increment the major version number.
.PARAMETER Minor
    Switch to increment the minor version number.
.PARAMETER SkipPublish
    Build only, don't publish to the marketplace.
.PARAMETER PersonalAccessToken
    VS Marketplace Personal Access Token. If not provided, checks VS_MARKETPLACE_PAT env var.
.EXAMPLE
    ./publish.ps1
    Builds with date-based version (Major.Minor.YY.MMDD).
.EXAMPLE
    ./publish.ps1 -Minor -SkipPublish
    Increments minor version and builds without publishing.
.EXAMPLE
    ./publish.ps1 -PersonalAccessToken "your-pat-here"
    Builds and publishes using the provided PAT.
.NOTES
    Version format: Major.Minor.YY.MMDD[-suffix]
    Requires Visual Studio 2022 with VSSDK installed.
    For publishing, requires a Personal Access Token from the VS Marketplace.
#>

param(
    [switch]$Major,
    [switch]$Minor,
    [switch]$SkipPublish,
    [string]$PersonalAccessToken,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# =============================================================================
# DATE-BASED VERSIONING FUNCTIONS
# Version format: Major.Minor.YY.MMDD[-suffix]
# Note: VS extensions require 4-part numeric versions, so suffix becomes build number
# =============================================================================

function Get-DateVersionParts {
    $now = Get-Date
    return @{
        YY = [int]$now.ToString("yy")
        MMDD = [int]$now.ToString("MMdd")
        DateString = "$($now.ToString('yy')).$($now.ToString('MMdd'))"
    }
}

function Parse-VsixVersion {
    param([string]$VersionString)

    # Pattern: Major.Minor.Build.Revision (4-part version)
    $pattern = "^(\d+)\.(\d+)\.(\d+)\.(\d+)$"
    if ($VersionString -match $pattern) {
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2]
            Build = [int]$Matches[3]  # YY or YYMM
            Revision = [int]$Matches[4]  # MMDD or DD+suffix
        }
    }

    # Fallback: 3-part version
    $pattern = "^(\d+)\.(\d+)\.(\d+)$"
    if ($VersionString -match $pattern) {
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2]
            Build = [int]$Matches[3]
            Revision = 0
        }
    }

    return $null
}

function Build-NewVsixVersion {
    param(
        [hashtable]$CurrentVersion,
        [string]$Increment
    )

    $dateParts = Get-DateVersionParts
    $maj = $CurrentVersion.Major
    $min = $CurrentVersion.Minor

    # Handle Major/Minor increment
    if ($Increment -eq "Major") {
        $maj++
        $min = 0
        return "$maj.$min.$($dateParts.YY).$($dateParts.MMDD)"
    }
    elseif ($Increment -eq "Minor") {
        $min++
        return "$maj.$min.$($dateParts.YY).$($dateParts.MMDD)"
    }

    # No Major/Minor change - check if same day release
    $currentYY = $CurrentVersion.Build
    $currentMMDD = $CurrentVersion.Revision

    # Extract base MMDD (remove any suffix that was encoded)
    $baseMMDD = [int]([string]$currentMMDD).Substring(0, [Math]::Min(4, ([string]$currentMMDD).Length))

    if ($currentYY -eq $dateParts.YY -and $baseMMDD -eq $dateParts.MMDD) {
        # Same day - increment revision by 1 to indicate another release
        # We encode this as MMDD * 10 + release_number (e.g., 01241 for second release on 0124)
        if ($currentMMDD -eq $dateParts.MMDD) {
            $newRevision = $dateParts.MMDD * 10 + 1
        } else {
            # Already has a suffix, increment it
            $suffix = $currentMMDD % 10
            $newRevision = $dateParts.MMDD * 10 + $suffix + 1
        }
        return "$maj.$min.$($dateParts.YY).$newRevision"
    }

    # Different day - new date
    return "$maj.$min.$($dateParts.YY).$($dateParts.MMDD)"
}

# =============================================================================
# MAIN SCRIPT
# =============================================================================

Write-Host "FIGLet Visual Studio Extension Publisher" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Determine version increment type
$VersionIncrement = $null
if ($Major) {
    $VersionIncrement = "Major"
    Write-Host "Version increment: Major" -ForegroundColor Cyan
}
elseif ($Minor) {
    $VersionIncrement = "Minor"
    Write-Host "Version increment: Minor" -ForegroundColor Cyan
}

# Find the manifest file
$ManifestPath = Join-Path $PSScriptRoot "source.extension.vsixmanifest"
if (-not (Test-Path $ManifestPath)) {
    Write-Error "Could not find source.extension.vsixmanifest"
    exit 1
}

Write-Host "Found manifest: $ManifestPath" -ForegroundColor Green

# Read and parse the manifest
[xml]$Manifest = Get-Content $ManifestPath
$IdentityNode = $Manifest.PackageManifest.Metadata.Identity

$CurrentVersion = $IdentityNode.Version
Write-Host "Current version: $CurrentVersion" -ForegroundColor Cyan

# Parse and build new version
$ParsedVersion = Parse-VsixVersion -VersionString $CurrentVersion

if ($null -eq $ParsedVersion) {
    Write-Host "Could not parse version, using defaults" -ForegroundColor Yellow
    $ParsedVersion = @{
        Major = 1
        Minor = 0
        Build = 0
        Revision = 0
    }
}

$NewVersion = Build-NewVsixVersion -CurrentVersion $ParsedVersion -Increment $VersionIncrement
Write-Host "New version: $NewVersion" -ForegroundColor Cyan

# Update manifest
$IdentityNode.Version = $NewVersion
$Manifest.Save($ManifestPath)
Write-Host "Updated manifest with new version" -ForegroundColor Green
Write-Host ""

# =============================================================================
# BUILD
# =============================================================================

Write-Host "Building extension..." -ForegroundColor Yellow

# Find MSBuild
$msbuildPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $null
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        $msbuild = $path
        break
    }
}

if ($null -eq $msbuild) {
    Write-Error "Could not find MSBuild. Please install Visual Studio 2022."
    exit 1
}

Write-Host "Using MSBuild: $msbuild" -ForegroundColor Gray

# Find project file
$ProjectFile = Get-ChildItem -Path $PSScriptRoot -Filter "*.csproj" | Select-Object -First 1
if ($null -eq $ProjectFile) {
    Write-Error "Could not find .csproj file"
    exit 1
}

# Restore and build
& $msbuild $ProjectFile.FullName /t:Restore /p:Configuration=$Configuration /v:minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "NuGet restore failed"
    exit 1
}

& $msbuild $ProjectFile.FullName /t:Build /p:Configuration=$Configuration /p:DeployExtension=false /v:minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""

# Find the VSIX file
$OutputDir = Join-Path $PSScriptRoot "bin\$Configuration"
$VsixFile = Get-ChildItem -Path $OutputDir -Filter "*.vsix" | Select-Object -First 1

if ($null -eq $VsixFile) {
    Write-Error "Could not find VSIX file in $OutputDir"
    exit 1
}

Write-Host "Created VSIX: $($VsixFile.FullName)" -ForegroundColor Green
Write-Host ""

# =============================================================================
# PUBLISH
# =============================================================================

if ($SkipPublish) {
    Write-Host "Skipping publish (use without -SkipPublish to publish)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To publish manually:" -ForegroundColor Yellow
    Write-Host "  1. Go to https://marketplace.visualstudio.com/manage" -ForegroundColor White
    Write-Host "  2. Upload: $($VsixFile.FullName)" -ForegroundColor White
    exit 0
}

# Check for PAT
if ([string]::IsNullOrEmpty($PersonalAccessToken)) {
    $PersonalAccessToken = $env:VS_MARKETPLACE_PAT
}

if ([string]::IsNullOrEmpty($PersonalAccessToken)) {
    Write-Host "No Personal Access Token provided." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To publish, you need a PAT from the VS Marketplace:" -ForegroundColor Yellow
    Write-Host "  1. Go to https://marketplace.visualstudio.com/manage" -ForegroundColor White
    Write-Host "  2. Click your profile > Personal Access Tokens" -ForegroundColor White
    Write-Host "  3. Create a token with 'Marketplace (Publish)' scope" -ForegroundColor White
    Write-Host ""
    Write-Host "Then run:" -ForegroundColor Yellow
    Write-Host "  ./publish.ps1 -PersonalAccessToken 'your-pat'" -ForegroundColor White
    Write-Host "Or set environment variable: VS_MARKETPLACE_PAT" -ForegroundColor White
    Write-Host ""
    Write-Host "VSIX file ready for manual upload: $($VsixFile.FullName)" -ForegroundColor Cyan
    exit 0
}

# Find VsixPublisher
$vsixPublisherPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"
)

$vsixPublisher = $null
foreach ($path in $vsixPublisherPaths) {
    if (Test-Path $path) {
        $vsixPublisher = $path
        break
    }
}

if ($null -eq $vsixPublisher) {
    Write-Host "VsixPublisher.exe not found. Please install the VS SDK workload." -ForegroundColor Red
    Write-Host "VSIX file ready for manual upload: $($VsixFile.FullName)" -ForegroundColor Cyan
    exit 1
}

Write-Host "Using VsixPublisher: $vsixPublisher" -ForegroundColor Gray

# Create publish manifest
$PublishManifestPath = Join-Path $PSScriptRoot "publishManifest.json"
$PublishManifest = @{
    '$schema' = 'http://json.schemastore.org/vsix-publish'
    categories = @('coding')
    identity = @{
        internalName = 'figlet-comment-generator'
    }
    overview = 'readme.md'
    priceCategory = 'free'
    publisher = 'PauloSantos-PaulStSmith'
    private = $false
    qna = $true
    repo = 'https://github.com/PaulStSmith/figlet-comment-generator'
} | ConvertTo-Json -Depth 10

$PublishManifest | Out-File -FilePath $PublishManifestPath -Encoding UTF8

Write-Host "Publishing to VS Marketplace..." -ForegroundColor Yellow

& $vsixPublisher publish `
    -payload $VsixFile.FullName `
    -publishManifest $PublishManifestPath `
    -personalAccessToken $PersonalAccessToken

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publishing failed"
    Remove-Item $PublishManifestPath -ErrorAction SilentlyContinue
    exit 1
}

# Cleanup
Remove-Item $PublishManifestPath -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=== Published Successfully! ===" -ForegroundColor Green
Write-Host "Version: $NewVersion" -ForegroundColor Cyan
Write-Host "Marketplace: https://marketplace.visualstudio.com/items?itemName=PauloSantos-PaulStSmith.figlet-comment-generator" -ForegroundColor Cyan
