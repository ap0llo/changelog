using System;
using System.Collections.Generic;
using System.Linq;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Xunit fixture that tests must use in order to modify environment variables.
    /// </summary>
    /// <remarks>
    /// To use this fixture, tests need to be part of the test collection defined by <see cref="EnvironmentVariableCollection"/>
    /// </remarks>
    /// <seealso cref="EnvironmentVariableCollection"/>
    public class EnvironmentVariableFixture : IDisposable
    {
        private class ResetEnvironmentVariable : IDisposable
        {
            private readonly string m_Variable;
            private readonly string? m_Value;

            public ResetEnvironmentVariable(string variable, string? value)
            {
                m_Variable = variable;
                m_Value = value;
            }

            public void Dispose()
            {
                Environment.SetEnvironmentVariable(m_Variable, m_Value, EnvironmentVariableTarget.Process);
            }
        }

        private readonly List<IDisposable> m_InitiallyClearedEnvironmentVariables = new List<IDisposable>();


        public EnvironmentVariableFixture()
        {
            // clear environment variables (might be set in environment the test is running in)
            var envVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            foreach (var key in envVars.Keys.Cast<string>().Where(x => x?.StartsWith("CHANGELOG__") == true))
            {
                m_InitiallyClearedEnvironmentVariables.Add(SetEnvironmentVariable(key!, null));
            }
        }

        /// <summary>
        /// Sets the specified environment variable for the current process
        /// </summary>
        /// <returns>
        /// Returns a disposable object that resets the variable's value back to it's previous value when it is disposed.
        /// </returns>
        public IDisposable SetEnvironmentVariable(string variable, string? value)
        {
            if (String.IsNullOrWhiteSpace(variable))
                throw new ArgumentException("Value must not be null or whitespace", nameof(variable));

            var previousValue = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Process);
            return new ResetEnvironmentVariable(variable, previousValue);
        }


        public void Dispose()
        {
            for (var i = m_InitiallyClearedEnvironmentVariables.Count - 1; i >= 0; i--)
            {
                m_InitiallyClearedEnvironmentVariables[i].Dispose();
            }
        }
    }
}
