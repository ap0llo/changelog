using System;
using System.Linq;
using ChangeLogCreator.Model;
using Crayon;

namespace ChangeLogCreator.Tasks
{
    internal sealed class PrintToConsoleTask : IChangeLogTask
    {
        public void Run(ChangeLog changeLog)
        {
            Output.Enable();

            //TODO: Breaking changes

            foreach (var versionChangeLog in changeLog)
            {
                Console.WriteLine();
                Console.WriteLine(" " + $"Version {versionChangeLog.Version.Version.ToNormalizedString()}".Bold().Underline());
                Console.WriteLine();


                var features = versionChangeLog.FeatureEntries.ToArray();
                if (features.Length > 0)
                {
                    Console.WriteLine("\tNew Features:".Bold());
                    Console.WriteLine();
                    foreach (var entry in features)
                    {
                        PrintEntry(entry);
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
                        PrintEntry(entry);
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


        private void PrintEntry(ChangeLogEntry entry)
        {
            if (!String.IsNullOrEmpty(entry.Scope))
            {
                Console.WriteLine($"\t - {entry.Scope.Bold()}: {entry.Summary} ({entry.Commit})");
            }
            else
            {
                Console.WriteLine($"\t - {entry.Summary} ({entry.Commit})");
            }
        }
    }
}
