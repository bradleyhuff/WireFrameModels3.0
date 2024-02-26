using BasicObjects.GeometricTransformations;
using BasicObjects.Transformations;
using E = BasicObjects.Math;
using SMath = System.Math;

namespace BasicObjects.GeometricObjects
{
    public class Plane
    {
        public Plane(Point3D P, Point3D Q, Point3D R) : this(P, Q - P, R - P) { }

        public Plane(Point3D P, Vector3D PQ, Vector3D PR) : this(P, Vector3D.Cross(PQ.Direction, PR.Direction)) { }

        public Plane(Ray3D ray) : this(ray.Point, ray.Normal) { }

        public Plane(Point3D P, Vector3D vector)
        {
            A = vector.X;
            B = vector.Y;
            C = vector.Z;
            D = E.Double.RoundToZero(A * P.X + B * P.Y + C * P.Z);
            Center = P;
        }

        public double A { get; }
        public double B { get; }
        public double C { get; }
        public double D { get; }

        public Point3D Center { get; }

        private Vector3D _normal = null;

        public Vector3D Normal
        {
            get
            {
                if (_normal is null)
                {
                    _normal = new Vector3D(A, B, C).Direction;
                }
                return _normal;
            }
        }

        public Point3D Projection(Point3D point)
        {
            double α = (D - A * point.X - B * point.Y - C * point.Z) / (A * A + B * B + C * C);
            return new Point3D(point.X + α * A, point.Y + α * B, point.Z + α * C);
        }

        public double Distance(Point3D point)
        {
            double length = A * A + B * B + C * C;
            double α = (D - A * point.X - B * point.Y - C * point.Z) / length;
            return SMath.Abs(α) * SMath.Sqrt(length);
        }

        public bool PointIsFrontOfPlane(Point3D point)
        {
            var vector = (point - Projection(point)).Direction;
            return Vector3D.Dot(Normal, vector) > 0;
        }

        public bool PointIsOnPlane(Point3D point)
        {
            return Distance(point) < 5 * E.Double.DifferenceError;
        }

        public bool LineIsOnPlane(Line3D line)
        {
            if (SMath.Abs(Vector3D.Dot(Normal.Direction, line.Vector.Direction)) > E.Double.RadianDifferenceError) { return false; }
            var intercept = line.Xintercept is not null ? line.Xintercept : (line.Yintercept is not null ? line.Yintercept : line.Zintercept);
            return PointIsOnPlane(intercept);
        }

        public bool LineIsParallel(Line3D line)
        {
            return SMath.Abs(Vector3D.Dot(Normal.Direction, line.Vector.Direction)) < E.Double.RadianDifferenceError;
        }

        public Point3D Intersection(Line3D line)
        {
            Point3D Q = line.Start;
            Vector3D V = line.Vector;
            double α = (D - A * Q.X - B * Q.Y - C * Q.Z) / (A * V.X + B * V.Y + C * V.Z);
            return new Point3D(Q.X + α * V.X, Q.Y + α * V.Y, Q.Z + α * V.Z);
        }

        public LineSegment3D Intersection(Triangle3D triangle)
        {
            var line = Intersection(this, triangle.Plane);
            if (line is null) { return null; }

            return triangle.SegmentIntersection(line);
        }

        public Point3D Intersection(LineSegment3D line)
        {
            Point3D Q = line.Start;
            Vector3D V = line.Vector;
            double α = (D - A * Q.X - B * Q.Y - C * Q.Z) / (A * V.X + B * V.Y + C * V.Z);
            if (!E.Double.IsBetweenZeroAndOne(α)) { return null; }
            return new Point3D(Q.X + α * V.X, Q.Y + α * V.Y, Q.Z + α * V.Z);
        }

        public Line3D Projection(Line3D line)
        {
            return new Line3D(Projection(line.Start), Projection(line.End));
        }

        public LineSegment3D Projection(LineSegment3D line)
        {
            return new LineSegment3D(Projection(line.Start), Projection(line.End));
        }

        public Triangle3D Projection(Triangle3D triangle)
        {
            return new Triangle3D(Projection(triangle.A), Projection(triangle.B), Projection(triangle.C));
        }

        public bool Intersects(Rectangle3D box)
        {
            var diagonal0 = new LineSegment3D(box.GetVertex(0), box.GetVertex(7));
            if (Intersection(diagonal0) is not null) { return true; }

            var diagonal1 = new LineSegment3D(box.GetVertex(1), box.GetVertex(6));
            if (Intersection(diagonal1) is not null) { return true; }

            var diagonal2 = new LineSegment3D(box.GetVertex(2), box.GetVertex(5));
            if (Intersection(diagonal2) is not null) { return true; }

            var diagonal3 = new LineSegment3D(box.GetVertex(3), box.GetVertex(4));
            if (Intersection(diagonal3) is not null) { return true; }

            return false;
        }

        public static Line3D Intersection(Plane aa, Plane bb, double threshold = E.Double.DifferenceError)
        {
            double a = 0, b = 0, c = 0; // cross of plane and other plane to give the normal plane.
            Matricies.Cross3D(aa.A, aa.B, aa.C, bb.A, bb.B, bb.C, out a, out b, out c);
            var vector = new Vector3D(a, b, c);
            if (vector.Magnitude < threshold) { return null; }

            double x = 0, y = 0, z = 0;// solves for a point in line which is intersection between given planes and the normal plane.
            E.LinearSystems.Solve3x3(
                aa.A, aa.B, aa.C,
                bb.A, bb.B, bb.C,
                a, b, c,
                aa.D, bb.D, 0,
                out x, out y, out z);

            return new Line3D(new Point3D(x, y, z), new Vector3D(a, b, c));
        }

        public override string ToString()
        {
            return $"A: {A} B: {B} C: {C} D: {D}";
        }
    }
}
