
using BasicObjects.GeometricObjects;
using Operations.Groupings.Basics;
using Operations.Groupings.Interface;
using Double = BasicObjects.Math.Double;

namespace Operations.Groupings.Types
{
    internal class Planar : IGrouping
    {
        public GroupingTriangle FirstTriangle { get; set; }
        public bool EdgeFilter(IEnumerable<GroupingTriangle> triangles)
        {
            return true;
        }

        public void SeedSet(GroupingTriangle triangle)
        {
            GetSeedValue(triangle);
        }

        public bool TriangleFilter(GroupingTriangle a, GroupingTriangle b)
        {
            return IsOnPlane(a, b, FirstTriangle.Triangle.Plane);
        }

        private bool IsOnPlane(GroupingTriangle f, GroupingTriangle t, Plane plane)
        {
            return f.Triangle.IsOnPlane(plane) && t.Triangle.IsOnPlane(plane);
        }

        private void GetSeedValue(GroupingTriangle triangle)
        {
            var minHeight = Math.Max(triangle.Triangle.MinHeight, Double.DifferenceError);
            var heightIndex = -Math.Log10(minHeight) * 100;
            triangle.Seed = (int)heightIndex;
        }
    }
}
