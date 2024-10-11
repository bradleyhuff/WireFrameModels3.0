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
    public static class Icosahedron
    {
        private static double _phi = (1 + Math.Sqrt(5)) / 2;//Golden ratio

        // Verticies
        private static Point3D _a = new Point3D(_phi, 1, 0); //0
        private static Point3D _b = new Point3D(-_phi, 1, 0);//1
        private static Point3D _c = new Point3D(_phi, -1, 0);//2
        private static Point3D _d = new Point3D(-_phi, -1, 0);//3

        private static Point3D _e = new Point3D(1, 0, _phi);//4
        private static Point3D _f = new Point3D(1, 0, -_phi);//5
        private static Point3D _g = new Point3D(-1, 0, _phi);//6
        private static Point3D _h = new Point3D(-1, 0, -_phi);//7

        private static Point3D _i = new Point3D(0, _phi, 1);//8
        private static Point3D _j = new Point3D(0, -_phi, 1);//9
        private static Point3D _k = new Point3D(0, _phi, -1);//10
        private static Point3D _l = new Point3D(0, -_phi, -1);//11

        //Faces:
        // (_a, _i, _e) A
        private static Vector3D _normalA = Vector3D.Cross((_i - _a).Direction, (_e - _i).Direction);

        // (_a, _f, _k) B
        private static Vector3D _normalB = Vector3D.Cross((_f - _a).Direction, (_k - _f).Direction);

        // (_c, _e, _j) C
        private static Vector3D _normalC = Vector3D.Cross((_e - _c).Direction, (_j - _e).Direction);

        // (_c, _l, _f) D
        private static Vector3D _normalD = Vector3D.Cross((_l - _c).Direction, (_f - _l).Direction);

        // (_b, _g, _i) E
        private static Vector3D _normalE = Vector3D.Cross((_g - _b).Direction, (_i - _g).Direction);

        // (_b, _k, _h) F
        private static Vector3D _normalF = Vector3D.Cross((_k - _b).Direction, (_h - _k).Direction);

        // (_d, _j, _g) G
        private static Vector3D _normalG = Vector3D.Cross((_j - _d).Direction, (_g - _j).Direction);

        // (_d, _h, _l) H
        private static Vector3D _normalH = Vector3D.Cross((_h - _d).Direction, (_l - _h).Direction);

        // (_a, _k, _i) I
        private static Vector3D _normalI = Vector3D.Cross((_k - _a).Direction, (_i - _k).Direction);

        // (_b, _i, _k) J
        private static Vector3D _normalJ = Vector3D.Cross((_i - _b).Direction, (_k - _i).Direction);

        // (_c, _j, _l) K
        private static Vector3D _normalK = Vector3D.Cross((_j - _c).Direction, (_l - _j).Direction);

        // (_l, _j, _d) L
        private static Vector3D _normalL = Vector3D.Cross((_j - _l).Direction, (_d - _j).Direction);

        // (_e, _c, _a) M
        private static Vector3D _normalM = Vector3D.Cross((_c - _e).Direction, (_a - _c).Direction);

        // (_f, _a, _c) N
        private static Vector3D _normalN = Vector3D.Cross((_a - _f).Direction, (_c - _a).Direction);

        // (_g, _b, _d) O
        private static Vector3D _normalO = Vector3D.Cross((_b - _g).Direction, (_d - _b).Direction);

        // (_h, _d, _b) P
        private static Vector3D _normalP = Vector3D.Cross((_d - _h).Direction, (_b - _d).Direction);

        // (_i, _g, _e) Q
        private static Vector3D _normalQ = Vector3D.Cross((_g - _i).Direction, (_e - _g).Direction);

        // (_j, _e, _g) R
        private static Vector3D _normalR = Vector3D.Cross((_e - _j).Direction, (_g - _e).Direction);

        // (_k, _f, _h) S
        private static Vector3D _normalS = Vector3D.Cross((_f - _k).Direction, (_h - _f).Direction);

        // (_l, _h, _f) T
        private static Vector3D _normalT = Vector3D.Cross((_h - _l).Direction, (_f - _h).Direction);

        public static IWireFrameMesh Build(int divisions)
        {
            var mesh = WireFrameMesh.Create();
            mesh.AddGrid(Polyhedron.BuildFace(_a, _i, _e, _normalA, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _f, _k, _normalB, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_c, _e, _j, _normalC, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_c, _l, _f, _normalD, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_b, _g, _i, _normalE, divisions));

            mesh.AddGrid(Polyhedron.BuildFace(_b, _k, _h, _normalF, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_d, _j, _g, _normalG, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_d, _h, _l, _normalH, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_a, _k, _i, _normalI, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_b, _i, _k, _normalJ, divisions));

            mesh.AddGrid(Polyhedron.BuildFace(_c, _j, _l, _normalK, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_l, _j, _d, _normalL, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_e, _c, _a, _normalM, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_f, _a, _c, _normalN, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_g, _b, _d, _normalO, divisions));

            mesh.AddGrid(Polyhedron.BuildFace(_h, _d, _b, _normalP, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_i, _g, _e, _normalQ, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_j, _e, _g, _normalR, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_k, _f, _h, _normalS, divisions));
            mesh.AddGrid(Polyhedron.BuildFace(_l, _h, _f, _normalT, divisions));

            return mesh;
        }
    }
}
