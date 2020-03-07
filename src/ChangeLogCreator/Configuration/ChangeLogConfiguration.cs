using System;

namespace ChangeLogCreator.Configuration
{
    public class ChangeLogConfiguration
    {
        public class ScopeConfiguration
        {
            public string? Name { get; set; }

            public string? DisplayName { get; set; }
        }


        public ScopeConfiguration[] Scopes { get; set; } = Array.Empty<ScopeConfiguration>();

    }
}

