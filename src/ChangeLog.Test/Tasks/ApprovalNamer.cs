using System;
using System.CodeDom;
using System.IO;
using ApprovalTests.Namers;

namespace Grynwald.ChangeLog.Test.Tasks
{
    internal class ApprovalNamer : UnitTestFrameworkNamer
    {
        private readonly string m_TypeName;

        public override string Subdirectory => Path.Combine(base.Subdirectory, "_referenceResults");

        public override string Name
        {
            get
            {
                // base name uses <TYPENAME>.<METHODNAME> format
                // to make the namer work with test cases implemented in base classes
                // replace the type name with the name passed to the constructor
                var baseName = base.Name; ;

                return $"{m_TypeName}.{baseName.Substring(baseName.IndexOf(".") + 1)}";
            }

        }


        public ApprovalNamer(string typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(typeName));

            m_TypeName = typeName;
        }
    }
}
