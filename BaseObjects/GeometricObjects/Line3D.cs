using BaseObjects.Transformations.Interfaces;
using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Line3D : IShape3D<Line3D>
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

        public Point3D[] CardinalPoints { get { return [Start, End]; } }
        public Vector3D[] CardinalVectors { get { return []; } }
        public Line3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new Line3D(cardinalPoints[0], cardinalPoints[1]);
        }

        private Vector3D _vector = null;
        public Vector3D Vector
        {
            get
            {
                if (_vector is null)
                {
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
            if (vector.Magnitude < E.Double.ProximityError) { return null; }
            Point3D start = Start;
            double α = (vector.X * (p.X - start.X) + vector.Y * (p.Y - start.Y) + vector.Z * (p.Z - start.Z)) /
                (vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

            Point3D projection = new Point3D(start.X + α * vector.X, start.Y + α * vector.Y, start.Z + α * vector.Z);
            return projection;
        }

        public double Distance(Point3D p)
        {
            return Point3D.Distance(p, Projection(p));
        }

        public bool PointIsOnLine(Point3D point, double error = E.Double.ProximityError)
        {
            return Point3D.Distance(point, Projection(point)) < error;
        }

        public bool SegmentIsOnLine(LineSegment3D segment, double error = E.Double.ProximityError)
        {
            return PointIsOnLine(segment.Start, error) && PointIsOnLine(segment.End, error);
        }



        internal static Point3D MidPointIntersection(Point3D aStart, Vector3D aVector, Point3D bStart, Vector3D bVector, out double gap)
        {
            var aVectorNormal = aVector.Direction;
            var bVectorNormal = bVector.Direction;
            var direction = Vector3D.Cross(aVectorNormal, bVectorNormal);
            if (direction.Magnitude < E.Double.ProximityError) { gap = double.NaN; return null; }// take out later

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

        public static Point3D PointIntersection(LineSegment3D a, Line3D b)
        {
            if (b.PointIsOnLine(a.Start)) { return a.Start; }
            if (b.PointIsOnLine(a.End)) { return a.End; }
            var intersection = MidPointIntersection(a.Start, a.Vector, b.Start, b.Vector, out double gap);
            if (gap > E.Double.ProximityError) { return null; }
            return a.PointIsAtOrBetweenEndpoints(intersection) ? intersection : null;
        }

    }
}
