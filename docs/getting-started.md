# Getting Started

This guide assumes you are familiar with 
[Conventional Commits](https://www.conventionalcommits.org/)
and your git commit messages follow the convention described there.
Furthermore it assumes you're using [Semantic Versioning](https://semver.org/)
for your project.

## Installation

ChangeLog is distributed as .NET (global) tool and requires either
the .NET Core SDK 2.1 or 3.1 to be installed.

ChangeLog is not (yet) available on NuGet.org, but can find prerelease
build on
[MyGet](https://www.myget.org/feed/ap0llo-changelog/package/nuget/Grynwald.ChangeLog).

To install (e.g. version `0.1.64-pre`), run:

```sh
dotnet tool install -g Grynwald.ChangeLog --version 0.1.64-pre --add-source https://www.myget.org/F/ap0llo-changelog/api/v3/index.json
```

## Generating a changelog

ChangeLog assumes you have a local git repository which's commit messages
follow the [Conventional Commits](https://www.conventionalcommits.org/) format.

The versions of your project are read from the git repository's tags. Versions
need to follow the [Semantic Versioning](https://semver.org/) format.
How tags are parsed can be customized, by default all tag names that are
semantic versions and tag names that are semantic versions prefixed with `v`
are recognized (e.g. `1.2.3-alpha` or `v1.2`)

To generate a changelog from a git repository, run and pass in the
path to the git repository. By default, the changelog is saved
to `changelog.md` in the repository.

You can override the output path using the `--outputPath` parameters.

```sh
changelog --repository <LOCALPATH>
```

For all full list of commandline parameters, see
[Commandline reference](./commandline-reference/index.md)

## Configuration

The behaviour of ChangeLog can be customized using a configuration file
and environment variables.

For details, see [Configuration](./configuration.md).
