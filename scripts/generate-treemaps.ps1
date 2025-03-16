& "$PSScriptRoot\treemap.ps1" -RootDir "/tmp/release/windows" -PathToPckExplorer "/tmp/release/pckexplorer/GodotPCKExplorer.Console.dll" > /tmp/release/TreeMap.windows
& "$PSScriptRoot\treemap.ps1" -RootDir "/tmp/release/linux" -PathToPckExplorer "/tmp/release/pckexplorer/GodotPCKExplorer.Console.dll" > /tmp/release/TreeMap.linux
& "$PSScriptRoot\treemap.ps1" -RootDir "/tmp/release/mac" -PathToPckExplorer "/tmp/release/pckexplorer/GodotPCKExplorer.Console.dll" > /tmp/release/TreeMap.mac

Get-Content /tmp/release/TreeMap.windows | treemap -o /tmp/release/treemaps/AnkiTaiso-BundleSize-windows.html -f html
Get-Content /tmp/release/TreeMap.linux | treemap -o /tmp/release/treemaps/AnkiTaiso-BundleSize-linux.html -f html
Get-Content /tmp/release/TreeMap.mac | treemap -o /tmp/release/treemaps/AnkiTaiso-BundleSize-mac.html -f html
