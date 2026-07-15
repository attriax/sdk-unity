$ErrorActionPreference = 'Stop'

$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Export with the editor this project actually targets. Exporting with a different
# editor than ProjectSettings/ProjectVersion.txt forces a full asset reimport and
# can bake the wrong serialization version into the .unitypackage, so the version
# is derived from the project rather than hardcoded.
function Get-ProjectEditorVersion {
  param([string]$ProjectPath)

  $projectVersionPath = Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
  if (-not (Test-Path $projectVersionPath)) {
    throw "Could not determine the Unity editor version: $projectVersionPath does not exist.`nSet UNITY_EDITOR_PATH to the Unity.exe to export with."
  }

  $match = Select-String -Path $projectVersionPath -Pattern '^m_EditorVersion:\s*(.+)$' | Select-Object -First 1
  if (-not $match) {
    throw "Could not determine the Unity editor version: no m_EditorVersion entry in $projectVersionPath.`nSet UNITY_EDITOR_PATH to the Unity.exe to export with."
  }

  return $match.Matches[0].Groups[1].Value.Trim()
}

if ($env:UNITY_EDITOR_PATH -and $env:UNITY_EDITOR_PATH.Trim()) {
  $unityExe = $env:UNITY_EDITOR_PATH.Trim()
  if (-not (Test-Path $unityExe)) {
    throw "Could not find the Unity editor executable. Looked for: $unityExe`nUNITY_EDITOR_PATH is set but does not point at an existing Unity.exe."
  }
} else {
  $editorVersion = Get-ProjectEditorVersion -ProjectPath $projectPath
  $unityExe = "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
  if (-not (Test-Path $unityExe)) {
    throw "Could not find the Unity editor executable. This project targets Unity $editorVersion (per ProjectSettings/ProjectVersion.txt) and no editor is installed at: $unityExe`nInstall Unity $editorVersion via Unity Hub, or set UNITY_EDITOR_PATH to override (note: exporting with a different editor version forces a full asset reimport)."
  }
}

Write-Host "Exporting with Unity editor: $unityExe"

$staleExportRoot = Join-Path $projectPath 'Assets\__AttriaxExport'
$staleExportMeta = Join-Path $projectPath 'Assets\__AttriaxExport.meta'

if (Test-Path $staleExportRoot) {
  Remove-Item $staleExportRoot -Recurse -Force
}

if (Test-Path $staleExportMeta) {
  Remove-Item $staleExportMeta -Force
}

$process = Start-Process `
  -FilePath $unityExe `
  -ArgumentList @(
    '-batchmode',
    '-projectPath', $projectPath,
    '-attriaxExportPackage',
    '-logFile', '-'
  ) `
  -Wait `
  -PassThru `
  -NoNewWindow

exit $process.ExitCode