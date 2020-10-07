using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
internal class BuildProcess : NukeBuild
{
    public static int Main() => Execute<BuildProcess>(x => x.Build);


    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] private readonly Solution Solution;
    [GitRepository] private readonly GitRepository GitRepository;

    [Parameter("During test execution, collect code coverage")]
    private readonly bool CollectCoverage = false;

    [Parameter("Skip execution of the '" + nameof(Build) + "' target")]
    private readonly bool NoBuild = false;

    [Parameter("Skip execution of the '" + nameof(Restore) + "' target")]
    private readonly bool NoRestore = false;

    private AbsolutePath SourceDirectory => RootDirectory / "src";

    private AbsolutePath RootOutputDirectory => Host == HostType.AzurePipelines
        ? (AbsolutePath)AzurePipelines.Instance.BinariesDirectory
        : (RootDirectory / "Binaries");

    private AbsolutePath TestResultsDirectory => RootOutputDirectory / "TestResults";

    private Target Clean => _ => _
        .Before(Restore)            
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/obj").ForEach(DeleteDirectory);
            DeleteDirectory(RootOutputDirectory);
        });

    private Target Restore => _ => _
        .OnlyWhenDynamic(() => !(NoBuild || NoRestore))
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });

    private Target Build => _ => _
        .OnlyWhenDynamic(() => !NoBuild)
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
            );
        });

    private Target Test => _ => _
        .DependsOn(Build)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .When(CollectCoverage, x => x
                    .SetDataCollector("XPlat Code Coverage")
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            ));

            CoberturaCoverageReports.ForEach(result => Info($"Coverage result file: {result}"));
        });

    private IEnumerable<string> CoberturaCoverageReports => Directory.GetFiles(TestResultsDirectory, "*.cobertura.xml", SearchOption.AllDirectories);
}
