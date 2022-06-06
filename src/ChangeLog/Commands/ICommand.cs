using System.Threading.Tasks;

namespace Grynwald.ChangeLog.Commands
{
    internal interface ICommand
    {
        Task<int> RunAsync();
    }
}
