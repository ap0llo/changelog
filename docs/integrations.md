# Integrations

While a changelog can be generated from any git repository, "Integrations"
provide extended features for repositories hosted by any of the supported
providers.

Currently supported providers are:

- [GitHub](#github)
- [GitLab](#gitlab)

## Configuring integrations

By default, no integrations are enabled. To specify the integration provider,
use the using the `changelog:integrations:provider` setting:

```json
{
    "changelog": {
        "integrations": {
            "provider": "GitHub"
        }
    }
}
```

Allowed values are:

- `none` (default)
- `GitHub` 
- `GitLab`

## GitHub

The GitHub integration provides the following features:

- Adds link to the GitHub web site to the commit for every changelog entry
- Adds link to the GitHub web site to issues and pull requests if they
  are referenced in footers.<br>
  The following types of references are recognized:
  - `#<id>`: Reference to an issue or pull request in the same project,
    e.g. `#123`
  - `<user>/<project>#<id>`: Reference to an issue or pull request in a
    different project, e.g. `ap0llo/changelog#123`
  - `GH-<id>`: Reference to an issue or pull request in the same project,
    e.g.`GH-123`
  
The GitHub integration should work with both github.com and GitHub Enterprise
installations. However, it was not yet tested with GitHub Enterprise.

The GitHub server address and the project name are read from the url of the
*origin* remote in the local git repository.

To access private repositories, an access token must be specified using the
`--gitHubAccessToken` commandline parameter. Because GitHub has a quite low
rate limit for unauthenticated API requests, it is recommended to use a access
token even if you only access public repositories.

## GitLab

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

The address of the GitLab server and the project name are read from the url
of the *origin* remote in the local git repository.

To access private repositories, an access token must be specified using the
`--gitLabAccessToken` commandline parameter.

## See Also

- [Configuration](./configuration.md)