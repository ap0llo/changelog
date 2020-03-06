using System.Linq;
using ChangeLogCreator.Model;
using Xunit;

namespace ChangeLogCreator.Test.Model
{
    public class ChangeLogTest : TestBase
    {
        [Fact]
        public void ChangeLogs_are_returned_ordered_by_version()
        {
            // ARRANGE
            var item1 = GetSingleVersionChangeLog("1.0.1");
            var item2 = GetSingleVersionChangeLog("3.5.0");
            var item3 = GetSingleVersionChangeLog("2.0.7");

            var expectedOrdered = new[] { item2, item3, item1 };

            var sut = new ChangeLog() { item1, item2, item3 };

            // ACT
            var actualOrdered1 = sut.ChangeLogs.ToArray();
            var actualOrdered2 = sut.ToArray();

            // ASSERT
            Assert.Equal(expectedOrdered, actualOrdered1);
            Assert.Equal(expectedOrdered, actualOrdered2);
        }
    }
}
