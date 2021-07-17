using Xunit;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Test collection of all tests that read or modify environment variables.
    /// </summary>
    /// <remarks>
    /// Because a test that modifies environment variables might influence other tests that interact with environment variables,
    /// these tests must not run in parallel.
    /// To avoid parallel execution, they are all added to this test collection which xunit will not run in parallel.
    /// <para>
    /// All modifications to environment variables should be performed through <see cref="EnvironmentVariableFixture"/>.
    /// </para>
    /// </remarks>
    /// <seealso href="https://xunit.net/docs/running-tests-in-parallel">Running Tests in Parallel (xunit Documentation)</seealso>
    /// <seealso href="https://xunit.net/docs/shared-context">Shared Context between Tests (xunit Documentation)</seealso>
    [CollectionDefinition(nameof(EnvironmentVariableCollection))]
    public class EnvironmentVariableCollection : ICollectionFixture<EnvironmentVariableFixture>
    {
    }
}
