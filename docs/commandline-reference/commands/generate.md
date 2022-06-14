﻿<!--  
  <auto-generated>   
    The contents of this file were generated by a tool.  
    Changes to this file may be list if the file is regenerated  
  </auto-generated>   
-->

# `generate` Command

**Application:** [changelog](../index.md)

Generate a change log from a git repository

## Usage

```
changelog generate [--configurationFilePath|-c <VALUE>]
                   [--currentVersion <VALUE>]
                   [--githubAccessToken <VALUE>]
                   [--gitlabAccessToken <VALUE>]
                   [--integrationProvider <VALUE>]
                   [--outputPath|-o <VALUE>]
                   [--repository|-r <VALUE>]
                   [--template <VALUE>]
                   [--versionRange <VALUE>]
                   [--verbose|-v]
```

## Parameters

| Name                                                      | Short Name                            | Description                                                                                                                                                                                                                                    |
| --------------------------------------------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [configurationFilePath](#configurationfilepath-parameter) | [c](#configurationfilepath-parameter) | The path of the configuration file to use. When no configuration file path is specified, changelog looks for a file named 'changelog.settings.json' in the repository directory. If no configuration file is found, default settings are used. |
| [currentVersion](#currentversion-parameter)               |                                       | Sets the version of the currently checked\-out commit. Value must be a valid semantic version                                                                                                                                                  |
| [githubAccessToken](#githubaccesstoken-parameter)         |                                       | The access token to use if the GitHub integration is enabled.                                                                                                                                                                                  |
| [gitlabAccessToken](#gitlabaccesstoken-parameter)         |                                       | The access token to use if the GitLab integration is enabled.                                                                                                                                                                                  |
| [integrationProvider](#integrationprovider-parameter)     |                                       | Sets the integration provider to use                                                                                                                                                                                                           |
| [outputPath](#outputpath-parameter)                       | [o](#outputpath-parameter)            | The path to save the changelog to.                                                                                                                                                                                                             |
| [repository](#repository-parameter)                       | [r](#repository-parameter)            | The local path of the git repository to generate a change log for.                                                                                                                                                                             |
| [template](#template-parameter)                           |                                       | Sets the template to use for generating the changelog.                                                                                                                                                                                         |
| [versionRange](#versionrange-parameter)                   |                                       | The range of versions to include in the change log. Value must be a valid NuGet version range.                                                                                                                                                 |
| [verbose](#verbose-parameter)                             | [v](#verbose-parameter)               | Increase the level of detail for messages logged to the console.                                                                                                                                                                               |

### `configurationFilePath` Parameter

The path of the configuration file to use. When no configuration file path is specified, changelog looks for a file named 'changelog.settings.json' in the repository directory. If no configuration file is found, default settings are used.

|                |                       |
| -------------- | --------------------- |
| Name:          | configurationFilePath |
| Short name:    | c                     |
| Position:      | *Named parameter*     |
| Required:      | No                    |
| Default value: | *None*                |

___

### `currentVersion` Parameter

Sets the version of the currently checked\-out commit. Value must be a valid semantic version

|                |                   |
| -------------- | ----------------- |
| Name:          | currentVersion    |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `githubAccessToken` Parameter

The access token to use if the GitHub integration is enabled.

|                |                   |
| -------------- | ----------------- |
| Name:          | githubAccessToken |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `gitlabAccessToken` Parameter

The access token to use if the GitLab integration is enabled.

|                |                   |
| -------------- | ----------------- |
| Name:          | gitlabAccessToken |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `integrationProvider` Parameter

Sets the integration provider to use

|                |                     |
| -------------- | ------------------- |
| Name:          | integrationProvider |
| Position:      | *Named parameter*   |
| Required:      | No                  |
| Default value: | *None*              |

___

### `outputPath` Parameter

The path to save the changelog to.

|                |                   |
| -------------- | ----------------- |
| Name:          | outputPath        |
| Short name:    | o                 |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `repository` Parameter

The local path of the git repository to generate a change log for.

|                |                   |
| -------------- | ----------------- |
| Name:          | repository        |
| Short name:    | r                 |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `template` Parameter

Sets the template to use for generating the changelog.

|                |                   |
| -------------- | ----------------- |
| Name:          | template          |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `versionRange` Parameter

The range of versions to include in the change log. Value must be a valid NuGet version range.

|                |                   |
| -------------- | ----------------- |
| Name:          | versionRange      |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

___

### `verbose` Parameter

Increase the level of detail for messages logged to the console.

|                |                           |
| -------------- | ------------------------- |
| Name:          | verbose                   |
| Short name:    | v                         |
| Position:      | *None (Switch Parameter)* |
| Required:      | *No (Switch Parameter)*   |
| Default value: | `false`                   |

___

*Documentation generated by [MdDocs](https://github.com/ap0llo/mddocs)*