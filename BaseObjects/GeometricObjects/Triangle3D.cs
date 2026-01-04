using BaseObjects.Transformations.Interfaces;
using BasicObjects.Math;
using Double = BasicObjects.Math.Double;
using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Triangle3D : IShape3D<Triangle3D>
    {
        public Triangle3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz) : 
            this(new Point3D(ax, ay, az), new Point3D(bx, by, bz), new Point3D(cx, cy, cz)) { }
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

        private Vector3D _normalAB = null;

        public Vector3D NormalAB
        {
            get
            {
                if (_normalAB is null)
                {
                    _normalAB = EdgeAB.LineExtension.Projection(C) - C;
                }
                return _normalAB;
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

        private Vector3D _normalBC = null;

        public Vector3D NormalBC
        {
            get
            {
                if (_normalBC is null)
                {
                    _normalBC = EdgeBC.LineExtension.Projection(A) - A;
                }
                return _normalBC;
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

        private Vector3D _normalCA = null;

        public Vector3D NormalCA
        {
            get
            {
                if (_normalCA is null)
                {
                    _normalCA = EdgeCA.LineExtension.Projection(B) - B;
                }
                return _normalCA;
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

        private LineSegment3D _minEdge = null;
        public LineSegment3D MinEdge
        {
            get
            {
                if (_minEdge is null)
                {
                    _minEdge = LineSegment3D.MinLength(EdgeAB, EdgeBC, EdgeCA);
                }
                return _minEdge;
            }
        }

        private LineSegment3D _maxEdge = null;
        public LineSegment3D MaxEdge
        {
            get
            {
                if (_maxEdge is null)
                {
                    _maxEdge = LineSegment3D.MaxLength(EdgeAB, EdgeBC, EdgeCA);
                }
                return _maxEdge;
            }
        }

        private LineSegment3D _middleEdge = null;
        public LineSegment3D MiddleEdge
        {
            get
            {
                if (_middleEdge is null)
                {
                    _middleEdge = Edges.SingleOrDefault(e => e.Length != MaxEdge.Length && e.Length != MinEdge.Length);
                }
                return _middleEdge;
            }
        }

        /// <summary>
        /// Skinny triangle Aspect ratio -> 0
        /// </summary>
        public double AspectRatio
        {
            get
            {
                if (IsCollinear) { return 0; }
                return MinimumHeight.Magnitude / MaxEdge.Length;
            }
        }

        private Vector3D _minimumHeight = null;

        public Vector3D MinimumHeight
        {
            get
            {
                if (_minimumHeight is null)
                {
                    var maxEdge = MaxEdge;
                    var minEdgePoints = MinEdge.Points;
                    var maxEdgePoints = maxEdge.Points.ToArray();
                    var loosePoint = minEdgePoints.Single(p => p != maxEdgePoints[0] && p != maxEdgePoints[1]);

                    _minimumHeight = loosePoint - MaxEdge.LineExtension.Projection(loosePoint);
                }
                return _minimumHeight;
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

        public static bool operator ==(Triangle3D a, Triangle3D b)
        {
            return a is not null && a.Equals(b);
        }
        public static bool operator !=(Triangle3D a, Triangle3D b)
        {
            return !(a is not null && a.Equals(b));
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
        public bool AoverhangsBC
        {
            get {
                var projection = EdgeBC.LineExtension.Projection(A); 
                return !Double.IsEqual(Point3D.Distance(B, projection) + Point3D.Distance(C, projection) - LengthBC, 0);
            }
        }
        public bool BoverhangsCA
        {
            get {
                var projection = EdgeCA.LineExtension.Projection(B);
                return !Double.IsEqual(Point3D.Distance(C, projection) + Point3D.Distance(A, projection) - LengthCA, 0); 
            }
        }
        public bool CoverhangsAB
        {
            get {
                var projection = EdgeAB.LineExtension.Projection(C);
                return !Double.IsEqual(Point3D.Distance(A, projection) + Point3D.Distance(B, projection) - LengthAB, 0); 
            }
        }

        public enum Matching { None, AB, BC, CA}

        public Matching GetEdgeMatch(double length)
        {
            if (Double.Equals(length, LengthAB)) { return Matching.AB; }
            if (Double.Equals(length, LengthBC)) { return Matching.BC; }
            if (Double.Equals(length, LengthCA)) { return Matching.CA; }
            return Matching.None;
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
            get { return HeightAtoBC < Double.ProximityError || HeightBtoCA < Double.ProximityError || HeightCtoAB < Double.ProximityError; }
        }
        private LineSegment3D _longestEdge = null;
        public LineSegment3D LongestEdge
        {
            get
            {
                if (_longestEdge is null)
                {
                    _longestEdge = GetLongestEdge();
                }
                return _longestEdge;
            }
        }

        private LineSegment3D GetLongestEdge()
        {
            if (EdgeAB.Length > EdgeBC.Length && EdgeAB.Length > EdgeCA.Length)
            {
                return EdgeAB;
            }
            if (EdgeBC.Length > EdgeAB.Length && EdgeBC.Length > EdgeCA.Length)
            {
                return EdgeBC;
            }
            return EdgeCA;
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

        public double MinimumAngle
        {
            get
            {
                return E.Math.Min(AngleAtA, AngleAtB, AngleAtC);
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

        public IEnumerable<Point3D> Vertices
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        public IEnumerable<Point3D> DisjointVertices(Triangle3D compare)
        {
            if (A != compare.A && A != compare.B && A != compare.C) { yield return A; }
            if (B != compare.A && B != compare.B && B != compare.C) { yield return B; }
            if (C != compare.A && C != compare.B && C != compare.C) { yield return C; }
        }

        public static IEnumerable<Point3D> CommonVertices(Triangle3D a, Triangle3D b)
        {
            if (Point3D.AreEqual(a.A, b.A)) { yield return b.A; }
            if (Point3D.AreEqual(a.A, b.B)) { yield return b.B; }
            if (Point3D.AreEqual(a.A, b.C)) { yield return b.C; }

            if (Point3D.AreEqual(a.B, b.A)) { yield return b.A; }
            if (Point3D.AreEqual(a.B, b.B)) { yield return b.B; }
            if (Point3D.AreEqual(a.B, b.C)) { yield return b.C; }

            if (Point3D.AreEqual(a.C, b.A)) { yield return b.A; }
            if (Point3D.AreEqual(a.C, b.B)) { yield return b.B; }
            if (Point3D.AreEqual(a.C, b.C)) { yield return b.C; }
        }

        public static double LengthOfCommonSide(Triangle3D a, Triangle3D b)
        {
            var commons = Triangle3D.CommonVertices(a, b).ToArray();
            if (commons.Length == 2)
            {
                return Point3D.Distance(commons[0], commons[1]);
            }
            return System.Double.NaN;
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

        public Point3D[] CardinalPoints
        {
            get { return [A, B, C]; }
        }
        public Vector3D[] CardinalVectors { get { return []; } }
        public Triangle3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new Triangle3D(cardinalPoints[0], cardinalPoints[1], cardinalPoints[2]);
        }

        public static bool AreCoplanar(Triangle3D a, Triangle3D b)
        {
            return a.Plane.PointIsOnPlane(b.A) && a.Plane.PointIsOnPlane(b.B) && a.Plane.PointIsOnPlane(b.C);
        }

        public bool PointIsIn(Point3D point)
        {
            return Plane.PointIsOnPlane(point) && !PointIsOnPerimeter(point) && GetBarycentricCoordinate(point).IsInUnitInterval();
        }

        public bool PointIsOn(Point3D point)
        {
            return Plane.PointIsOnPlane(point) && (PointIsOnPerimeter(point) || GetBarycentricCoordinate(point).IsOnUnitInterval());
        }

        public bool PointIsContainedOn(Point3D point)
        {
            return PointIsOnPerimeter(point) || GetBarycentricCoordinate(point).IsOnUnitInterval();
        }

        public bool PointIsContainedOn(Point3D point, double zone)
        {
            return PointIsOnPerimeter(point, zone) || GetBarycentricCoordinate(point).IsOnUnitInterval(zone);
        }

        public bool PointIsContainedIn(Point3D point)
        {
            return GetBarycentricCoordinate(point).IsInUnitInterval();
        }

        public bool PointIsOnPerimeter(Point3D point)
        {
            return (EdgeAB.LineExtension.PointIsOnLine(point) && EdgeAB.PointIsAtOrBetweenEndpoints(point)) ||
                (EdgeBC.LineExtension.PointIsOnLine(point) && EdgeBC.PointIsAtOrBetweenEndpoints(point)) ||
                (EdgeCA.LineExtension.PointIsOnLine(point) && EdgeCA.PointIsAtOrBetweenEndpoints(point));
        }

        public bool PointIsOnPerimeter(Point3D point, double zone)
        {
            return (EdgeAB.LineExtension.PointIsOnLine(point, zone) && EdgeAB.PointIsAtOrBetweenEndpoints(point, zone)) ||
                (EdgeBC.LineExtension.PointIsOnLine(point, zone) && EdgeBC.PointIsAtOrBetweenEndpoints(point, zone)) ||
                (EdgeCA.LineExtension.PointIsOnLine(point, zone) && EdgeCA.PointIsAtOrBetweenEndpoints(point, zone));
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

        public Point3D GetPoint(λ coordinate)
        {
            return coordinate.λ1 * A + coordinate.λ2 * B + coordinate.λ3 * C;
        }

        public Vector3D GetNormal(λ coordinate, Vector3D a, Vector3D b, Vector3D c)
        {
            return coordinate.λ1 * a.Direction + coordinate.λ2 * b.Direction + coordinate.λ3 * c.Direction;
        }

        public override string ToString()
        {
            return $"Triangle A: {A} B: {B} C: {C}";
        }

        public static IEnumerable<LineSegment3D> LineSegmentIntersections(Triangle3D a, Triangle3D b)
        {
            if (a.IsCollinear && b.IsCollinear)
            {
                var match = LineSegment3D.Intersection(a.LongestEdge, b.LongestEdge);
                if (match is not null) { yield return match; }
                yield break;
            }
            if (a.IsCollinear)
            {
                foreach (var edge in b.Edges)
                {
                    var match = LineSegment3D.Intersection(edge, a.LongestEdge);
                    if (match is not null) { yield return match; yield break; }
                }
            }
            if (b.IsCollinear)
            {
                foreach (var edge in a.Edges)
                {
                    var match = LineSegment3D.Intersection(edge, b.LongestEdge);
                    if (match is not null) { yield return match; yield break; }
                }
            }

            if (AreCoplanar(a, b))
            {
                foreach (var intersection in CoplanarIntersections(a, b)) { yield return intersection; }
                yield break;
            }

            foreach (var edge in a.Edges)
            {
                foreach (var edge2 in b.Edges)
                {
                    var match = LineSegment3D.Intersection(edge, edge2);
                    if (match is not null) { if (edge != edge2) yield return match; yield break; }
                }
            }

            foreach (var edge in a.Edges)
            {
                var match = b.LineSegmentIntersection(edge);
                if (match is not null) { yield return match; yield break; }
            }

            foreach (var edge in b.Edges)
            {
                var match = a.LineSegmentIntersection(edge);
                if (match is not null) { yield return match; yield break; }
            }

            Line3D line = Plane.Intersection(a.Plane, b.Plane);
            if (line is null) { yield break; }

            var segmentA = a.LineSegmentIntersection(line);
            var segmentB = b.LineSegmentIntersection(line);
            {
                var match = LineSegment3D.Intersection(segmentA, segmentB);
                if (match is not null) { yield return match; }
            }

            yield break;
        }

        private static IEnumerable<LineSegment3D> CoplanarIntersections(Triangle3D a, Triangle3D b)
        {
            var output = new List<LineSegment3D>();

            foreach (var edge in a.Edges)
            {
                var splits = edge.LineSegmentSplit(b.Edges.ToArray()).Where(s => s != edge).ToArray();
                foreach (var split in splits.Where(s => b.PointIsOn(s.Center))) { output.Add(split); }
            }

            foreach (var edge in b.Edges)
            {
                var splits = edge.LineSegmentSplit(a.Edges.ToArray()).Where(s => s != edge).ToArray();
                foreach (var split in splits.Where(s => a.PointIsOn(s.Center))) { output.Add(split); }
            }

            foreach (var edge in a.Edges.Where(e => !b.Edges.Any(ee => e == ee)))
            {
                if (b.PointIsOn(edge.Start) && b.PointIsOn(edge.End)) { output.Add(edge); }
            }

            foreach (var edge in b.Edges.Where(e => !a.Edges.Any(ee => e == ee)))
            {
                if (a.PointIsOn(edge.Start) && a.PointIsOn(edge.End)) { output.Add(edge); }
            }

            foreach (var element in output.DistinctBy(l => l)) { yield return element; }

        }
        public LineSegment3D LineSegmentIntersection(Line3D line)
        {
            Point3D ab = Line3D.PointIntersection(EdgeAB, line);
            Point3D bc = Line3D.PointIntersection(EdgeBC, line);
            Point3D ca = Line3D.PointIntersection(EdgeCA, line);

            if (ab is null && bc is not null && ca is not null) { return ReturnLineSegment(new LineSegment3D(bc, ca)); }//Check that these points lie within edge segment.
            if (bc is null && ab is not null && ca is not null) { return ReturnLineSegment(new LineSegment3D(ab, ca)); }
            if (ca is null && ab is not null && bc is not null) { return ReturnLineSegment(new LineSegment3D(ab, bc)); }

            if (ab is not null && bc is null && ca is null) { return ReturnLineSegment(new LineSegment3D(C, ab)); }
            if (ab is null && bc is not null && ca is null) { return ReturnLineSegment(new LineSegment3D(A, bc)); }
            if (ab is null && bc is null && ca is not null) { return ReturnLineSegment(new LineSegment3D(B, ca)); }
            if (ab is not null && bc is not null && ca is not null)
            {
                if (ab == A || ca == A) { return ReturnLineSegment(new LineSegment3D(A, bc)); }
                if (ab == B || bc == B) { return ReturnLineSegment(new LineSegment3D(B, ca)); }
                if (bc == C || ca == C) { return ReturnLineSegment(new LineSegment3D(C, ab)); }
                Console.WriteLine($"Line segment intersection error.");
            }

            return null;
        }

        private LineSegment3D ReturnLineSegment(LineSegment3D segment)
        {
            return segment is not null && !segment.IsDegenerate ? segment : null;
        }

        public LineSegment3D LineSegmentIntersection(LineSegment3D segment)
        {
            var match = LineSegmentIntersection(segment.LineExtension);
            if (match is null) { return null; }
            return LineSegment3D.Intersection(match, segment);
        }

        public static bool Overlaps(Triangle3D a, Triangle3D b)
        {
            if (!AreCoplanar(a, b)) { return false; }
            if (a.PointIsContainedOn(b.A) && a.PointIsContainedOn(b.B) && a.PointIsContainedOn(b.C)) { return true; }
            if (b.PointIsContainedOn(a.A) && b.PointIsContainedOn(a.B) && b.PointIsContainedOn(a.C)) { return true; }

            return false;
        }

        public static bool Intersects(Triangle3D a, Triangle3D b)
        {
            if (!AreCoplanar(a, b)) { return false; }

            var segmentsA = a.Edges.ToArray();
            var segmentsB = b.Edges.ToArray();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == j) { continue; }
                    if (segmentsA[i].Start == segmentsB[j].Start) { continue; }
                    if (segmentsA[i].Start == segmentsB[j].End) { continue; }
                    if (segmentsA[i].End == segmentsB[j].Start) { continue; }
                    if (segmentsA[i].End == segmentsB[j].End) { continue; }
                    if (LineSegment3D.PointIntersection(segmentsA[i], segmentsB[j]) is not null) { return true; }
                }
            }

            return false;
        }

    }
}
