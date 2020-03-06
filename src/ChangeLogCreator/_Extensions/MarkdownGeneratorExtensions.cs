using Grynwald.MarkdownGenerator;

namespace ChangeLogCreator
{
    internal static class MarkdownGeneratorExtensions
    {
        public static MdHeading AddHeading(this MdContainerBlock container, int level, MdSpan text, string id)
        {
            var heading = new MdHeading(level, text) { Anchor = id };
            container.Add(heading);
            return heading;
        }

        public static MdBulletList AddBulletList(this MdContainerBlock container)
        {
            var list = new MdBulletList();
            container.Add(list);
            return list;
        }
    }
}
