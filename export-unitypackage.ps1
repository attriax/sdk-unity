$ErrorActionPreference = 'Stop'

$unityExe = if ($env:UNITY_EDITOR_PATH -and $env:UNITY_EDITOR_PATH.Trim()) {
  $env:UNITY_EDITOR_PATH.Trim()
} else {
  'C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe'
}
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Test-Path $unityExe)) {
  throw "Could not find the Unity editor executable. Looked for: $unityExe`nSet UNITY_EDITOR_PATH to the correct Unity.exe location before rerunning the export."
}

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