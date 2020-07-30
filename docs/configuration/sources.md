# Configuration Sources

Settings are loaded from a number of *setting sources*:

1. [Default settings](#default-settings)
2. [Configuration file](#configuration-file)
3. [Environment variables](#environment-variables)
4. [Commandline parameters](#commandline-parameters)

Settings are loaded in the above order and sources loaded later can override values for sources loaded before.

## Default settings

The default settings are embedded in the ChangeLog executable and apply, if no other source specifies a specific setting.

The default settings are defined in [`defaultSettings.json`](../../src/ChangeLog/Configuration/defaultSettings.json).

### Configuration file

You can customize settings by placing them in a configuration file.
The configuration file is a JSON file and uses the same schema as [`defaultSettings.json`](../../src/ChangeLog/Configuration/defaultSettings.json).
A JSON Schema for the configuration file can be found at <https://raw.githubusercontent.com/ap0llo/changelog/master/schemas/configuration/settings/schema.json>.
The schema provides auto-completion support when editing the configuration file in editors that support JSON Schema (e.g. Visual Studio Code).

The use of a configuration file is **optional**.
By default, ChangeLog will attempt to load settings from a file called `changelog.settings.json` in the root of the git repository a change log is being generated for.
Alternatively, you can specify the path of the configuration file using the `configurationFilePath` commandline parameter (see [Commandline reference](../commandline-reference/index.md)).

### Environment variables

Settings will also be loaded from environment variables.
This is especially useful for passing secrets (like GitHub access tokens)
when running in CI environments.

The environment variables that will be loaded are listed below.

### Commandline parameters

Some settings can also be overridden using commandline parameter.
Commandline parameters are considered to be the "most specific" setting
for an execution of ChangeLog and can override settings from all other
sources.

Which commandline parameters override which setting is listed below.

## See Also

- [Configuration Overview](../configuration.md)
