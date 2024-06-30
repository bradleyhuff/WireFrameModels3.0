using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
namespace Operations.Groupings.Basics
{
    public class GroupEdge
    {
        internal GroupEdge(PositionNormal a, PositionNormal b)
        {
            A = a;
            B = b;
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

        public LineSegment3D Segment
        {
            get { return new LineSegment3D(A.Position, B.Position); }
        }
    }
}
