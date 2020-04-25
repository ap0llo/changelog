using System;

namespace Grynwald.ChangeLog.Templates
{
    [Serializable]
    internal class InvalidTemplateConfigurationException : Exception
    {
        public InvalidTemplateConfigurationException(string message) : base(message)
        { }
    }
}
