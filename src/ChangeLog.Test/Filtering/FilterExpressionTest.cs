using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Filtering;
using Xunit;

namespace Grynwald.ChangeLog.Test.Filtering
{
    /// <summary>
    /// Tests for <see cref="FilterExpression"/>
    /// </summary>
    public class FilterExpressionTest : TestBase
    {
        public static IEnumerable<object?[]> TestCases()
        {
            object?[] TestCase(string id, string? filterType = "*", string? filterScope = "*", string? entryType = "sometype", string? entryScope = null, bool expectedResult = true)
            {
                return new object?[] { id, filterType, filterScope, entryType, entryScope, expectedResult };
            }

            //
            // Match entry type
            //

            // "" would match entries without a type (which is not a valid entry and hence no entry is matched)
            yield return TestCase("T01", filterType: "", entryType: "feat", expectedResult: false);

            // "*" matches any type
            yield return TestCase("T02", filterType: "*", entryType: "feat", expectedResult: true);
            yield return TestCase("T03", filterType: "*", entryType: "fix", expectedResult: true);
            yield return TestCase("T04", filterType: "*", entryType: "some-type", expectedResult: true);

            // wildcard types matches entries with types matching the wildcard (case-insensitive)
            yield return TestCase("T05", filterType: "f*", entryType: "feat", expectedResult: true);
            yield return TestCase("T06", filterType: "f*", entryType: "fix", expectedResult: true);
            yield return TestCase("T07", filterType: "f*", entryType: "docs", expectedResult: false);

            // non-wildcard scope filters match the scope exactly (but are case-insensitive)
            yield return TestCase("T08", filterType: "feat", entryType: "feat", expectedResult: true);
            yield return TestCase("T09", filterType: "fix", entryType: "fix", expectedResult: true);
            yield return TestCase("T10", filterType: "feat", entryType: "fix", expectedResult: false);
            yield return TestCase("T11", filterType: "fix", entryType: "feat", expectedResult: false);

            //
            // Match scope
            //

            // "" matches only entries without scope
            yield return TestCase("T12", filterScope: "", entryScope: "", expectedResult: true);
            yield return TestCase("T13", filterScope: "", entryScope: "some-scope", expectedResult: false);

            // "*" matches entries with any scope (including entries without scope)
            yield return TestCase("T14", filterScope: "*", entryScope: null, expectedResult: true);
            yield return TestCase("T15", filterScope: "*", entryScope: "", expectedResult: true);
            yield return TestCase("T16", filterScope: "*", entryScope: "some-scope", expectedResult: true);

            // wildcard scopes matches entries with scopes matching the wildcard
            yield return TestCase("T17", filterScope: "some*", entryScope: "some-scope", expectedResult: true);
            yield return TestCase("T18", filterScope: "some*", entryScope: "SOME-SCOPE", expectedResult: true);
            yield return TestCase("T19", filterScope: "some*", entryScope: "some-other-scope", expectedResult: true);
            yield return TestCase("T20", filterScope: "some*", entryScope: "non-matching-scope", expectedResult: false);

            // non-wildcard scope filters match the scope exactly (but are case-insensitive)
            yield return TestCase("T21", filterScope: "some-scope", entryScope: "some-scope", expectedResult: true);
            yield return TestCase("T22", filterScope: "some-scope", entryScope: "SOME-scope", expectedResult: true);
            yield return TestCase("T23", filterScope: "some-scope", entryScope: "some-other-scope", expectedResult: false);

            //
            // Match type and scope: Entry is only matched if both type and scope match
            //
            yield return TestCase("T24", filterType: "*", filterScope: "*", entryType: "feat", entryScope: "some-scope", expectedResult: true);
            yield return TestCase("T24", filterType: "feat", filterScope: "some-scope", entryType: "feat", entryScope: "some-scope", expectedResult: true);
            yield return TestCase("T24", filterType: "feat", filterScope: "some-scope", entryType: "fix", entryScope: "some-scope", expectedResult: false);
            yield return TestCase("T24", filterType: "feat", filterScope: "some-scope", entryType: "feat", entryScope: "some-other-scope", expectedResult: false);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void IsMatch_returns_expected_result(string id, string filterType, string filterScope, string entryType, string entryScope, bool expected)
        {
            _ = id;

            // ARRANGE
            var sut = new FilterExpression(type: filterType, scope: filterScope);
            var entry = GetChangeLogEntry(type: entryType, scope: entryScope);

            // ACT 
            var actual = sut.IsMatch(entry);

            // ASSERT
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Type_must_not_be_null_or_whitespace(string type)
        {
            Assert.Throws<ArgumentException>(() => new FilterExpression(type, "*"));
        }

        [Theory]
        [InlineData("")]
        public void Type_may_be_an_empty_string(string type)
        {
            _ = new FilterExpression(type, "*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Scope_must_not_be_null_or_whitespace(string scope)
        {
            Assert.Throws<ArgumentException>(() => new FilterExpression("*", scope));
        }

        [Theory]
        [InlineData("")]
        public void Scope_may_be_an_empty_string(string scope)
        {
            _ = new FilterExpression("*", scope);
        }


        [Fact]
        public void IsMatch_throws_ArgumentNullException_if_entry_is_null()
        {
            // ARRANGE
            var sut = new FilterExpression("*", "*");

            // ACT / ASSERT
            Assert.Throws<ArgumentNullException>(() => sut.IsMatch(null!));
        }
    }
}
