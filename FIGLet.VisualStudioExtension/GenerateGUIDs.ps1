param(
    [Parameter(Mandatory=$true)]
    [string]$TemplatePath,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    
    [Parameter(Mandatory=$true)]
    [string]$HeaderPath
)

# Function to extract values from a C-style header file
function Get-HeaderDefines {
    param (
        [string]$FilePath
    )
    
    $defines = @{}
    $content = Get-Content $FilePath
    
    foreach ($line in $content) {
        if ($line -match '^\s*#define\s+(\w+)\s+(.+)$') {
            $key = $matches[1]
            $value = $matches[2].Trim()
            
            # Remove surrounding quotes if present
            if ($value -match '^"(.*)"$') {
                $value = $matches[1]
            }
            
            $defines[$key] = $value
        }
    }
    
    return $defines
}

# Function to replace template values
function Replace-TemplateValues {
    param (
        [string]$Template,
        [hashtable]$Values
    )
    
    $result = $Template
    foreach ($key in $Values.Keys) {
        $result = $result -replace "\{$key\}", $Values[$key]
    }
    
    return $result
}

try {
    # Read header file and extract defines
    Write-Host "Reading defines from: $HeaderPath"
    $defines = Get-HeaderDefines -FilePath $HeaderPath
    Write-Host "Found defines:"
    $defines.Keys | ForEach-Object { Write-Host "  $_" }

    # Read template
    Write-Host "Reading template from: $TemplatePath"
    $templateContent = Get-Content -Path $TemplatePath -Raw

    # Generate new content
    $newContent = Replace-TemplateValues -Template $templateContent -Values $defines

    # Check if output file exists and is different
    $generateFile = $true
    if (Test-Path $OutputPath) {
        $existingContent = Get-Content -Path $OutputPath -Raw
        if ($existingContent -eq $newContent) {
            Write-Host "No changes needed for: $OutputPath"
            $generateFile = $false
        }
    }

    # Write output file if needed
    if ($generateFile) {
        $newContent | Out-File -FilePath $OutputPath -Encoding UTF8 -NoNewline
        Write-Host "Generated file at: $OutputPath"
    }

    exit 0
} catch {
    Write-Error "An error occurred during generation:"
    Write-Error $_.Exception.Message
    Write-Error $_.ScriptStackTrace
    exit 1
}