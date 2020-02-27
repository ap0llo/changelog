using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChangeLogCreator.Git;
using NuGet.Versioning;

namespace ChangeLogCreator.ChangeLog
{
    internal class ChangeLogBuilder
    {

        public static IReadOnlyList<VersionInfo> GetVersions(IEnumerable<GitTag> tags)
        {
            static IEnumerable<VersionInfo> DoGetVersions(IEnumerable<GitTag> tags)
            {
                foreach(var tag in tags)
                {
                    var tagName = tag.Name.TrimStart('v'); //TODO: Prefix should be configurable
                    if (SemanticVersion.TryParse(tagName, out var version))
                        yield return new VersionInfo(version, tag);
                }
            }

            return DoGetVersions(tags).ToList();
        }
    }
}
