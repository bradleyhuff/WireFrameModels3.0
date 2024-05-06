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
            var octant = OctantMesh(radius, octantSubDivisions);

            var octantClones = octant.Clones(3).ToArray();
            for(int i = 0; i < 3; i++)
            {
                octantClones[i].Apply(Transform.Rotation(Vector3D.BasisZ, (i + 1) * Math.PI / 2));
            }

            var hemisphere = WireFrameMesh.Create();
            hemisphere.AddGrid(octant);
            hemisphere.AddGrids(octantClones);
 
            var hemisphere2 = hemisphere.Clone();
            hemisphere2.Apply(Transform.Rotation(Vector3D.BasisX, Math.PI));

            var sphere = WireFrameMesh.Create();
            sphere.AddGrid(hemisphere);
            sphere.AddGrid(hemisphere2);

            return sphere;
        }

        private static IWireFrameMesh OctantMesh(double radius, int subdivisions)
        {
            int cubeSubdivisions = subdivisions / 2;

            IWireFrameMesh octantMesh = WireFrameMesh.Create();

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
            double theta = alpha * Math.PI / 4;
            return Math.Tan(theta);
        }
    }
}
