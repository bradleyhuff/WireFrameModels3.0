
namespace BasicObjects.Math
{
    public static class Double
    {
        public const double ProximityError = 1.0e-10;
        public const double DifferenceError = 1.0e-14;
        public const double RadianDifferenceError = 1e-9;

        public static bool IsEqual(double x, double y, double epsilon = DifferenceError)
        {
            if (x == y) { return true; }

            double absoluteDifference = System.Math.Abs(x - y);
            return absoluteDifference < epsilon;
        }

        public static double RoundToZero(double x)
        {
            if (System.Math.Abs(x) < DifferenceError) { return 0; }
            return x;
        }

        public static bool IsZero(double x)
        {
            return x > -DifferenceError && x < DifferenceError;
        }

        public static double Distance(double x, double y)
        {
            return RoundToZero(System.Math.Abs(x - y));
        }

        public static double Alpha(double minimum, double maximum, double value)
        {
            return (value - minimum) / (maximum - minimum);
        }

        public static bool IsValid(double value)
        {
            if (double.IsNaN(value)) { return false; }
            if (double.IsInfinity(value)) { return false; }
            return true;
        }

        /// <summary>
        /// a <= x <= b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsOnInterval(double a, double x, double b)
        {
            return a - DifferenceError < x && x < b + DifferenceError;
        }

        public static bool IsOutsideInterval(double a, double x, double b)
        {
            return a - DifferenceError > x || x > b + DifferenceError;
        }

        /// <summary>
        /// a < x < b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsInInterval(double a, double x, double b)
        {
            return a + DifferenceError < x && x < b - DifferenceError;
        }

        public static bool IsBetween(this double x, double min, double max)
        {
            return x >= min && x <= max;
        }
    }
}
