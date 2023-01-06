using System;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestRepositoryContent : RepositoryContent
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestRepositoryContent(string htmlUrl, string? content)
        {
            HtmlUrl = htmlUrl;

            var encodedContent = content is null
                ? null
                : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)); ;

            // No really a good idea but probably okay for test code
            typeof(RepositoryContent)
                .GetProperty(nameof(EncodedContent))!
                .SetMethod!
                .Invoke(this, new[] { encodedContent });
        }
    }
}
