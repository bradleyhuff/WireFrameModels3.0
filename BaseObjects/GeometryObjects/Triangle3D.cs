
using BasicObjects.Math;
using Double = BasicObjects.Math.Double;
using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Triangle3D
    {
        public Triangle3D(Point3D A, Point3D B, Point3D C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public Point3D A { get; }
        public Point3D B { get; }
        public Point3D C { get; }

        private LineSegment3D _edgeAB = null;

        public LineSegment3D EdgeAB
        {
            get
            {
                if (_edgeAB is null)
                {
                    _edgeAB = new LineSegment3D(A, B);
                }
                return _edgeAB;
            }
        }

        private LineSegment3D _edgeBC = null;

        public LineSegment3D EdgeBC
        {
            get
            {
                if (_edgeBC is null)
                {
                    _edgeBC = new LineSegment3D(B, C);
                }
                return _edgeBC;
            }
        }

        private LineSegment3D _edgeCA = null;

        public LineSegment3D EdgeCA
        {
            get
            {
                if (_edgeCA is null)
                {
                    _edgeCA = new LineSegment3D(C, A);
                }
                return _edgeCA;
            }
        }

        public IEnumerable<LineSegment3D> Edges
        {
            get
            {
                yield return EdgeAB;
                yield return EdgeBC;
                yield return EdgeCA;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Triangle3D) { return false; }
            Triangle3D compare = (Triangle3D)obj;

            if (A.Equals(compare.A))
            {
                return (B.Equals(compare.B) && C.Equals(compare.C)) || (B.Equals(compare.C) && C.Equals(compare.B));
            }
            if (A.Equals(compare.B))
            {
                return (B.Equals(compare.A) && C.Equals(compare.C)) || (B.Equals(compare.C) && C.Equals(compare.A));
            }
            if (A.Equals(compare.C))
            {
                return (B.Equals(compare.B) && C.Equals(compare.A)) || (B.Equals(compare.A) && C.Equals(compare.B));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        private Point3D _center = null;
        public Point3D Center
        {
            get
            {
                if (_center is null)
                {
                    _center = (A + B + C) / 3;
                }
                return _center;
            }
        }

        double _radius = 0;
        bool _radiusIsFound = false;
        public double Radius
        {
            get
            {
                if (!_radiusIsFound)
                {
                    _radius = E.Math.Max(Point3D.Distance(A, Center), Point3D.Distance(B, Center), Point3D.Distance(C, Center));
                    _radiusIsFound = true;
                }
                return _radius;
            }
        }

        public double LengthAB
        {
            get
            {
                return EdgeAB.Length;
            }
        }

        public double LengthBC
        {
            get
            {
                return EdgeBC.Length;
            }
        }

        public double LengthCA
        {
            get
            {
                return EdgeCA.Length;
            }
        }

        public double Area
        {
            get
            {
                return 0.5 * LengthAB * HeightCtoAB;
            }
        }

        private double _heightAtoBC = 0;
        private bool _heightAtoBCfound = false;
        public double HeightAtoBC
        {
            get
            {
                if (!_heightAtoBCfound)
                {
                    _heightAtoBC = Point3D.Distance(A, EdgeBC.LineExtension.Projection(A));
                    _heightAtoBCfound = true;
                }
                return _heightAtoBC;
            }
        }

        private double _heightBtoCA = 0;
        private bool _heightBtoCAfound = false;
        public double HeightBtoCA
        {
            get
            {
                if (!_heightBtoCAfound)
                {
                    _heightBtoCA = Point3D.Distance(B, EdgeCA.LineExtension.Projection(B));
                    _heightBtoCAfound = true;
                }
                return _heightBtoCA;
            }
        }

        private double _heightCtoAB = 0;
        private bool _heightCtoABfound = false;
        public double HeightCtoAB
        {
            get
            {
                if (!_heightCtoABfound)
                {
                    _heightCtoAB = Point3D.Distance(C, EdgeAB.LineExtension.Projection(C));
                    _heightCtoABfound = true;
                }
                return _heightCtoAB;
            }
        }

        public double MinHeight
        {
            get { return E.Math.Min(HeightAtoBC, HeightBtoCA, HeightCtoAB); }
        }

        public bool IsCollinear
        {
            get { return HeightAtoBC < Double.DifferenceError || HeightBtoCA < Double.DifferenceError || HeightCtoAB < Double.DifferenceError; }
        }

        private double _angleAtA = 0;
        private bool _angleAtAfound = false;
        public double AngleAtA
        {
            get
            {
                if (!_angleAtAfound)
                {
                    _angleAtA = Vector3D.Angle(EdgeAB.Vector, -EdgeCA.Vector);
                    _angleAtAfound = true;
                }
                return _angleAtA;
            }
        }

        private double _angleAtB = 0;
        private bool _angleAtBfound = false;
        public double AngleAtB
        {
            get
            {
                if (!_angleAtBfound)
                {
                    _angleAtB = Vector3D.Angle(-EdgeAB.Vector, EdgeBC.Vector);
                    _angleAtBfound = true;
                }
                return _angleAtB;
            }
        }

        private double _angleAtC = 0;
        private bool _angleAtCfound = false;
        public double AngleAtC
        {
            get
            {
                if (!_angleAtCfound)
                {
                    _angleAtC = Vector3D.Angle(EdgeCA.Vector, -EdgeBC.Vector);
                    _angleAtCfound = true;
                }
                return _angleAtC;
            }
        }

        private Vector3D _normal = null;
        public Vector3D Normal
        {
            get
            {
                if (_normal is null)
                {
                    _normal = Vector3D.Cross(A - B, A - C).Direction;
                }
                return _normal;
            }
        }

        private Plane _plane = null;
        public Plane Plane
        {
            get
            {
                if (_plane is null)
                {
                    _plane = new Plane(A, Normal);
                }
                return _plane;
            }
        }

        public Triangle3D Scale(double scale)
        {
            Point3D center = Center;
            Vector3D aCenter = A - center;
            Vector3D bCenter = B - center;
            Vector3D cCenter = C - center;

            return new Triangle3D(center + scale * aCenter, center + scale * bCenter, center + scale * cCenter);
        }

        public Triangle3D Margin(double margin)
        {
            var a = (A - Center).Direction;
            var b = (B - Center).Direction;
            var c = (C - Center).Direction;

            var aa = margin / System.Math.Sin(AngleAtA / 2);
            var bb = margin / System.Math.Sin(AngleAtB / 2);
            var cc = margin / System.Math.Sin(AngleAtC / 2);

            return new Triangle3D(A + aa * a, B + bb * b, C + cc * c);
        }

        public IEnumerable<Point3D> Verticies
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        public IEnumerable<Point3D> DisjointVerticies(Triangle3D compare)
        {
            if (A != compare.A && A != compare.B && A != compare.C) { yield return A; }
            if (B != compare.A && B != compare.B && B != compare.C) { yield return B; }
            if (C != compare.A && C != compare.B && C != compare.C) { yield return C; }
        }

        public static IEnumerable<Point3D> CommonVerticies(Triangle3D a, Triangle3D b)
        {
            if (Point3D.AreEqual(a.A, b.A)) { yield return a.A; }
            if (Point3D.AreEqual(a.A, b.B)) { yield return a.B; }
            if (Point3D.AreEqual(a.A, b.C)) { yield return a.C; }

            if (Point3D.AreEqual(a.B, b.A)) { yield return a.A; }
            if (Point3D.AreEqual(a.B, b.B)) { yield return a.B; }
            if (Point3D.AreEqual(a.B, b.C)) { yield return a.C; }

            if (Point3D.AreEqual(a.C, b.A)) { yield return a.A; }
            if (Point3D.AreEqual(a.C, b.B)) { yield return a.B; }
            if (Point3D.AreEqual(a.C, b.C)) { yield return a.C; }
        }

        private Rectangle3D _box = null;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(A, B, C);
                }
                return _box;
            }
        }

        public static bool AreCoplanar(Triangle3D a, Triangle3D b)
        {
            return a.Plane.PointIsOnPlane(b.A) && a.Plane.PointIsOnPlane(b.B) && a.Plane.PointIsOnPlane(b.C);
        }

        public LineSegment3D SegmentIntersection(Line3D line)
        {
            Point3D ab = Line3D.PointIntersection(EdgeAB, line);
            Point3D bc = Line3D.PointIntersection(EdgeBC, line);
            if (ab is null && bc is null) { return null; } //There can be no intersection if 2 of the sides don't intersect.
            Point3D ca = Line3D.PointIntersection(EdgeCA, line);

            int digit1 = (ab is null) ? 0 : 1;
            int digit2 = (bc is null) ? 0 : 1;
            int digit3 = (ca is null) ? 0 : 1;

            int caseNumber = digit3 << 2 | digit2 << 1 | digit1;

            switch (caseNumber)
            {
                case 3: return new LineSegment3D(ab, bc);
                case 5: return new LineSegment3D(ab, ca);
                case 6: return new LineSegment3D(bc, ca);
                case 7:
                    if (ab == bc) { return new LineSegment3D(ab, ca); }
                    if (bc == ca) { return new LineSegment3D(ab, bc); }
                    if (ca == ab) { return new LineSegment3D(ab, bc); }
                    return null;
            }

            return null;
        }

        public static IEnumerable<LineSegment3D> SegmentIntersections(Triangle3D a, Triangle3D b)
        {
            var commonVerticies = CommonVerticies(a, b).ToArray();
            if (commonVerticies.Length > 1) { yield return new LineSegment3D(commonVerticies[0], commonVerticies[1]); yield break; }

            if (AreCoplanar(a, b))
            {
                //


                yield break;
            }

            Line3D line = Plane.Intersection(a.Plane, b.Plane);
            if (line is null) 
            { 

                yield break; 
            }

            if (commonVerticies.Length == 1)
            {
                var aDisjoints = a.DisjointVerticies(b).ToArray();

                LineSegment3D ab1 = new LineSegment3D(aDisjoints[0], aDisjoints[1]);

                Point3D p1 = Line3D.PointIntersection(ab1, line);
                if (p1 is null) { yield break; }

                var bDisjoints = b.DisjointVerticies(a).ToArray();
                LineSegment3D ab2 = new LineSegment3D(bDisjoints[0], bDisjoints[1]);
                Point3D p2 = Line3D.PointIntersection(ab2, line);
                if (p2 is null) { yield break; }

                Point3D c = commonVerticies[0];//common point
                LineSegment3D ac1 = new LineSegment3D(p1, c);
                LineSegment3D ac2 = new LineSegment3D(p2, c);

                yield return LineSegment3D.SegmentIntersection(ac1, ac2);
                yield break;
            }

            LineSegment3D segmentA = a.SegmentIntersection(line);
            if (segmentA is null) { yield break; }
            LineSegment3D segmentB = b.SegmentIntersection(line);
            if (segmentB is null) { yield break; }

            yield return LineSegment3D.SegmentIntersection(segmentA, segmentB);
        }


        public static LineSegment3D SegmentIntersection(Triangle3D a, Triangle3D b)
        {
            var commonVerticies = CommonVerticies(a, b).ToArray();
            if (commonVerticies.Length > 1) { return new LineSegment3D(commonVerticies[0], commonVerticies[1]); }

            Line3D line = Plane.Intersection(a.Plane, b.Plane);
            if (line is null) { return null; }
            if (commonVerticies.Length == 1)
            {
                var aDisjoints = a.DisjointVerticies(b).ToArray();

                LineSegment3D ab1 = new LineSegment3D(aDisjoints[0], aDisjoints[1]);

                Point3D p1 = Line3D.PointIntersection(ab1, line);
                if (p1 is null) { return null; }

                var bDisjoints = b.DisjointVerticies(a).ToArray();
                LineSegment3D ab2 = new LineSegment3D(bDisjoints[0], bDisjoints[1]);
                Point3D p2 = Line3D.PointIntersection(ab2, line);
                if (p2 is null) { return null; }

                Point3D c = commonVerticies[0];//common point
                LineSegment3D ac1 = new LineSegment3D(p1, c);
                LineSegment3D ac2 = new LineSegment3D(p2, c);

                return LineSegment3D.SegmentIntersection(ac1, ac2);
            }

            LineSegment3D segmentA = a.SegmentIntersection(line);
            if (segmentA is null) { return null; }
            LineSegment3D segmentB = b.SegmentIntersection(line);
            if (segmentB is null) { return null; }

            return LineSegment3D.SegmentIntersection(segmentA, segmentB);
        }

        public bool PointIsIn(Point3D point)
        {
            return Plane.PointIsOnPlane(point) && GetBarycentricCoordinate(point).IsInUnitInterval();
        }

        public bool OutsideLineSegmentMatchesOneVertex(LineSegment3D segment)
        {
            if (segment.Start == A && !PointIsOn(segment.End)) { return true; }
            if (segment.End == A && !PointIsOn(segment.Start)) { return true; }
            if (segment.Start == B && !PointIsOn(segment.End)) { return true; }
            if (segment.End == B && !PointIsOn(segment.Start)) { return true; }
            if (segment.Start == C && !PointIsOn(segment.End)) { return true; }
            if (segment.End == C && !PointIsOn(segment.Start)) { return true; }

            return false;
        }

        public bool PointIsOn(Point3D point)
        {
            return Plane.PointIsOnPlane(point) && GetBarycentricCoordinate(point).IsOnUnitInterval();
        }

        public bool IsOnPlane(Plane plane)
        {
            return plane.PointIsOnPlane(A) && plane.PointIsOnPlane(B) && plane.PointIsOnPlane(C);
        }

        public λ GetBarycentricCoordinate(Point3D point)
        {
            Point3D r4 = Center + Normal;

            LinearSystems.Solve3x3(
                A.X - r4.X, B.X - r4.X, C.X - r4.X,
                A.Y - r4.Y, B.Y - r4.Y, C.Y - r4.Y,
                A.Z - r4.Z, B.Z - r4.Z, C.Z - r4.Z,
                point.X - r4.X, point.Y - r4.Y, point.Z - r4.Z,
                out double λ1, out double λ2, out double λ3);
            return new λ(λ1, λ2, λ3);
        }

        public bool HasVertex(Point3D point)
        {
            return A.Equals(point) || B.Equals(point) || C.Equals(point);
        }

        public override string ToString()
        {
            return $"Triangle A: {A} B: {B} C: {C}";
        }

    }
}
