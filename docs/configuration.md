# Configuration

The behaviour of ChangeLog can be customized by changing the configuration.
All settings are optional and ChangeLog aims to use sensible default values
for all of them.

## Configuration sources

Settings are loaded from a number of *setting sources*:

1. Default settings
2. Configuration file
3. Environment variables
4. Commandline parameters

Settings are loaded in the above order and sources loaded later can override
values for sources loaded before.

### Default settings

The default settings are embedded in the ChangeLog executable and apply,
if no other source specifies a specific setting.

The default settings are defined in [`defaultSettings.json`](../src/ChangeLog/Configuration/defaultSettings.json).

### Configuration file

You can customize settings by placing them in a configuration file.
The configuration file is a JSON file and uses the same schema as
[`defaultSettings.json`](../src/ChangeLog/Configuration/defaultSettings.json),

The use of a configuration file is **optional**.
By default, ChangeLog will attempt to load settings from a file called
`changelog.settings.json` in the root of the git repository a changelog
is being generated for.
Alternatively, you can specify the path of the configuration file using the
`configurationFilePath` commandline parameter
(see [Commandline reference](./commandline-reference/index.md)).

### Environment variables

Settings will also be loaded from environment variables.
This is especially useful for passing secrets (like GitHub access tokens)
when running in CI environments.

The environment variables that will be loaded are listed in the
table below.

### Commandline parameters

Some settings can alos be overridden using commandline parameter.
Commandline parameters are considered to be the "most specific" setting
for an execution of ChangeLog and can override settings from all other
sources.

Which commandline parameters override which setting is listed
in the table below.

## Settings

Setting names in the following table are separated by `:` which denote keys
and sub-keys the JSON configuration file. E.g. a setting the value of
`key:subkey` to `value` would need to be specified in the configuration file
like this:

```json
{
    "key" : {
        "subkey" : "value"
    }
}
```

### Available settings

- [Scopes](#scopes)
- [Footer](#footers)
- [Markdown Preset](#markdown-preset)
- [Tag Patterns](#tag-patterns)
- [Output Path](#output-path)
- [Integration Provider](#integration-provider)
- [GitHub Access Token](#github-access-token)
- [GitLab Access Token](#gitlab-access-token)
- [Version Range](#version-range)
- [Current Version](#current-version)

### Scopes

### Footers

### Markdown Preset

### Tag Patterns

### Output Path

### Integration Provider

### GitHub Access Token

### GitLab Access Token

### Version Range

### Current Version

## See Also

- [Commandline reference](./commandline-reference/index.md)
- [`defaultSettings.json`](../src/ChangeLog/Configuration/defaultSettings.json)
