
using Operations.Groupings.Basics;
using Operations.Groupings.Interface;

namespace Operations.Groupings.Types
{
    internal class Surface : IGrouping
    {
        private static Surface _surface = null;
        public static IGrouping Get()
        {
            if (_surface is null) { _surface = new Surface(); }
            return _surface;
        }
        private Surface() { }
        public GroupingTriangle FirstTriangle { get; set; }
        public bool EdgeFilter(IEnumerable<GroupingTriangle> triangles)
        {
            return triangles.Count() == 1;
        }

        public void SeedSet(GroupingTriangle triangle)
        {
            triangle.Seed = 0;
        }

        public bool TriangleFilter(GroupingTriangle a, GroupingTriangle b)
        {
            return true;
        }
    }
}
