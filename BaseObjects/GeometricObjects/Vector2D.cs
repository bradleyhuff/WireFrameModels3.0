using BasicObjects.GeometricObjects;

namespace BaseObjects.GeometryObjects
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
    }
}
