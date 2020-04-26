using System;
using System.Runtime.Serialization;

namespace Grynwald.ChangeLog.Templates
{
    [Serializable]
    internal class TemplateExecutionException : Exception
    {
        public TemplateExecutionException(string message) : base(message)
        { }
    }
}
