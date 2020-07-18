using System;
using System.Collections.Generic;
using Xunit;

namespace Grynwald.ChangeLog.Test
{
    public interface IEqualityTestDataProvider<TTestee>
    {
        IEnumerable<(TTestee left, TTestee right)> GetEqualTestCases();

        IEnumerable<(TTestee left, TTestee right)> GetUnequalTestCases();
    }

    /// <summary>
    /// Base class for tests of <see cref="IEquatable{T}"/> implementations.
    /// </summary>
    public abstract class EqualityTest<TTestee, TDataProvider>
        where TTestee : IEquatable<TTestee>
        where TDataProvider : IEqualityTestDataProvider<TTestee>, new()
    {
        public static IEnumerable<object[]> EqualityTestCases()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, right) in dataProvider.GetEqualTestCases())
            {
                yield return new object[] { left, right };
            }
        }

        public static IEnumerable<object[]> InequalityTestCases()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, right) in dataProvider.GetUnequalTestCases())
            {
                yield return new object[] { left, right };
            }
        }

        public static IEnumerable<object[]> SampleInstance()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, _) in dataProvider.GetUnequalTestCases())
            {
                yield return new object[] { left };
                yield break;
            }

        }


        [Theory]
        [MemberData(nameof(EqualityTestCases))]
        public void Comparision_of_two_equal_instances_yield_expected_result(TTestee left, TTestee right)
        {
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.Equal(left, right);
            Assert.True(left.Equals(right));
            Assert.True(left.Equals((object)right));
            Assert.True(right.Equals(left));
            Assert.True(right.Equals((object)left));
        }

        [Theory]
        [MemberData(nameof(InequalityTestCases))]
        public void Comparision_of_two_unequal_instances_yield_expected_result(TTestee left, TTestee right)
        {
            Assert.NotEqual(left, right);
            Assert.False(left.Equals(right));
            Assert.False(left.Equals((object)right));
            Assert.False(right.Equals(left));
            Assert.False(right.Equals((object)left));
        }

        [Theory]
        [MemberData(nameof(SampleInstance))]
        public void Equals_retuns_false_if_argument_is_of_a_different_type(TTestee sut)
        {
            Assert.False(sut.Equals(new object()));
        }
    }
}
