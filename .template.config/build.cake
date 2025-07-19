#tool nuget:?package=Cake.DotNetTool.Module&version=0.4.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("version", "1.0.0");

Task("Clean")
    .Does(() =>
    {
        CleanDirectory("./bin");
        CleanDirectory("./obj");
        CleanDirectory("./nupkg");
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore("./DotNetCleanTemplate.sln");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild("./DotNetCleanTemplate.sln", new DotNetBuildSettings
        {
            Configuration = configuration,
            NoRestore = true
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetTest("./DotNetCleanTemplate.sln", new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true
        });
    });

Task("Pack-Template")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var packSettings = new DotNetPackSettings
        {
            Configuration = configuration,
            OutputDirectory = "./nupkg",
            NoBuild = true,
            NoRestore = true,
            IncludeSymbols = true,
            SymbolPackageFormat = "snupkg"
        };

        DotNetPack("./.template.config/templatepack.csproj", packSettings);
    });

Task("Install-Template-Local")
    .IsDependentOn("Pack-Template")
    .Does(() =>
    {
        DotNetTool("new", "--install", "./nupkg/DotNetCleanTemplate.1.0.0.nupkg");
    });

Task("Uninstall-Template-Local")
    .Does(() =>
    {
        DotNetTool("new", "--uninstall", "DotNetCleanTemplate");
    });

Task("Default")
    .IsDependentOn("Pack-Template");

RunTarget(target); 