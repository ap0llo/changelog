# GitLab Release Template

The GitLab Release Template renders a change log suited to be used as the description of a [GitLab Release](https://docs.gitlab.com/ee/user/project/releases/).

This template **only supports including the changes of a single version**, so it should be combined with the [Version Range setting](../configuration/settings/version-range.md).
Compared to the default template, the GitLab Release template omits the "Change Log" and version headings and adjusts the heading levels so the changelog can is properly rendered in the Releases view of the GitLab web interface.

**Note:** The GitLab Release template is independent of the [GitLab Integration](../integrations/gitlab.md) for links.
Both features can be used independently of each other.

For configuration options, see [GitLabRelease Template Settings](../configuration/settings/gitlabrelease-template.md).

## Version support

Support for templates and the "GitLab Release" template was introduced in version 0.2.  

## See Also
 
- [Templates Overview](./README.md)
- [GitLabRelease Template Settings](../configuration/settings/gitlabrelease-template.md)
- [GitLab Releases](https://docs.gitlab.com/ee/user/project/releases/)
