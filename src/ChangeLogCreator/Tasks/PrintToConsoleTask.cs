using System;
using System.Linq;
using ChangeLogCreator.Model;

namespace ChangeLogCreator.Tasks
{
    public class PrintToConsoleTask : IChangeLogTask
    {
        public void Run(ChangeLog changeLog)
        {
            foreach (var versionChangeLog in changeLog.OrderByDescending(x => x.Version.Version))
            {
                Console.WriteLine();
                Console.WriteLine($"{versionChangeLog.Version.Version} ({versionChangeLog.Version.Commit})");
                foreach (var entry in versionChangeLog.OrderBy(x => x.Type).ThenBy(x => x.Type))
                {
                    Console.WriteLine($"\t{entry.Commit}: {entry.Type}: {entry.Summary}");
                }
            }
        }
    }
}
