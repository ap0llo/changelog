# Templates

Template allow customizing the generated changelog and generate
a changelog in different formats of for different environments.

The template to use can be configured using the `changelog:template:name`
setting (see [Configuration](./configuration.md#template-name) for details).

The following templates are supported:

- [Default](#default-template)

## Default Template

The default template renders the changelog to a Markdown file.
It is the most generic template and should work in most Markdown implementations.

The default template supports customizing serialization settings for
the generated markdown.
For details, see
[Markdown Preset (Default Template)](./configuration.md#markdown-preset-default-template).

## Version support

Support for templates were introduced in version 0.2.

## See Also

- [Configuration](./configuration.md)
