#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Updates and publishes the ByteForge.FIGLet NuGet package.
.DESCRIPTION
    This script builds, packs, and publishes the ByteForge.FIGLet library to NuGet.
    It also handles version incrementing and validation.
.PARAMETER ProjectPath
    Path to the project directory. Default is current directory.
.PARAMETER Major
    Switch to increment the major version number.
.PARAMETER Minor
    Switch to increment the minor version number.
.PARAMETER Patch
    Switch to increment the patch version number.
.PARAMETER Configuration
    Build configuration (Debug, Release). Default is Release.
.PARAMETER OutputPath
    Path where the NuGet package will be saved. Default is ./nupkg.
.PARAMETER SkipPublish
    Skip the publishing step. Default is false.
.PARAMETER ApiKey
    NuGet API key for publishing. If not provided, will check NUGET_KEY environment variable.
.EXAMPLE
    ./update-nuget-package.ps1 -minor
    Increments the minor version number and publishes the package.
.EXAMPLE
    ./update-nuget-package.ps1 -major -s
    Increments the major version number but skips publishing.
.NOTES
    Author: Paulo Santos
    Requires: .NET SDK, NuGet CLI
#>

param(
    [Parameter(Mandatory=$false)]
    [Alias("p")]
    [string]$ProjectPath = ".",
    
    [Parameter(Mandatory=$false)]
    [switch]$Major,
    
    [Parameter(Mandatory=$false)]
    [switch]$Minor,
    
    [Parameter(Mandatory=$false)]
    [switch]$Patch,
    
    [Parameter(Mandatory=$false)]
    [Alias("c")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [Alias("o")]
    [string]$OutputPath = "./nupkg",
    
    [Parameter(Mandatory=$false)]
    [Alias("s")]
    [switch]$SkipPublish = $false,

    [Parameter(Mandatory=$false)]
    [Alias("k")]
    [string]$ApiKey = $null
)

# Apply switch-based version increment if specified
if ($Major) {
    $VersionIncrement = "Major"
    Write-Host "Using switch parameter: Major version increment" -ForegroundColor Cyan
}
elseif ($Minor) {
    $VersionIncrement = "Minor" 
    Write-Host "Using switch parameter: Minor version increment" -ForegroundColor Cyan
}
elseif ($Patch) {
    $VersionIncrement = "Patch"
    Write-Host "Using switch parameter: Patch version increment" -ForegroundColor Cyan
}

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
    Write-Host "Created output directory: $OutputPath" -ForegroundColor Green
}

# Find the project file
$ProjectFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" | Select-Object -First 1
if ($null -eq $ProjectFile) {
    Write-Error "No .csproj file found in $ProjectPath"
    exit 1
}

Write-Host "Found project file: $($ProjectFile.FullName)" -ForegroundColor Green

# Read current version from the project file
$ProjectXml = [xml](Get-Content $ProjectFile.FullName)

# Handle multiple PropertyGroup elements
$PropertyGroups = $ProjectXml.Project.PropertyGroup
if ($null -eq $PropertyGroups) {
    Write-Error "No PropertyGroup found in project file"
    exit 1
}

if ($PropertyGroups -is [array]) {
    Write-Host "Multiple PropertyGroup elements found" -ForegroundColor Yellow

    # Look through each PropertyGroup for a Version element
    $VersionNode = $null
    foreach ($pg in $PropertyGroups) {
        if ($pg.Version -is [array]) {
            Write-Host "Multiple Version elements found in PropertyGroup" -ForegroundColor Red
            Write-Host "Please ensure only one Version element is present in the project file." -ForegroundColor Red
            exit 1
        }

        if ($pg.Version) {
            Write-Host "Found Version element" -ForegroundColor Green
            $VersionNode = $pg.SelectSingleNode("Version")
            break
        }
    }
} else {
    Write-Host "Single PropertyGroup found" -ForegroundColor Green
    # Single PropertyGroup
    $VersionNode = $PropertyGroups.SelectSingleNode("Version")
}

$CurrentVersion = $VersionNode.InnerText

Write-Host "Current version in project file: $CurrentVersion" -ForegroundColor Cyan

if ($null -eq $VersionNode) {
    $CurrentVersion = "1.0.0"
    $VersionNode = $ProjectXml.CreateElement("Version")
    $VersionNode.InnerText = $CurrentVersion
    $PropertyGroups.AppendChild($VersionNode) | Out-Null
    $ProjectXml.Save($ProjectFile.FullName)
    Write-Host "Added initial version '$CurrentVersion' to project file" -ForegroundColor Yellow
}

Write-Host "Current package version: $CurrentVersion" -ForegroundColor Cyan

# Parse the version
$VersionParts = $CurrentVersion.Split('.')
$Maj = [int]$VersionParts[0]
$Min = [int]$VersionParts[1]
$Pat = [int]$VersionParts[2]

# Increment version based on parameter
switch ($VersionIncrement) {
    "Major" {
        $Maj++
        $Min = 0
        $Pat = 0
    }
    "Minor" {
        $Min++
        $Pat = 0
    }
    "Patch" {
        $Pat++
    }
}

$NewVersion = "$Maj.$Min.$Pat"
Write-Host "New version will be: $NewVersion" -ForegroundColor Cyan

# Update version in project file
$VersionNode.InnerText = $NewVersion


$ProjectXml.Save($ProjectFile.FullName)
Write-Host "Updated project file with new version" -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
dotnet clean $ProjectFile.FullName --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to clean the project"
    exit 1
}

# Build the project
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build $ProjectFile.FullName --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build the project"
    exit 1
}

# Pack the project
Write-Host "Creating NuGet package..." -ForegroundColor Cyan
dotnet pack $ProjectFile.FullName --configuration $Configuration --output $OutputPath --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create the NuGet package"
    exit 1
}

# Find the created package
$PackagePath = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | 
               Where-Object { $_.Name -match "FIGLet\.$NewVersion" } | 
               Select-Object -First 1

if ($null -eq $PackagePath) {
    Write-Error "Failed to find the created NuGet package"
    exit 1
}

Write-Host "Created package: $($PackagePath.FullName)" -ForegroundColor Green

# Skip publishing if requested
if ($SkipPublish) {
    Write-Host "Skipping publishing as requested" -ForegroundColor Yellow
    Write-Host "To publish manually, use:" -ForegroundColor Yellow
    Write-Host "dotnet nuget push $($PackagePath.FullName) --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
    exit 0
}

# Check for API key in environment variable, parameter, or prompt
if ([string]::IsNullOrEmpty($ApiKey)) {
    # Try to get from environment variable
    $ApiKey = [Environment]::GetEnvironmentVariable("NUGET_KEY")
    
    # If still not found, prompt the user
    if ([string]::IsNullOrEmpty($ApiKey)) {
        Write-Host "NuGet API key not found in environment variable NUGET_KEY" -ForegroundColor Yellow
        $ApiKey = Read-Host "Enter your NuGet API key (or press Enter to skip publishing)"
        
        if ([string]::IsNullOrEmpty($ApiKey)) {
            Write-Host "No API key provided, skipping publishing" -ForegroundColor Yellow
            Write-Host "To publish manually, use:" -ForegroundColor Yellow
            Write-Host "dotnet nuget push $($PackagePath.FullName) --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
            exit 0
        }
    } else {
        Write-Host "Using NuGet API key from NUGET_KEY environment variable" -ForegroundColor Green
    }
}

# Publish to NuGet
Write-Host "Publishing to NuGet..." -ForegroundColor Cyan
dotnet nuget push $PackagePath.FullName --api-key $ApiKey --source https://api.nuget.org/v3/index.json
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish the NuGet package"
    exit 1
}

Write-Host "Successfully published ByteForge.FIGLet $NewVersion to NuGet!" -ForegroundColor Green