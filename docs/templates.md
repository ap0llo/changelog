# Templates

Template allow customizing the generated changelog and generate a changelog in different formats of for different environments.

The template to use can be configured using the `changelog:template:name` setting (see [Configuration](./configuration.md#template-name) for details).

The following templates are supported:

- [Default](#default-template)
- [GitHubRelease](#githubrelease-template)
- [GitLabRelease](#gitlabrelease-template)

## Default Template

The default template renders the change log to a Markdown file.
It is the most generic template and should work in most Markdown implementations.

The default template supports customizing serialization settings for the generated markdown.
For details, see [Markdown Preset (Default Template)](./configuration.md#markdown-preset-default-template).

## GitHub Release Template

The GitHub Release Template renders a change log suited to be used as the description of a [GitHub Release](https://help.github.com/en/github/administering-a-repository/about-releases).

This template **only supports including the changes of a single version**, so it should be combined with the [Version Range setting](./configuration.md#version-range).
Compared to the default template, the GitHub Release template omits the "Change Log" and version headings and adjusts the heading levels so the changelog can is properly rendered in the Releases view of the GitHub web interface.

**Note:** The GitHub Release template is independent of the [GitHub integration](./integrations/github.md) for links.
Both features can be used independently of each other.

## GitLab Release Template

The GitLab Release Template renders a change log suited to be used as the description of a [GitLab Release](https://docs.gitlab.com/ee/user/project/releases/).

This template **only supports including the changes of a single version**, so it should be combined with the [Version Range setting](./configuration.md#version-range).
Compared to the default template, the GitLab Release template omits the "Change Log" and version headings and adjusts the heading levels so the changelog can is properly rendered in the Releases view of the GitLab web interface.

**Note:** The GitLab Release template is independent of the [GitLab integration](./integrations/gitlab.md) for links.
Both features can be used independently of each other.

## Version support

Support for templates were introduced in version 0.2.

## See Also

- [Configuration](./configuration.md)
- [GitLab Releases](https://docs.gitlab.com/ee/user/project/releases/)
