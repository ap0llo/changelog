# Automatic References

ChangeLog will recognize references to commits, issues and Pull Requests in commit message footers (see [Conventional Commits](https://www.conventionalcommits.org/) for documentation on the commit message format) and attempt to automatically convert them to links.

---

ℹ Support for GitHub and GitLab references will only be available, if the corresponding [Integration Provider](./integrations.md) is enabled.

---

The following references are supported:

- [Change Log Entry References](#change-log-entry-references)
- GitHub References (when [GitHub Integration](./integrations/github.md) is enabled)
  - Pull Request and Issue references (e.g. `#42`)
- GitLab References (when [GitLab Integration](./integrations/gitlab.md) is enabled)
  - Issue References (e.g. `#23`)
  - Merge Request References (e.g. `!42`)
  - Milestone References (e.g. `%123`)

## Change Log Entry References

Entries in the change log can reference other change log entries using the entry's commit hash.

When a footer's value is a git commit hash and the change log contains an entry for that commit, the footer will be replaced by a link to the referenced entry.

For example, consider the following two commits:

Commit 1:

```txt
commit cd73d01418587529acbae4233580b7ea0cc01819
Author: ...
Date:   ...

    feat: Implemented a new feature

    This is the description of the first feature.
```

Commit 2:

```txt
commit 2da10790088ad30b54315065fae2ab02195409d7
Author: ...
Date:   ...

    feat: Implemented another feature

    This is the description of the second feature.

    See-Also: cd73d01418587529acbae4233580b7ea0cc01819
```

The second commit uses a footer to reference the first commit.
In the change log, the footer of the second commit will be replaced with a link to the first feature which results in a change log similar to the following Markdown sample:

```md
## New Features

- [Implemented a new feature](implemented-a-new-feature)
- [Implemented another feature](#implemented-another-feature)

## Details

### Implemented a new feature

This is the description of the first feature.

### Implemented another feature

This is the description of the second feature.

See Also: [Implemented a new feature](implemented-a-new-feature)
```

If no change log entry that matches a commit hash can be found, the footer will be included in the output unchanged.

## See Also

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Integrations](./integrations.md)
- [GitHub Integration](./integrations/github.md)
- [GitLab Integration](./integrations/gitlab.md)