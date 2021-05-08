using System;

namespace Grynwald.ChangeLog.Templates
{
    [Serializable]
    internal class TemplateExecutionException : Exception
    {
        public TemplateExecutionException(string message) : base(message)
        { }

        public TemplateExecutionException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
