using Cake.Common;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Grynwald.SharedBuild;

namespace Build
{
    [TaskName("FetchGitNotes")]
    public class FetchGitNotesTask : FrostingTask<IBuildContext>
    {
        public override void Run(IBuildContext context)
        {
            var fileName = "git";
            var args = "fetch origin \"refs/notes/*:refs/notes/*\"";

            context.Log.Information("Fetching git notes from remote 'origin'");
            var exitCode = context.StartProcess(fileName, args);

            if (exitCode != 0)
                throw new CakeException($"Command '{fileName} {args}' failed with exist code {exitCode}");
        }
    }
}
