using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Frameworks;
using Nuke.Common.Utilities.Collections;

static class TrxFile
{
    public sealed class TrxAssemblyInfo : IEquatable<TrxAssemblyInfo>
    {
        public string Path { get; }

        public string Name { get; }

        public NuGetFramework? Framework { get; }

        public TrxAssemblyInfo(string path, string name, NuGetFramework? framework)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be null or whitespace", nameof(path));

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            Path = path;
            Name = name;
            Framework = framework;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals([AllowNull] object obj) => Equals(obj as TrxAssemblyInfo);

        public bool Equals([AllowNull] TrxAssemblyInfo other)
        {
            return other != null &&
                StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) &&
                StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name);
        }
    }




    static readonly XNamespace s_TrxNamespace = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

    public static IEnumerable<string> GetAssemblyPaths(string trxFilePath)
    {
        var document = XDocument.Load(trxFilePath);

        return document.Descendants(s_TrxNamespace.GetName("TestMethod"))
            .Select(x => x.Attribute("codeBase")?.Value)
            .WhereNotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)!;
    }

    public static IEnumerable<TrxAssemblyInfo> GetAssemblyInfos(string trxFilePath)
    {
        return GetAssemblyPaths(trxFilePath)
            .Select(path =>
            {
                // asummes target framework is appended to output path
                var folderName = Path.GetFileName(Path.GetDirectoryName(path));
                NuGetFramework? framework;
                try
                {
                    framework = NuGetFramework.ParseFolder(folderName);
                }
                catch (Exception)
                {
                    framework = null;
                }

                var name = Path.GetFileName(path);
                return new TrxAssemblyInfo(path, name, framework);
            })
            .Distinct();
    }
}

