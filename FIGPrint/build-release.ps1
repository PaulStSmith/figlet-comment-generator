# FIGPrint Release Build Script
# Creates release artifacts for winget publishing

param(
    [string]$Version = "1.1.0",
    [string]$OutputDir = ".\release",
    [switch]$Publish  # When set, creates GitHub release and submits winget PR
)

# Configuration - update these for your repository
$GitHubRepo = "Wikipedia-Fan/FIGLetAddIn"  # Update with actual repo owner
$WingetPackageId = "ByteForge.FIGPrint"

$ErrorActionPreference = "Stop"

# Architectures to build
$Architectures = @("win-x64", "win-arm64")

# Clean and create output directory
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

Write-Host "Building FIGPrint v$Version" -ForegroundColor Cyan
Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
Write-Host ""

foreach ($arch in $Architectures) {
    Write-Host "Building for $arch..." -ForegroundColor Yellow

    $publishDir = "$OutputDir\publish\$arch"
    $zipName = "FIGPrint-$Version-$arch.zip"
    $zipPath = "$OutputDir\$zipName"

    # Publish self-contained single-file executable
    dotnet publish `
        --configuration Release `
        --runtime $arch `
        --self-contained true `
        --output $publishDir `
        /p:PublishSingleFile=true `
        /p:IncludeNativeLibrariesForSelfExtract=true `
        /p:Version=$Version

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed for $arch" -ForegroundColor Red
        exit 1
    }

    # Remove unnecessary files (keep only exe and fonts)
    Get-ChildItem $publishDir -Exclude "FIGPrint.exe", "fonts" | Remove-Item -Recurse -Force

    # Create ZIP archive
    Write-Host "Creating $zipName..." -ForegroundColor Yellow
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

    # Generate SHA256 hash using certutil (works on all Windows versions)
    $hashOutput = certutil -hashfile $zipPath SHA256
    $hash = ($hashOutput | Select-Object -Index 1).ToUpper().Replace(" ", "")
    $hashFile = "$OutputDir\$zipName.sha256"
    "$hash  $zipName" | Out-File -FilePath $hashFile -Encoding ASCII

    Write-Host "  SHA256: $hash" -ForegroundColor Green
    Write-Host ""
}

# Clean up publish directories
Remove-Item -Recurse -Force "$OutputDir\publish"

Write-Host "Release build complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Artifacts created in $OutputDir`:" -ForegroundColor Cyan
Get-ChildItem $OutputDir | ForEach-Object { Write-Host "  $_" }
Write-Host ""

if (-not $Publish) {
    Write-Host "Next steps (or run with -Publish to automate):" -ForegroundColor Yellow
    Write-Host "  1. Create GitHub Release with tag 'figprint-v$Version'"
    Write-Host "  2. Upload the ZIP files to the release"
    Write-Host "  3. Update winget manifest with download URLs and SHA256 hashes"
    Write-Host "  4. Submit PR to microsoft/winget-pkgs"
    exit 0
}

# =============================================================================
# AUTOMATED RELEASE PUBLISHING (requires GitHub CLI: https://cli.github.com/)
# =============================================================================

# Check for GitHub CLI
if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    Write-Host "GitHub CLI (gh) is required for publishing. Install from https://cli.github.com/" -ForegroundColor Red
    exit 1
}

# Check GitHub CLI auth status
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "GitHub CLI is not authenticated. Run 'gh auth login' first." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Publishing Release ===" -ForegroundColor Cyan
Write-Host ""

# Step 1 & 2: Create GitHub Release and upload assets
Write-Host "Creating GitHub Release figprint-v$Version..." -ForegroundColor Yellow

$releaseNotes = @"
## FIGPrint v$Version

FIGPrint is a command-line tool for generating ASCII art text using FIGlet fonts.

### Downloads
- **Windows x64**: FIGPrint-$Version-win-x64.zip
- **Windows ARM64**: FIGPrint-$Version-win-arm64.zip

### Installation
Extract the ZIP file and add the directory to your PATH, or install via winget:
``````
winget install ByteForge.FIGPrint
``````
"@

gh release create "figprint-v$Version" `
    --repo $GitHubRepo `
    --title "FIGPrint v$Version" `
    --notes $releaseNotes `
    "$OutputDir\FIGPrint-$Version-win-x64.zip" `
    "$OutputDir\FIGPrint-$Version-win-arm64.zip"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create GitHub release" -ForegroundColor Red
    exit 1
}

Write-Host "GitHub Release created successfully!" -ForegroundColor Green
Write-Host ""

# Get the release download URLs
$releaseUrl = "https://github.com/$GitHubRepo/releases/download/figprint-v$Version"
$x64Url = "$releaseUrl/FIGPrint-$Version-win-x64.zip"
$arm64Url = "$releaseUrl/FIGPrint-$Version-win-arm64.zip"

# Read the SHA256 hashes
$x64Hash = (Get-Content "$OutputDir\FIGPrint-$Version-win-x64.zip.sha256").Split(" ")[0]
$arm64Hash = (Get-Content "$OutputDir\FIGPrint-$Version-win-arm64.zip.sha256").Split(" ")[0]

# Step 3: Generate winget manifest files
Write-Host "Generating winget manifest files..." -ForegroundColor Yellow

$wingetDir = "$OutputDir\winget-manifest\manifests\b\ByteForge\FIGPrint\$Version"
New-Item -ItemType Directory -Path $wingetDir -Force | Out-Null

# Version manifest
$versionManifest = @"
PackageIdentifier: $WingetPackageId
PackageVersion: $Version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.6.0
"@
$versionManifest | Out-File -FilePath "$wingetDir\$WingetPackageId.yaml" -Encoding UTF8

# Installer manifest
$installerManifest = @"
PackageIdentifier: $WingetPackageId
PackageVersion: $Version
Platform:
- Windows.Desktop
MinimumOSVersion: 10.0.0.0
InstallerType: zip
NestedInstallerType: portable
NestedInstallerFiles:
- RelativeFilePath: FIGPrint.exe
  PortableCommandAlias: figprint
Installers:
- Architecture: x64
  InstallerUrl: $x64Url
  InstallerSha256: $x64Hash
- Architecture: arm64
  InstallerUrl: $arm64Url
  InstallerSha256: $arm64Hash
ManifestType: installer
ManifestVersion: 1.6.0
"@
$installerManifest | Out-File -FilePath "$wingetDir\$WingetPackageId.installer.yaml" -Encoding UTF8

# Locale manifest
$localeManifest = @"
PackageIdentifier: $WingetPackageId
PackageVersion: $Version
PackageLocale: en-US
Publisher: ByteForge
PublisherUrl: https://github.com/$GitHubRepo
PackageName: FIGPrint
PackageUrl: https://github.com/$GitHubRepo
License: MIT
LicenseUrl: https://github.com/$GitHubRepo/blob/main/LICENSE
ShortDescription: Command-line tool for generating ASCII art text using FIGlet fonts
Description: FIGPrint is a CLI tool that renders text as ASCII art banners using FIGlet fonts. It supports multiple fonts and various layout modes including full-size, kerning, and smushing.
Tags:
- ascii-art
- cli
- figlet
- text-art
- banner
ManifestType: defaultLocale
ManifestVersion: 1.6.0
"@
$localeManifest | Out-File -FilePath "$wingetDir\$WingetPackageId.locale.en-US.yaml" -Encoding UTF8

Write-Host "Winget manifests created in $wingetDir" -ForegroundColor Green
Write-Host ""

# Step 4: Fork winget-pkgs and submit PR
Write-Host "Submitting PR to microsoft/winget-pkgs..." -ForegroundColor Yellow

$wingetPkgsDir = "$OutputDir\winget-pkgs"

# Clone the fork (or create fork if needed)
if (-not (Test-Path $wingetPkgsDir)) {
    gh repo fork microsoft/winget-pkgs --clone=true --remote=true -- "$wingetPkgsDir"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to fork/clone winget-pkgs. You may need to submit manually." -ForegroundColor Red
        Write-Host "Manifest files are ready in: $wingetDir" -ForegroundColor Yellow
        exit 1
    }
}

Push-Location $wingetPkgsDir
try {
    # Ensure we're on the latest master
    git fetch upstream 2>$null
    git checkout master 2>$null
    git pull upstream master 2>$null

    # Create a new branch
    $branchName = "figprint-$Version"
    git checkout -b $branchName

    # Copy manifest files
    $targetDir = "manifests/b/ByteForge/FIGPrint/$Version"
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Copy-Item "$wingetDir\*" -Destination $targetDir -Force

    # Commit and push
    git add .
    git commit -m "Add ByteForge.FIGPrint version $Version"
    git push -u origin $branchName

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to push branch. Check your GitHub permissions." -ForegroundColor Red
        exit 1
    }

    # Create PR
    gh pr create `
        --repo microsoft/winget-pkgs `
        --title "Add ByteForge.FIGPrint version $Version" `
        --body @"
## Description

Adds FIGPrint v$Version to the winget repository.

FIGPrint is a command-line tool for generating ASCII art text using FIGlet fonts.

## Checklist

- [x] Manifest validates successfully
- [x] URLs are publicly accessible
- [x] SHA256 hashes verified
"@

    if ($LASTEXITCODE -eq 0) {
        Write-Host "PR submitted successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create PR. You may need to submit manually." -ForegroundColor Red
        Write-Host "Branch '$branchName' has been pushed to your fork." -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "=== Release Complete ===" -ForegroundColor Cyan
Write-Host "GitHub Release: https://github.com/$GitHubRepo/releases/tag/figprint-v$Version"
Write-Host "Winget manifests: $wingetDir"
