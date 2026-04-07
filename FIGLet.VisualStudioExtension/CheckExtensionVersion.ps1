param(
    [Parameter(Mandatory=$true)]
    [string]$ExtensionId,

    [Parameter(Mandatory=$true)]
    [string]$CurrentVersion,

    [Parameter(Mandatory=$false)]
    [string]$OutputFile
)

$skipDeploy = $false

try {
    $current = [Version]$CurrentVersion
} catch {
    Write-Host "SKIP_DEPLOY=false"
    if ($OutputFile) { "false" | Out-File -FilePath $OutputFile -NoNewline }
    exit 0
}

# Search only known Visual Studio extension locations to avoid scanning full install roots
$searchPaths = @(
    "C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\Extensions",
    "C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\Extensions",
    ([Environment]::GetFolderPath('LocalApplicationData') + "\Microsoft\VisualStudio\*\Extensions")
)

foreach ($searchPath in $searchPaths) {
    # Search for extension.vsixmanifest files only within extension directories
    $manifests = Get-ChildItem -Path $searchPath -Recurse -Filter 'extension.vsixmanifest' -ErrorAction SilentlyContinue
    foreach ($manifest in $manifests) {
        try {
            [xml]$xml = Get-Content $manifest.FullName -ErrorAction Stop
            $identity = $xml.PackageManifest.Metadata.Identity

            if ($identity.Id -eq $ExtensionId) {
                $installedVersion = [Version]$identity.Version

                if ($installedVersion -ge $current) {
                    Write-Host "INFO: Extension already installed with version $installedVersion (building: $CurrentVersion). Skipping deployment."
                    $skipDeploy = $true
                    break
                }
            }
        } catch { }
    }
    if ($skipDeploy) { break }
}

if ($skipDeploy) {
    Write-Host "SKIP_DEPLOY=true"
    if ($OutputFile) { "true" | Out-File -FilePath $OutputFile -NoNewline }
    exit 0
} else {
    Write-Host "SKIP_DEPLOY=false"
    if ($OutputFile) { "false" | Out-File -FilePath $OutputFile -NoNewline }
}