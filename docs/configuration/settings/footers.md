# Footers Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:footers</code></td>
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

The *Footers* setting configures how [Conventional Commit](https://www.conventionalcommits.org/) footers are displayed.
By default, footers are included unchanged in the output.
Using this setting, you can configure a footer's display name.
When a display name is configured, it will be used in the output instead of the footer's name.

## Example

```json
{
    "changelog" : {
        "footers" : [
            { "name":  "see-also", "displayName":  "See Also" }
        ]
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
