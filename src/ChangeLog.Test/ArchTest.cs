using System;
using System.Linq;
using NetArchTest.Rules;
using Xunit;
using Xunit.Sdk;

namespace Grynwald.ChangeLog
{
    public class ArchTest
    {
        [Fact]
        public void Implementations_of_IEquatable_are_sealed()
        {
            var result = Types.InAssembly(typeof(Program).Assembly)
                .That().ImplementInterface(typeof(IEquatable<>))
                .And().AreNotAbstract()
                .Should().BeSealed()
                .GetResult();

            if (!result.IsSuccessful)
            {
                throw new XunitException(
                    "The following types should be sealed: \r\n" +
                    String.Join("\r\n", result.FailingTypeNames.Select(x => $" - {x}")));
            }
        }
    }
}
