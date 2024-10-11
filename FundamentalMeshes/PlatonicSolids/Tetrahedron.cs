using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundamentalMeshes.PlatonicSolids
{
    public static class Tetrahedron
    {
        // Verticies
        private static Point3D _a = new Point3D(1, 1, 1); //0
        private static Point3D _b = new Point3D(1, -1, -1); //1
        private static Point3D _c = new Point3D(-1, 1, -1); //2
        private static Point3D _d = new Point3D(-1, -1, 1); //3

        //Faces:
        // (_a, _b, _c) A
        private static Vector3D _normalA = Vector3D.Cross((_b - _a).Direction, (_c - _b).Direction);
        // (_a, _b, _d) B
        private static Vector3D _normalC = Vector3D.Cross((_b - _a).Direction, (_b - _d).Direction);
        // (_a, _c, _d) C
        private static Vector3D _normalD = Vector3D.Cross((_a - _c).Direction, (_c - _d).Direction);
        // (_b, _c, _d) D
        private static Vector3D _normalB = Vector3D.Cross((_c - _b).Direction, (_c - _d).Direction);

        public static IWireFrameMesh Build(int divisions)
        {
            var mesh = WireFrameMesh.Create();
            mesh.AddGrid(Polyhedron.BuildFace(_a, _b, _c, _normalA, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _b, _d, _normalC, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _c, _d, _normalD, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_b, _c, _d, _normalB, divisions));

            return mesh;
        }
    }
}
