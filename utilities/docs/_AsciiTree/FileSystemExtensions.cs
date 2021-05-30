using Zio;

namespace docs
{
    internal static class FileSystemExtensions
    {
        public static string ToAsciiTree(this IFileSystem fileSystem)
        {
            void AddFolder(IFileSystem fileSystem, UPath path, AsciiTreeNode parentNode)
            {

                foreach (var directoryPath in fileSystem.EnumerateDirectories(path))
                {
                    var directoryNode = new AsciiTreeNode(directoryPath.GetName());
                    parentNode.Children.Add(directoryNode);

                    AddFolder(fileSystem, directoryPath, directoryNode);
                }

                foreach (var filePath in fileSystem.EnumerateFiles(path))
                {
                    parentNode.Children.Add(new AsciiTreeNode(filePath.GetName()));
                }
            }

            var rootNode = new AsciiTreeNode("<root>");
            AddFolder(fileSystem, UPath.Root, rootNode);

            var treeWriter = new AsciiTreeWriter();
            treeWriter.WriteNode(rootNode);
            return treeWriter.ToString();
        }
    }
}
