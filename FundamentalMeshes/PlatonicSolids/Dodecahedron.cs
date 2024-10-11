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
    public static class Dodecahedron
    {
        private static double _phi = (1 + Math.Sqrt(5)) / 2;//Golden ratio
        private static double I_phi = 1 / _phi;

        // Verticies
        private static Point3D _a = new Point3D(-1, 1, 1);
        private static Point3D _b = new Point3D(0, I_phi, _phi);
        private static Point3D _c = new Point3D(0, -I_phi, _phi);
        private static Point3D _d = new Point3D(-1, -1, 1);

        private static Point3D _e = new Point3D(-_phi, 0.0, I_phi);
        private static Point3D _f = new Point3D(1.0, 1.0, 1.0);
        private static Point3D _g = new Point3D(_phi, 0.0, I_phi);
        private static Point3D _h = new Point3D(1.0, -1.0, 1.0);

        private static Point3D _i = new Point3D(I_phi, _phi, 0.0);
        private static Point3D _j = new Point3D(-I_phi, _phi, 0.0);
        private static Point3D _k = new Point3D(-1.0, 1.0, -1.0);
        private static Point3D _l = new Point3D(-_phi, 0.0, -I_phi);

        private static Point3D _m = new Point3D(-1.0, -1.0, -1.0);
        private static Point3D _n = new Point3D(-I_phi, -_phi, 0.0);
        private static Point3D _o = new Point3D(I_phi, -_phi, 0.0);
        private static Point3D _p = new Point3D(1.0, 1.0, -1.0);

        private static Point3D _q = new Point3D(0.0, I_phi, -_phi);
        private static Point3D _r = new Point3D(_phi, 0.0, -I_phi);
        private static Point3D _s = new Point3D(0.0, -I_phi, -_phi);
        private static Point3D _t = new Point3D(1.0, -1.0, -1.0);

        //Faces:
        // (_a, _b, _c, _d, _e) A
        private static Point3D _centerA = (_a + _b + _c + _d + _e) / 5;
        private static Vector3D _normalA = Vector3D.Cross((_c - _b).Direction, (_c - _d).Direction);

        // (_e, _d, _n, _m, _l) B
        private static Point3D _centerB = (_e + _d + _n + _m + _l) / 5;
        private static Vector3D _normalB = Vector3D.Cross((_n - _d).Direction, (_n - _m).Direction);
        // (_d, _c, _h, _o, _n) C
        private static Point3D _centerC = (_d + _c + _h + _o + _n) / 5;
        private static Vector3D _normalC = Vector3D.Cross((_h - _c).Direction, (_h - _o).Direction);
        // (_c, _b, _f, _g, _h) D
        private static Point3D _centerD = (_c + _b + _f + _g + _h) / 5;
        private static Vector3D _normalD = Vector3D.Cross((_f - _b).Direction, (_f - _g).Direction);
        // (_b, _a, _j, _i, _f) E
        private static Point3D _centerE = (_b + _a + _j + _i + _f) / 5;
        private static Vector3D _normalE = Vector3D.Cross((_j - _a).Direction, (_j - _i).Direction);
        // (_a, _e, _l, _k, _j) F
        private static Point3D _centerF = (_a + _e + _l + _k + _j) / 5;
        private static Vector3D _normalF = Vector3D.Cross((_l - _e).Direction, (_l - _k).Direction);

        // (_q, _s, _m, _l, _k) G
        private static Point3D _centerG = (_q + _s + _m + _l + _k) / 5;
        private static Vector3D _normalG = Vector3D.Cross((_s - _m).Direction, (_m - _l).Direction);
        // (_s, _t, _o, _n, _m) H
        private static Point3D _centerH = (_s + _t + _o + _n + _m) / 5;
        private static Vector3D _normalH = Vector3D.Cross((_t - _o).Direction, (_o - _n).Direction);
        // (_t, _r, _g, _h, _o) I
        private static Point3D _centerI = (_t + _r + _g + _h + _o) / 5;
        private static Vector3D _normalI = Vector3D.Cross((_r - _g).Direction, (_g - _h).Direction);
        // (_r, _p, _i, _f, _g) J
        private static Point3D _centerJ = (_r + _p + _i + _f + _g) / 5;
        private static Vector3D _normalJ = Vector3D.Cross((_p - _i).Direction, (_i - _f).Direction);
        // (_p, _q, _k, _j, _i) K
        private static Point3D _centerK = (_p + _q + _k + _j + _i) / 5;
        private static Vector3D _normalK = Vector3D.Cross((_q - _k).Direction, (_k - _j).Direction);

        // (_p, _r, _t, _s, _q) L
        private static Point3D _centerL = (_p + _r + _t + _s + _q) / 5;
        private static Vector3D _normalL = Vector3D.Cross((_r - _t).Direction, (_t - _s).Direction);

        public static IWireFrameMesh Build(int divisions)
        {
            var mesh = WireFrameMesh.Create();

            // (_a, _b, _c, _d, _e) A
            var faceA = Polyhedron.BuildFace(new Point3D[] { _a, _b, _c, _d, _e }, _centerA, _normalA, divisions);
            // (_e, _d, _n, _m, _l) B
            var faceB = Polyhedron.BuildFace(new Point3D[] { _e, _d, _n, _m, _l }, _centerB, _normalB, divisions);
            // (_d, _c, _h, _o, _n) C
            var faceC = Polyhedron.BuildFace(new Point3D[] { _d, _c, _h, _o, _n }, _centerC, _normalC, divisions);
            // (_c, _b, _f, _g, _h) D
            var faceD = Polyhedron.BuildFace(new Point3D[] { _c, _b, _f, _g, _h }, _centerD, _normalD, divisions);
            // (_b, _a, _j, _i, _f) E
            var faceE = Polyhedron.BuildFace(new Point3D[] { _b, _a, _j, _i, _f }, _centerE, _normalE, divisions);
            // (_a, _e, _l, _k, _j) F
            var faceF = Polyhedron.BuildFace(new Point3D[] { _a, _e, _l, _k, _j }, _centerF, _normalF, divisions);

            // (_q, _s, _m, _l, _k) G
            var faceG = Polyhedron.BuildFace(new Point3D[] { _q, _s, _m, _l, _k }, _centerG, _normalG, divisions);
            // (_s, _t, _o, _n, _m) H
            var faceH = Polyhedron.BuildFace(new Point3D[] { _s, _t, _o, _n, _m }, _centerH, _normalH, divisions);
            // (_t, _r, _g, _h, _o) I
            var faceI = Polyhedron.BuildFace(new Point3D[] { _t, _r, _g, _h, _o }, _centerI, _normalI, divisions);
            // (_r, _p, _i, _f, _g) J
            var faceJ = Polyhedron.BuildFace(new Point3D[] { _r, _p, _i, _f, _g }, _centerJ, _normalJ, divisions);
            // (_p, _q, _k, _j, _i) K
            var faceK = Polyhedron.BuildFace(new Point3D[] { _p, _q, _k, _j, _i }, _centerK, _normalK, divisions);
            // (_p, _r, _t, _s, _q) L
            var faceL = Polyhedron.BuildFace(new Point3D[] { _p, _r, _t, _s, _q }, _centerL, _normalL, divisions);

            mesh.AddGrid(faceA);
            mesh.AddGrid(faceB);
            mesh.AddGrid(faceC);
            mesh.AddGrid(faceD);
            mesh.AddGrid(faceE);
            mesh.AddGrid(faceF);

            mesh.AddGrid(faceG);
            mesh.AddGrid(faceH);
            mesh.AddGrid(faceI);
            mesh.AddGrid(faceJ);
            mesh.AddGrid(faceK);
            mesh.AddGrid(faceL);

            return mesh;
        }
    }
}
