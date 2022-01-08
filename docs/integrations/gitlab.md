# GitLab Integration

The GitLab integration provides the following features:

- Add link to the GitLab web site to the commit for every change log entry
- Add web links for commit references that are detected in footers (see [Automatic References](../auto-references.md#commit-references))
- Recognize [GitLab References](#references)

The GitLab integration should work with both gitlab.com and self-hosted GitLab installations.

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

Version 0.3 of ChangeLog introduced settings to customize this behavior:

- [GitLab Remote Name](../configuration/settings/gitlab-integration.md#gitlab-remote-name)
  - Specifies the name of the git remote which's URL to parse.
  - This allows automatically determining the GitLab project information when the remote is not `origin`
- [GitLab Host](../configuration/settings/gitlab-integration.md#gitlab-host)
  - Allows explicitly specifying the host to use.
  - This setting takes precedence over the host name parsed from the remote URL.
- [GitLab Namespace](../configuration/settings/gitlab-integration.md#gitlab-namespace)
  - Allows explicitly specifying the project namespace to use.
  - This setting takes precedence over the namespace parsed from the remote URL.
- [GitLab Project Name](../configuration/settings/gitlab-integration.md#gitlab-project-name)
  - Allows explicitly specifying the repository name to use.
  - This setting takes precedence over the project name parsed from the remote URL.

When both host, namespace and project name settings are specified, the remote URL is not parsed and the *GitLab Remote Name* setting has no effect.

## Access Token

To access private repositories, an access token must be specified.
This can be achieved using either commandline parameters or environment variables. See [Configuration - GitLab Access token](../configuration/settings/gitlab-integration.md#gitlab-access-token) for details.

## References

ChangeLog will recognize GitLab-specific references in commit message footers (see also [Automatic References](../auto-references.md)).

The following types of references are recognized:

- Issues
  - `#<id>`: Reference to a issue in the same project, e.g. `#123`
  - `<project>#<id>`: Reference to an issue in a different project in the same namespace, e.g. `<changelog>#123`
  - `<user>/<project>#<id>`: Reference to an issue in a different project and namespace, e.g. `ap0llo/changelog#123`
- Merge Requests
  - `!<id>`: Reference to a merge request in the same project, e.g. `!123`
  - `<project>!<id>`: Reference to a merge request in a different project in the same namespace, e.g. `<changelog>!123`
  - `<user>/<project>!<id>`: Reference to a merge request in a different project and namespace, e.g. `ap0llo/changelog!123`
- Milestones
  - `%<id>`: Reference to a milestone in the same project, e.g. `%123`
  - `<project>%<id>`: Reference to a milestone in a different project in the same namespace, e.g. `<changelog>%123`
  - `<user>/<project>%<id>`: Reference to a milestone request in a different project and namespace, e.g. `ap0llo/changelog%123`

When [Reference Normalization](../auto-references.md#normalization) is enabled, references are converted to their minimal form:

- When an item in the same project is referenced, the reference is converted to use only the reference-specific prefix and reference id.<br>
  For example, a reference to the issue `user/project#23` is replaced with `#23` when generating the change log for the project `user/project`.
- References to items in a different project within the same project namespace (user or group) will include the project name in the reference.<br>
  For example, a reference to the Merge Request `user/projectA!23` will be replaced with `proejctA!23` when generating the change log for the project `user/projectB`.
- References to items in a different namespace (user or group) will include the project name and namespace in the reference.<br>
  For example, a reference to the Milestone `user1/projectA%23` will be retained as `user1/projectA%23` when generating the change log for the project `user2/projectB`.

## See Also

- [Integrations](../integrations.md)
- [Configuration](../configuration.md)
- [GitLab Integration Configuration](../configuration/settings/gitlab-integration.md)
- [Automatic References](../auto-references.md)