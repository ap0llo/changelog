using System.IO;
using ApprovalTests.Namers;

namespace ChangeLogCreator.Test.Tasks
{
    internal class ApprovalNamer : UnitTestFrameworkNamer
    {
        public override string Subdirectory => Path.Combine(base.Subdirectory, "_referenceResults");
    }
}
