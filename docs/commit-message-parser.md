# Commit Message Parser

ChangeLog expects commit messages to follow the [Conventional Commits Specification](https://www.conventionalcommits.org/en/v1.0.0).
Messages that use that format can be parsed and included in the generated change log.

The parser has two *modes*:

- *Strict* mode enforces commit messages to follow the specification precisely.
- *Loose* mode is more lenient and allows minor deviations from the specification.

By default, the *Loose* mode is used.
For documentation on how to change the parser mode, see [Configuration](./configuration/settings/parser-mode.md).

## Differences between *Strict* and *Loose* Parser modes

The differences between *Strict* and *Loose* modes are listed below.

### Ignore trailing blank lines

In loose mode, all trailing blank lines at the end of the commit message are ignored.
The following commit message is valid in *Loose* mode but invalid in *Strict* mode:

```txt
feat: New Feature

Description of the feature




```

### Ignore duplicate blank lines

Blank lines separate the commit message header from the body, different paragraphs of the message body as well as the message body from the commit message footers.

In *Loose* mode, multiple consecutive blank lines are treated the same as a single blank line.

The following commit message is valid in *Loose* mode but invalid in *Strict* mode:

```txt
feat: New Feature



Description of the feature


Second paragraph of the message body


Reviewed-By: someone@example.com
Closes #123
```

### Allow blank lines between footers

Commit messages may contain one or more commit message footers.
In *Loose* mode, one or more blank lines may appear between the individual footer while it is invalid in *Strict* mode.

The following commit message is valid in *Loose* mode but invalid in *Strict* mode:

```txt
feat: New Feature

Description of the feature.
Some more description.

Reviewed-By: someone@example.com

Closes #123
```

### Treat whitespace-only lines as blank lines

In *Loose* mode, blank lines that only contain whitespace characters are treated as blank lines.
In *Strict* mode, a blank line my only contain a line-break (`\r\n` or `\n`).

The following commit message is valid in *Loose* mode but invalid in *Strict* mode
(there is whitespace in the second line of the commit message):

```txt
feat: New Feature
       
Description of the feature
```

## See Also

- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0)
- [Configuration - Parser Mode Setting](./configuration/settings/parser-mode.md)
