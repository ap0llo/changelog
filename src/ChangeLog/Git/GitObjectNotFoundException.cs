using System;

namespace Grynwald.ChangeLog.Git
{
    public class GitObjectNotFoundException : Exception
    {
        public GitObjectNotFoundException(string? message) : base(message)
        { }
    }
}
