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
`changelog.settings.json` in the root of the git repository a change log
is being generated for.
Alternatively, you can specify the path of the configuration file using the
`configurationFilePath` commandline parameter
(see [Commandline reference](./commandline-reference/index.md)).

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

### Overview

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
- [Template Name](#template-name)
- [Markdown Preset (Default Template)](#markdown-preset-default-template)
- [Entry types](#entry-types)

### Scopes

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:scopes</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><em>Empty</em></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *Scopes* setting configures how
[Conventional Commit](https://www.conventionalcommits.org/) scopes are display.
By default, the scopes are included unchanged in the output.
Using this setting, you can configure a scope's display name.
When a display name is configured, it will be used in the output instead
of the scope's name.

#### Example

```json
{
    "changelog" : {
        "scopes" : [
            { "name":  "myScope", "displayName":  "Scope Display Name" }
        ]
    }
}
```

### Footers

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:footers</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><em>Empty</em></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *Footers* setting configures how
[Conventional Commit](https://www.conventionalcommits.org/) footers are displayed.
By default, footers are included unchanged in the output.
Using this setting, you can configure a footer's display name.
When a display name is configured, it will be used in the output instead
of the footer's name.

#### Example

```json
{
    "changelog" : {
        "footers" : [
            { "name":  "see-also", "displayName":  "See Also" }
        ]
    }
}
```

### Markdown Preset

⚠️ **The Markdown Preset setting was removed in version 0.2.16:** With the
introduction of templates, output settings have become template-specific.
To set the preset for the default template use the
[Markdown Preset (Default Template)](#markdown-preset-default-template) setting.

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:markdown:preset</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__MARKDOWN__PRESET</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>default</code></td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>default</code></li>
                <li><code>MkDocs</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td> 0.1, Removed in version 0.2</td>
    </tr>
</table>

The *Markdown Preset* setting can be used to customize the serialization
of Markdown.

Supported values are:

- `default`: Produces Markdown that should work in most environments, including
  GitHub and GitLab
- `MkDocs`: Produces Markdown optimized for being rendered by Python-Markdown
  and [MkDocs](https://www.mkdocs.org/)

For details on the differences between the presets, see also
[Markdown Generator docs](https://github.com/ap0llo/markdown-generator/blob/master/docs/apireference/Grynwald/MarkdownGenerator/MdSerializationOptions/Presets/index.md)

#### Example

```json
{
    "changelog" : {
        "markdown" : {
            "preset" : "MkDocs"
        }
    }
}
```

### Tag Patterns

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:tagpatterns</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>[ "^(?&lt;version&gt;\d+\.\d+\.\d+.*)", "^v(?&lt;version&gt;\d+\.\d+\.\d+.*)" ]</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *Tag Patterns* setting controls how versions are read from a git
repository's tags. The setting defines a list of regular expressions that
are used go extract the version from the tag name.
All regular expressions must define a `version` sub-expression which matches
the version. The matched value must be a valid
[semantic version](https://semver.org/).

The default setting matches tag names that are semantic versions or
tags names that are semantic versions prefixed with `v`.

### Output Path

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:outputPath</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__OUTPUTPATH</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>outputPath</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>changelog.md</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

Specifies the output path for the generated change log.

When the value of the setting is a relative path, the path is interpreted
to be relative to

- The repository root directory when setting the output path in the
  configuration file or through environment variables
- The current working directory when specifying the output path using
  the `outputPath` commandline parameter

#### Example

```json
{
    "changelog" : {
        "outputPath" : "my-custom-output-path.md"
    }
}
```

### Integration Provider

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:provider</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__PROVIDER</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>none</code></td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>none</code></li>
                <li><code>GitHub</code></li>
                <li><code>GitLab</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

Sets the *Integration Provider* to use.
For details on see [Integrations](./integrations.md).

#### Example

Enable the GitHub integration provider:

```json
{
    "changelog" : {
        "integrations" :{
            "provider" : "github"
        }
    }
}
```

### GitHub Access Token

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:github:accessToken</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITHUB__ACCESSTOKEN</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>githubAccessToken</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *GitHub Access Token* setting configures the access token to use for
accessing the GitHub API when the GitHub integration is enabled.

**❌ While it is possible to set the access token in the configuration file**
**you should use the command line parameter or environment variable options**
**instead.**

### GitLab Access Token

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:gitlab:accessToken</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITLAB__ACCESSTOKEN</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>gitlabAccessToken</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *GitLab Access Token* setting configures the access token to use for
accessing the GitLab  API when the GitLab integration is enabled.

**❌ While it is possible to set the access token in the configuration file**
**you should use the command line parameter or environment variable options**
**instead.**

### Version Range

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:versionRange</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__VERSIONRANGE</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>versionRange</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

By default, **all versions** are included in the change log.
To limit the versions to include, you can specify a version range using
this setting.

The value must be a valid [NuGet Version Range](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges)

#### Example

To only include versions newer than version `2.1` in the change log, use the
following version range:

```json
{
    "changelog" : {
        "versionRange" : "[2.1, )"
    }
}
```

### Current Version

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:currentVersion</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__CURRENTVERSION</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>currentVersion</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

By default, versions are only read from a git repository's tags and only
tagged versions are included in the change log. To include the currently checked
out commit (the repository HEAD), you can specify the current version.
When specified, the current version is included in the change log as well.

The value must be a valid semantic version.

### Template Name

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:template:name</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__TEMPLATE__NAME</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>template</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>default</code></td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>Default</code></li>
                <li><code>GitHubRelease</code></li>
                <li><code>GitLabRelease</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.2+</td>
    </tr>
</table>

Sets the template to use for generating the changelog.
For details see [Templates](./templates.md).

#### Example

```json
{
    "changelog" : {
        "template" : {
            "name": "default"
        }
    }
}
```

### Markdown Preset (Default template)

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:template:default:markdownPreset</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__TEMPLATE__DEFAULT__MARKDOWNPRESET</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>default</code></td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>default</code></li>
                <li><code>MkDocs</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.2+</td>
    </tr>
</table>

The *Markdown Preset (Default Template)* customizes serialization
of Markdown for the default template (see [Template Name setting](#template-name)).

**Note:** This setting has no effect when a template other than `default` is used.

Supported values are:

- `default`: Produces Markdown that should work in most environments, including
  GitHub and GitLab
- `MkDocs`: Produces Markdown optimized for being rendered by Python-Markdown
  and [MkDocs](https://www.mkdocs.org/)

For details on the differences between the presets, see also
[Markdown Generator docs](https://github.com/ap0llo/markdown-generator/blob/master/docs/apireference/Grynwald/MarkdownGenerator/MdSerializationOptions/Presets/index.md).

#### Example

```json
{
    "changelog" : {
        "template" : {
            "default": {
                "markdownPreset" : "MkDocs"
            }
        }
    }
}
```

### Entry types

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:entryTypes</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>
        <code>[ 
            { "type": "feat", "DisplayName": "New Features" },
            { "type": "fix", "DisplayName": "Bug Fixes" }
        ]
        </code>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.2+</td>
    </tr>
</table>

The "Entry Types" setting controls which types of change log entries are included in the change log.
By default, all entries of type `feat` and `fix` are included.
The `DisplayName` property controls, the title that is used for displaying entries of this type.

⚠️ Using this setting in a configuration file will overwrite the default value.
To also include changes of type `feat` and `fix`, you need to include the default value in your configuration file.
It is not possible to just add additional types.
For example, to include all changes of type `feat`, `fix` and `docs`, you must include all three types in the configuration file (see Example).

ℹ️ Change log entries that contain breaking changes are always included, regardless of this setting or the entry type.

#### Example

The follwoing example shows ho to include changes of type `feat`, `fix` and `docs` in the change log:

```json
{
    "changelog" :{
        "entryTypes": [
            { "type": "feat", "DisplayName": "New Features" },
            { "type": "fix", "DisplayName": "Bug Fixes" },
            { "type": "docs", "DisplayName" : "Documenation changes"}
        ]
    }
}
```

## See Also

- [Commandline reference](./commandline-reference/index.md)
- [`defaultSettings.json`](../src/ChangeLog/Configuration/defaultSettings.json)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Integrations](./integrations.md)
- [NuGet Version Ranges](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges)
- [Templates](./templates.md)