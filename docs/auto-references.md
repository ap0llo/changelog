# Automatic References

ChangeLog will recognize references to commits, issues and Pull Requests in commit message footers (see [Conventional Commits](https://www.conventionalcommits.org/) for documentation on the commit message format) and attempt to automatically convert them to links.

---

ℹ Support for GitHub and GitLab references will only be available, if the corresponding [Integration Provider](./integrations.md) is enabled.

---

## Supported References

The following types of references are supported:

- [Web Urls](#web-urls)
- [Commit references](#commit-references)
- [Change Log Entry References](#change-log-entry-references)
- [GitHub References](./integrations/github.md#references)
- [GitLab References](./integrations/gitlab.md#references)
- [Markdown links](#markdown-links)

### Web Urls

If a footer's value is a valid `http` or `https` url, the footer will be included in the generated change log as link.

### Commit References

When a footer's value is a git commit id and the commit can be found in the repository, the footer is treated as *commit reference*.

Commit references are rendered as code spans in the generated change log.

If the [GitHub](./integrations/github.md) or [GitLab](./integrations/gitlab.md) integration is enabled, commit references will be rendered as links to the referenced commit on GitHub respectively GitLab.

Note that a if the reference refers to a commit that corresponds to a different change log entry, the reference will be converted to a [Change Log Entry Reference](#change-log-entry-references).

### Change Log Entry References

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

If no change log entry that matches a commit hash can be found, the footer value is treated as [commit reference](#commit-references).

### Markdown Links

**Supported versions:** 1.3+

If a footer's value is a valid [Markdown link](https://spec.commonmark.org/0.30/#links) which's destination is a `http` or `https` address, the footer's value will be replaced with a link with in the output.
If the Markdown link does not specify a link text, the url will be used.

#### Examples

| Footer Value                       | Output (Markdown)                            | Output (HTML)                                           |
|------------------------------------|----------------------------------------------|---------------------------------------------------------|
| `[Link Text](https://example.com)` | `[Link Text](https://example.com)`           | `<a href="https://example.com">Link Text</a>`           |
| `[](https://example.com)`          | `[https://example.com](https://example.com)` | `<a href="https://example.com">https://example.com</a>` |



## Normalization

In addition to converting references into links, changelog will also try to normalize references to a common format.

Normalization is supported for the following types of references:

- [Commit references](#commit-reference-normalization)
- [GitHub References](./integrations/github.md#references)
- [GitLab References](./integrations/gitlab.md#references)

Normalization of references is enabled by default, but can be disabled in the template settings for the configured template:

- [Default Template](./configuration/settings/default-template.md#normalize-references)
- [Html Template](./configuration/settings/html-template.md#normalize-references)
- [GitHubRelease Template](./configuration/settings/githubrelease-template.md#normalize-references)
- [GitLabRelease Template](./configuration/settings/gitlabrelease-template.md#normalize-references)

### Commit Reference Normalization

When normalization is enabled, the commit's git id is replaced with the abbreviated/short commit id.

For example, a footer `See-Also: cd73d01418587529acbae4233580b7ea0cc01819` will be normalized to `See-Also: cd73d01`

## See Also

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Integrations](./integrations.md)
- [GitHub Integration](./integrations/github.md)
- [GitLab Integration](./integrations/gitlab.md)
- [Links (CommonMark Spec, version 0.30)](https://spec.commonmark.org/0.30/#links)
