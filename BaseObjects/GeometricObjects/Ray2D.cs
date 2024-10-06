using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicObjects.GeometricObjects
{
    public class Ray2D
    {
        public Ray2D(Point2D point, Vector2D normal)
        {
            Point = point;
            Normal = normal;
        }

        public Point2D Point { get; }
        public Vector2D Normal { get; private set; }

        public static Ray2D Average(IEnumerable<Ray2D> source)
        {
            return new Ray2D(Point2D.Average(source.Select(s => s.Point)), Vector2D.Average(source.Select(s => s.Normal)));
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Ray2D) { return false; }
            Ray2D compare = (Ray2D)obj;
            return compare.Point.Equals(Point) && Vector2D.DirectionsEqual(compare.Normal, Normal);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return $"{Point}{Normal.Direction}";
        }

        public static bool operator ==(Ray2D a, Ray2D b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Ray2D a, Ray2D b)
        {
            return !a.Equals(b);
        }
    }
}
