using System.Linq;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Filtering
{
    internal static class ConfigurationExtensions
    {
        /// <summary>
        /// Initializes a new <see cref="Filter"/> from the specified configuration object.
        /// </summary>
        public static Filter ToFilter(this ChangeLogConfiguration.FilterConfiguration configuration)
        {
            var includeExpressions = configuration.Include.Select(x => x.ToFilterExpression()).ToArray();
            var excludeExpressions = configuration.Exclude.Select(x => x.ToFilterExpression()).ToArray();

            return new Filter(includeExpressions, excludeExpressions);

        }

        /// <summary>
        /// Initializes a new <see cref="FilterExpression"/> from the specified configuration object.
        /// </summary>
        public static FilterExpression ToFilterExpression(this ChangeLogConfiguration.FilterExpressionConfiguration configuration)
        {
            return new FilterExpression(configuration.Type, configuration.Scope);
        }
    }
}
