using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Grynwald.ChangeLog.IO;
using Xunit;
using Zio;

namespace Grynwald.ChangeLog.Test.IO
{
    /// <summary>
    /// Tests for <see cref="EmbeddedResourcesFileSystem"/>
    /// </summary>
    public class EmbeddedResourcesFileSystemTest
    {
        public class ConvertPathFromInternal
        {
            [Theory]
            [InlineData("/some/path")]
            public void Returns_expected_path(string systemPath)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                var expected = (UPath)systemPath;

                // ACT 
                var actual = sut.ConvertPathFromInternal(systemPath);

                // ASSERT
                Assert.Equal(expected, actual);
            }
        }

        public class ConvertPathToInternal
        {
            [Theory]
            [InlineData("/some/path")]
            public void Returns_expected_path(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                var actual = sut.ConvertPathToInternal((UPath)path);

                // ASSERT
                Assert.Equal(path, actual);
            }
        }

        public class FileExists
        {
            [Theory]
            [InlineData("/file1.txt", true)]
            [InlineData("/FILE1.txt", true)]
            [InlineData("/DIR1/file2.txt", true)]
            [InlineData("/does-not-exist.txt", false)]
            [InlineData("/DOES-NOT-EXIST.TXT", false)]
            [InlineData("/dir1/does-not-exist.txt", false)]
            [InlineData("/DIR1/does-not-exist.txt", false)]
            [InlineData("/dir1/../file1.txt", true)]
            [InlineData("/dir1/../dir1/file2.txt", true)]
            [InlineData("/dir1", false)]
            public void Returns_expected_value(string path, bool expected)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                var actual = sut.FileExists(path);

                // ASSERT
                Assert.Equal(expected, actual);
            }
        }

        public class DirectoryExists
        {
            [Theory]
            [InlineData("/", true)]
            [InlineData("/dir1/..", true)]
            [InlineData("/dir1/../dir1", true)]
            [InlineData("/DIR1", true)]
            [InlineData("/dir2", false)]
            [InlineData("/DIR2", false)]
            [InlineData("/file1.txt", false)]
            [InlineData("/dir1/file2.txt", false)]
            public void Returns_expected_value(string path, bool expected)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                var actual = sut.DirectoryExists(path);

                // ASSERT
                Assert.Equal(expected, actual);
            }
        }

        public class GetAttributes
        {
            [Theory]
            [InlineData("/file1.txt", FileAttributes.ReadOnly)]
            [InlineData("/file2.txt", FileAttributes.ReadOnly)]
            [InlineData("/dir1/file2.txt", FileAttributes.ReadOnly)]
            [InlineData("/", FileAttributes.ReadOnly | FileAttributes.Directory)]
            [InlineData("/dir1", FileAttributes.ReadOnly | FileAttributes.Directory)]
            public void Returns_expected_attributes(string path, FileAttributes expected)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                var actual = sut.GetAttributes(path);

                // ASSERT
                Assert.Equal(expected, actual);
            }

            [Theory]
            [InlineData("/does-not-exist.txt")]
            [InlineData("/dir1/does-not-exist.txt")]
            public void Throws_IOException_if_file_or_directory_does_not_exist(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                Action act = () => sut.GetAttributes(path);

                // ACT 
                var ex = Record.Exception(act);

                // ASSERT
                Assert.IsType<IOException>(ex);
                Assert.Contains("does not exist", ex.Message);
            }
        }

        public class CanWatch
        {
            [Theory]
            [InlineData("/file1.txt")]
            [InlineData("/file2.txt")]
            [InlineData("/dir1/file2.txt")]
            [InlineData("/")]
            [InlineData("/does-not-exist")]
            public void Returns_false(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT / ASSERT
                Assert.False(sut.CanWatch(path));
            }

        }

        public class Watch
        {
            [Theory]
            [InlineData("/file1.txt")]
            [InlineData("/file2.txt")]
            [InlineData("/dir1/file2.txt")]
            [InlineData("/")]
            [InlineData("/does-not-exist")]
            public void Throws_NotSupportedException(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                Action act = () => sut.Watch(path);

                // ACT
                var ex = Record.Exception(act);

                // ASSERT
                Assert.IsType<NotSupportedException>(ex);
            }
        }

        public class OpenFile
        {
            [Fact]
            public void Throws_IOException_for_write_access()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT / ASSERT
                Assert.Throws<IOException>(() => sut.OpenFile("/file", FileMode.Open, FileAccess.Write, FileShare.None));
                Assert.Throws<IOException>(() => sut.OpenFile("/file", FileMode.Open, FileAccess.ReadWrite, FileShare.None));

                foreach (var fileMode in Enum.GetValues(typeof(FileMode)).Cast<FileMode>())
                {
                    if (fileMode == FileMode.Open)
                        continue;

                    Assert.Throws<IOException>(() => sut.OpenFile("/file", fileMode, FileAccess.Read, FileShare.None));
                }
            }

            [Theory]
            [InlineData("/file1.txt")]
            [InlineData("/file2.txt")]
            [InlineData("/dir1/file2.txt")]
            public void Succeeds_for_read_access(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                using var stream = sut.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.None);

                // ASSERT
                Assert.False(stream.CanWrite);
                Assert.True(stream.CanRead);
            }

            [Theory]
            [InlineData("/does-not-exist")]
            [InlineData("/dir1")]
            [InlineData("/")]
            public void FileNotFoundException_if_file_does_not_exist(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                Action act = () => sut.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.None);

                // ACT 
                var ex = Record.Exception(act);

                // ASSERT
                Assert.IsType<FileNotFoundException>(ex);
            }
        }

        public class GetFileLength
        {
            [Theory]
            [InlineData("/file1.txt", 15L)]
            [InlineData("/file2.txt", 15L)]
            [InlineData("/dir1/file2.txt", 3L)]
            public void Returns_expected_length(string path, long expected)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT 
                var actual = sut.GetFileLength(path);

                // ASSERT
                Assert.Equal(expected, actual);
            }

            [Theory]
            [InlineData("/does-not-exist")]
            [InlineData("/dir1")]
            [InlineData("/")]
            public void Throws_FileNotFoundException_if_file_does_not_exist(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                Action act = () => sut.GetFileLength(path);

                // ACT 
                var ex = Record.Exception(act);

                // ASSERT
                Assert.IsType<FileNotFoundException>(ex);
            }
        }

        public class EnumeratePaths
        {
            [Theory]
            // Base test cases: get both files and directories without recursion
            [InlineData("T01", "/", "*", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { "/dir1", "/file1.txt", "/file2.txt" })]
            [InlineData("T02", "/dir1", "*", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { "/dir1/file2.txt" })]
            // Use denormalized paths
            [InlineData("T03", "/dir1/../", "*", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { "/dir1", "/file1.txt", "/file2.txt" })]
            [InlineData("T04", "/dir1/../dir1", "*", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { "/dir1/file2.txt" })]
            // Specify search-pattern
            [InlineData("T05", "/", "file1.*", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { "/file1.txt" })]
            [InlineData("T06", "/dir1", "*.xml", SearchOption.TopDirectoryOnly, SearchTarget.Both, new string[] { })]
            // Get all items recursively
            [InlineData("T07", "/", "*", SearchOption.AllDirectories, SearchTarget.Both, new string[] { "/dir1", "/file1.txt", "/file2.txt", "/dir1/file2.txt" })]
            [InlineData("T08", "/dir1", "*", SearchOption.AllDirectories, SearchTarget.Both, new string[] { "/dir1/file2.txt" })]
            // Get files recursively
            [InlineData("T09", "/", "*", SearchOption.AllDirectories, SearchTarget.File, new string[] { "/file1.txt", "/file2.txt", "/dir1/file2.txt" })]
            [InlineData("T10", "/dir1", "*", SearchOption.AllDirectories, SearchTarget.File, new string[] { "/dir1/file2.txt" })]
            // Get directories recursively
            [InlineData("T11", "/", "*", SearchOption.AllDirectories, SearchTarget.Directory, new string[] { "/dir1" })]
            [InlineData("T12", "/dir1", "*", SearchOption.AllDirectories, SearchTarget.Directory, new string[] { })]
            // Get only files
            [InlineData("T13", "/", "*", SearchOption.TopDirectoryOnly, SearchTarget.File, new string[] { "/file1.txt", "/file2.txt" })]
            [InlineData("T14", "/dir1", "*", SearchOption.TopDirectoryOnly, SearchTarget.File, new string[] { "/dir1/file2.txt" })]
            // Get only directories
            [InlineData("T15", "/", "*", SearchOption.TopDirectoryOnly, SearchTarget.Directory, new string[] { "/dir1" })]
            [InlineData("T16", "/dir1", "*", SearchOption.TopDirectoryOnly, SearchTarget.Directory, new string[] { })]
            public void Returns_expected_values(string id, string path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget, string[] expected)
            {
                // ARRANGE
                _ = id;
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                var expectedPaths = expected.Select(x => (UPath)x).ToHashSet();

                // ACT
                var actualPaths = sut.EnumeratePaths(path, searchPattern, searchOption, searchTarget);

                // ASSERT
                Assert.True(expectedPaths.SetEquals(actualPaths));
            }

            [Theory]
            [InlineData("/file1.txt")]
            [InlineData("/does-not-exist")]
            public void Throws_DirectoryNotFoundException_when_directory_does_not_exist(string path)
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
                Action act = () => sut.EnumeratePaths(path, "*", SearchOption.AllDirectories, SearchTarget.Both).ToArray();

                // ACT 
                var ex = Record.Exception(act);

                // ASSERT
                Assert.IsType<DirectoryNotFoundException>(ex);
            }

        }

        public class CreateDirectory
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.CreateDirectory("/dir2"));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class DeleteDirectory
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.DeleteDirectory("/dir1", isRecursive: false));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class MoveDirectory
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.MoveDirectory("/dir1", "/dir2"));

                // ASSERT
                Assert.IsType<IOException>(ex);

            }
        }

        public class CopyFile
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.CopyFile("/file1.txt", "/to", overwrite: false));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class DeleteFile
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.DeleteFile("/file1.txt"));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class MoveFile
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.MoveFile("/file1.txt", "/to"));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class ReplaceFile
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.ReplaceFile("/file1.txt", "/file2.txt", "/backup", false));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class SetAttributes
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.SetAttributes("/file1.txt", FileAttributes.Normal));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class SetCreationTime
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.SetCreationTime("/file1.txt", new DateTime(2020, 01, 01)));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class SetLastAccessTime
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.SetLastAccessTime("/file1.txt", new DateTime(2020, 01, 01)));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class SetLastWriteTime
        {
            [Fact]
            public void Throws_IOException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.SetLastWriteTime("/file1.txt", new DateTime(2020, 01, 01)));

                // ASSERT
                Assert.IsType<IOException>(ex);
            }
        }

        public class GetCreationTime
        {
            [Fact]
            public void Throws_NotSupportedException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.GetCreationTime("/file1.txt"));

                // ASSERT
                Assert.IsType<NotSupportedException>(ex);
            }
        }

        public class GetLastAccessTime
        {
            [Fact]
            public void Throws_NotSupportedException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.GetLastAccessTime("/file1.txt"));

                // ASSERT
                Assert.IsType<NotSupportedException>(ex);
            }
        }

        public class GetLastWriteTime
        {
            [Fact]
            public void Throws_NotSupportedException()
            {
                // ARRANGE
                var sut = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

                // ACT
                var ex = Record.Exception(() => sut.GetLastWriteTime("/file1.txt"));

                // ASSERT
                Assert.IsType<NotSupportedException>(ex);
            }
        }
    }
}
