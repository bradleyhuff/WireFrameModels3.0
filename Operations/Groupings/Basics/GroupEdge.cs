using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
namespace Operations.Groupings.Basics
{
    public class GroupEdge
    {
        internal GroupEdge(PositionNormal a, PositionNormal b)
        {
            A = a;
            B = b;
            Key = new Combination2(A.PositionObject.Id, B.PositionObject.Id);
        }
        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
            }
        }

        public Combination2 Key { get; }

        public bool IsDegenerate
        {
            get
            {
                return A.Id == B.Id;
            }
        }

        public IEnumerable<PositionTriangle> Triangles
        {
            get
            {
                return A.PositionObject.Triangles.IntersectBy(B.PositionObject.Triangles.Select(t => t.Id), t => t.Id);
            }
        }

        public bool IsOpenEdge
        {
            get { return Triangles.Count() < 2; }
        }

        public bool IsIntersectingEdge
        {
            get { return Triangles.Count() > 2; }
        }

        public LineSegment3D Segment
        {
            get { return new LineSegment3D(A.Position, B.Position); }
        }
    }
}
