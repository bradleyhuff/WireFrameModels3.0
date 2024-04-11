using BaseObjects.Transformations.Interfaces;
using E = BasicObjects.Math;

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

        private Point3D _center = null;
        public Point3D Center
        {
            get
            {
                if(_center is null)
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

        public Point3D Projection(Point3D p)
        {
            if (p is null) { return null; }
            Vector3D vector = Vector;
            if (vector.Magnitude < E.Double.DifferenceError) { return null; }
            Point3D start = Start;
            double α = (vector.X * (p.X - start.X) + vector.Y * (p.Y - start.Y) + vector.Z * (p.Z - start.Z)) /
                (vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

            Point3D projection = new Point3D(start.X + α * vector.X, start.Y + α * vector.Y, start.Z + α * vector.Z);
            var totalDistance = Point3D.Distance(Start, projection) + Point3D.Distance(End, projection);
            var gap = System.Math.Abs(totalDistance - Length);
            if (gap > E.Double.DifferenceError) { return null; }
            return projection;
        }

        public Point3D Projection(Point3D p, out double gap)
        {
            Point3D projection = Projection(p);
            if (projection is null) { gap = double.NaN; return null; }
            gap = System.Math.Sqrt(E.Math.Square(p.X - projection.X) + E.Math.Square(p.Y - projection.Y) + E.Math.Square(p.Z - projection.Z));
            return projection;
        }

        public bool PointIsAtAnEndpoint(Point3D p)
        {
            return Start == p || End == p;
        }

        public bool PointIsOnLineSegment(Point3D p, double error = E.Double.DifferenceError)
        {
            if (PointIsAtAnEndpoint(p)) { return true; }
            var projection = Projection(p);
            if (projection is null) { return false; }
            var distance = Point3D.Distance(projection, p);
            return distance < error;
        }

        public bool PointIsOnLineSegmentAndBetweenEnds(Point3D p, double error = E.Double.DifferenceError)
        {
            if (PointIsAtAnEndpoint(p)) { return false; }
            var projection = Projection(p);
            if (projection is null) { return false; }
            var distance = Point3D.Distance(projection, p);
            return distance < error;
        }

        public bool Contains(LineSegment3D segment)
        {
            return PointIsOnLineSegment(segment.Start) && PointIsOnLineSegment(segment.End);
        }

        public Point3D OppositePoint(Point3D p)
        {
            if (End != p) { return End; }
            if (Start != p) { return Start; }
            return null;
        }

        public LineSegment3D StartMargin(double margin)
        {
            var vectorMargin = margin * Vector.Direction;
            return new LineSegment3D(Start + -vectorMargin, End);
        }

        public LineSegment3D EndMargin(double margin)
        {
            var vectorMargin = margin * Vector.Direction;
            return new LineSegment3D(Start, End + vectorMargin);
        }

        public LineSegment3D Margins(double margin)
        {
            var vectorMargin = margin * Vector.Direction;
            return new LineSegment3D(Start + -vectorMargin, End + vectorMargin);
        }

        public static bool SegmentsOverlap(LineSegment3D a, LineSegment3D b)
        {
            return SegmentIntersection(a, b) is not null;
        }

        public static bool SegmentsAreStrictlyDisjoint(LineSegment3D a, LineSegment3D b, double maxError = E.Double.DifferenceError)
        {
            var intersection = SegmentIntersection(a, b, maxError);
            return intersection is null || intersection.Length < E.Double.DifferenceError;
        }

        public static LineSegment3D SegmentIntersection(LineSegment3D a, LineSegment3D b, double maxError = E.Double.DifferenceError)
        {
            Point3D projection1 = a.Projection(b.Start);
            Point3D projection2 = a.Projection(b.End);
            Point3D projection3 = b.Projection(a.Start);
            Point3D projection4 = b.Projection(a.End);

            int digit1 = (projection1 is null || Point3D.Distance(projection1, b.Start) > maxError) ? 0 : 1;
            int digit2 = (projection2 is null || Point3D.Distance(projection2, b.End) > maxError) ? 0 : 1;
            int digit3 = (projection3 is null || Point3D.Distance(projection3, a.Start) > maxError) ? 0 : 1;
            int digit4 = (projection4 is null || Point3D.Distance(projection4, a.End) > maxError) ? 0 : 1;

            int caseNumber = digit4 << 3 | digit3 << 2 | digit2 << 1 | digit1;

            switch (caseNumber)
            {
                case 3: return new LineSegment3D(Point3D.MidPoint(projection1, b.Start), Point3D.MidPoint(projection2, b.End));
                case 5: return new LineSegment3D(Point3D.MidPoint(projection1, b.Start), Point3D.MidPoint(projection3, a.Start));
                case 6: return new LineSegment3D(Point3D.MidPoint(projection2, b.End), Point3D.MidPoint(projection3, a.Start));
                case 7://a -> b.start, a -> b.end and b -> a.start
                    return new LineSegment3D(b.Start, b.End);
                case 9: return new LineSegment3D(Point3D.MidPoint(projection1, b.Start), Point3D.MidPoint(projection4, a.End));
                case 10: return new LineSegment3D(Point3D.MidPoint(projection2, b.End), Point3D.MidPoint(projection4, a.End));
                case 11://a -> b.start, a -> b.end and b -> a.end
                    return new LineSegment3D(b.Start, b.End);
                case 12: return new LineSegment3D(Point3D.MidPoint(projection3, a.Start), Point3D.MidPoint(projection4, a.End));
                case 13://b -> a.start, b -> a.end and a -> b.start
                case 14://b -> a.start, b -> a.end and a -> b.end
                case 15: return new LineSegment3D(a.Start, a.End);
            }

            return null;
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
    }
}
