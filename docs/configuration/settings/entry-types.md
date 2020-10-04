# Entry Types Setting

---

⚠️ The behaviour of the "Entry Types" setting has changed in version 0.3 of changelog.

For documentation on the "Entry Types" setting in version 0.2, see [Entry Types Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#entry-types).

---

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
        <td>
        <ul>
            <li>0.2, see <a href="https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#entry-types">Entry Types Setting (v0.2)</a></li>
            <li>0.3+</li>
        </ul>        
        </td>
    </tr>
</table>

The "Entry Types" setting controls how different types of entries are display in the output.

In the output, change log entries are grouped by their type.
The display name configured for each entry type is used as heading for that group of entries.
If no display name is configured, the entry type itself is used instead.

In the default configuration, the following display names are pre-configured:

| Entry Type | Display Name |
|------------|--------------|
| `feat`     | New Features |
| `fix`      | Bug Fixes    |


---

⚠️ Using this setting in a configuration file will overwrite the default value.

It is not possible to just configured display names for additional entry types.
If you specify this setting in a configuration file, be sure to also include a display name for the preconfigured entry types.

---

## Example

The following example shows how to define a display name for entries of type `feat`, `fix` and `docs`:

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
- [Entry Types Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#entry-types)