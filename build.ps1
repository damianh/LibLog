if (-not (Test-Path env:APPVEYOR_BUILD_NUMBER)) { $env:APPVEYOR_BUILD_NUMBER = '0' }
$suffix = "build." + $env:APPVEYOR_BUILD_NUMBER
$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"

dotnet build src\LibLog.sln -c Release
dotnet test src\LibLog.Tests -c Release --no-build

Get-ChildItem ./src/*.pp -Recurse | ForEach-Object { Remove-Item $_ }

$files = (Get-ChildItem -Path ./src/LibLog -Filter *.cs -File) +
         (Get-ChildItem -Path ./src/LibLog/LogProviders -Filter *.cs -File)

$files | ForEach-Object { 
    (Get-Content $_.FullName).Replace('YourRootNamespace.', '$rootnamespace$.') |
    Set-Content ($_.FullName + ".pp")
}

& $NUGET_EXE pack src/LibLog/LibLog.nuspec -Suffix $suffix -OutputDirectory artifacts -MinClientVersion 4.3