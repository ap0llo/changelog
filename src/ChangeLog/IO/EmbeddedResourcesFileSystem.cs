using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Zio;
using Zio.FileSystems;

namespace Grynwald.ChangeLog.IO
{
    /// <summary>
    /// Provides access to an assembly's embedded resources as virtual (read-only) file system
    /// </summary>
    public class EmbeddedResourcesFileSystem : FileSystem
    {
        private abstract class FileSystemEntry
        {
            public UPath Path { get; }

            public string Name => Path.GetName();

            public abstract FileAttributes Attributes { get; }


            public FileSystemEntry(UPath path)
            {
                if (path.IsEmpty || path.IsNull)
                    throw new ArgumentException("Path must not be null or empty", nameof(path));

                if (!path.IsAbsolute)
                    throw new ArgumentException("Path must be an absolute path", nameof(path));

                Path = path;
            }
        }

        private class Directory : FileSystemEntry
        {
            private List<File> m_Files = new();
            private List<Directory> m_Directories = new();

            public override FileAttributes Attributes => FileAttributes.ReadOnly | FileAttributes.Directory;


            public Directory(UPath path) : base(path)
            { }


            public IReadOnlyList<File> Files => m_Files;

            public IReadOnlyList<Directory> Directories => m_Directories;


            public void Add(Directory directory) => m_Directories.Add(directory);

            public void Add(File file) => m_Files.Add(file);
        }

        private class File : FileSystemEntry
        {
            public override FileAttributes Attributes => FileAttributes.ReadOnly;

            public string ResourceName { get; }


            public File(UPath path, string resourceName) : base(path)
            {
                if (String.IsNullOrWhiteSpace(resourceName))
                    throw new ArgumentException("Value must not be null or whitespace", nameof(resourceName));

                if (path == UPath.Root)
                    throw new ArgumentException($"'{path}' is not a valid file path", nameof(path));

                ResourceName = resourceName;
            }
        }


        protected const string s_ReadOnlyErrorMessage = "EmbeddedResources filesystem is read-only";

        private readonly IReadOnlyDictionary<UPath, FileSystemEntry> m_FileSystemEntries;
        private readonly Assembly m_Assembly;


        public EmbeddedResourcesFileSystem(Assembly assembly)
        {
            m_Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            m_FileSystemEntries = LoadFileSystem();
        }


        protected override UPath ConvertPathFromInternalImpl(string innerPath) => new UPath(innerPath);

        protected override string ConvertPathToInternalImpl(UPath path) => path.FullName;

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            IEnumerable<UPath> DoEnumerate(Directory directory, SearchPattern searchPattern, SearchOption searchOption, SearchTarget searchTarget)
            {
                if (searchTarget == SearchTarget.Directory || searchTarget == SearchTarget.Both)
                {
                    foreach (var subDirectory in directory.Directories)
                    {
                        if (searchPattern.Match(subDirectory.Name))
                            yield return subDirectory.Path;
                    }
                }

                if (searchTarget == SearchTarget.File || searchTarget == SearchTarget.Both)
                {
                    foreach (var file in directory.Files)
                    {
                        if (searchPattern.Match(file.Name))
                            yield return file.Path;
                    }
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (var subDirectory in directory.Directories)
                    {
                        foreach (var path in DoEnumerate(subDirectory, searchPattern, searchOption, searchTarget))
                        {
                            yield return path;
                        }
                    }
                }
            }

            if (TryGetFileSystemEntry(path, SearchTarget.Directory) is Directory directory)
            {
                var pattern = SearchPattern.Parse(ref path, ref searchPattern);
                return DoEnumerate(directory, pattern, searchOption, searchTarget);
            }
            else
            {
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist");
            }
        }

        protected override bool DirectoryExistsImpl(UPath path) => TryGetFileSystemEntry(path, SearchTarget.Directory) is not null;

        protected override bool FileExistsImpl(UPath path) => TryGetFileSystemEntry(path, SearchTarget.File) is not null;

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            if (TryGetFileSystemEntry(path, SearchTarget.Both) is FileSystemEntry entry)
            {
                return entry.Attributes;
            }

            throw new IOException($"File or directory '{path}' does not exist");
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            using var stream = OpenFile(path, FileMode.Open, FileAccess.Read);
            return stream.Length;
        }

        protected override bool CanWatchImpl(UPath path) => false;

        protected override IFileSystemWatcher WatchImpl(UPath path) =>
            throw new NotSupportedException("EmbeddedResources filesystem does not support watching");

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            if (mode != FileMode.Open)
            {
                throw new IOException(s_ReadOnlyErrorMessage);
            }

            if (access.HasFlag(FileAccess.Write))
            {
                throw new IOException(s_ReadOnlyErrorMessage);
            }

            if (TryGetFileSystemEntry(path, SearchTarget.File) is File file)
            {
                return m_Assembly.GetManifestResourceStream(file.ResourceName)!;
            }

            throw new FileNotFoundException($"File '{path}' does not exist");
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            throw new NotSupportedException($"EmbeddedResources does not support operation {nameof(IFileSystem.GetCreationTime)}");
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            throw new NotSupportedException($"EmbeddedResources does not support operation {nameof(IFileSystem.GetLastAccessTime)}");
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            throw new NotSupportedException($"EmbeddedResources does not support operation {nameof(IFileSystem.GetLastWriteTime)}");
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void DeleteFileImpl(UPath path)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(s_ReadOnlyErrorMessage);
        }


        private IReadOnlyDictionary<UPath, FileSystemEntry> LoadFileSystem()
        {
            //TODO: Handle resource names that only differ in casing
            //TODO: Handle duplicate resource names

            var root = new Directory(UPath.Root);

            var entries = new Dictionary<UPath, FileSystemEntry>();
            entries.Add(root.Path, root);

            foreach (var resourceName in m_Assembly.GetManifestResourceNames())
            {
                var normalizedName = resourceName.Replace("\\", "/");
                while (normalizedName.Contains("//"))
                {
                    normalizedName = normalizedName.Replace("//", "/");
                }

                var path = normalizedName.Split('/');

                var currentDir = root;
                for (var i = 0; i < path.Length - 1; i++)
                {
                    var newDirPath = currentDir.Path / path[i];

                    if (!entries.ContainsKey(newDirPath))
                    {
                        entries.Add(newDirPath, new Directory(newDirPath));
                    }

                    var newDir = (Directory)entries[newDirPath];
                    currentDir.Add(newDir);
                    currentDir = newDir;
                }

                var file = new File(currentDir.Path / path[path.Length - 1], resourceName);
                entries.Add(file.Path, file);
                currentDir.Add(file);
            }

            return entries;
        }

        private FileSystemEntry? TryGetFileSystemEntry(UPath path, SearchTarget searchTarget)
        {
            foreach (var currentPath in m_FileSystemEntries.Keys)
            {
                if (UPath.DefaultComparerIgnoreCase.Compare(currentPath, path) == 0)
                {
                    var entry = m_FileSystemEntries[currentPath];
                    return (searchTarget, entry) switch
                    {
                        (SearchTarget.Both, _) => entry,
                        (SearchTarget.Directory, Directory dir) => entry,
                        (SearchTarget.File, File file) => entry,
                        _ => null
                    };
                }
            }

            return null;
        }
    }
}
