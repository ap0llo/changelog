# Parser Mode Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:parser:mode</code></td>
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
            <code>Loose</code>
        </td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>Loose</code></li>
                <li><code>Strict</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.2+</td>
    </tr>
</table>

The "Parser Mode" setting controls how lenient the commit message parser treats commit messages.

Available options are

- `Loose` (default)
- `Struct`

For details, see [Commit Message Parser](../../commit-message-parser.md)

## Example

The following example sets the parser mode to `strict`:

```json
{
    "changelog" : {
        "parser" : {
            "mode": "strict"
        }
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
