Set-Location (Split-Path -Path $Script:MyInvocation.MyCommand.Path)

$sourceFolder = "../godot/src"
$targetFolder = "../docs/reference"

if (-not (Test-Path -Path $sourceFolder -PathType Container)) {
  Write-Error "Error: The folder path '$sourceFolder' does not exist or is not a directory."
  exit 1
}

# Find all .puml files
$pumlFiles = Get-ChildItem -Path $sourceFolder -Filter "*.puml" -Recurse -File

if ($pumlFiles.Count -eq 0) {
  Write-Host "No .puml files found in '$sourceFolder'."
  exit
}

# Get all .puml files
$pumlFiles = Get-ChildItem -Path $sourceFolder -Filter "*.puml" -Recurse -File

if ($pumlFiles.Count -eq 0) {
  Write-Host "No .puml files found in '$sourceFolder'."
  exit
}

# Check for duplicates in SOURCE (same filename)
$duplicateCount = ($pumlFiles.Name | Group-Object | Where-Object Count -GT 1).Count
if ($duplicateCount -gt 0) {
  Write-Warning "WARNING: Found $duplicateCount duplicate filenames in source! These will overwrite each other in target."
}

# Check existing files in TARGET
$existingFiles = Get-ChildItem -Path $targetFolder -Filter "*.puml" -File
if ($existingFiles.Count -gt 0) {
  Write-Host "Target folder contains $($existingFiles.Count) .puml files that may be overwritten."
}

# Show summary
Write-Host "`nCopy Plan:"
Write-Host "Source:     $sourceFolder ($($pumlFiles.Count) .puml files)"
Write-Host "Target:     $targetFolder (flat dump)"
Write-Host "Duplicates: $duplicateCount in source, $($existingFiles.Count) existing in target`n"

# Copy all files (flat structure)
$skipped = 0
$copied = 0
foreach ($file in $pumlFiles) {
  $destPath = Join-Path -Path $targetFolder -ChildPath $file.Name
  try {
    Copy-Item -Path $file.FullName -Destination $destPath -Force
    Write-Host "Copied: $($file.Name)"
    $copied++
  }
  catch {
    Write-Warning "Failed to copy $($file.Name): $_"
    $skipped++
  }
}

# Generate markdown document
$mdPath = Join-Path -Path $targetFolder -ChildPath "state-flow.md"
$mdContent = @("# State Flow`n`n")

Get-ChildItem -Path $targetFolder -Filter "*.puml" -File | Sort-Object Name | ForEach-Object {
  # Convert filename to heading (remove extension and split camelCase)
  $baseName = $_.BaseName.Replace(".g", "")
  Write-Host "`nbaseName $baseName"
  $heading = [System.Text.RegularExpressions.Regex]::Replace(
    $baseName,
    '(?<!^)(?=[A-Z])',
    ' '
  ).Replace(".", " ").Trim()

  # Add section to markdown
  $mdContent += "## $heading`n"
  $mdContent += "![$heading](./$($_.Name))`n`n"
}

$mdContent | Out-File -Path $mdPath -Encoding utf8

# Results
Write-Host "`nOperation complete:"
Write-Host "- Copied:   $copied"
Write-Host "- Skipped:  $skipped"
Write-Host "- Final files in target: $(@(Get-ChildItem $targetFolder).Count)"
Write-Host "- Generated documentation: $mdPath"
