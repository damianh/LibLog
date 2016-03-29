properties {
    $projectName = "LibLog"
    $buildNumber = 0
    $rootDir  = Resolve-Path .\
    $buildOutputDir = "$rootDir\build"
    $reportsDir = "$buildOutputDir\reports"
    $srcDir = "$rootDir\src"
    $solutionFilePath = "$srcDir\$projectName.sln"
}

task default -depends RunTests, CreateNuGetPackage

task Clean {
    Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
    exec { msbuild /nologo /verbosity:quiet $solutionFilePath /t:Clean }
}

task Compile {
    exec { msbuild /nologo /verbosity:quiet $solutionFilePath /p:Configuration=Release }
}

task RunTests -depends Compile {
    $xunitRunner = "$srcDir\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe"

    New-Item $reportsDir\xUnit\LibLog.Tests -Type Directory -ErrorAction SilentlyContinue
    .$xunitRunner "$srcDir\LibLog.Tests\bin\LibLog\Release\LibLog.Tests.dll" -html "$reportsDir\xUnit\LibLog.Tests\index.html"

    New-Item $reportsDir\xUnit\LibLogPCL.Tests -Type Directory  -ErrorAction SilentlyContinue
    .$xunitRunner "$srcDir\LibLog.Tests\bin\LibLogPCL\Release\LibLogPCL.Tests.dll" -html "$reportsDir\xUnit\LibLogPCL.Tests\index.html"

    New-Item $reportsDir\xUnit\LibLog.Tests.NLog4 -Type Directory  -ErrorAction SilentlyContinue
    .$xunitRunner "$srcDir\LibLog.Tests.NLog4\bin\Release\LibLog.Tests.NLog4.dll" -html "$reportsDir\xUnit\LibLog.Tests.NLog4\index.html"
}

task CreatePP {
    (Get-Content $srcDir\$projectName\$projectName.cs) | Foreach-Object {
        $_ -replace 'YourRootNamespace\.', '$rootnamespace$.'
        } | Set-Content $buildOutputDir\$projectName.cs.pp -Encoding UTF8
}

task CreateNuGetPackage -depends CreatePP {
    $nuspecFilePath = "$buildOutputDir\$projectName.nuspec"
    Copy-Item $srcDir\$projectName\$projectName.nuspec $nuspecFilePath

    [Xml]$fileContents = Get-Content -Path $nuspecFilePath
    $fileContents.package.metadata.version
    $packageVersion = $fileContents.package.metadata.version + "-build" + $buildNumber.ToString().PadLeft(5,'0')
    .$srcDir\.nuget\nuget.exe pack $nuspecFilePath -o $buildOutputDir -version $packageVersion
}
