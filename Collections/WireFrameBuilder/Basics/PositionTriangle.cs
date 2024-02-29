using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using M = BasicObjects.Math;

namespace Collections.WireFrameBuilder.Basics
{
    public class PositionTriangle : IBox
    {
        private static int _id = 0;
        public PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            A = a;
            B = b;
            C = c;
            Id = _id++;
        }

        public int Id { get; }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }
        public PositionNormal C { get; private set; }

        private Rectangle3D _box = null;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && Triangle is not null)
                {
                    _box = Triangle.Box.Margin(M.Double.DifferenceError);
                }
                return _box;
            }
        }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        private Triangle3D _triangle = null;
        public Triangle3D Triangle
        {
            get
            {
                if (_triangle is null)
                {
                    _triangle = new Triangle3D(
                        A.Position.Point,
                        B.Position.Point,
                        C.Position.Point);
                }
                return _triangle;
            }
        }

        private SurfaceTriangle _surfaceTriangle = null;
        public SurfaceTriangle SurfaceTriangle
        {
            get
            {
                if (_surfaceTriangle is null)
                {
                    _surfaceTriangle = new SurfaceTriangle(
                        A.Ray,
                        B.Ray,
                        C.Ray);
                }
                return _surfaceTriangle;
            }
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                if (A.Position?.Point is not null) yield return A.Position.Point;
                if (B.Position?.Point is not null) yield return B.Position.Point;
                if (C.Position?.Point is not null) yield return C.Position.Point;
            }
        }
    }
}
