using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Filtering
{
    /// <summary>
    /// Implements a filter consisting of multiple "include" and "exclude" expressions.
    /// </summary>
    /// <remarks>
    /// The filter applies the specified expressions in two steps:
    /// <list type="number">
    ///     <item><description>
    ///         First, the 'include' expressions are applied.
    ///         The filter 'includes' an entry  if *any* include expression matches the entry.
    ///     </description></item>
    ///     <item><description>
    ///         Secondly, the 'exclude' expressions are applied to the entry if it was matched by the include expressions.
    ///         If any of the exclude expression matches the change log entry, the entry is not 'included' by the filter.
    ///     </description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="FilterExpression"/>
    internal sealed class Filter
    {
        private readonly IReadOnlyList<FilterExpression> m_IncludeExpressions;
        private readonly IReadOnlyList<FilterExpression> m_ExcludeExpressions;


        /// <summary>
        /// Initializes a new instance of <see cref="Filter"/>
        /// </summary>
        /// <param name="includeExpressions">The 'include' expressions to use.</param>
        /// <param name="excludeExpressions">The 'exclude' expressions to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="includeExpressions"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="excludeExpressions"/> is <c>null</c>.</exception>
        public Filter(IReadOnlyList<FilterExpression> includeExpressions, IReadOnlyList<FilterExpression> excludeExpressions)
        {
            m_IncludeExpressions = includeExpressions ?? throw new ArgumentNullException(nameof(includeExpressions));
            m_ExcludeExpressions = excludeExpressions ?? throw new ArgumentNullException(nameof(excludeExpressions));
        }

        /// <summary>
        /// Determines whether the specified entry should be included in the output.
        /// </summary>
        /// <remarks>
        /// A entry is included if it is matched by *any* of the 'include' expressions and *not* by any of the 'exclude' expressions.
        /// </remarks>
        /// <param name="entry">The entry to evaluate the filter for.</param>
        /// <returns>Returns <c>true</c> if the entry was matched by the filter, otherwise returns <c>false</c></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is <c>null</c></exception>
        public bool IsIncluded(ChangeLogEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            return m_IncludeExpressions.Any(expr => expr.IsMatch(entry)) &&
                   !m_ExcludeExpressions.Any(expr => expr.IsMatch(entry));
        }
    }
}
