# Integrations

While a change log can be generated from any git repository, *Integrations* provide extended features for repositories hosted by any of the supported providers.

Currently supported providers are:

- [GitHub](./integrations/github.md)
- [GitLab](./integrations/gitlab.md)

## Configuring integrations

By default, no integrations are enabled. To specify the integration provider,
use the using the `changelog:integrations:provider` setting:

```json
{
    "changelog": {
        "integrations": {
            "provider": "GitHub"
        }
    }
}
```

Allowed values are:

- `none` (default)
- `GitHub`
- `GitLab`

## See Also

- [Configuration Overview](./configuration.md)
- [GitHub Integration](./integrations/github.md)
- [GitLab Integration](./integrations/gitlab.md)