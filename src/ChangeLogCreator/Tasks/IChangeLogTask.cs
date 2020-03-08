using System.Threading.Tasks;
using ChangeLogCreator.Model;

namespace ChangeLogCreator.Tasks
{
    public interface IChangeLogTask
    {
        Task RunAsync(ChangeLog changeLog);
    }
}
