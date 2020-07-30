# Markdown Preset Setting

⚠️ **The Markdown Preset setting was removed in version 0.2.16:**
With the introduction of templates, output settings have become template-specific.
To set the preset for the default template use the [Markdown Preset (Default Template)](#markdown-preset-default-template) setting.

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:markdown:preset</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__MARKDOWN__PRESET</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>default</code></td>
    </tr>
    <tr>
        <td><b>Allowed values</b></td>
        <td>
            <ul>
                <li><code>default</code></li>
                <li><code>MkDocs</code></li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td> 0.1, Removed in version 0.2</td>
    </tr>
</table>

The *Markdown Preset* setting can be used to customize the serialization of Markdown.

Supported values are:

- `default`: Produces Markdown that should work in most environments, including GitHub and GitLab
- `MkDocs`: Produces Markdown optimized for being rendered by Python-Markdown and [MkDocs](https://www.mkdocs.org/)

For details on the differences between the presets, see also [Markdown Generator docs](https://github.com/ap0llo/markdown-generator/blob/master/docs/apireference/Grynwald/MarkdownGenerator/MdSerializationOptions/Presets/index.md)

## Example

```json
{
    "changelog" : {
        "markdown" : {
            "preset" : "MkDocs"
        }
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
