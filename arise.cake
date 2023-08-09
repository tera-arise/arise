#nullable enable

// Arguments

var target = Argument("t", "default");
var configuration = Argument("c", "Debug");
var vendor = Argument("vendor", string.Empty);

// Paths

var root = Context.Environment.WorkingDirectory;
var ariseProj = root.CombineWithFilePath("arise.proj");
var outLog = root.Combine("out").Combine("log");

// Utilities

DotNetMSBuildSettings ConfigureMSBuild(string target)
{
    var prefix = $"{target}_{Environment.UserName}_{Environment.MachineName}_";
    var time = DateTime.Now;

    string name;

    do
    {
        name = $"{prefix}{time:yyyy-MM-dd_HH_mm_ss}.binlog";
        time = time.AddSeconds(1);
    }
    while (System.IO.File.Exists(name));

    var settings = new DotNetMSBuildSettings
    {
        // TODO: https://github.com/dotnet/msbuild/issues/6756
        NoLogo = true,
        BinaryLogger = new()
        {
            Enabled = true,
            FileName = outLog.CombineWithFilePath(name).FullPath,
        },
        ConsoleLoggerSettings = new()
        {
            NoSummary = true,
        },
        // TODO: https://github.com/cake-build/cake/issues/4144
        ArgumentCustomization = args => args.Append("-ds:false"),
    };

    if (!string.IsNullOrWhiteSpace(vendor))
        settings.Properties.Add(
            "AriseVendorProjectPath",
            new[] { new FilePath(vendor).MakeAbsolute(Context.Environment).FullPath });

    return settings;
}

// Tasks

Task("default")
    .IsDependentOn("publish");

Task("restore")
    .Does(() =>
        DotNetRestore(
            ariseProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("restore"),
            }));

Task("build")
    .IsDependentOn("restore")
    .Does(() =>
        DotNetBuild(
            ariseProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("build"),
                Configuration = configuration,
                NoRestore = true,
            }));

Task("publish")
    .IsDependentOn("build")
    .Does(() =>
        DotNetPublish(
            ariseProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("publish"),
                Configuration = configuration,
                NoBuild = true,
            }));

RunTarget(target);
