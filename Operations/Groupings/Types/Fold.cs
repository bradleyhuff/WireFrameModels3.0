using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Groupings.Types
{
    internal class Fold : IGrouping
    {
        public GroupingTriangle FirstTriangle { get; set; }

        public bool EdgeFilter(GroupingTriangle t, IEnumerable<GroupingTriangle> triangles)
        {
            return triangles.Any();
        }

        public void SeedSet(GroupingTriangle triangle)
        {
            triangle.Seed = 0;
        }

        public bool TriangleFilter(GroupingTriangle a, GroupingTriangle b)
        {
            return !TriangleIsAtFold(a, b);
        }

        private bool TriangleIsAtFold(GroupingTriangle f, GroupingTriangle t)
        {
            if (PositionsMatch(f.A, f.B, t.A, t.B))
            {
                return IsAFold(f.A, f.B);
            }
            if (PositionsMatch(f.A, f.B, t.B, t.C))
            {
                return IsAFold(f.A, f.B);
            }
            if (PositionsMatch(f.A, f.B, t.C, t.A))
            {
                return IsAFold(f.A, f.B);
            }
            if (PositionsMatch(f.B, f.C, t.A, t.B))
            {
                return IsAFold(f.B, f.C);
            }
            if (PositionsMatch(f.B, f.C, t.B, t.C))
            {
                return IsAFold(f.B, f.C);
            }
            if (PositionsMatch(f.B, f.C, t.C, t.A))
            {
                return IsAFold(f.B, f.C);
            }
            if (PositionsMatch(f.C, f.A, t.A, t.B))
            {
                return IsAFold(f.C, f.A);
            }
            if (PositionsMatch(f.C, f.A, t.B, t.C))
            {
                return IsAFold(f.C, f.A);
            }
            if (PositionsMatch(f.C, f.A, t.C, t.A))
            {
                return IsAFold(f.C, f.A);
            }

            return false;
        }

        private bool PositionsMatch(PositionNormal fa, PositionNormal fb, PositionNormal ta, PositionNormal tb)
        {
            return (fa.PositionObject?.Id == ta.PositionObject?.Id && fb.PositionObject?.Id == tb.PositionObject?.Id) ||
                (fa.PositionObject?.Id == tb.PositionObject?.Id && fb.PositionObject?.Id == ta.PositionObject?.Id);
        }

        private bool IsAFold(PositionNormal fa, PositionNormal fb)
        {
            var triangles = fa.Triangles.IntersectBy(fb.Triangles.Select(t => t.Id), t => t.Id).ToArray();
            if (triangles.Length != 2) { return false; }

            var line = new Line3D(fa.Position, fb.Position);

            var oppositePointA = triangles[0].Positions.Single(p => p.Id != fa.Id && p.Id != fb.Id);
            var oppositePointB = triangles[1].Positions.Single(p => p.Id != fa.Id && p.Id != fb.Id);

            var projectionPointA = line.Projection(oppositePointA.Position);
            var projectionPointB = line.Projection(oppositePointB.Position);

            var vectorA = oppositePointA.Position - projectionPointA;
            var vectorB = oppositePointB.Position - projectionPointB;

            var angle = Vector3D.Angle(vectorA, vectorB);
            var isAFold = angle < 1.5;
            return isAFold;
        }
    }
}
