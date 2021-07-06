using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Pipeline
{
    public interface IChangeLogTask
    {
        Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog);
    }
}
