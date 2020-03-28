using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Tasks
{
    internal abstract class SynchronousChangeLogTask : IChangeLogTask
    {
        public Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog)
        {
            var result = Run(changeLog);
            return Task.FromResult(result);
        }

        protected abstract ChangeLogTaskResult Run(ApplicationChangeLog changelog);
    }
}
