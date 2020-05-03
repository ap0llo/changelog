using Grynwald.ChangeLog.Configuration;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Grynwald.ChangeLog.Test.Configuration
{
    public class ConfigurationValidatorTest
    {
        private readonly ILogger<ConfigurationValidator> m_Logger = NullLogger<ConfigurationValidator>.Instance;


        [Fact]
        public void No_errors_are_found_in_default_configuration()
        {
            // ARRANGE
            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(ChangeLogConfigurationLoader.GetDefaultConfiguration());

            // ASSERT
            Assert.True(valid);
        }



    }
}
