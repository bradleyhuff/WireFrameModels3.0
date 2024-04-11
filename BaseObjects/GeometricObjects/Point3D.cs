using BaseObjects.Transformations.Interfaces;
using Double = BasicObjects.Math.Double;

namespace BasicObjects.GeometricObjects
{
    public class Point3D : IShape3D<Point3D>
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3D[] CardinalPoints { get { return [this]; } }
        public Vector3D[] CardinalVectors { get { return []; } }
        public Point3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return cardinalPoints[0];
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Point3D) { return false; }
            Point3D compare = (Point3D)obj;
            return AreEqual(this, compare);
        }

        public static bool AreEqual(Point3D a, Point3D b)
        {
            var distanceX = System.Math.Abs(a.X - b.X);
            if (distanceX > Double.DifferenceError) { return false; }
            var distanceY = System.Math.Abs(a.Y - b.Y);
            if (distanceY > Double.DifferenceError) { return false; }
            var distanceZ = System.Math.Abs(a.Z - b.Z);
            if (distanceZ > Double.DifferenceError) { return false; }
            return System.Math.Sqrt(distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ) < Double.DifferenceError;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public Point3D? GetNearestPoint(params Point3D[] points)
        {
            Point3D? nearestPoint = null;
            double nearestDistance = double.MaxValue;

            foreach (var point in points)
            {
                double distance = Distance(point, this);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        public Rectangle3D Margin(double margin)
        {
            return new Rectangle3D(this + new Vector3D(-margin, -margin, -margin), this + new Vector3D(margin, margin, margin));
        }

        public static bool AreCollinear(params Point3D[] points)
        {
            if (points.Length < 3) { throw new InvalidOperationException($"At least 3 points are required to check collinearity."); }
            var line = new Line3D(points.First(), points.Last());

            return points.Skip(1).SkipLast(1).All(p => line.PointIsOnLine(p));
        }

        public static bool AreCoplanar(params Point3D[] points)
        {
            if (points.Length < 4) { throw new InvalidOperationException($"At least 4 points are required to check coplanarity."); }
            var plane = new Plane(points.First(), points.Skip(points.Length >> 1).First(), points.Last());

            return points.Skip(1).SkipLast(1).All(p => plane.PointIsOnPlane(p));
        }

        public static Vector3D operator -(Point3D a, Point3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Point3D operator +(Point3D a, Point3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Point3D operator +(Point3D a, Vector3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Point3D operator *(double a, Point3D b)
        {
            return new Point3D(a * b.X, a * b.Y, a * b.Z);
        }
        public static Point3D operator /(Point3D b, double a)
        {
            var divide = 1 / a;
            return new Point3D(b.X * divide, b.Y * divide, b.Z * divide);
        }
        public static bool operator ==(Point3D a, Point3D b)
        {
            return AreEqual(a, b);
        }
        public static bool operator !=(Point3D a, Point3D b)
        {
            return !AreEqual(a, b);
        }
        public static Point3D MidPoint(Point3D a, Point3D b)
        {
            return new Point3D((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5, (a.Z + b.Z) * 0.5);
        }
        public static double Distance(Point3D a, Point3D b)
        {
            if (a is null || b is null) { return double.NaN; }
            return System.Math.Sqrt(Math.Math.Square(a.X - b.X) + Math.Math.Square(a.Y - b.Y) + Math.Math.Square(a.Z - b.Z));
        }

        public static Point3D Average(IEnumerable<Point3D> source)
        {
            List<Point3D> list = source.ToList();

            double xSum = list[0].X;
            double ySum = list[0].Y;
            double zSum = list[0].Z;

            for (int i = 1; i < list.Count; i++)
            {
                xSum += list[i].X;
                ySum += list[i].Y;
                zSum += list[i].Z;
            }

            return new Point3D(xSum / list.Count, ySum / list.Count, zSum / list.Count);
        }

        public static Point3D Zero
        {
            get
            {
                return new Point3D(0, 0, 0);
            }
        }
        public override string ToString()
        {
            return $"[ X: {X.ToString("#,##0.00000000000000")} Y: {Y.ToString("#,##0.00000000000000")} Z: {Z.ToString("#,##0.00000000000000")} ]";
            //return $"[ X: {X.ToString("#,##0.0000000000")} Y: {Y.ToString("#,##0.0000000000")} Z: {Z.ToString("#,##0.0000000000")} ]";
            //return $"[ X: {X.ToString("#,##0.0000")} Y: {Y.ToString("#,##0.0000")} Z: {Z.ToString("#,##0.0000")} ]";
        }
    }
}
