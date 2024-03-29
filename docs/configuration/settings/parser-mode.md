<!--
  <auto-generated>
    The contents of this file were generated by a tool.
    Any changes to this file will be overwritten.
    To change the content of this file, edit 'parser-mode.md.scriban'
  </auto-generated>
-->
# Parser Mode Setting

<table>
<tr>
    <td><b>Setting</b></td>
    <td><code>changelog:parser:mode</code></td>
</tr>
<tr>
    <td><b>Environment Variable</b></td>
    <td><code>CHANGELOG__PARSER__MODE</code></td>
</tr>
<tr>
    <td><b>Commandline Parameter</b></td>
    <td>
        -
    </td>
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
            <li><code>Strict</code></li>
            <li><code>Loose</code></li>
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
