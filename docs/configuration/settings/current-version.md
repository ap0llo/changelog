# Current Version Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:currentVersion</code></td>
    </tr>
    <tr>
        <td><b>Environment Variable</b></td>
        <td><code>CHANGELOG__CURRENTVERSION</code></td>
    </tr>
    <tr>
        <td><b>Commandline Parameter</b></td>
        <td><code>currentVersion</code></td>
    </tr>
    <tr>
        <td><b>Default value</b></td>
        <td>-</td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.1+</td>
    </tr>
</table>

By default, versions are only read from a git repository's tags and only tagged versions are included in the change log. 
To include the currently checked out commit (the repository HEAD), you can specify the current version.
When specified, the current version is included in the change log as well.

The value must be a valid semantic version.

## See Also

[Configuration Overview](../../configuration.md)