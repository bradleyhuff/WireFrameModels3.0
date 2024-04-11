
using BaseObjects.Transformations.Interfaces;

namespace BasicObjects.GeometricObjects
{
    public class SurfaceTriangle : IShape3D<SurfaceTriangle>
    {
        public SurfaceTriangle(Ray3D a, Ray3D b, Ray3D c)
        {
            A = a;
            B = b;
            C = c;
            Triangle = new Triangle3D(a.Point, b.Point, c.Point);
        }

        public Ray3D A { get; }
        public Ray3D B { get; }
        public Ray3D C { get; }

        public IEnumerable<Ray3D> Verticies { get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        public Triangle3D Triangle { get; }

        public Ray3D RayFromProjectedPoint(Point3D point)
        {
            var projection = Triangle.Plane.Projection(point);

            var c = Triangle.GetBarycentricCoordinate(projection);

            return new Ray3D(projection, (c.λ1 * A.Normal + c.λ2 * B.Normal + c.λ3 * C.Normal).Direction);
        }

        public Point3D[] CardinalPoints { get { return [A.Point, B.Point, C.Point]; } }
        public Vector3D[] CardinalVectors { get { return [A.Normal, B.Normal, C.Normal]; } }
        public SurfaceTriangle Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new SurfaceTriangle(new Ray3D(cardinalPoints[0], cardinalVectors[0]), new Ray3D(cardinalPoints[1], cardinalVectors[1]), new Ray3D(cardinalPoints[2], cardinalVectors[2]));
        }

        public override string ToString()
        {
            return $"Lengths {Triangle.LengthAB.ToString("0.000000")}, {Triangle.LengthBC.ToString("0.000000")}, {Triangle.LengthCA.ToString("0.000000")}  A: {A} B: {B} C: {C}";
        }
    }
}
