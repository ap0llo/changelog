# Getting Started

This document assumes you are familiar with [Conventional Commits](https://www.conventionalcommits.org/) and your git commit messages follow the convention described there.
Furthermore it assumes you're using [Semantic Versioning](https://semver.org/) for your project.

## Installation

ChangeLog is distributed as [.NET (global) tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) and requires at least the .NET 6 SDK to be installed (.NET 7 is suppored as well).

ChangeLog is available on [NuGet.org](https://www.nuget.org/packages/Grynwald.ChangeLog).
Prerelease builds on are available on [MyGet](https://www.myget.org/feed/ap0llo-changelog/package/nuget/Grynwald.ChangeLog).

To install ChangeLog, run:

```sh
dotnet tool install -g Grynwald.ChangeLog
```

## Generating a change log

ChangeLog assumes you have a local git repository which's commit messages follow the [Conventional Commits](https://www.conventionalcommits.org/) format.

The versions of your project are read from the git repository's tags.
Versions need to follow the [Semantic Versioning](https://semver.org/) format.

By default all tag names that are semantic versions and tag names that are semantic versions prefixed with `v` are recognized (e.g. `1.2.3-alpha` or `v1.2`).
Parsing of tags can be customized using the [Tag Patterns Setting](./configuration/settings/tag-patterns.md).

To generate a change log from a git repository, run the `changelog generate` command and specify the path to the git repository.

```sh
changelog generate --repository <LOCALPATH>
```

ℹ The repository path can be omitted when `changelog` is run from a directory within the git repository.

By default, the change log is saved to `changelog.md` in the repository.
You can override the output path using the `--outputPath` parameter or in the configuration file (see [Configuration](./configuration.md)).

For all full list of commandline parameters, see
[Commandline reference](./commandline-reference/index.md)

## Configuration

The behavior of ChangeLog can be customized using a configuration file and environment variables.

For details, see [Configuration](./configuration.md).

## See Also

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Commandline reference](./commandline-reference/index.md)
- [Configuration](./configuration.md)