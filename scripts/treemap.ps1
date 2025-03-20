param(
  [Parameter(Mandatory = $true)]
  [string]$RootDir,
  [Parameter(Mandatory = $true)]
  [string]$PathToPckExplorer,
  [Parameter(Mandatory = $true)]
  [string]$OutputFile
)

# Create a temporary directory
$tempDir = New-Item -ItemType Directory -Path ([System.IO.Path]::Combine($env:TEMP,[System.Guid]::NewGuid().ToString()))

# Copy all files and folders recursively to the temporary directory
$null = Copy-Item -Path "$RootDir\*" -Destination $tempDir.FullName -Recurse -Container

# Function to extract .pck files
function Extract-PCKFiles
{
  param (
    [string]$Directory
  )

  # Get all .pck files in the directory
  $pckFiles = Get-ChildItem -Path $Directory -Recurse -Filter '*.pck'

  foreach ($pckFile in $pckFiles)
  {
    $extractDir = "$( $pckFile.DirectoryName )/$( $pckFile.BaseName )_pck"
    # Create a directory for extraction
    New-Item -ItemType Directory -Path $extractDir -Force | Out-Null

    # Extract the .pck file
    dotnet $PathToPckExplorer -e $pckFile.FullName $extractDir
    # Remove the original .pck file
    Remove-Item -Path $pckFile.FullName -Force
  }
}

# Extract all .pck files in the temporary directory
Extract-PCKFiles -Directory $tempDir.FullName

# Generate Treemap HTML from temp folder
treemap -o $OutputFile -f html du $tempDir.FullName

# Remove the temporary directory
Remove-Item -Path $tempDir.FullName -Recurse -Force
