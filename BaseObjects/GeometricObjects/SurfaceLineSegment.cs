
using BaseObjects.Transformations.Interfaces;

namespace BasicObjects.GeometricObjects
{
    public class SurfaceLineSegment : IShape3D<SurfaceLineSegment>
    {
        public SurfaceLineSegment(Ray3D a, Ray3D b)
        {
            A = a;
            B = b;
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public Ray3D A { get; }
        public Ray3D B { get; }
        public IEnumerable<Ray3D> Rays
        {
            get
            {
                yield return A;
                yield return B;
            }
        }
        public LineSegment3D Segment { get; }

        private Ray3D _center = null;
        public Ray3D Center
        {
            get
            {
                if (_center is null)
                {
                    Point3D point = new Point3D((A.Point.X + B.Point.X) * 0.5, (A.Point.Y + B.Point.Y) * 0.5, (A.Point.Z + B.Point.Z) * 0.5);
                    Vector3D normal = (A.Normal + B.Normal).Direction;
                    _center = new Ray3D(point, normal);
                }
                return _center;
            }
        }

        public Point3D[] CardinalPoints { get { return [A.Point, B.Point]; } }
        public Vector3D[] CardinalVectors { get { return [A.Normal, B.Normal]; } }
        public SurfaceLineSegment Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new SurfaceLineSegment(new Ray3D(cardinalPoints[0], cardinalVectors[0]), new Ray3D(cardinalPoints[1], cardinalVectors[1]));
        }

        public bool RayIsAtAnEndpoint(Ray3D p)
        {
            return A == p || B == p;
        }
        public Ray3D OppositeRay(Ray3D p)
        {
            if (B != p) { return B; }
            if (A != p) { return A; }
            return null;
        }

        public SurfaceLineSegment Orient(Point3D center)
        {
            if (A.Point == center) { return this; }
            if (B.Point == center) { return new SurfaceLineSegment(B, A); }
            throw new InvalidOperationException($"Center didn't match either line segment endpoint.");
        }

        public Ray3D RayFromProjectedPoint(Point3D point)
        {
            if (A.Point == point) { return A; }
            if (B.Point == point) { return B; }

            var projection = Segment.LineExtension.Projection(point);
            if (projection is null) { return null; }

            var α = Point3D.Distance(A.Point, projection) / Segment.Length;
            var normal = α * B.Normal + (1 - α) * A.Normal;
            return new Ray3D(point, normal);
        }

        public void InvertNormals()
        {
            A.InvertNormal();
            B.InvertNormal();
        }


        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not SurfaceLineSegment) { return false; }
            SurfaceLineSegment compare = (SurfaceLineSegment)obj;
            return (compare.A.Equals(A) || compare.A.Equals(B)) && (compare.B.Equals(A) || compare.B.Equals(B));
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(SurfaceLineSegment a, SurfaceLineSegment b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(SurfaceLineSegment a, SurfaceLineSegment b)
        {
            return !a.Equals(b);
        }

        public static Ray3D LinkingPoint(SurfaceLineSegment a, SurfaceLineSegment b)
        {
            if (a.A.Point == b.A.Point || a.A.Point == b.B.Point) { return a.A; }
            if (a.B.Point == b.A.Point || a.B.Point == b.B.Point) { return a.B; }
            return null;
        }
    }
}
