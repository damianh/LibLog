param(
    [int]$buildNumber = 0
    )

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER){
    $buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
    Write-Host "Using APPVEYOR_BUILD_NUMBER"
}

"Build number $buildNumber"

src\.nuget\nuget.exe install src\.nuget\packages.config -o src\packages

$packageConfigs = Get-ChildItem . -Recurse | where{$_.Name -like "packages.*config"}
foreach($packageConfig in $packageConfigs){
    Write-Host "Restoring" $packageConfig.FullName
    src\.nuget\nuget.exe install $packageConfig.FullName -o src\packages
}

Import-Module .\src\packages\psake.4.6.0\tools\psake.psm1
Invoke-Psake .\default.ps1 default -framework "4.5.1x64" -properties @{ buildNumber=$buildNumber }
Remove-Module psake
