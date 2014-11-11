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
	$xunitRunner = "$srcDir\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"
	gci . -Recurse -Include *Tests.csproj, Tests.*.csproj | % {
		$project = $_.BaseName
		if(!(Test-Path $reportsDir\xUnit\$project)){
			New-Item $reportsDir\xUnit\$project -Type Directory
		}
        .$xunitRunner "$srcDir\$project\bin\Release\$project.dll" /html "$reportsDir\xUnit\$project\index.html"
    }
}

task CreatePP {
	(Get-Content $srcDir\$projectName\$projectName.cs) | Foreach-Object {
		$_ -replace 'namespace LibLog', 'namespace $rootnamespace$' `
		-replace 'using LibLog', 'using $rootnamespace$'
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
