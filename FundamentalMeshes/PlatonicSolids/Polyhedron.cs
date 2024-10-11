using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;

namespace FundamentalMeshes.PlatonicSolids
{
    internal class Polyhedron
    {
        public static IWireFrameMesh BuildFace(Point3D[] polygon, Point3D center, Vector3D normal, int divisions)
        {
            var face = WireFrameMesh.Create();

            for (int i = 0; i < polygon.Length; i++)
            {
                face.AddGrid(BuildTriangleGrid(polygon[i], polygon[(i + 1) % polygon.Length], center, normal, divisions));
            }

            return face;
        }

        public static IWireFrameMesh BuildFace(Point3D a, Point3D b, Point3D c, Vector3D normal, int divisions)
        {
            var face = BuildTriangleGrid(a, b, c, normal, divisions);

            return face;
        }

        private static IWireFrameMesh BuildTriangleGrid(Point3D a, Point3D b, Point3D c, Vector3D normal, int divisions)
        {
            var triangleGrid = WireFrameMesh.Create();

            for (int i = 0; i <= divisions; i++)
            {
                double alphaI = (double)i / divisions;
                Point3D pointA = Point3D.Interpolation(a, c, alphaI);
                Point3D pointB = Point3D.Interpolation(b, c, alphaI);

                if (i == divisions)
                {
                    triangleGrid.AddPoint(c, normal);
                    triangleGrid.EndRow();
                }
                else
                {
                    for (int j = 0; j <= (divisions - i); j++)
                    {
                        double alphaJ = (double)j / (divisions - i);
                        Point3D point = Point3D.Interpolation(pointA, pointB, alphaJ);
                        triangleGrid.AddPoint(point, normal);
                    }
                    triangleGrid.EndRow();
                }
            }

            triangleGrid.EndGrid();

            return triangleGrid;
        }
    }
}
