
namespace BasicObjects.GeometricObjects
{
    public class SurfaceTriangle
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

        public void InvertNormals()
        {
            A.InvertNormal();
            B.InvertNormal();
            C.InvertNormal();
        }

        public override string ToString()
        {
            return $"Lengths {Triangle.LengthAB.ToString("0.000000")}, {Triangle.LengthBC.ToString("0.000000")}, {Triangle.LengthCA.ToString("0.000000")}  A: {A} B: {B} C: {C}";
        }
    }
}
