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
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Text_must_not_be_null_or_whitespace(string? text)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new WebLinkTextElement(text!, new Uri("https://example.com")));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
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
