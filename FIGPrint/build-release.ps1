# FIGPrint Release Build Script
# Creates release artifacts for winget publishing

param(
    [string]$Version = "1.1.0",
    [string]$OutputDir = ".\release"
)

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
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Create GitHub Release with tag 'figprint-v$Version'"
Write-Host "  2. Upload the ZIP files to the release"
Write-Host "  3. Update winget manifest with download URLs and SHA256 hashes"
Write-Host "  4. Submit PR to microsoft/winget-pkgs"
