# Configuration Sources

Settings are loaded from a number of *configuration sources*:

1. [Default settings](#default-settings)
2. [Configuration file](#configuration-file)
3. [Environment variables](#environment-variables)
4. [Commandline parameters](#commandline-parameters)

Configuration is loaded from the different sources in the above order.
When settings are loaded from a source, the configuration values defined in that location replace values from the previous sources. 


## Default settings

The default settings are embedded in the ChangeLog executable and apply, if no other source specifies a specific setting.

The default settings are defined in [`defaultSettings.json`](../../src/ChangeLog/Configuration/defaultSettings.json).

### Configuration file

You can customize settings by placing them in a configuration file.
The configuration file is a JSON file and uses the same schema as the [default settings](../../src/ChangeLog/Configuration/defaultSettings.json).

A JSON Schema for the configuration file can be found at 
<https://raw.githubusercontent.com/ap0llo/changelog/master/schemas/configuration/schema.json>.  
The schema provides auto-completion support when editing the configuration file in editors that support JSON Schema (e.g. Visual Studio Code).

The use of a configuration file is **optional**.

The path of the configuration file can be specified using the `configurationFilePath` commandline parameter (see [Commandline reference](../commandline-reference/index.md)).

If no configuration file path is specified, Changelog will look for a configuration file inside the git repository being processed.
Note that **only the first** configuration file that is found is loaded.

The configuration file can be placed inside the repository at either of these paths:

- `<REPOSITOTRY>/changelog.settings.json`
- `<REPOSITOTRY>/.config/changelog/settings.json`

#### Template

If you want to create a configuration file, you can use this template as a starting point:

```json
{
  "$schema": "https://raw.githubusercontent.com/ap0llo/changelog/master/schemas/configuration/schema.json",
  "changelog": {
    
  }
}
```

### Environment variables

Settings will also be loaded from environment variables.
This is especially useful for passing secrets (like GitHub access tokens)
when running in CI environments.

If a setting can be set through environment variables, the name of the variable is documented with the individual settings.
See [Settings list](../configuration.md#settings-list) for details.

### Commandline parameters

A subset of all settings can also be specified as commandline parameters for the `generate` command.
Commandline parameters are considered to be the "most specific" setting for an execution of ChangeLog and can override settings from all other sources.

The name of the commandline parameters are documented with the individual settings.
See [Settings list](../configuration.md#settings-list) for details.

## See Also

- [Configuration Overview](../configuration.md)
