using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Tests for <see cref="GitLabReference"/>
    /// </summary>
    public class GitLabReferenceTest : EqualityTest<GitLabReference, GitLabReferenceTest>, IEqualityTestDataProvider<GitLabReference>
    {
        public IEnumerable<(GitLabReference left, GitLabReference right)> GetEqualTestCases()
        {
            var project = new GitLabProjectInfo("example.com", "user", "project");

            yield return (
                new GitLabReference(project, GitLabReferenceType.Issue, 23),
                new GitLabReference(project, GitLabReferenceType.Issue, 23));
            yield return (
                new GitLabReference(project, GitLabReferenceType.MergeRequest, 23),
                new GitLabReference(project, GitLabReferenceType.MergeRequest, 23));
            yield return (
                new GitLabReference(project, GitLabReferenceType.Milestone, 23),
                new GitLabReference(project, GitLabReferenceType.Milestone, 23));
        }

        public IEnumerable<(GitLabReference left, GitLabReference right)> GetUnequalTestCases()
        {
            var project1 = new GitLabProjectInfo("example.com", "user", "project1");
            var project2 = new GitLabProjectInfo("example.com", "user", "project2");

            yield return (
                new GitLabReference(project1, GitLabReferenceType.Issue, 23),
                new GitLabReference(project2, GitLabReferenceType.Issue, 23));

            yield return (
                new GitLabReference(project1, GitLabReferenceType.Issue, 23),
                new GitLabReference(project1, GitLabReferenceType.MergeRequest, 23));

            yield return (
                new GitLabReference(project1, GitLabReferenceType.Issue, 23),
                new GitLabReference(project1, GitLabReferenceType.Issue, 42));
        }

        public class Constructor
        {

            [Fact]
            public void Project_must_not_be_null()
            {
                // ARRANGE

                // ACT 
                var ex = Record.Exception(() => new GitLabReference(null!, GitLabReferenceType.Issue, 23));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("project", argumentNullException.ParamName);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-23)]
            public void Id_must_not_be_zeo_or_negative(int id)
            {
                // ARRANGE
                var project = new GitLabProjectInfo("example.com", "user", "project");

                // ACT 
                var ex = Record.Exception(() => new GitLabReference(project, GitLabReferenceType.Issue, id));

                // ASSERT
                var argumentOfRangeException = Assert.IsType<ArgumentOutOfRangeException>(ex);
                Assert.Equal("id", argumentOfRangeException.ParamName);
            }

        }


        public class ToString_
        {

            [Theory]
            // User or Group Namespace
            [InlineData("user", "project", GitLabReferenceType.Issue, 23, "user/project#23")]
            [InlineData("user", "project", GitLabReferenceType.MergeRequest, 23, "user/project!23")]
            [InlineData("user", "project", GitLabReferenceType.Milestone, 23, "user/project%23")]
            // Subgroup namespace
            [InlineData("group/subgroup", "project", GitLabReferenceType.Issue, 23, "group/subgroup/project#23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.MergeRequest, 23, "group/subgroup/project!23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Milestone, 23, "group/subgroup/project%23")]
            // Reference must always be lower-case
            [InlineData("USER", "project", GitLabReferenceType.Issue, 23, "user/project#23")]
            [InlineData("user", "PROJECT", GitLabReferenceType.Issue, 23, "user/project#23")]
            [InlineData("GROUP/SUBGROUP", "project", GitLabReferenceType.Issue, 23, "group/subgroup/project#23")]
            public void ToString_returns_expected_value(string @namespace, string project, GitLabReferenceType type, int id, string expected)
            {
                // ARRANGE
                var sut = new GitLabReference(new("example.com", @namespace, project), type, id);

                // ACT 
                var actual = sut.ToString();

                // ASSERT
                Assert.Equal(expected, actual);
            }


            [Theory]
            // User or Group Namespace
            [InlineData("user", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Full, "user/project#23")]
            [InlineData("user", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.ProjectAndItem, "project#23")]
            [InlineData("user", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Item, "#23")]
            [InlineData("user", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.Full, "user/project!23")]
            [InlineData("user", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.ProjectAndItem, "project!23")]
            [InlineData("user", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.Item, "!23")]
            [InlineData("user", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.Full, "user/project%23")]
            [InlineData("user", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.ProjectAndItem, "project%23")]
            [InlineData("user", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.Item, "%23")]
            // Subgroup namespace
            [InlineData("group/subgroup", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Full, "group/subgroup/project#23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.ProjectAndItem, "project#23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Item, "#23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.Full, "group/subgroup/project!23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.ProjectAndItem, "project!23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.MergeRequest, 23, GitLabReferenceFormat.Item, "!23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.Full, "group/subgroup/project%23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.ProjectAndItem, "project%23")]
            [InlineData("group/subgroup", "project", GitLabReferenceType.Milestone, 23, GitLabReferenceFormat.Item, "%23")]
            // Reference must always be lower-case
            [InlineData("USER", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Full, "user/project#23")]
            [InlineData("user", "PROJECT", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.Full, "user/project#23")]
            [InlineData("USER", "project", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.ProjectAndItem, "project#23")]
            [InlineData("user", "PROJECT", GitLabReferenceType.Issue, 23, GitLabReferenceFormat.ProjectAndItem, "project#23")]
            public void ToString_returns_expected_value_when_format_is_specified(string @namespace, string project, GitLabReferenceType type, int id, GitLabReferenceFormat format, string expected)
            {
                // ARRANGE
                var sut = new GitLabReference(new("example.com", @namespace, project), type, id);

                // ACT 
                var actual = sut.ToString(format);

                // ASSERT
                Assert.Equal(expected, actual);
            }

        }

        public class TryParse
        {


            [Theory]
            // Reference within the same project
            [InlineData("#23", "currentNamespace", "currentProject", "currentNamespace", "currentProject", GitLabReferenceType.Issue, 23)]
            [InlineData("!23", "currentNamespace", "currentProject", "currentNamespace", "currentProject", GitLabReferenceType.MergeRequest, 23)]
            [InlineData("%23", "currentNamespace", "currentProject", "currentNamespace", "currentProject", GitLabReferenceType.Milestone, 23)]
            // Reference to another project in the same namespace
            [InlineData("anotherProject#23", "currentNamespace", "currentProject", "currentNamespace", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData("anotherProject!23", "currentNamespace", "currentProject", "currentNamespace", "anotherProject", GitLabReferenceType.MergeRequest, 23)]
            [InlineData("anotherProject%23", "currentNamespace", "currentProject", "currentNamespace", "anotherProject", GitLabReferenceType.Milestone, 23)]
            // Reference to a project in a different namespace
            [InlineData("anotherUser/anotherProject#23", "currentNamespace", "currentProject", "anotherUser", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData("anotherUser/anotherProject!23", "currentNamespace", "currentProject", "anotherUser", "anotherProject", GitLabReferenceType.MergeRequest, 23)]
            [InlineData("anotherUser/anotherProject%23", "currentNamespace", "currentProject", "anotherUser", "anotherProject", GitLabReferenceType.Milestone, 23)]
            [InlineData("group/subgroup/anotherProject#23", "currentNamespace", "currentProject", "group/subgroup", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData("group/subgroup/anotherProject!23", "currentNamespace", "currentProject", "group/subgroup", "anotherProject", GitLabReferenceType.MergeRequest, 23)]
            [InlineData("group/subgroup/anotherProject%23", "currentNamespace", "currentProject", "group/subgroup", "anotherProject", GitLabReferenceType.Milestone, 23)]
            // leading and trailing whitespace must be ignored
            [InlineData(" #23", "currentNamespace", "currentProject", "currentNamespace", "currentProject", GitLabReferenceType.Issue, 23)]
            [InlineData("#23 ", "currentNamespace", "currentProject", "currentNamespace", "currentProject", GitLabReferenceType.Issue, 23)]
            [InlineData(" anotherProject#23", "currentNamespace", "currentProject", "currentNamespace", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData("anotherProject#23 ", "currentNamespace", "currentProject", "currentNamespace", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData(" group/subgroup/anotherProject#23", "currentNamespace", "currentProject", "group/subgroup", "anotherProject", GitLabReferenceType.Issue, 23)]
            [InlineData("group/subgroup/anotherProject#23  ", "currentNamespace", "currentProject", "group/subgroup", "anotherProject", GitLabReferenceType.Issue, 23)]
            public void TryParse_returns_expected_reference(string input, string currentNamespace, string currentProject, string expectedNamespace, string expectedProject, GitLabReferenceType expectedType, int expectedId)
            {
                // ARRANGE

                // ACT 
                var success = GitLabReference.TryParse(input, new("example.com", currentNamespace, currentProject), out var parsed);

                // ASSERT
                Assert.True(success);
                Assert.Equal("example.com", parsed!.Project.Host);
                Assert.Equal(expectedNamespace, parsed.Project.Namespace);
                Assert.Equal(expectedProject, parsed.Project.Project);
                Assert.Equal(expectedType, parsed.Type);
                Assert.Equal(expectedId, parsed.Id);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("\t")]
            [InlineData("not-a-reference")]
            [InlineData("Not a/reference#0")]
            [InlineData("Not a/reference#xyz")]
            [InlineData("#xyz")]
            [InlineData("#1 2 3")]
            public void TryParse_fails_for_invalid_references(string input)
            {
                // ARRANGE
                var currentProject = new GitLabProjectInfo("example.com", "namespace", "project");

                // ACT 
                var success = GitLabReference.TryParse(input, currentProject, out var result);

                // ASSERT
                Assert.False(success);
                Assert.Null(result);
            }

            [Fact]
            public void TryParse_throws_ArgumentNullException_when_current_project_is_null()
            {
                // ARRANGE

                // ACT
                var ex = Record.Exception(() => GitLabReference.TryParse("#23", null!, out var result));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("currentProject", argumentNullException.ParamName);
            }
        }
    }
}
