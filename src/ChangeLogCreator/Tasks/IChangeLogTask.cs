using ChangeLogCreator.Model;

namespace ChangeLogCreator.Tasks
{
    public interface IChangeLogTask
    {
        void Run(ChangeLog changeLog);
    }
}
