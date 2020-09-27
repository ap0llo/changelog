using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Filtering;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Test.Model;
using Xunit;

namespace Grynwald.ChangeLog.Test.Filtering
{
    /// <summary>
    /// Tests for <see cref="Filter"/>
    /// </summary>
    public class FilterTest : TestBase
    {
        public static IEnumerable<object[]> TestCases()
        {
            object[] TestCase(string id, ChangeLogEntry entry, IReadOnlyList<FilterExpression> include, IReadOnlyList<FilterExpression> exclude, bool expected)
            {
                return new object[]
                {
                    id,
                    new XunitSerializableChangeLogEntry(entry),
                    include.Select(XunitSerializableFilterExpression.Wrap).ToArray(),
                    exclude.Select(XunitSerializableFilterExpression.Wrap).ToArray(),
                    expected
                };
            }

            var testData = new TestDataFactory();

            // Base Test Case: Include and exclude nothing
            yield return TestCase(
                "T10",
                testData.GetChangeLogEntry(type: "feat", scope: null),
                include: Array.Empty<FilterExpression>(),
                exclude: Array.Empty<FilterExpression>(),
                expected: false);

            // Include everything, exclude nothing
            yield return TestCase(
                "T20",
                testData.GetChangeLogEntry(type: "feat", scope: null),
                include: new[] { new FilterExpression("*", "*") },
                exclude: Array.Empty<FilterExpression>(),
                expected: true);


            // include specific entries
            yield return TestCase(
                "T31",
                testData.GetChangeLogEntry(type: "feat", scope: null),
                include: new[] { new FilterExpression("feat", "*") },
                exclude: Array.Empty<FilterExpression>(),
                expected: true);
            yield return TestCase(
                "T32",
                testData.GetChangeLogEntry(type: "fix", scope: null),
                include: new[] { new FilterExpression("feat", "*") },
                exclude: Array.Empty<FilterExpression>(),
                expected: false);

            // multiple includes
            yield return TestCase(
                "T41",
                testData.GetChangeLogEntry(type: "feat", scope: null),
                include: new[]
                {
                    new FilterExpression("feat", "*"),
                    new FilterExpression("fix", "*")
                },
                exclude: Array.Empty<FilterExpression>(),
                expected: true);
            yield return TestCase(
                "T42",
                testData.GetChangeLogEntry(type: "fix", scope: null),
                include: new[]
                {
                    new FilterExpression("feat", "*"),
                    new FilterExpression("fix", "*")
                },
                exclude: Array.Empty<FilterExpression>(),
                expected: true);


            // include everything, then exclude some entries
            yield return TestCase(
                "T51",
                testData.GetChangeLogEntry(type: "feat", scope: null),
                include: new[]
                {
                    new FilterExpression("*", "*"),
                },
                exclude: new[]
                {
                    new FilterExpression("fix", "*")
                },
                expected: true);
            yield return TestCase(
                "T52",
                testData.GetChangeLogEntry(type: "fix", scope: null),
                include: new[]
                {
                    new FilterExpression("*", "*"),
                },
                exclude: new[]
                {
                    new FilterExpression("fix", "*")
                },
                expected: false);

            // multiple include and exclude expression
            {
                var include = new[]
                {
                    new FilterExpression("feat", "*"),
                    new FilterExpression("fix", "*"),
                };
                var exclude = new[]
                {
                    new FilterExpression("fix", "ignored-scope"),
                    new FilterExpression("feat", "ignored-scope")
                };


                yield return TestCase(
                    "T61",
                    testData.GetChangeLogEntry(type: "feat", scope: null),
                    include: include,
                    exclude: exclude,
                    expected: true);

                yield return TestCase(
                    "T62",
                    testData.GetChangeLogEntry(type: "fix", scope: null),
                    include: include,
                    exclude: exclude,
                    expected: true);

                yield return TestCase(
                    "T63",
                    testData.GetChangeLogEntry(type: "feat", scope: "some-scope"),
                    include: include,
                    exclude: exclude,
                    expected: true);

                yield return TestCase(
                    "T64",
                    testData.GetChangeLogEntry(type: "fix", scope: "some-scope"),
                    include: include,
                    exclude: exclude,
                    expected: true);

                yield return TestCase(
                    "T65",
                    testData.GetChangeLogEntry(type: "feat", scope: "ignored-scope"),
                    include: include,
                    exclude: exclude,
                    expected: false);

                yield return TestCase(
                    "T66",
                    testData.GetChangeLogEntry(type: "fix", scope: "ignored-scope"),
                    include: include,
                    exclude: exclude,
                    expected: false);
            }
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void IsIncluded_returns_expected_result(string id, XunitSerializableChangeLogEntry entry, IReadOnlyList<XunitSerializableFilterExpression> include, IReadOnlyList<XunitSerializableFilterExpression> exclude, bool expected)
        {
            // ARRANGE
            _ = id;

            var sut = new Filter(include.Select(x => x.Value).ToArray(), exclude.Select(x => x.Value).ToArray());

            // ACT 
            var actual = sut.IsIncluded(entry.Value);

            // ASSERT
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void IncludeExpressions_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new Filter(null!, Array.Empty<FilterExpression>()));
        }

        [Fact]
        public void ExcludeExpressions_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new Filter(Array.Empty<FilterExpression>(), null!));
        }

        [Fact]
        public void IsIncluded_throws_ArgumentNullException_if_entry_is_null()
        {
            // ARRANGE
            var sut = new Filter(Array.Empty<FilterExpression>(), Array.Empty<FilterExpression>());

            // ACT / ASSERT
            Assert.Throws<ArgumentNullException>(() => sut.IsIncluded(null!));
        }

    }
}
