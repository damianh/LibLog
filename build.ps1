$suffix = "build." + $env:APPVEYOR_BUILD_NUMBER
$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"

dotnet build src\LibLog.sln -c Release -
dotnet test src\LibLog.Tests -c Release --no-build
(Get-Content ./src/LibLog/LibLog.cs).Replace('YourRootNamespace.', '$rootnamespace$.') `
    | Set-Content ./src/LibLog/LibLog.cs.pp

& $NUGET_EXE pack src/LibLog/LibLog.nuspec -Suffix $suffix -OutputDirectory artifacts -MinClientVersion 4.3