#addin "nuget:?package=NuGet.Core&version=2.8.6"
#addin "Cake.FileHelpers"
#tool "nuget:?package=xunit.runner.console&version=2.1.0"

var target        = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildNumber   = Argument("buildnumber", "0");
var buildDir      = Directory("./artifacts");
var solution      = "./src/LibLog.sln";


Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
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
    .Does(() => {
        TransformTextFile("./src/LibLog/LibLog.cs")
        .WithToken("YourRootNamespace.", "$rootnamespace$.")
        .Save(buildDir.Path + "/LibLog.cs.pp");
});

Task("CreateNugetPackages")
    .IsDependentOn("CreatePreProcessedFiles")
    .Does(() => 
{
    var nuspecFilePath = buildDir.Path + "/LibLog.nuspec";
    CopyFile("./src/LibLog/LibLog.nuspec", nuspecFilePath);
    var version = System.IO.File.ReadLines("version.txt").First() + "-build" + buildNumber.PadLeft(5, '0');
    var settings = new NuGetPackSettings 
    {
        OutputDirectory = buildDir.Path,
        Version = version
    };
    NuGetPack(nuspecFilePath, settings);
});

Task("Default")
    .IsDependentOn("RunTests")
    .IsDependentOn("CreateNugetPackages");

RunTarget(target);