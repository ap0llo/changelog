using Grynwald.ChangeLog.Logging;
using Grynwald.Utilities.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Grynwald.ChangeLog.Test.Logging
{
    /// <summary>
    /// Tests for <see cref="ContainerBuilderExtensions"/>
    /// </summary>
    public class ContainerBuilderExtensionsTest : ContainerTestBase
    {
        [Fact]
        public void RegisterLogging_registers_logger_infrastructure()
        {
            // ARRANGE / ACT
            using var container = BuildContainer(
                b => b.RegisterLogging(SimpleConsoleLoggerConfiguration.Default)
            );

            // ASSERT
            AutofacAssert.CanResolveType<ILoggerFactory>(container);
            AutofacAssert.CanResolveType<ILogger<object>>(container);
            AutofacAssert.CanResolveType<ILogger<string>>(container);
        }
    }
}
