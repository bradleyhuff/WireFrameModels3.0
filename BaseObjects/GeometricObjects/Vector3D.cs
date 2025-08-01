﻿using BaseObjects.Transformations.Interfaces;
using System.Collections.Generic;
using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Vector3D : IShape3D<Vector3D>
    {
        public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3D[] CardinalPoints { get { return []; } }
        public Vector3D[] CardinalVectors { get { return [this]; } }
        public Vector3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return cardinalVectors[0];
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3D operator *(double a, Vector3D b)
        {
            return new Vector3D(a * b.X, a * b.Y, a * b.Z);
        }
        public static Vector3D operator /(Vector3D a, double b)
        {
            var divide = 1 / b;
            return new Vector3D(a.X * divide, a.Y * divide, a.Z * divide);
        }

        public static double Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        public static Vector3D Cross(Vector3D a, Vector3D b)
        {
            return Cross(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
        }
        public static Vector3D Cross(double aX, double aY, double aZ, double bX, double bY, double bZ)
        {
            return new Vector3D(
                aY * bZ - aZ * bY,
                aZ * bX - aX * bZ,
                aX * bY - aY * bX
                );
        }

        public static Vector3D Average(IEnumerable<Vector3D> source)
        {
            List<Vector3D> list = source.ToList();

            Vector3D sum = list[0].Direction;

            for (int i = 1; i < list.Count; i++)
            {
                sum += list[i].Direction;
            }

            return sum.Direction;
        }

        public static Vector3D Sum(IEnumerable<Vector3D> source)
        {
            List<Vector3D> list = source.ToList();

            Vector3D sum = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum;
        }

        public static Vector3D MinMagnitude(params Vector3D[] source)
        {
            double minValue = double.MaxValue;
            Vector3D minVector = null;
            foreach (Vector3D element in source)
            {
                if(element.Magnitude < minValue)
                {
                    minValue = element.Magnitude;
                    minVector = element;
                }
            }
            return minVector;
        }

        public static Vector3D MaxMagnitude(params Vector3D[] source)
        {
            double maxValue = 0;
            Vector3D maxVector = null;
            foreach (Vector3D element in source)
            {
                if (element.Magnitude > maxValue)
                {
                    maxValue = element.Magnitude;
                    maxVector = element;
                }
            }
            return maxVector;
        }

        public static Vector3D Interpolation(Vector3D a, Vector3D b, double alpha)
        {
            return (1 - alpha) * a + alpha * b;
        }

        private double _magnitude = 0;
        private bool _magnitudeFound = false;

        public double Magnitude
        {
            get
            {
                if (!_magnitudeFound)
                {
                    _magnitude = System.Math.Sqrt(X * X + Y * Y + Z * Z);
                    _magnitudeFound = true;
                }
                return _magnitude;
            }
        }

        private Vector3D _direction = null;
        public Vector3D Direction
        {
            get
            {
                if (_direction is null)
                {
                    if (E.Double.IsEqual(Magnitude, 0)) { return Vector3D.Zero; }
                    _direction = this / Magnitude;
                }
                return _direction;
            }
        }

        public static bool AreParallel(Vector3D a, Vector3D b, double ε = E.Double.RadianDifferenceError)
        {
            var cross = Cross(a.Direction, b.Direction);
            return cross.Magnitude < ε && System.Math.Sign(Dot(a.Direction, b.Direction)) == 1;
        }

        public static bool ArePolar(Vector3D a, Vector3D b, double ε = E.Double.RadianDifferenceError)
        {
            var cross = Cross(a.Direction, b.Direction);
            return cross.Magnitude < ε && System.Math.Sign(Dot(a.Direction, b.Direction)) == -1;
        }

        public static bool AreOpposite(Vector3D a, Vector3D b, double ε = E.Double.RadianDifferenceError)
        {
            return System.Math.Sign(Dot(a.Direction, b.Direction)) == -1;
        }

        public static bool ArePerpendicular(Vector3D a, Vector3D b, double ε = E.Double.RadianDifferenceError)
        {
            var dot = System.Math.Abs(Dot(a.Direction, b.Direction));
            return dot < ε;
        }

        public override string ToString()
        {
            return $"[ X: {X.ToString("##0.000000")} Y: {Y.ToString("##0.000000")} Z: {Z.ToString("##0.000000")} ]";
        }

        public static Vector3D Zero { get; } = new Vector3D(0, 0, 0);
        public static Vector3D BasisX { get; } = new Vector3D(1, 0, 0);
        public static Vector3D BasisY { get; } = new Vector3D(0, 1, 0);
        public static Vector3D BasisZ { get; } = new Vector3D(0, 0, 1);

        public static double Angle(Vector3D a, Vector3D b)
        {
            var minus = (a.Direction - b.Direction).Magnitude;
            var plus = (a.Direction + b.Direction).Magnitude;
            double theta = 2 * System.Math.Atan2(minus, plus);

            if (E.Double.IsEqual(theta, 0, E.Double.RadianDifferenceError)) { return 0; }
            if (E.Double.IsEqual(theta, System.Math.PI, E.Double.RadianDifferenceError)) { return System.Math.PI; }
            return theta;
        }

        public static double SignedAngle(Vector3D n, Vector3D a, Vector3D b)
        {
            var angle = Angle(a, b);
            var cross = Cross(b, a);
            var dot = Dot(cross, n);
            var sign = System.Math.Sign(dot);
            if (sign == 0)
            {
                if (System.Math.Sign(Dot(a, b)) == 1) { return 0; }
                return System.Math.PI;
            }
            return sign * angle;
        }

        public static (Vector3D VectorA, Vector3D VectorB) GetNearestParallelPair(IEnumerable<Vector3D> setA, IEnumerable<Vector3D> setB)
        {
            double maxDotProduct = -1;
            Vector3D pairA = Vector3D.Zero;
            Vector3D pairB = Vector3D.Zero;
            foreach (var normalA in setA)
            {
                foreach (var normalB in setB)
                {
                    var dotProduct = Dot(normalA, normalB);
                    if (dotProduct > maxDotProduct)
                    {
                        maxDotProduct = dotProduct;
                        pairA = normalA;
                        pairB = normalB;
                    }
                }
            }
            return ( pairA, pairB );
        }
    }
}
