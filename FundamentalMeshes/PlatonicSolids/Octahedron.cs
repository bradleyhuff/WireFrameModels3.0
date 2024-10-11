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
    public static class Octahedron
    {
        // Verticies
        private static Point3D _a = new Point3D(0, 0, 2); //0
        private static Point3D _b = new Point3D(0, -2, 0); //1
        private static Point3D _c = new Point3D(2, 0, 0); //2
        private static Point3D _d = new Point3D(0, 2, 0); //3

        private static Point3D _e = new Point3D(-2, 0, 0); //4
        private static Point3D _f = new Point3D(0, 0, -2); //5

        //Faces:
        // (_a, _b, _c) A
        private static Vector3D _normalA = Vector3D.Cross((_b - _a).Direction, (_c - _b).Direction);
        // (_a, _c, _d) B
        private static Vector3D _normalB = Vector3D.Cross((_c - _a).Direction, (_d - _c).Direction);
        // (_a, _d, _e) C
        private static Vector3D _normalC = Vector3D.Cross((_d - _a).Direction, (_e - _d).Direction);
        // (_a, _e, _b) D
        private static Vector3D _normalD = Vector3D.Cross((_e - _a).Direction, (_b - _e).Direction);
        // (_f, _b, _c) E
        private static Vector3D _normalE = Vector3D.Cross((_b - _f).Direction, (_b - _c).Direction);
        // (_f, _c, _d) F
        private static Vector3D _normalF = Vector3D.Cross((_c - _f).Direction, (_c - _d).Direction);
        // (_f, _d, _e) G
        private static Vector3D _normalG = Vector3D.Cross((_d - _f).Direction, (_d - _e).Direction);
        // (_f, _e, _b) H
        private static Vector3D _normalH = Vector3D.Cross((_e - _f).Direction, (_e - _b).Direction);

        public static IWireFrameMesh Build(int divisions)
        {
            var mesh = WireFrameMesh.Create();
            mesh.AddGrid(Polyhedron.BuildFace(_a, _b, _c, _normalA, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _c, _d, _normalB, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _d, _e, _normalC, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _e, _b, _normalD, divisions));

            mesh.AddGrid(Polyhedron.BuildFace(_f, _b, _c, _normalE, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_f, _c, _d, _normalF, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_f, _d, _e, _normalG, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_f, _e, _b, _normalH, divisions));

            return mesh;
        }
    }
}
