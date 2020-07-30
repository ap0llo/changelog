# Entry Types Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:entryTypes</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>
        <code>[
            { "type": "feat", "DisplayName": "New Features" },
            { "type": "fix", "DisplayName": "Bug Fixes" }
        ]
        </code>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.2+</td>
    </tr>
</table>

The "Entry Types" setting controls which types of change log entries are included in the change log.
By default, all entries of type `feat` and `fix` are included.
The `DisplayName` property controls, the title that is used for displaying entries of this type.

⚠️ Using this setting in a configuration file will overwrite the default value.
To also include changes of type `feat` and `fix`, you need to include the default value in your configuration file.
It is not possible to just add additional types.
For example, to include all changes of type `feat`, `fix` and `docs`, you must include all three types in the configuration file (see Example).

ℹ️ Change log entries that contain breaking changes are always included, regardless of this setting or the entry type.

## Example

The following example shows how to include changes of type `feat`, `fix` and `docs` in the change log:

```json
{
    "changelog" :{
        "entryTypes": [
            { "type": "feat", "DisplayName": "New Features" },
            { "type": "fix", "DisplayName": "Bug Fixes" },
            { "type": "docs", "DisplayName" : "Documentation changes"}
        ]
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
