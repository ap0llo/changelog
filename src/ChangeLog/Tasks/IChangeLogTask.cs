using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Tasks
{
    public interface IChangeLogTask
    {
        Task RunAsync(ApplicationChangeLog changeLog);
    }
}
