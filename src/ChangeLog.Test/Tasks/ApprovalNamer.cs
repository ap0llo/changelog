using System.IO;
using ApprovalTests.Namers;

namespace Grynwald.ChangeLog.Test.Tasks
{
    internal class ApprovalNamer : UnitTestFrameworkNamer
    {
        public override string Subdirectory => Path.Combine(base.Subdirectory, "_referenceResults");
    }
}
