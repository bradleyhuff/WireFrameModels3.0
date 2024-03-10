using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundamentalMeshes
{
    public static class Sphere
    {
        public static IWireFrameMesh Create(double radius, int octantSubDivisions)
        {
            var sphere = WireFrameMesh.CreateMesh();
            var octant = OctantMeshBuilder(radius, octantSubDivisions);

            var octant2 = octant.Clone();
            var octant3 = octant.Clone();
            var octant4 = octant.Clone();

            var transform = Transform.Rotation(new Vector3D(0, 0, 1), Math.PI / 2);
            octant2.Transformation(p => transform.Apply(p));
            transform = Transform.Rotation(new Vector3D(0, 0, 1), Math.PI);
            octant3.Transformation(p => transform.Apply(p));
            transform = Transform.Rotation(new Vector3D(0, 0, 1), -Math.PI / 2);
            octant4.Transformation(p => transform.Apply(p));

            var hemisphere = WireFrameMesh.CreateMesh();
            hemisphere.AddGrid(octant);
            hemisphere.AddGrid(octant2);
            hemisphere.AddGrid(octant3);
            hemisphere.AddGrid(octant4);

            var hemisphere2 = hemisphere.Clone();
            transform = Transform.Rotation(new Vector3D(1, 0, 0), Math.PI);
            hemisphere2.Transformation(p => transform.Apply(p));

            sphere.AddGrid(hemisphere);
            sphere.AddGrid(hemisphere2);

            return sphere;
        }

        private static IWireFrameMesh OctantMeshBuilder(double radius, int subdivisions)
        {
            int cubeSubdivisions = subdivisions / 2;

            IWireFrameMesh octantMesh = WireFrameMesh.CreateMesh();

            //Z triad
            for (int x = 0; x <= cubeSubdivisions; x++)
            {
                for (int y = 0; y <= cubeSubdivisions; y++)
                {
                    Vector3D sphericalVector = SphericalVector((double)x / cubeSubdivisions, (double)y / cubeSubdivisions, 0);
                    double ux = sphericalVector.X;
                    double uy = sphericalVector.Y;
                    double uz = sphericalVector.Z;
                    octantMesh.AddPoint(new Point3D(radius * ux, radius * uy, radius * uz), new Vector3D(ux, uy, uz));
                }
                octantMesh.EndRow();
            }
            octantMesh.EndGrid();

            //Y triad
            for (int x = 0; x <= cubeSubdivisions; x++)
            {
                for (int z = 0; z <= cubeSubdivisions; z++)
                {
                    Vector3D sphericalVector = SphericalVector((double)x / cubeSubdivisions, 0, (double)z / cubeSubdivisions);
                    double ux = sphericalVector.X;
                    double uy = sphericalVector.Y;
                    double uz = sphericalVector.Z;
                    octantMesh.AddPoint(new Point3D(radius * ux, radius * uy, radius * uz), new Vector3D(ux, uy, uz));
                }
                octantMesh.EndRow();
            }
            octantMesh.EndGrid();

            //X triad
            for (int y = 0; y <= cubeSubdivisions; y++)
            {
                for (int z = 0; z <= cubeSubdivisions; z++)
                {
                    Vector3D sphericalVector = SphericalVector(0, (double)y / cubeSubdivisions, (double)z / cubeSubdivisions);
                    double ux = sphericalVector.X;
                    double uy = sphericalVector.Y;
                    double uz = sphericalVector.Z;
                    octantMesh.AddPoint(new Point3D(radius * ux, radius * uy, radius * uz), new Vector3D(ux, uy, uz));
                }
                octantMesh.EndRow();
            }
            octantMesh.EndGrid();

            return octantMesh;
        }

        //Alphas must be between 0 and 1 and at least one alpha must be zero
        private static Vector3D SphericalVector(double alphaX, double alphaY, double alphaZ)
        {
            return (TangentConversion(1 - alphaX) * Vector3D.BasisX + TangentConversion(1 - alphaY) * Vector3D.BasisY + TangentConversion(1 - alphaZ) * Vector3D.BasisZ).Direction;
        }

        private static double TangentConversion(double alpha)
        {
            if (alpha == 0)
            {
                return 0;
            }
            double theta = Math.PI / 2 - alpha * Math.PI / 4;
            return 1 / Math.Tan(theta);
        }
    }
}
