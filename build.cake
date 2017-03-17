#addin "nuget:?package=NuGet.Core&version=2.8.6"
#addin "Cake.FileHelpers"
#tool "nuget:?package=xunit.runner.console&version=2.1.0"

var target        = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildNumber   = Argument("buildnumber", "0");
var artifactsDir  = Directory("./artifacts");
var releasesDir   = Directory("./releases");
var solution      = "./src/LibLog.sln";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    MSBuild(solution, settings => settings
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
    );
});

Task("RunTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new XUnitSettings 
    { 
        ToolPath = "./tools/xunit.runner.console/tools/xunit.console.exe"
    };

    XUnit($"./src/LibLog.Tests/bin/LibLog/{configuration}/*.Tests*dll", settings);
    XUnit($"./src/LibLog.Tests/bin/LibLogPCL/{configuration}/*.Tests*dll", settings);
    XUnit($"./src/**/bin/{configuration}/*.Tests*dll", settings);
});

Task("CreatePreProcessedFiles")
    .IsDependentOn("Build")
    .Does(() => 
{
    CreateDirectory(artifactsDir.Path + "/net40");
    var content = System.IO.File.ReadAllText("./src/LibLog/LibLog.cs")
        .Replace("YourRootNamespace.", "$rootnamespace$.");
    System.IO.File.WriteAllText(artifactsDir.Path + "/LibLog.cs.pp", content);
});

Task("CreateNugetPackage")
    .IsDependentOn("CreatePreProcessedFiles")
    .Does(() => 
{
    var nuspecFilePath = artifactsDir.Path + "/LibLog.nuspec";
    CopyFile("./src/LibLog/LibLog.nuspec", nuspecFilePath);
    var version = System.IO.File.ReadLines("version.txt").First() + "-build" + buildNumber.PadLeft(5, '0');
    var settings = new NuGetPackSettings 
    {
        OutputDirectory = artifactsDir,
        Version = version
    };
    NuGetPack(nuspecFilePath, settings);
});

Task("BuildRelease")
    .IsDependentOn("CreatePreProcessedFiles")
    .Does(() => 
{
    var version = System.IO.File.ReadLines("version.txt").First();
    CreateDirectory(releasesDir.Path + "/" + version);
    CopyFile(artifactsDir.Path + "/LibLog.cs.pp", releasesDir.Path + "/" + version + "/LibLog.cs");
    CreateDirectory(releasesDir.Path + "/latest");
    CopyFile(artifactsDir.Path + "/LibLog.cs.pp", releasesDir.Path + "/latest/LibLog.cs");
});

Task("Default")
    .IsDependentOn("RunTests")
    .IsDependentOn("CreateNugetPackage")
    .IsDependentOn("BuildRelease");

RunTarget(target);