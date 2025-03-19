# Define the path to your published executable.
$targetPath = "E:\RC2\bin\Release\net8.0-windows7.0\win-x64\RC2.exe"

# Get the current user's desktop folder.
$desktop = [System.Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktop "RC2.lnk"

# Create the shortcut using the WScript.Shell COM object.
$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetPath
$shortcut.WorkingDirectory = Split-Path $targetPath
$shortcut.IconLocation = "$targetPath,0"
$shortcut.Save()

Write-Output "Desktop shortcut created at $shortcutPath"