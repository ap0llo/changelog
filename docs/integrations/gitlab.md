# GitLab Integration

The GitLab integration provides the following features:

- Adds link to the GitLab web site to the commit for every changelog entry
- Adds link to the GitLab web site for references in footers.<br>
  The following types of references are recognized:
  - Issues
    - `#<id>`: Reference to a issue in the same project, e.g. `#123`
    - `<project>#<id>`: Reference to an issue in a different project in
      the same namespace, e.g. `<changelog>#123`
    - `<user>/<project>#<id>`: Reference to an issue in a different project and
      namespace, e.g. `ap0llo/changelog#123`
  - Merge Requests
    - `!<id>`: Reference to a merge request in the same project, e.g. `!123`
    - `<project>!<id>`: Reference to a merge request in a different project in
      the same namespace, e.g. `<changelog>!123`
    - `<user>/<project>!<id>`: Reference to a merge request in a different
      project and namespace, e.g. `ap0llo/changelog!123`
  - Milestones
    - `%<id>`: Reference to a milestone in the same project, e.g. `%123`
    - `<project>%<id>`: Reference to a milestone in a different project in
      the same namespace, e.g. `<changelog>%123`
    - `<user>/<project>%<id>`: Reference to a milestone request in a different
      project and namespace, e.g. `ap0llo/changelog%123`

The GitLab integration should work with both gitlab.com and self-hosted
GitLab installations. However, it was not yet tested with a self-hosted
GitLab instance.

## Project information

By default, the GitLab server name, the project's namespace and the project's name are read from the URL of the *origin* remote in the local git repository.

ChangeLog assumes, the remote URL follows the default schema used by gitlab.com:

```txt
https://gitlab.com/group/subgroup/example-project.git
        └────────┘ └────────────┘ └─────────────┘
            ▲           ▲                ▲
            │           │                │
            │           │                └── Project Name: "example-project"
            │           │
            │           └── Project Namespace: "group/subgroup"
            │
            └── Host: "gitlab.com"
```

Version 0.3 of ChangeLog introduced settings to customize this behaviour:

- [GitLab Remote Name](../configuration.md#gitlab-remote-name)
  - Specifies the name of the git remote which's URL to parse.
  - This allows automatically determining the GitLab project information when the remote is not `origin`
- [GitLab Host](../configuration.md#gitlab-host)
  - Allows explicitly specifying the host to use.
  - This setting takes precedence over the host name parsed from the remote URL.
- [GitLab Namespace](../configuration.md#gitlab-namespace)
  - Allows explicitly specifying the project namespace to use.
  - This setting takes precedence over the namespace parsed from the remote URL.
- [GitLab Project Name](../configuration.md#gitlab-project-name)
  - Allows explicitly specifying the repository name to use.
  - This setting takes precedence over the project name parsed from the remote URL.

When both host, namespace and project name settings are specified, the remote URL is not parsed and the *GitLab Remote Name* setting has no effect.

## Access Token

To access private repositories, an access token must be specified.
This can be achieved using either commandline parameters or environment variables. See [Configuration - GitLab Access token](../configuration.md#gitlab-access-token) for details.

## See Also

- [Integrations](../integrations.md)
- [Configuration](../configuration.md)