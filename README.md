# ChangeLog

[![Build Status](https://dev.azure.com/ap0llo/OSS/_apis/build/status/changelog?branchName=master)](https://dev.azure.com/ap0llo/OSS/_build/latest?definitionId=17&branchName=master)
[![MyGet](https://img.shields.io/myget/ap0llo-changelog/vpre/Grynwald.ChangeLog.svg?label=myget)](https://www.myget.org/feed/ap0llo-changelog/package/nuget/Grynwald.ChangeLog)
[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)](https://conventionalcommits.org)

## Overview

ChangeLog is a tool to generate a changelog based on git commit history using
[Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).

## Getting Started

See [Getting started](./docs/getting-started.md)

## Building from source

ChangeLog is a .NET Core application. Building it from source
requires the .NET SDK 3.1 (version 3.1.100)

```ps1
dotnet restore .\src\ChangeLog.sln

dotnet build .\src\ChangeLog.sln

dotnet pack .\src\ChangeLog.sln
```

## Issues

If you run into any issues or if you are missing a feature, feel free
to open an [issue](https://github.com/ap0llo/changelog/issues).

I'm also using issues as a backlog of things that come into my mind or
things I plan to implement, so don't be surprised if many issues were
created by me without anyone else being involved in the discussion.

## Acknowledgments

This project was made possible through a number of libraries (aside from
.NET Core). Thanks to all the people contribution to these projects:

- [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning/)
- [FluentValidation](https://fluentvalidation.net/)
- [LibGit2Sharp](https://github.com/libgit2/libgit2sharp)
- [CommandLineParser](https://github.com/gsscoder/commandline)
- [Microsoft.Extensions.Configuration](https://github.com/dotnet/extensions)
- [Microsoft.Extensions.Logging](https://github.com/dotnet/extensions)
- [NuGet.Versioning](https://github.com/NuGet/NuGet.Client)
- [OctoKit](https://github.com/octokit/octokit.net)
- [GitLabApiClient](https://github.com/nmklotas/GitLabApiClient)
- [Autofac](https://autofac.org/)
- [ApprovalTests](https://github.com/approvals/ApprovalTests.Net)
- [Moq](https://github.com/moq/moq4)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Mono.Cecil](https://github.com/jbevain/cecil/)
- [xUnit](http://xunit.github.io/)

## Versioning and Branching

The version of the library is automatically derived from git and the information
in `version.json` using [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning):

- The master branch  always contains the latest version. Packages produced from
  master are always marked as pre-release versions (using the `-pre` suffix).
- Stable versions are built from release branches. Build from release branches
  will have no `-pre` suffix
- Builds from any other branch will have both the `-pre` prerelease tag and the git
  commit hash included in the version string

To create a new release branch use the [`nbgv` tool](https://www.nuget.org/packages/nbgv/)
(at least version `3.0.4-beta`):

```ps1
dotnet tool install --global nbgv --version 3.0.4-beta
nbgv prepare-release
```
