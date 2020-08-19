# GitHub Integration Settings

The *GitHub Integration* settings control the behaviour of the GitHub integration.
See also [Integrations - GitHub](../../integrations/github.md).

- [GitHub Access Token](#github-access-token)
- [GitHub Remote Name](#github-remote-name)
- [GitHub Host](#github-host)
- [GitHub Repository Owner](#github-repository-owner)
- [GitHub Repository Name](#github-repository-name)

## GitHub Access Token

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

## GitHub Remote Name

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:github:remoteName</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITHUB__REMOTENAME</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>origin</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The GitHub integration requires information about the repository on GitHub in order to function.
This information includes the host (typically `github.com` but might differ for GitHub Enterprise servers), the name of the repository owner (GitHub user or organization) as well as the name of the repository.

When these settings are not explicitly set in the configuration file, ChangeLog will parse the remote URL of the git repository to determine the project information.

By default, the URL of the `origin` remote is used, but the name of the remote can be adjusted using the *GitHub Remote Name* setting.

Note that this setting is not used, when [host](#github-host), [repository owner](#github-repository-owner) and [repository name](#github-repository-name) are set explicitly.
When the repository information is specified in the configuration partially, ChangeLog will attempt to add the missing information from the remote URL.

For details on how the remote URL is parsed, see [Integrations - GitHub](../../integrations/github.md).

## GitHub Host

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:github:host</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITHUB__HOST</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The *GitHub Host* setting specifies the host-name of the GitHub server to communicate with.

Typically, this will be `github.com`, but the host name will be different when using a GitHub Enterprise installation.

When no host name is specified (default behaviour), ChangeLog will attempt to determine the host name from the git repository's remote URL (see also [*GitHub Remote Name* setting](#github-remote-name)).

## GitHub Repository Owner

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:github:owner</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITHUB__OWNER</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The *GitHub Repository Owner* setting specifies the name of the owner (user or organization) of the GitHub repository to use .

When no owner is specified (default behaviour), ChangeLog will attempt to determine the owner from the git repository's remote URL (see also [*GitHub Remote Name* setting](#github-remote-name)).

## GitHub Repository Name

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:integrations:github:repository</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__INTEGRATIONS__GITHUB__REPOSITORY</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The *GitHub Repository Name* setting specifies the name of the GitHub repository to use.

When no repository name is specified (default behaviour), ChangeLog will attempt to determine the repository name from the git repository's remote URL (see also [*GitHub Remote Name* setting](#github-remote-name)).

## See Also

- [Configuration Overview](../../configuration.md)
- [GitHub Integration](../../integrations/github.md)