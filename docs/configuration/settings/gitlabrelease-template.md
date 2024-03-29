<!--
  <auto-generated>
    The contents of this file were generated by a tool.
    Any changes to this file will be overwritten.
    To change the content of this file, edit 'gitlabrelease-template.md.scriban'
  </auto-generated>
-->
# GitLabRelease Template Settings

This pages describes the configuration options of the [GitHub Release Template](../../templates/gitlabrelease.md).

## Normalize References

<table>
<tr>
    <td><b>Setting</b></td>
    <td><code>changelog:template:gitlabrelease:normalizeReferences</code></td>
</tr>
<tr>
    <td><b>Environment Variable</b></td>
    <td><code>CHANGELOG__TEMPLATE__GITLABRELEASE__NORMALIZEREFERENCES</code></td>
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
        <code>true</code>
    </td>
</tr>

<tr>
    <td><b>Version Support</b></td>
    <td>0.3+</td>
</tr>
</table>

The *Normalize References* settings controls whether references in the change log are normalized when using the GitLabRelease template.

See [Reference Normalization](../../auto-references.md#normalization) for details.

## Custom Directory

<table>
<tr>
    <td><b>Setting</b></td>
    <td><code>changelog:template:gitlabrelease:customDirectory</code></td>
</tr>
<tr>
    <td><b>Environment Variable</b></td>
    <td><code>CHANGELOG__TEMPLATE__GITLABRELEASE__CUSTOMDIRECTORY</code></td>
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
        -
    </td>
</tr>

<tr>
    <td><b>Version Support</b></td>
    <td>0.4+</td>
</tr>
</table>

The "Custom Directory" settings allows specifying the path for a directory that contains customizations for the template.
For details see [Customization (GitLab Release Template)](../../templates/gitlabrelease.md#customization)

## See Also

- [GitLab Release Template](../../templates/gitlabrelease.md)
- [Templates Overview](../../templates/README.md)
- [Configuration Overview](../../configuration.md)
- [Template Name Setting](./template-name.md)
