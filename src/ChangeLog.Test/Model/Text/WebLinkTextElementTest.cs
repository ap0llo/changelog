using System;
using Grynwald.ChangeLog.Model.Text;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="WebLinkTextElement"/>
    /// </summary>
    public class WebLinkTextElementTest
    {
        [Fact]
        public void Text_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new WebLinkTextElement(null!, new Uri("https://example.com")));

            // ASSERT
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void Uri_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new WebLinkTextElement("Text", null!));

            // ASSERT
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }
    }
}
