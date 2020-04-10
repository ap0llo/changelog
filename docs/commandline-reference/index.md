# changelog Command Line Reference

**Version:** 0.1.63\-pre+b4d7c6b5b7

## Usage

```
changelog [--configurationPath|-c <VALUE>]
          [--currentVersion <VALUE>]
          [--githubAccessToken <VALUE>]
          [--gitlabAccessToken <VALUE>]
          [--outputpath|-o <VALUE>]
          --repository|-r <VALUE>
          [--verbose|-v]
          [--versionRange <VALUE>]
```

## Parameters

| Name                                              | Short Name                        | Required | Description                                                                                                                                                                                                                                    |
| ------------------------------------------------- | --------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [configurationPath](#configurationpath-parameter) | [c](#configurationpath-parameter) | No       | The path of the configuration file to use. When no configuration file path is specified, changelog looks for a file named 'changelog.settings.json' in the repository directory. If no configuration file is found, default settings are used. |
| [currentVersion](#currentversion-parameter)       |                                   | No       | Sets the version of the currently checked\-out commit. Value must be a valid semantic version                                                                                                                                                  |
| [githubAccessToken](#githubaccesstoken-parameter) |                                   | No       | The access token to use if the GitHub integration is enabled.                                                                                                                                                                                  |
| [gitlabAccessToken](#gitlabaccesstoken-parameter) |                                   | No       | The access token to use if the GitLab integration is enabled.                                                                                                                                                                                  |
| [outputpath](#outputpath-parameter)               | [o](#outputpath-parameter)        | No       | The path to save the changelog to.                                                                                                                                                                                                             |
| [repository](#repository-parameter)               | [r](#repository-parameter)        | Yes      | The local path of the git repository to generate a change log for.                                                                                                                                                                             |
| [verbose](#verbose-parameter)                     | [v](#verbose-parameter)           | No       | Increase the level of detail for messages logged to the console.                                                                                                                                                                               |
| [versionRange](#versionrange-parameter)           |                                   | No       | The range of versions to include in the change log. Value must be a valid NuGet version range.                                                                                                                                                 |

### `configurationPath` Parameter

The path of the configuration file to use. When no configuration file path is specified, changelog looks for a file named 'changelog.settings.json' in the repository directory. If no configuration file is found, default settings are used.

|                |                   |
| -------------- | ----------------- |
| Name:          | configurationPath |
| Short name:    | c                 |
| Position:      | *Named parameter* |
| Required:      | No                |
| Default value: | *None*            |

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

### `outputpath` Parameter

The path to save the changelog to.

|                |                   |
| -------------- | ----------------- |
| Name:          | outputpath        |
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
| Required:      | Yes               |
| Default value: | *None*            |

___

### `verbose` Parameter

Increase the level of detail for messages logged to the console.

|                |                       |
| -------------- | --------------------- |
| Name:          | verbose               |
| Short name:    | v                     |
| Position:      | *Named parameter*     |
| Required:      | No (Switch parameter) |
| Default value: | `False`               |

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

*Documentation generated by [MdDocs](https://github.com/ap0llo/mddocs)*
