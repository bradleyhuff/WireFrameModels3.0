
using Collections.WireFrameMesh.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.Interface;

namespace Operations.Groupings.Types
{
    internal class Face : IGrouping
    {
        private static Face _face = null;
        public static IGrouping Get()
        {
            if (_face is null) { _face = new Face(); }
            return _face;
        }
        private Face() { }
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
            return !TriangleIsAtEdge(a, b);
        }

        private bool TriangleIsAtEdge(GroupingTriangle f, GroupingTriangle t)
        {
            if (PositionsMatch(f.A, f.B, t.A, t.B))
            {
                return !IndiciesMatch(f.A, f.B, t.A, t.B);
            }
            if (PositionsMatch(f.A, f.B, t.B, t.C))
            {
                return !IndiciesMatch(f.A, f.B, t.B, t.C);
            }
            if (PositionsMatch(f.A, f.B, t.C, t.A))
            {
                return !IndiciesMatch(f.A, f.B, t.C, t.A);
            }
            if (PositionsMatch(f.B, f.C, t.A, t.B))
            {
                return !IndiciesMatch(f.B, f.C, t.A, t.B);
            }
            if (PositionsMatch(f.B, f.C, t.B, t.C))
            {
                return !IndiciesMatch(f.B, f.C, t.B, t.C);
            }
            if (PositionsMatch(f.B, f.C, t.C, t.A))
            {
                return !IndiciesMatch(f.B, f.C, t.C, t.A);
            }
            if (PositionsMatch(f.C, f.A, t.A, t.B))
            {
                return !IndiciesMatch(f.C, f.A, t.A, t.B);
            }
            if (PositionsMatch(f.C, f.A, t.B, t.C))
            {
                return !IndiciesMatch(f.C, f.A, t.B, t.C);
            }
            if (PositionsMatch(f.C, f.A, t.C, t.A))
            {
                return !IndiciesMatch(f.C, f.A, t.C, t.A);
            }

            return true;
        }

        private bool PositionsMatch(PositionNormal fa, PositionNormal fb, PositionNormal ta, PositionNormal tb)
        {
            return (fa.PositionObject?.Id == ta.PositionObject?.Id && fb.PositionObject?.Id == tb.PositionObject?.Id) ||
                (fa.PositionObject?.Id == tb.PositionObject?.Id && fb.PositionObject?.Id == ta.PositionObject?.Id);
        }

        private bool IndiciesMatch(PositionNormal fa, PositionNormal fb, PositionNormal ta, PositionNormal tb)
        {
            return (fa.Id == ta.Id && fb.Id == tb.Id) ||
                (fa.Id == tb.Id && fb.Id == ta.Id);
        }
    }
}
