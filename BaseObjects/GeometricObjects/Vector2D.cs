using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Vector2D
    {
        public Vector2D(double x, double y) { X = x; Y = y; }

        public double X { get; }
        public double Y { get; }

        public static Vector2D operator -(Vector2D vector)
        {
            return new Vector2D(-vector.X, -vector.Y);
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D operator *(double a, Vector2D b)
        {
            return new Vector2D(a * b.X, a * b.Y);
        }
        public static Vector2D operator /(Vector2D a, double b)
        {
            var divide = 1 / b;
            return new Vector2D(a.X * divide, a.Y * divide);
        }
        public static double Dot(Vector2D a, Vector2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector3D Cross(Vector2D a, Vector2D b)
        {
            return Cross(a.X, a.Y, b.X, b.Y);
        }
        public static Vector3D Cross(double aX, double aY, double bX, double bY)
        {
            return new Vector3D(
                0,
                0,
                aX * bY - aY * bX
                );
        }

        public static Vector2D Average(IEnumerable<Vector2D> source)
        {
            List<Vector2D> list = source.ToList();

            Vector2D sum = list[0].Direction;

            for (int i = 1; i < list.Count; i++)
            {
                sum += list[i].Direction;
            }

            return sum.Direction;
        }

        public static Vector2D Sum(IEnumerable<Vector2D> source)
        {
            List<Vector2D> list = source.ToList();

            Vector2D sum = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum;
        }

        private double _magnitude = 0;
        private bool _magnitudeFound = false;

        public double Magnitude
        {
            get
            {
                if (!_magnitudeFound)
                {
                    _magnitude = System.Math.Sqrt(X * X + Y * Y);
                    _magnitudeFound = true;
                }
                return _magnitude;
            }
        }

        private Vector2D _direction = null;
        public Vector2D Direction
        {
            get
            {
                if (_direction is null)
                {
                    _direction = this / Magnitude;
                }
                return _direction;
            }
        }

        public static bool DirectionsEqual(Vector2D a, Vector2D b, double ε = E.Double.RadianDifferenceError)
        {
            var cross = Cross(a.Direction, b.Direction);
            return cross.Magnitude < ε && System.Math.Sign(Dot(a.Direction, b.Direction)) == 1;
        }

        public override string ToString()
        {
            return $"[ X: {X.ToString("##0.000000")} Y: {Y.ToString("##0.000000")} ]";
        }

        public static Vector2D Zero { get; } = new Vector2D(0, 0);
        public static Vector2D BasisX { get; } = new Vector2D(1, 0);
        public static Vector2D BasisY { get; } = new Vector2D(0, 1);

        public static double Angle(Vector2D a, Vector2D b)
        {
            var dot = Dot(a, b) / (a.Magnitude * b.Magnitude);
            dot = E.Math.Max(-1, dot);
            dot = E.Math.Min(1, dot);
            double theta = System.Math.Acos(dot);
            if (E.Double.IsEqual(theta, 0, E.Double.RadianDifferenceError)) { return 0; }
            if (E.Double.IsEqual(theta, System.Math.PI, E.Double.RadianDifferenceError)) { return System.Math.PI; }
            return theta;
        }
    }
}
