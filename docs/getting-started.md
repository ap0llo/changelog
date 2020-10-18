# Getting Started

This document assumes you are familiar with [Conventional Commits](https://www.conventionalcommits.org/) and your git commit messages follow the convention described there.
Furthermore it assumes you're using [Semantic Versioning](https://semver.org/) for your project.

## Installation

ChangeLog is distributed as .NET (global) tool and requires either the .NET Core SDK 2.1 or 3.1 to be installed.

ChangeLog is available on [NuGet.org](https://www.nuget.org/packages/Grynwald.ChangeLog).
Prerelease builds on are available on [MyGet](https://www.myget.org/feed/ap0llo-changelog/package/nuget/Grynwald.ChangeLog).

To install ChangeLog, run:

```sh
dotnet tool install -g Grynwald.ChangeLog
```

## Generating a changelog

ChangeLog assumes you have a local git repository which's commit messages follow the [Conventional Commits](https://www.conventionalcommits.org/) format.

The versions of your project are read from the git repository's tags.
Versions need to follow the [Semantic Versioning](https://semver.org/) format.

By default all tag names that are semantic versions and tag names that are semantic versions prefixed with `v` are recognized (e.g. `1.2.3-alpha` or `v1.2`).
Parsing of tags can be customized using the [Tag Patterns Setting](./configuration/settings/tag-patterns.md).

To generate a changelog from a git repository, run Changelog and specify the path to the git repository.
By default, the change log is saved to `changelog.md` in the repository.

```sh
changelog --repository <LOCALPATH>
```

You can override the output path using the `--outputPath` parameters.

For all full list of commandline parameters, see
[Commandline reference](./commandline-reference/index.md)

## Configuration

The behaviour of ChangeLog can be customized using a configuration file
and environment variables.

For details, see [Configuration](./configuration.md).

## See Also

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Commandline reference](./commandline-reference/index.md)
- [Configuration](./configuration.md)