# Scope Settings

---

⚠️ The format of the "Scopes" setting has changed in version 0.3 of changelog.

For documentation on the "Scopes" setting in version 0.2, see [Scopes Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#scopes).

---

## Scope Display Name

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:scopes&lt;SCOPENAME&gt;:displayname</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__SCOPES__&lt;SCOPENAME&gt;__DISPLAYNAME</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>{ }</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td> 
        <td>0.3+</td>
    </tr>
</table>

The *Scope Display Name* setting configures how [Conventional Commit](https://www.conventionalcommits.org/) scopes are displayed.

By default, the scopes are included unchanged in the output.
Using this setting, you can configure a scope's display name which will be used in the output instead of the scope's name.

### Example

To set the display name for the `myScope` scope, in the configuration file, use

```json
{
    "changelog" : {
        "scopes" : {
            "myScope" : { "displayName":  "Scope Display Name" }
        }
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Scopes Setting (v0.2)](https://github.com/ap0llo/changelog/blob/release/v0.2/docs/configuration.md#scopes)