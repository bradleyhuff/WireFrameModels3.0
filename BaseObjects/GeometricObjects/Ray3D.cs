
using BaseObjects.Transformations.Interfaces;

namespace BasicObjects.GeometricObjects
{
    public class Ray3D : IShape3D<Ray3D>
    {
        public Ray3D(Point3D point, Vector3D normal)
        {
            Point = point;
            Normal = normal;
        }

        public Point3D Point { get; }
        public Vector3D Normal { get; private set; }

        private Plane _plane;
        public Plane Plane
        {
            get
            {
                if (_plane is null)
                {
                    _plane = new Plane(this);
                }
                return _plane;
            }
        }

        private Line3D _line;

        public Line3D Line
        {
            get
            {
                if(_line is null)
                {
                    _line = new Line3D(Point, Normal);
                }
                return _line;
            }
        }

        public Point3D[] CardinalPoints { get { return [Point]; } }
        public Vector3D[] CardinalVectors { get { return [Normal]; } }
        public Ray3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new Ray3D(cardinalPoints[0], cardinalVectors[0]);
        }

        public static Ray3D Average(IEnumerable<Ray3D> source)
        {
            return new Ray3D(Point3D.Average(source.Select(s => s.Point)), Vector3D.Average(source.Select(s => s.Normal)));
        }

        public static Ray3D Interpolation(Ray3D a, Ray3D b, double alpha)
        {
            return new Ray3D(Point3D.Interpolation(a.Point, b.Point, alpha), Vector3D.Interpolation(a.Normal, b.Normal, alpha));
        }

        private bool _wasInverted = false;

        public void InvertNormal()
        {
            if (_wasInverted || Normal is null) { return; }
            Normal = -Normal;
            _wasInverted = true;
        }


        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Ray3D) { return false; }
            Ray3D compare = (Ray3D)obj;
            return compare.Point.Equals(Point) && Vector3D.AreParallel(compare.Normal, Normal);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return $"{Point}{Normal.Direction}";
        }

        public static bool operator ==(Ray3D a, Ray3D b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Ray3D a, Ray3D b)
        {
            return !a.Equals(b);
        }
    }
}
