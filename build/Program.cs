using System.Collections.Generic;
using Cake.AzurePipelines.Module;
using Cake.Core;
using Cake.DotNetLocalTools.Module;
using Cake.Frosting;
using Grynwald.SharedBuild;

return new CakeHost()
    .UseModule<AzurePipelinesModule>()
    .UseModule<LocalToolsModule>()
    .InstallToolsFromManifest(".config/dotnet-tools.json")
    .UseSharedBuild<BuildContext>(
        // Import all tasks except the GenerateChangeLogTask.
        // For generating the change log, use the GenerateChangeLogTask defined in this assembly
        // which uses the locally-built version of the changelog tool
        taskType => taskType != typeof(Grynwald.SharedBuild.Tasks.GenerateChangeLogTask)
    )
    .Run(args);


public class BuildContext : DefaultBuildContext
{
    public override IReadOnlyCollection<IPushTarget> PushTargets { get; } = new[]
    {
        new PushTarget(
            PushTargetType.MyGet,
            "https://www.myget.org/F/ap0llo-changelog/api/v3/index.json",
            context => context.Git.IsMainBranch || context.Git.IsReleaseBranch
        ),
        KnownPushTargets.NuGetOrg(isActive: context => context.Git.IsReleaseBranch)
    };

    public BuildContext(ICakeContext context) : base(context)
    { }
}
