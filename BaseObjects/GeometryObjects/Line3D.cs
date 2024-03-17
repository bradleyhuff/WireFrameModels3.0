using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Line3D
    {
        public Line3D(Point3D point, Vector3D vector)
        {
            Start = point;
            End = new Point3D(Start.X + vector.X, Start.Y + vector.Y, Start.Z + vector.Z);
        }

        public Line3D(Point3D start, Point3D end)
        {
            Start = start;
            End = end;
        }
        internal protected Point3D Start { get; }
        internal protected Point3D End { get; }

        private Vector3D _vector = null;
        public Vector3D Vector
        {
            get
            {
                if (_vector is null)
                {
                    //Matricies.Normalize3D(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z, out double v0, out double v1, out double v2);
                    //_vector = new Vector3D(v0, v1, v2);
                    _vector = new Vector3D(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z).Direction;
                }
                return _vector;
            }
        }

        private Point3D _xIntercept = null;
        private bool _xInterceptIsFound = false;

        public Point3D Xintercept
        {
            get
            {
                if (!_xInterceptIsFound)
                {
                    double α = -Start.X / Vector.X;
                    if (E.Double.IsValid(α))
                    {
                        _xIntercept = new Point3D(0, Start.Y + α * Vector.Y, Start.Z + α * Vector.Z);
                    }
                    _xInterceptIsFound = true;
                }
                return _xIntercept;
            }
        }

        private Point3D _yIntercept = null;
        private bool _yInterceptIsFound = false;
        public Point3D Yintercept
        {
            get
            {
                if (!_yInterceptIsFound)
                {
                    double α = -Start.Y / Vector.Y;
                    if (E.Double.IsValid(α))
                    {
                        _yIntercept = new Point3D(Start.X + α * Vector.X, 0, Start.Z + α * Vector.Z);
                    }
                    _yInterceptIsFound = true;
                }
                return _yIntercept;
            }
        }

        private Point3D _zIntercept = null;
        private bool _zInterceptIsFound = false;

        public Point3D Zintercept
        {
            get
            {
                if (!_zInterceptIsFound)
                {
                    double α = -Start.Z / Vector.Z;
                    if (E.Double.IsValid(α))
                    {
                        _zIntercept = new Point3D(Start.X + α * Vector.X, Start.Y + α * Vector.Y, 0);
                    }
                    _zInterceptIsFound = true;
                }
                return _zIntercept;
            }
        }

        public override string ToString()
        {
            return $"Line X-intercept {Xintercept} Y-intercept {Yintercept} Z-intercept {Zintercept}";
        }

        public Point3D Projection(Point3D p)
        {
            Vector3D vector = Vector;
            if (vector.Magnitude < E.Double.DifferenceError) { return null; }
            Point3D start = Start;
            double α = (vector.X * (p.X - start.X) + vector.Y * (p.Y - start.Y) + vector.Z * (p.Z - start.Z)) /
                (vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

            Point3D projection = new Point3D(start.X + α * vector.X, start.Y + α * vector.Y, start.Z + α * vector.Z);
            return projection;
        }

        public double Distance(Point3D p)
        {
            Vector3D vector = Vector;
            if (vector.Magnitude < E.Double.DifferenceError) { return double.PositiveInfinity; }
            Point3D start = Start;
            double distance = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
            double α = (vector.X * (p.X - start.X) + vector.Y * (p.Y - start.Y) + vector.Z * (p.Z - start.Z)) /
                distance;

            return System.Math.Abs(α) * System.Math.Sqrt(distance);
        }

        public bool PointIsOnLine(Point3D point, double error = E.Double.DifferenceError)
        {
            return Point3D.Distance(point, Projection(point)) < error;
        }

        public bool SegmentIsOnLine(LineSegment3D segment, double error = E.Double.DifferenceError)
        {
            return PointIsOnLine(segment.Start, error) && PointIsOnLine(segment.End, error);
        }

        public Point3D Projection(Point3D p, out double gap)
        {
            Point3D projection = Projection(p);
            gap = System.Math.Sqrt(E.Math.Square(p.X - projection.X) + E.Math.Square(p.Y - projection.Y) + E.Math.Square(p.Z - projection.Z));
            return projection;
        }

        public static Point3D PointIntersection(Line3D a, Line3D b)
        {
            double gap;
            var point = MidPointIntersection(a, b, out gap);
            return gap < E.Double.DifferenceError ? point : null;
        }

        public static Point3D MidPointIntersection(Line3D a, Line3D b, out double gap)
        {
            return MidPointIntersection(a.Start, a.Vector, b.Start, b.Vector, out gap);
        }

        public static IEnumerable<Point3D> PointIntersections(LineSegment3D segment, IEnumerable<LineSegment3D> matches)
        {
            foreach (var match in matches)
            {
                var point = PointIntersection(segment, match);
                if (point is not null) { yield return point; }
            }
        }

        private static IEnumerable<Point3D> PointIntersectionsFullSegment(LineSegment3D segment, IEnumerable<LineSegment3D> matches)
        {
            yield return segment.Start;

            foreach (var match in matches)
            {
                var point = PointIntersection(segment, match);
                if (point is not null) { yield return point; }
            }

            yield return segment.End;
        }

        private static IEnumerable<Point3D> PointIntersectionsFullSegment(LineSegment3D segment, IEnumerable<Point3D> points)
        {
            yield return segment.Start;

            foreach (var point in points.Where(p => segment.PointIsOnLineSegment(p)))
            {
                yield return point;
            }

            yield return segment.End;
        }

        private static IEnumerable<Point3D> PointIntersectionsOrderedPointsFullSegment(LineSegment3D segment, IEnumerable<LineSegment3D> matches)
        {
            return PointIntersectionsFullSegment(segment, matches).OrderBy(p => Point3D.Distance(segment.Start, p));
        }

        private static IEnumerable<Point3D> PointIntersectionsOrderedPointsFullSegment(LineSegment3D segment, IEnumerable<Point3D> points)
        {
            return PointIntersectionsFullSegment(segment, points).OrderBy(p => Point3D.Distance(segment.Start, p));
        }

        public static IEnumerable<LineSegment3D> PointIntersectionDivisions(LineSegment3D segment, IEnumerable<LineSegment3D> matches)
        {
            var array = PointIntersectionsOrderedPointsFullSegment(segment, matches).ToArray();

            for (int i = 0; i < array.Length - 1; i++)
            {
                var division = new LineSegment3D(array[i], array[i + 1]);
                if (division.Length < E.Double.DifferenceError) { continue; }
                yield return division;
            }
        }

        public static IEnumerable<LineSegment3D> PointIntersectionDivisions(LineSegment3D segment, IEnumerable<Point3D> points)
        {
            var array = PointIntersectionsOrderedPointsFullSegment(segment, points).ToArray();

            for (int i = 0; i < array.Length - 1; i++)
            {
                var division = new LineSegment3D(array[i], array[i + 1]);
                if (division.Length < E.Double.DifferenceError) { continue; }
                yield return division;
            }
        }

        public static Point3D PointIntersection(LineSegment3D a, LineSegment3D b)
        {
            double gap;
            var point = MidPointIntersection(a, b, out gap);
            return gap < E.Double.DifferenceError ? point : null;
        }

        private static Point3D MidPointIntersection(LineSegment3D a, LineSegment3D b, out double gap)
        {
            var intersection = MidPointIntersection(a.Start, a.Vector, b.Start, b.Vector, out gap);
            if (intersection is null) { return null; }
            if (a is not null && !a.PointIsOnLineSegment(intersection)) { return null; }
            if (b is not null && !b.PointIsOnLineSegment(intersection)) { return null; }
            return intersection;
        }

        private static Point3D MidPointIntersection(Point3D aStart, Vector3D aVector, Point3D bStart, Vector3D bVector, out double gap)
        {
            // cross of lines a and b

            var aVectorNormal = aVector.Direction;
            var bVectorNormal = bVector.Direction;
            //Matricies.Cross3D(aVectorNormal.X, aVectorNormal.Y, aVectorNormal.Z, bVectorNormal.X, bVectorNormal.Y, bVectorNormal.Z, out double c0, out double c1, out double c2);
            var direction = Vector3D.Cross(aVectorNormal, bVectorNormal);

            if (E.Double.IsZero(direction.X) && E.Double.IsZero(direction.Y) && E.Double.IsZero(direction.Z)) { gap = double.NaN; return null; }

            E.LinearSystems.Solve3x3(
                aVector.X, -bVector.X, direction.X, aVector.Y,
                -bVector.Y, direction.Y, aVector.Z, -bVector.Z,
                direction.Z, bStart.X - aStart.X, bStart.Y - aStart.Y, bStart.Z - aStart.Z,
                out double α0, out double α1, out double ß);

            double ia0 = aStart.X + α0 * aVector.X; // intersection of line a
            double ia1 = aStart.Y + α0 * aVector.Y;
            double ia2 = aStart.Z + α0 * aVector.Z;

            double ib0 = bStart.X + α1 * bVector.X; // intersection of line b
            double ib1 = bStart.Y + α1 * bVector.Y;
            double ib2 = bStart.Z + α1 * bVector.Z;

            gap = System.Math.Sqrt(E.Math.Square(ia0 - ib0) + E.Math.Square(ia1 - ib1) + E.Math.Square(ia2 - ib2));

            return new Point3D((ia0 + ib0) * 0.5, (ia1 + ib1) * 0.5, (ia2 + ib2) * 0.5);
        }

        public static Point3D PointIntersection(Line3D a, LineSegment3D b)
        {
            double gap;
            var point = MidPointIntersection(a, b, out gap);
            return gap < E.Double.DifferenceError ? point : null;
        }

        public static Point3D PointIntersection(LineSegment3D a, Line3D b)
        {
            double gap;
            var point = MidPointIntersection(b, a, out gap);
            return gap < E.Double.DifferenceError ? point : null;
        }

        public static Point3D MidPointIntersection(LineSegment3D a, Line3D b, out double gap)
        {
            return MidPointIntersection(b, a, out gap);
        }
        public static Point3D MidPointIntersection(Line3D a, LineSegment3D b, out double gap)
        {
            var intersection = MidPointIntersection(a.Start, a.Vector, b.Start, b.Vector, out gap);
            if (b is not null && intersection is not null && !b.PointIsOnLineSegment(intersection)) { return null; }
            return intersection;
        }

    }
}
