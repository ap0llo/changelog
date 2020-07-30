# Scopes Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:scopes</code></td>
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
        <td><em>Empty</em></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *Scopes* setting configures how [Conventional Commit](https://www.conventionalcommits.org/) scopes are displayed.
By default, the scopes are included unchanged in the output.
Using this setting, you can configure a scope's display name.
When a display name is configured, it will be used in the output instead of the scope's name.

## Example

```json
{
    "changelog" : {
        "scopes" : [
            { "name":  "myScope", "displayName":  "Scope Display Name" }
        ]
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)