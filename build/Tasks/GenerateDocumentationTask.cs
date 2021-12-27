using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Run;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Grynwald.SharedBuild.Tasks;

namespace Build
{
    [TaskName("GenerateDocumentation")]
    [TaskDescription("Updates auto-generated documentation files")]
    [Dependency(typeof(BuildTask))]
    [IsDependeeOf(typeof(GenerateTask))]
    public class GenerateDocumentationTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DotNetCoreRun(
                "./utilities/docs/docs.csproj",
                new ProcessArgumentBuilder()
                    .Append("generate")
                    .Append("./docs"),
                new DotNetCoreRunSettings()
                {
                    Configuration = context.BuildSettings.Configuration,
                    NoBuild = true,
                    NoRestore = true,
                }
            );

        }
    }
}
