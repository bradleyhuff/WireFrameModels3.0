using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionEdge : IBox
    {
        public PositionEdge(PositionNormal a, PositionNormal b, PositionTriangle triangle)
        {
            A = a;
            B = b;
            Triangle = triangle;
            Key = new Combination2(a.PositionObject.Id, b.PositionObject.Id);
            SurfaceKey = new Combination2(a.Id, b.Id);
        }

        public PositionNormal A { get; }
        public PositionNormal B { get; }
        public PositionTriangle Triangle { get; }

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
        public Combination2 SurfaceKey { get; }
        public Combination2 Cardinality
        {
            get { return new Combination2(A.PositionObject?.Cardinality ?? 0, B.PositionObject?.Cardinality ?? 0); }
        }

        public IReadOnlyList<PositionTriangle> Triangles
        {
            get
            {
                return A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                    Intersect(B.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).ToList();
            }
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(Segment.Start.Margin(BoxBucket.MARGINS), Segment.End.Margin(BoxBucket.MARGINS));
                }
                return _box;
            }
        }

        public bool ContainsPosition(Position position)
        {
            return A.PositionObject?.Id == position.Id || B.PositionObject?.Id == position.Id;
        }
    }
}
