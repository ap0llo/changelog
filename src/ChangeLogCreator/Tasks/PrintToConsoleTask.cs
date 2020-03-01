using System;
using System.Linq;
using ChangeLogCreator.Model;
using Crayon;

namespace ChangeLogCreator.Tasks
{
    public class PrintToConsoleTask : IChangeLogTask
    {
        public void Run(ChangeLog changeLog)
        {
            Output.Enable();

            foreach (var versionChangeLog in changeLog.OrderByDescending(x => x.Version.Version))
            {
                Console.WriteLine();
                Console.WriteLine(" " + $"Version {versionChangeLog.Version.Version}".Bold().Underline());
                Console.WriteLine();

    
                var features = versionChangeLog.FeatureEntries.ToArray();
                if (features.Length > 0)
                {
                    Console.WriteLine("\tNew Features:".Bold());
                    Console.WriteLine();
                    foreach (var entry in features)
                    {
                        Console.WriteLine($"\t - {entry.Summary} ({entry.Commit})");
                    }
                    Console.WriteLine();
                }

                var bugfixes = versionChangeLog.BugFixEntries.ToArray();
                if (bugfixes.Length > 0)
                {
                    Console.WriteLine("\tBug Fixes:".Bold());
                    Console.WriteLine();
                    foreach (var entry in bugfixes)
                    {
                        Console.WriteLine($"\t - {entry.Summary} ({entry.Commit})");
                    }
                    Console.WriteLine();
                }


                if (features.Length == 0 && bugfixes.Length == 0)
                {
                    Console.WriteLine($"\t No Changes found");
                    Console.WriteLine();
                }


            }
        }
    }
}
