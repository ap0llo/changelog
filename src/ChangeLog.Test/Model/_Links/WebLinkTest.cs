using System;
using Grynwald.ChangeLog.Model;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    public class WebLinkTest
    {
        [Fact]
        public void Uri_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new WebLink(null!));

            // ASSERT
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }
    }
}
