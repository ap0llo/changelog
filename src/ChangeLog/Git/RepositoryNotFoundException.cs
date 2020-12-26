using System;

namespace Grynwald.ChangeLog.Git
{
    public class RepositoryNotFoundException : Exception
    {
        public RepositoryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
