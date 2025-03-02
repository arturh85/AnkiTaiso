Set-Location (Split-Path -Path $Script:MyInvocation.MyCommand.Path)

# either URL or just path containing API

$folderPath = "../godot/src"


if (-not (Test-Path -Path $folderPath -PathType Container)) {
  Write-Error "Error: The folder path '$folderPath' does not exist or is not a directory."
  exit 1
}

# Find all .puml files recursively
$pumlFiles = Get-ChildItem -Path $folderPath -Filter "*.puml" -Recurse -File

# Exit if no files found
if ($pumlFiles.Count -eq 0) {
  Write-Host "No .puml files found in '$folderPath'."
  exit
}

# Show file list and confirmation prompt
Write-Host "Found $($pumlFiles.Count) .puml files:"
$pumlFiles | ForEach-Object { Write-Host "  $($_.FullName)" }

# Delete files with confirmation
try {
  $pumlFiles | Remove-Item -Force -Verbose
  Write-Host "`nSuccessfully deleted $($pumlFiles.Count) .puml files."
}
catch {
  Write-Error "Error occurred while deleting files: $_"
  exit 2
}
