using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionEdge
    {
        public PositionEdge(PositionNormal a, PositionNormal b)
        {
            A = a;
            B = b;
            Key = new Combination2(a.PositionObject.Id, b.PositionObject.Id);
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

        private LineSegment3D _segment = null;

        public LineSegment3D Segment
        {
            get
            {
                if (_segment is null) { _segment = new LineSegment3D(A.Position, B.Position); }
                return _segment;
            }
        }

        public Triangle3D Plot
        {
            get
            {
                return new Triangle3D(Segment.Start, Segment.Center, Segment.End);
            }
        }

        public Combination2 Key { get; }
        public Combination2 Cardinality
        {
            get { return new Combination2(A.PositionObject?.Cardinality ?? 0, B.PositionObject?.Cardinality ?? 0); }
        }

        public bool ContainsPosition(Position position)
        {
            return A.PositionObject?.Id == position.Id || B.PositionObject?.Id == position.Id;
        }
    }
}
