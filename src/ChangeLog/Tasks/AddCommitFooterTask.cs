using System.Linq;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Tasks that adds a "Commit" footer with the commit's id to each change log entry
    /// </summary>
    internal sealed class AddCommitFooterTask : SynchronousChangeLogTask
    {
        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            var allEntries = changelog.ChangeLogs.SelectMany(x => x.AllEntries);

            foreach (var entry in allEntries)
            {
                var commitFooter = new ChangeLogEntryFooter(
                    new("Commit"),
                    new CommitReferenceTextElement(entry.Commit.ToString(abbreviate: true), entry.Commit)
                );
                entry.Add(commitFooter);
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
