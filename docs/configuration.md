# Configuration Overview

The behaviour of ChangeLog can be customized by changing the configuration.
All settings are optional and ChangeLog aims to use sensible default values for all of them.

Settings are loaded from a number of *setting sources*:

1. [Default settings](./configuration/sources.md#default-settings)
2. [Configuration file](./configuration/sources.md#configuration-file)
3. [Environment variables](./configuration/sources.md#environment-variables)
4. [Commandline parameters](./configuration/sources.md#commandline-parameters)

For details, see [Configuration Sources](./configuration/sources.md)

## Setting Name Notation

Setting names in the documentation are separated by `:` which denote keys and sub-keys the JSON configuration file.
For example setting `key:subkey` to `value` would need to be specified in the configuration file like this:

```json
{
    "key" : {
        "subkey" : "value"
    }
}
```

## Settings List

- [Scopes](./configuration/settings/scopes.md)
- [Footer Settings](./configuration/settings/footers.md)
  - [Footer Display Name](./configuration/settings/footers.md#footer-display-name)
- [Entry Types](./configuration/settings/entry-types.md)
- [Filter](./configuration/settings/filter.md)
- [Parser Mode](./configuration/settings/parser-mode.md)
- [Version Range](./configuration/settings/version-range.md)
- [Current Version](./configuration/settings/current-version.md)
- [Markdown Preset](./configuration/settings/markdown-preset.md)
- [Tag Patterns](./configuration/settings/tag-patterns.md)
- [Output Path](./configuration/settings/output-path.md)
- [Integration Provider](./configuration/settings/integration-provider.md)
- [GitHub Integration](./configuration/settings/github-integration.md)
  - [GitHub Access Token](./configuration/settings/github-integration.md#github-access-token)
  - [GitHub Remote Name](./configuration/settings/github-integration.md#github-remote-name)
  - [GitHub Host](./configuration/settings/github-integration.md#github-host)
  - [GitHub Repository Owner](./configuration/settings/github-integration.md#github-repository-owner)
  - [GitHub Repository Name](./configuration/settings/github-integration.md#github-repository-name)
- [GitLab Integration](./configuration/settings/gitlab-integration.md)
  - [GitLab Access Token](./configuration/settings/gitlab-integration.md#gitlab-access-token)
  - [GitLab Remote Name](./configuration/settings/gitlab-integration.md#gitlab-remote-name)
  - [GitLab Host](./configuration/settings/gitlab-integration.md#gitlab-host)
  - [GitLab Namespace](./configuration/settings/gitlab-integration.md#gitlab-namespace)
  - [GitLab Project Name](./configuration/settings/gitlab-integration.md#gitlab-project-name)
- [Template Name](./configuration/settings/template-name.md)
- [Default Template](./configuration/settings/default-template.md)
  - [Markdown Preset](./configuration/settings/default-template.md#markdown-preset)

## See Also

- [Commandline reference](./commandline-reference/index.md)
- [`defaultSettings.json`](../src/ChangeLog/configuration/defaultSettings.json)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Integrations](./integrations.md)
- [NuGet Version Ranges](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges)
- [Templates](./templates.md)
- [Commit Message Parser](./commit-message-parser.md)
