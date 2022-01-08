# Configuration Overview

The behavior of ChangeLog can be customized by changing the configuration.
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

- [Scope Settings](./configuration/settings/scopes.md)
  - [Scope Display Name](./configuration/settings/scopes.md#scope-display-name)
- [Footer Settings](./configuration/settings/footers.md)
  - [Footer Display Name](./configuration/settings/footers.md#footer-display-name)
- [Entry Type Settings](./configuration/settings/entry-types.md)
  - [Entry Type Display Name](./configuration/settings/entry-types.md#entry-type-display-name)
  - [Entry Type Priority](./configuration/settings/entry-types.md#entry-type-priority)
- [Filter](./configuration/settings/filter.md)
- [Parser Mode](./configuration/settings/parser-mode.md)
- [Version Range](./configuration/settings/version-range.md)
- [Current Version](./configuration/settings/current-version.md)
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
  - [Normalize References](./configuration/settings/default-template.md#normalize-references)
- [GitHubRelease Template](./configuration/settings/githubrelease-template.md)
  - [Normalize References](./configuration/settings/githubrelease-template.md#normalize-references)
- [GitLabRelease Template](./configuration/settings/gitlabrelease-template.md)
  - [Normalize References](./configuration/settings/gitlabrelease-template.md#normalize-references)
- [Html Template](./configuration/settings/html-template.md)
  - [Normalize References](./configuration/settings/html-template.md#normalize-references)


## See Also

- [Commandline reference](./commandline-reference/index.md)
- [`defaultSettings.json`](../src/ChangeLog/configuration/defaultSettings.json)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Integrations](./integrations.md)
- [NuGet Version Ranges](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges)
- [Templates](./templates/README.md)
- [Commit Message Parser](./commit-message-parser.md)
