param(
  [Parameter(Mandatory=$true)]
  [string]$RootDir,
  [Parameter(Mandatory=$true)]
  [string]$PathToPckExplorer
)

# Helper: recursively compute size and build a tree node.
function Get-TreeNode {
  param(
    [string]$Path,
    [string]$RelPath
  )

  $node = [PSCustomObject]@{
    RelativePath = $RelPath
    Size         = 0
    Children     = @()
  }

  if (Test-Path $Path -PathType Leaf) {
    $ext = [System.IO.Path]::GetExtension($Path)
    if ($ext -ieq ".pck") {
      # Create a node for the .pck file using its own size.
      $pckSize = (Get-Item $Path).Length
      $node.Size = $pckSize

      # Create a child node for the extracted contents.
      $pckBase = [System.IO.Path]::GetFileNameWithoutExtension($Path)
      $tempDir = New-Item -ItemType Directory -Path ([System.IO.Path]::Combine($env:TEMP, [System.Guid]::NewGuid().ToString()))
      # Extract the .pck file.
      dotnet $PathToPckExplorer -e $Path $tempDir.FullName | Out-Null
      $extractedNode = Get-TreeNode -Path $tempDir.FullName -RelPath (Join-Path $RelPath $pckBase)
      Remove-Item $tempDir.FullName -Recurse -Force

      # Attach extracted content as a child and add its size.
      $node.Children += $extractedNode
      $node.Size += $extractedNode.Size
    }
    else {
      # Normal file: get its size.
      $size = (Get-Item $Path).Length
      $node.Size = $size
    }
  }
  elseif (Test-Path $Path -PathType Container) {
    $childItems = Get-ChildItem $Path
    foreach ($item in $childItems) {
      if ([string]::IsNullOrEmpty($RelPath)) {
        $childRel = "all"
      }
      else {
        $childRel = Join-Path $RelPath $item.Name
      }
      $childNode = Get-TreeNode -Path $item.FullName -RelPath $childRel
      $node.Children += $childNode
      $node.Size += $childNode.Size
    }
  }

  return $node
}

# Helper: traverse the tree and output lines in the format "[size] [relative/path]"
function Write-TreeMap {
  param(
    [Parameter(Mandatory=$true)]
    $Node
  )
  Write-Output ("{0} {1}" -f $Node.Size, $Node.RelativePath)
  foreach ($child in $Node.Children) {
    Write-TreeMap -Node $child
  }
}

# Build the tree starting at the provided root.
$tree = Get-TreeNode -Path (Resolve-Path $RootDir) -RelPath "all"

# Output the treemap lines to stdout.
Write-TreeMap -Node $tree
