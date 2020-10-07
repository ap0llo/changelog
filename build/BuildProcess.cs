using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class BuildProcess : NukeBuild
{
    public static int Main() => Execute<BuildProcess>(x => x.Build);


    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    [GitRepository] readonly GitRepository GitRepository;

    [Parameter("During test execution, collect code coverage")]
    readonly bool CollectCoverage = false;

    [Parameter("Skip execution of the '" + nameof(Build) + "' target")]
    readonly bool NoBuild = false;

    [Parameter("Skip execution of the '" + nameof(Restore) + "' target")]
    readonly bool NoRestore = false;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath RootOutputDirectory => Host == HostType.AzurePipelines
        ? (AbsolutePath)AzurePipelines.Instance.BinariesDirectory
        : (RootDirectory / "Binaries");

    AbsolutePath TestResultsDirectory => RootOutputDirectory / "TestResults";

    AbsolutePath CodeCoverageReportDirectory => RootOutputDirectory / "TestResults" / "Coverage";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/obj").ForEach(DeleteDirectory);
            DeleteDirectory(RootOutputDirectory);
        });

    Target Restore => _ => _
        .OnlyWhenDynamic(() => !(NoBuild || NoRestore))
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });

    Target Build => _ => _
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

    Target Test => _ => _
        .DependsOn(Build, CleanTestResultsDirectory)
        .Executes(() =>
        {
            // Run tests
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetResultsDirectory(TestResultsDirectory)
                .When(CollectCoverage, _ => _
                    .SetDataCollector("XPlat Code Coverage")
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            ));

            // Print result files
            CoberturaCoverageReports.ForEach(result => Info($"Coverage result file: {result}"));

            // Generate coverage report
            ReportGenerator(_ => _
                .SetFramework("netcoreapp3.0")
                .SetReports(CoberturaCoverageReports)
                .SetTargetDirectory(CodeCoverageReportDirectory)
                .SetHistoryDirectory(CodeCoverageReportDirectory / "History")
            );
            CodeCoverageReportDirectory
                .GlobFiles("index.html")
                .ForEach(x => Success($"Coverage Report: {x}"));
        });


    Target CleanTestResultsDirectory => _ => _
        .Unlisted()
        .Executes(() =>
        {
            TestResultsDirectory.GlobDirectories("*")
                .Except(new[] { CodeCoverageReportDirectory })
                .ForEach(DeleteDirectory);
        });


    IEnumerable<string> CoberturaCoverageReports => TestResultsDirectory.GlobFiles("**/*.cobertura.xml").Select(x => (string)x);
}
