
using Operations.Groupings.Basics;
using Operations.Groupings.Interface;

namespace Operations.Groupings.Types
{
    internal class Cluster : IGrouping
    {
        public GroupingTriangle FirstTriangle { get; set; }
        public bool EdgeFilter(IEnumerable<GroupingTriangle> triangles)
        {
            return triangles.Any();
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
