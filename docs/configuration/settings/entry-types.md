# Entry Types Setting

---

⚠️ The behaviour and format of the "Entry Types" setting has changed in version 0.3 of changelog.

For documentation on the "Entry Types" setting in version 0.2, see [Entry Types Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#entry-types).

---

## Entry Type Display Name

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:entryTypes:&lt;ENTRYTYPE&gt;:displayname</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__ENTRYTYPES__&lt;ENTRYTYPE&gt;__DISPLAYNAME</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>See <a href="#default-value">Default Value</a></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The "Entry Type Display Name" setting controls how different types of entries are displayed in the output.

In the output, change log entries are grouped by their type.
The display name configured for each entry type is used as heading for that group of entries.
If no display name is configured, the entry type itself is used instead.

### Default Value

In the default configuration, the following display names are pre-configured (defined in [defaultSetttings.json](../../../src/ChangeLog/Configuration/defaultSettings.json)):

| Entry Type | Display Name |
|------------|--------------|
| `feat`     | New Features |
| `fix`      | Bug Fixes    |

### Example

The following example shows how to define a display name for entries of type `docs` and override the display name for entries of type `feat`:

```json
{
    "changelog" :{
        "entryTypes": {
            "docs" : { "displayName" : "Documentation changes" },
            "feat" : { "displayName": "Custom Display Name" }
        }
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Entry Types Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#entry-types)