using BasicObjects.GeometricObjects;
using Double = BasicObjects.Math.Double;
using Math = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Point2D
    {
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Point2D) { return false; }
            Point2D compare = (Point2D)obj;
            return AreEqual(this, compare);
        }

        public static bool AreEqual(Point2D a, Point2D b)
        {
            var distanceX = System.Math.Abs(a.X - b.X);
            if (distanceX > Double.DifferenceError) { return false; }
            var distanceY = System.Math.Abs(a.Y - b.Y);
            if (distanceY > Double.DifferenceError) { return false; }
            return System.Math.Sqrt(distanceX * distanceX + distanceY * distanceY) < Double.DifferenceError;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public Point2D? GetNearestPoint(params Point2D[] points)
        {
            Point2D? nearestPoint = null;
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

        public static Vector2D operator -(Point2D a, Point2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }
        public static Point2D operator +(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }
        public static Point2D operator +(Point2D a, Vector2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }
        public static Point2D operator *(double a, Point2D b)
        {
            return new Point2D(a * b.X, a * b.Y);
        }
        public static Point2D operator /(Point2D b, double a)
        {
            var divide = 1 / a;
            return new Point2D(b.X * divide, b.Y * divide);
        }
        public static bool operator ==(Point2D a, Point2D b)
        {
            return AreEqual(a, b);
        }
        public static bool operator !=(Point2D a, Point2D b)
        {
            return !AreEqual(a, b);
        }
        public static Point2D MidPoint(Point2D a, Point2D b)
        {
            return new Point2D((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5);
        }
        public static double Distance(Point2D a, Point2D b)
        {
            if (a is null || b is null) { return double.NaN; }
            return System.Math.Sqrt(Math.Math.Square(a.X - b.X));
        }

        public static Point2D Average(IEnumerable<Point2D> source)
        {
            List<Point2D> list = source.ToList();

            double xSum = list[0].X;
            double ySum = list[0].Y;

            for (int i = 1; i < list.Count; i++)
            {
                xSum += list[i].X;
                ySum += list[i].Y;
            }

            return new Point2D(xSum / list.Count, ySum / list.Count);
        }

        public static Point2D Zero
        {
            get
            {
                return new Point2D(0, 0);
            }
        }
        public override string ToString()
        {
            return $"[ X: {X.ToString("#,##0.0000000000")} Y: {Y.ToString("#,##0.0000000000")} ]";
        }
    }
}
