# Output Path Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:outputPath</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__OUTPUTPATH</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>outputPath</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td><code>changelog.md</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

Specifies the output path for the generated change log.

When the value of the setting is a relative path, the path is interpreted to be relative to

- The repository root directory when setting the output path in the configuration file or through environment variables.
- The current working directory when specifying the output path using the `outputPath` commandline parameter.

## Example

```json
{
    "changelog" : {
        "outputPath" : "my-custom-output-path.md"
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
