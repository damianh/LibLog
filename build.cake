#addin "Cake.ExtendedNuGet"
#addin "nuget:?package=NuGet.Core&version=2.8.6"
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

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solution, settings => settings.SetConfiguration(configuration));
});

Task("Run-Unit-Tests")
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

Task("Create-PP")
    .IsDependentOn("Build")
    .Does(() => {
        TransformTextFile("./src/LibLog/LibLog.cs")
        .WithToken("YourRootNamespace.", "$rootnamespace$.")
        .Save(buildDir.Path + "/LibLog.cs.pp");
});

Task("Create-Nuget-Package")
    .IsDependentOn("Create-PP")
    .Does(() => 
{
    var nuspecFilePath = buildDir.Path + "/LibLog.nuspec";
    CopyFile("./src/LibLog/LibLog.nuspec", nuspecFilePath);
    var settings = new NuGetPackSettings 
    {
        OutputDirectory = buildDir.Path
    };
    NuGetPack(nuspecFilePath, settings);
});

Task("Default")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Create-Nuget-Package");

RunTarget(target);