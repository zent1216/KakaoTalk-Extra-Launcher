$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Set-Location $PSScriptRoot
New-Item -ItemType Directory -Force obj | Out-Null

& $csc /nologo /target:exe /codepage:65001 /out:obj\MakeIcon.exe /reference:System.Drawing.dll MakeIcon.cs IconArt.cs
if ($LASTEXITCODE -ne 0) { throw "MakeIcon build failed" }
& .\obj\MakeIcon.exe obj\app.ico obj\icon-preview.png
if ($LASTEXITCODE -ne 0) { throw "Icon generation failed" }

& $csc /nologo /target:winexe /optimize+ /codepage:65001 `
  /win32icon:obj\app.ico `
  /out:KakaoTalk-Extra-Launcher.exe `
  /reference:System.Windows.Forms.dll `
  /reference:System.Drawing.dll `
  /reference:System.ServiceProcess.dll `
  ExtraLauncher.cs IconArt.cs
if ($LASTEXITCODE -ne 0) { throw "Launcher build failed" }
Write-Host "Built: $PSScriptRoot\KakaoTalk-Extra-Launcher.exe"
