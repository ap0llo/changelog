# Filter Setting

<table>
    <tr>
        <td><b>Setting</b></td>
        <td><code>changelog:filter</code></td>
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
        <code>
        {
            "include": [
                { "type": "feat" },
                { "type": "fix" }
            ],
            "exclude": []
        }
        </code>
        </td>
    </tr>
    <tr>
        <td><b>Version Support</b></td>
        <td>0.3+</td>
    </tr>
</table>

The "Filter" setting controls which entries of a change log are included in the output.

By default, all entries of type `feat` and `fix` are included.

---

⚠️ Using this setting in a configuration file will overwrite the default value.
To also include changes of type `feat` and `fix`, you need to include the default value in your configuration file.

---

ℹ️ Change log entries that contain breaking changes are always included, regardless of this setting.

---

## Filtering Syntax

The filter configuration differentiates between:

- [*Filter Expressions*](#filter-expressions): A configuration to match a entry.
- [*Include Expressions:*](#include-expressions) One or more filter expressions specifying which entries to include in the output.
- [*Exclude Expressions:*](#exclude-expressions) One or more filter expressions specifying which entries **not** to include in the output.

The filter is applied in a two-stage process:

1. For the changelog, all entries that match **any** of the *Include Expressions* are selected
2. Of the selected entries, all entries that match **none** of the *Exclude Expressions* are selected as output of the filter

For every *Filter Expression* (regardless of whether the expression is used as *include* or *exclude* expression), an entry must fulfil **all** the conditions specified by the expression in order to match.

### Filter Expressions

A single *filter expression* specifies an entry type and/or scope to match.
In order for a entry to be *matched*, the entry needs to fulfil **all** conditions specified by the filter expression (the filter expression's conditions hence are logically combined with an *AND* operator).

A filter expression can specify conditions for an entry's *type* or *scope* (for a definition see [Conventional Commits](https://www.conventionalcommits.org/)).
For both conditions, wildcards are supported.
If not set explicitly in the filter expression, both type and scope of a single filter expression will use a default value of `*` (matching any entry).

The following examples illustrate different filter expressions:

<table>
    <tr>
        <th>Expression</th>
        <th>Description</th>
    </tr>
    <tr>
        <td><code>{ "type" : "*", "scope" : "*" }</code></td>
        <td>Matches any entry.</td>
    </tr>
    <tr>
        <td><code>{ "type" : "*" }</code></td>
        <td>Matches any entry. Because the default condition for scope is <code>*</code>, this is equivalent to the example above.</td>
    </tr>    
    <tr>
        <td><code>{ "type" : "feat",  "scope" : "*" }</code></td>
        <td>Matches any entry of type `feat` regardless of the scope.</td>
    </tr>
    <tr>
        <td><code>{ "type" : "*", "scope" : "api-*" }</code></td>
        <td>Matches any entry which's scope starts with <code>api-</code>.</td>
    </tr>
</table>

### Include Expressions

A filter can specify one or more *Filter Expressions* to use as *Include Expression*.
In contrast to individual filter expressions, a entry does not have to fulfil *all* include expressions in order to be included in the output.
Instead, a entry is *included* if it matches **any** of the include filter expressions (a logical *OR*).

### Exclude Expressions

A filter can specify one or more *Filter Expressions* to use as *Exclude Expression*.
Analogous to *Include*, *Exclude Expressions*  remove ab entry if **any** of the individual expressions match an entry.

Exclude expressions represent the second stage of filtering and are applied *after* the *Include* expressions.

## Example

The following example shows how to define a filter that includes all entries of type `feat`, `fix` or `docs` *unless* the scope is `internal`:

```json
{
    "changelog" :{
        "filter" :{
            "include" : [
                { "type": "feat" },
                { "type": "fix" },
                { "type": "docs" }
            ],
            "exclude" : [
                { "scope" : "internal" }
            ]
        }
    }
}
```

## See Also

- [Configuration Overview](../../configuration.md)
- [Conventional Commits](https://www.conventionalcommits.org/)