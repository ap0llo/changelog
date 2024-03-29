<!--
  <auto-generated>
    The contents of this file were generated by a tool.
    Any changes to this file will be overwritten.
    To change the content of this file, edit 'version-range.md.scriban'
  </auto-generated>
-->
# Version Range Setting

<table>
<tr>
    <td><b>Setting</b></td>
    <td><code>changelog:versionRange</code></td>
</tr>
<tr>
    <td><b>Environment Variable</b></td>
    <td><code>CHANGELOG__VERSIONRANGE</code></td>
</tr>
<tr>
    <td><b>Commandline Parameter</b></td>
    <td>
        <code>versionRange</code>
    </td>
</tr>
<tr>
    <td><b>Default value</b></td>
    <td>
        -
    </td>
</tr>

<tr>
    <td><b>Version Support</b></td>
    <td>0.1+</td>
</tr>
</table>

By default, **all versions** are included in the change log.
To limit the versions to include, you can specify a version range using this setting.

The value must be a valid [NuGet Version Range](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges)

## Example

To only include versions newer than version `2.1` in the change log, use the
following version range:

```json
{
    "changelog" : {
        "versionRange" : "[2.1, )"
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
