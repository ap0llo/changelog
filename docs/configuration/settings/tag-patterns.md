# Tag Patterns Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:tagpatterns</code></td>
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
        <td><code>[ "^(?&lt;version&gt;\d+\.\d+\.\d+.*)", "^v(?&lt;version&gt;\d+\.\d+\.\d+.*)" ]</code></td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

The *Tag Patterns* setting controls how versions are read from a git repository's tags.
The setting defines a list of regular expressions that are used to extract the version from the tag name.
All regular expressions must define a `version` sub-expression which matches the version. 
The matched value must be a valid [semantic version](https://semver.org/).

The default setting matches tag names that are semantic versions or tags names that are semantic versions prefixed with `v`.

## See Also

- [Configuration Overview](../../configuration.md)
