using BaseObjects.Transformations.Interfaces;
using E = BasicObjects.Math;
using Double = BasicObjects.Math.Double;

namespace BasicObjects.GeometricObjects
{
    public class LineSegment3D : IShape3D<LineSegment3D>
    {
        public LineSegment3D(Point3D start, Point3D end)
        {
            Start = start;
            End = end;
        }
        public LineSegment3D(double startX, double startY, double startZ, double endX, double endY, double endZ)
        {
            Start = new Point3D(startX, startY, startZ);
            End = new Point3D(endX, endY, endZ);
        }
        public Point3D Start { get; }
        public Point3D End { get; }

        public IEnumerable<Point3D> Points
        {
            get
            {
                yield return Start;
                yield return End;
            }
        }

        private Vector3D _vector = null;
        public Vector3D Vector
        {
            get
            {
                if (_vector is null)
                {
                    _vector = new Vector3D(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z);
                }
                return _vector;
            }
        }

        public double Length
        {
            get
            {
                return Vector.Magnitude;
            }
        }

        public bool IsDegenerate
        {
            get { return Length < E.Double.ProximityError; }
        }

        private Point3D _center = null;
        public Point3D Center
        {
            get
            {
                if (_center is null)
                {
                    _center = new Point3D((Start.X + End.X) * 0.5, (Start.Y + End.Y) * 0.5, (Start.Z + End.Z) * 0.5);
                }
                return _center;
            }
        }

        private Line3D _lineExtension = null;
        public Line3D LineExtension
        {
            get
            {
                if (_lineExtension is null)
                {
                    _lineExtension = new Line3D(Start, End);
                }
                return _lineExtension;
            }
        }

        public Point3D[] CardinalPoints { get { return [Start, End]; } }
        public Vector3D[] CardinalVectors { get { return []; } }
        public LineSegment3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new LineSegment3D(cardinalPoints[0], cardinalPoints[1]);
        }

        public LineSegment3D Orient(Point3D center)
        {
            if (Start == center) { return this; }
            if (End == center) { return new LineSegment3D(End, Start); }
            throw new InvalidOperationException($"Center didn't match either line segment endpoint.");
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not LineSegment3D) { return false; }
            LineSegment3D compare = (LineSegment3D)obj;
            return (compare.Start.Equals(Start) && compare.End.Equals(End)) ||
                (compare.Start.Equals(End) && compare.End.Equals(Start));
        }

        public static bool AreEqual(LineSegment3D a, LineSegment3D b, double error = Double.DifferenceError)
        {
            return (Point3D.AreEqual(a.Start, b.Start, error) && Point3D.AreEqual(a.End, b.End, error)) || (Point3D.AreEqual(a.Start, b.End, error) && Point3D.AreEqual(a.End, b.Start, error));
        }

        public static bool operator ==(LineSegment3D a, LineSegment3D b)
        {
            return a is not null && a.Equals(b);
        }
        public static bool operator !=(LineSegment3D a, LineSegment3D b)
        {
            return !(a is not null && a.Equals(b));
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return $"[{Start}, {End}] {Length}";
        }

        public bool PointIsAtAnEndpoint(Point3D p)
        {
            return Start == p || End == p;
        }

        public Point3D OppositePoint(Point3D p)
        {
            if (End != p) { return End; }
            if (Start != p) { return Start; }
            return null;
        }

        public LineSegment3D Margins(double margin)
        {
            var vectorMargin = margin * Vector.Direction;
            return new LineSegment3D(Start + -vectorMargin, End + vectorMargin);
        }

        public static Point3D LinkingPoint(LineSegment3D a, LineSegment3D b)
        {
            if (a.Start == b.Start || a.Start == b.End) { return a.Start; }
            if (a.End == b.Start || a.End == b.End) { return a.End; }
            return null;
        }

        public static bool IsNonLinking(LineSegment3D a, LineSegment3D b)
        {
            return LinkingPoint(a, b) is null;
        }

        public static LineSegment3D LineSegmentIntersection(LineSegment3D a, LineSegment3D b)
        {
            if (a is null || b is null) { return null; }
            if (a.IsDegenerate && b.IsDegenerate) { return null; }

            bool aStartEqualsbStart = a.Start == b.Start;
            bool aStartEqualsbEnd = a.Start == b.End;
            bool aEndEqualsbStart = a.End == b.Start;
            bool aEndEqualsbEnd = a.End == b.End;
            bool anEndPointMatches = aStartEqualsbStart || aStartEqualsbEnd || aEndEqualsbStart || aEndEqualsbEnd;

            if (a.IsDegenerate && anEndPointMatches) { return null; }
            if (b.IsDegenerate && anEndPointMatches) { return null; }
            if (aStartEqualsbStart && aEndEqualsbEnd) { return a; }
            if (aStartEqualsbEnd && aEndEqualsbStart) { return a; }

            if (a.Length > b.Length)
            {
                return LineSegmentIntersectionMatch(a, b);
            }
            else
            {
                return LineSegmentIntersectionMatch(b, a);
            }
        }

        private static LineSegment3D LineSegmentIntersectionMatch(LineSegment3D a, LineSegment3D b)
        {
            var line = a.LineExtension;
            if (!line.SegmentIsOnLine(b)) { return null; }

            double aStartbStart = Point3D.Distance(a.Start, b.Start);
            double aStartbEnd = Point3D.Distance(a.Start, b.End);
            double aEndbStart = Point3D.Distance(a.End, b.Start);
            double aEndbEnd = Point3D.Distance(a.End, b.End);

            bool bStartIsBetween = aStartbStart < a.Length && aEndbStart < a.Length;
            bool bEndIsBetween = aStartbEnd < a.Length && aEndbEnd < a.Length;

            if (bStartIsBetween && bEndIsBetween) { return b; }
            if (bStartIsBetween && aEndbEnd < aStartbEnd) { return new LineSegment3D(b.Start, a.End); }
            if (bStartIsBetween && aStartbEnd < aEndbEnd) { return new LineSegment3D(a.Start, b.Start); }
            if (bEndIsBetween && aEndbStart < aStartbStart) { return new LineSegment3D(a.End, b.End); }
            if (bEndIsBetween && aStartbStart < aEndbStart) { return new LineSegment3D(a.Start, b.End); }
            return null;
        }

        public static Point3D PointIntersection(LineSegment3D a, LineSegment3D b)
        {
            if (a.IsDegenerate && b.IsDegenerate) { return null; }

            bool aStartEqualsbStart = a.Start == b.Start;
            bool aStartEqualsbEnd = a.Start == b.End;
            bool aEndEqualsbStart = a.End == b.Start;
            bool aEndEqualsbEnd = a.End == b.End;

            bool anEndPointMatches = aStartEqualsbStart || aStartEqualsbEnd || aEndEqualsbStart || aEndEqualsbEnd;

            if (a.IsDegenerate && anEndPointMatches) { return a.Center; }
            if (b.IsDegenerate && anEndPointMatches) { return b.Center; }
            if (aStartEqualsbStart && aEndEqualsbEnd) { return null; }
            if (aStartEqualsbEnd && aEndEqualsbStart) { return null; }
            if (aStartEqualsbStart || aStartEqualsbStart) { return a.Start; }
            if (aEndEqualsbStart || aEndEqualsbEnd) { return a.End; }

            if (a.Length > b.Length)
            {
                return PointIntersectionMatch(a, b);
            }
            else
            {
                return PointIntersectionMatch(b, a);
            }
        }

        private static Point3D PointIntersectionMatch(LineSegment3D a, LineSegment3D b)
        {
            var line = a.LineExtension;
            if (line.SegmentIsOnLine(b)) { return null; }

            var point = Line3D.MidPointIntersection(a.Start, a.Vector, b.Start, b.Vector, out double gap);
            if (gap > E.Double.ProximityError) { return null; }
            return a.PointIsAtOrBetweenEndpoints(point) && b.PointIsAtOrBetweenEndpoints(point) ? point : null;
        }

        public IEnumerable<LineSegment3D> LineSegmentSplit(params LineSegment3D[] segments)
        {
            if (IsDegenerate) { yield break; }
            var intersectionPoints = segments.Select(s => PointIntersection(this, s))
                .Where(i => i is not null && i != Start && i != End)
                .OrderBy(p => Point3D.Distance(Start, p)).ToArray();

            if (intersectionPoints.Length == 0) { yield return this; yield break; }

            {
                var output = new LineSegment3D(Start, intersectionPoints.First());
                if (!output.IsDegenerate) { yield return output; }
            }

            for (int i = 0; i < intersectionPoints.Length - 1; i++)
            {
                var output = new LineSegment3D(intersectionPoints[i], intersectionPoints[i + 1]);
                if (!output.IsDegenerate) { yield return output; }
            }

            {
                var output = new LineSegment3D(intersectionPoints.Last(), End);
                if (!output.IsDegenerate) { yield return output; }
            }
        }

        public bool PointIsAtOrBetweenEndpoints(Point3D point)
        {
            double distanceStart = Point3D.Distance(Start, point);
            double distanceEnd = Point3D.Distance(End, point);
            return distanceStart < E.Double.DifferenceError ||
                distanceEnd < E.Double.DifferenceError ||
                (distanceStart < Length && distanceEnd < Length);
        }
        public bool PointIsOnSegment(Point3D point, double error = E.Double.ProximityError)
        {
            if (!LineExtension.PointIsOnLine(point, error)) { return false; }
            return PointIsAtOrBetweenEndpoints(point);
        }
    }
}
