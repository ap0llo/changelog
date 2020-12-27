# GitHub Integration

The GitHub integration provides the following features:

- Add GitHub web link for a change log entry's commit.
- Add web links for commit references that are detected in footers (see [Automatic References](../auto-references.md#commit-references))
- Recognize GitHub Issue and Pull Request references (see also [Automatic References](../auto-references.md)) in footers and generate GitHub web links.<br>
  The following types of references are recognized:
  - `#<id>`: Reference to an issue or pull request in the same project, e.g. `#123`
  - `<user>/<project>#<id>`: Reference to an issue or pull request in a different project, e.g. `ap0llo/changelog#123`
  - `GH-<id>`: Reference to an issue or pull request in the same project, e.g.`GH-123`

The GitHub integration should work with both github.com and GitHub Enterprise
installations. However, it was not yet tested with GitHub Enterprise.

## Project information

By default, the GitHub server name, the repository owner and the repository name are read from the URL of the *origin* remote in the local git repository.

ChangeLog assumes, the remote URL follows the default schema used by github.com:

```txt
https://github.com/example-owner/example-repo.git
        └────────┘ └───────────┘ └──────────┘
            ▲           ▲             ▲
            │           │             │
            │           │             └── Repository Name: "example-repo"
            │           │
            │           └── Repository Owner: "example-owner"
            │
            └── Host: "github.com"
```

Version 0.3 of ChangeLog introduced settings to customize this behaviour:

- [GitHub Remote Name](../configuration/settings/github-integration.md#github-remote-name)
  - Specifies the name of the git remote which's URL to parse.
  - This allows automatically determining the GitHub project information when the remote is not `origin`
- [GitHub Host](../configuration/settings/github-integration.md#github-host)
  - Allows explicitly specifying the host to use.
  - This setting takes precedence over the host name parsed from the remote URL.
- [GitHub Repository Owner](../configuration/settings/github-integration.md#github-repository-owner)
  - Allows explicitly specifying the repository owner to use.
  - This setting takes precedence over the owner name parsed from the remote URL.
- [GitHub Repository Name](../configuration/settings/github-integration.md#github-repository-name)
  - Allows explicitly specifying the repository name to use.
  - This setting takes precedence over the repository name parsed from the remote URL.

When both host, repository owner and repository name settings are specified, the remote URL is not parsed and the *GitHub Remote Name* setting has no effect.

## Access Token

To access private repositories, an access token must be specified.
This can be achieved using either commandline parameters or environment variables. See [Configuration - GitHub Access token](../configuration/settings/github-integration.md#github-access-token) for details.

Because GitHub has a quite low rate limit for unauthenticated API requests, it is recommended to use a access token even if you only access public repositories.

## See Also

- [Integrations](../integrations.md)
- [Configuration](../configuration.md)
- [GitHub Integration Configuration](../configuration/settings/github-integration.md)
- [Automatic References](../auto-references.md)