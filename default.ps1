properties {
	$projectName = "DH.Logging"
	$buildNumber = 0
	$rootDir  = Resolve-Path .\
	$buildOutputDir = "$rootDir\build"
	$reportsDir = "$buildOutputDir\reports"
	$srcDir = "$rootDir\src"
	$solutionFilePath = "$srcDir\$projectName.sln"
	$toolsPath = "$rootDir\tools"
	$assemblyInfoFilePath = "$srcDir\SharedAssemblyInfo.cs"
}

task default -depends UpdateVersionNumbers, CleanBuildDir, Compile, CopyBuildOutput, CreateNuGetPackage, RunTests

task CleanBuildDir {
	Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
}

task CleanSolution {
	exec { msbuild $solutionFilePath /t:Clean }
}

task UpdateVersionNumbers {
	$version = Get-Version $assemblyInfoFilePath
	$oldVersion = New-Object Version $version
	$newVersion = New-Object Version ($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build, $buildNumber)
	Update-Version $newVersion $assemblyInfoFilePath
}

task Compile { 
	exec { msbuild $solutionFilePath /p:Configuration=Release }
}

task RunTests -depends Compile {
	New-Item $reportsDir\xUnit\Tests.$projectName -Type Directory
	$xunitRunner = "$toolsPath\xunit.runners\xunit.console.clr4.exe"
	.$xunitRunner "$srcDir\Tests.$projectName\bin\Release\Tests.$projectName.dll" /html "$reportsDir\xUnit\Tests.$projectName\index.html"
}

task CopyBuildOutput -depends Compile {
	$binOutputDir = "$buildOutputDir\$projectName\lib\net40"
	New-Item $binOutputDir -Type Directory
	Copy-Item "$srcDir\$projectName\bin\Release\*.*" $binOutputDir
}

task CreateNuGetPackage -depends CopyBuildOutput{
	$packageVersion = Get-Version $assemblyInfoFilePath
	exec { .$srcDir\.nuget\nuget.exe pack $srcDir\DH.Logging.nuspec -o $buildOutputDir -version $packageVersion }
}

function Get-Version
{
	param
	(
		[string]$assemblyInfoFilePath
	)
	Write-Host "path $assemblyInfoFilePath"
	$pattern = '(?<=^\[assembly\: AssemblyVersion\(\")(?<versionString>\d+\.\d+\.\d+\.\d+)(?=\"\))'
	$assmblyInfoContent = Get-Content $assemblyInfoFilePath
	return $assmblyInfoContent | Select-String -Pattern $pattern | Select -expand Matches |% {$_.Groups['versionString'].Value}
}

function Update-Version
{
	param 
    (
		[string]$version,
		[string]$assemblyInfoFilePath
	)
	
	$newVersion = 'AssemblyVersion("' + $version + '")';
	$newFileVersion = 'AssemblyFileVersion("' + $version + '")';
	$tmpFile = $assemblyInfoFilePath + ".tmp"

	Get-Content $assemblyInfoFilePath | 
		%{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $newVersion } |
		%{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $newFileVersion }  | Out-File -Encoding UTF8 $tmpFile

	Move-Item $tmpFile $assemblyInfoFilePath -force
}