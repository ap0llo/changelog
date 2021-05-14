# Templates

Templates allow generating a change log in different formats of for different environments.

The template to use can be configured using the [Template Name Setting](./configuration/settings/template-name.md) for details.

The following templates are supported:

- [Default](#default-template)
- [GitHubRelease](#github-release-template)
- [GitLabRelease](#gitlab-release-template)
- [Html](#html-template)

## Default Template

The default template renders the change log to a Markdown file.
It is the most generic template and should work in most Markdown implementations.

For configuration options, see [Default Template Settings](./configuration/settings/default-template.md).

## GitHub Release Template

The GitHub Release Template renders a change log suited to be used as the description of a [GitHub Release](https://help.github.com/en/github/administering-a-repository/about-releases).

This template **only supports including the changes of a single version**, so it should be combined with the [Version Range setting](./configuration/settings/version-range.md).
Compared to the default template, the GitHub Release template omits the "Change Log" and version headings and adjusts the heading levels so the changelog can is properly rendered in the Releases view of the GitHub web interface.

**Note:** The GitHub Release template is independent of the [GitHub integration](./integrations/github.md) for links.
Both features can be used independently of each other.

For configuration options, see [GitHubRelease Template Settings](./configuration/settings/githubrelease-template.md).

## GitLab Release Template

The GitLab Release Template renders a change log suited to be used as the description of a [GitLab Release](https://docs.gitlab.com/ee/user/project/releases/).

This template **only supports including the changes of a single version**, so it should be combined with the [Version Range setting](./configuration/settings/version-range.md).
Compared to the default template, the GitLab Release template omits the "Change Log" and version headings and adjusts the heading levels so the changelog can is properly rendered in the Releases view of the GitLab web interface.

**Note:** The GitLab Release template is independent of the [GitLab integration](./integrations/gitlab.md) for links.
Both features can be used independently of each other.

For configuration options, see [GitLabRelease Template Settings](./configuration/settings/gitlabrelease-template.md).

## Html Template

The Html template render a change log as a standalone Html page.

For configuration options, see [Html Template Settings](./configuration/settings/html-template.md).

## Version support

Support for templates were introduced in version 0.2.  
The "Html" template was introduced in verison 0.4.

## See Also

- [Configuration](./configuration.md)
- [Template Name Setting](./configuration/settings/template-name.md)
- Template settings
    - [Default Template Settings](./configuration/settings/default-template.md)
    - [GitHubRelease Template Settings](./configuration/settings/githubrelease-template.md)
    - [GitLabRelease Template Settings](./configuration/settings/gitlabrelease-template.md)
    - [Html Template Settings](./configuration/settings/html-template.md)
- [GitHub Releases](https://help.github.com/en/github/administering-a-repository/about-releases)
- [GitLab Releases](https://docs.gitlab.com/ee/user/project/releases/)
