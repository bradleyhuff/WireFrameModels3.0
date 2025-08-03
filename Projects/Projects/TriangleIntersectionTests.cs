using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Console = BaseObjects.Console;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class TriangleIntersectionTests : ProjectBase
    {
        protected override void RunProject()
        {
            var masterTriangleGrid = WireFrameMesh.Create();
            var masterIntersectionGrid = WireFrameMesh.Create();

            TestSuite(masterTriangleGrid, masterIntersectionGrid, Transform.Identity());
            TestSuite(masterTriangleGrid, masterIntersectionGrid, Transform.Rotation(0, 0, 1, 0.34509));
            TestSuite(masterTriangleGrid, masterIntersectionGrid, Transform.Rotation(0, 0, 1, 0.34509) * Transform.Scale(1e-2));
            TestSuite(masterTriangleGrid, masterIntersectionGrid, Transform.Rotation(0, 0, 1, 0.34509) * Transform.Scale(1e-4));

            WavefrontFile.Export(masterTriangleGrid, "Wavefront/MasterTriangles");
            WavefrontFile.Export(masterIntersectionGrid, "Wavefront/MasterIntersections");
        }

        int i = 0;
        int j = 0;

        private void TestSuite(IWireFrameMesh mt, IWireFrameMesh mi, Transform t)
        {
            var rightTriangle = new Triangle3D(0, 0, 0, 1, 0, 0, 0, 1, 0);
            var leftTriangle = new Triangle3D(0, 0, 0, -1, 0, 0, 0, 1, 0);
            var rightTriangle2 = new Triangle3D(0, 0.1, 0, 1, 0.1, 0, 0, 0.9, 0);
            var rightTriangle3 = new Triangle3D(0, 0.4, 0, 1, 0.4, 0, 0, 0.5, 0);
            var shiftedRightTriangle = rightTriangle.Translate(0, -0.1, 0);
            var shiftedRightTriangle2 = rightTriangle.Translate(0, 0.1, 0);

            var foldedRightTriangle = new Triangle3D(0, 0, 0, 1, 0, 0, 0, 0, 1);
            var foldedRightTriangle2 = rightTriangle2.Rotate(0, 1, 0, -Math.PI / 2);

            var rightTriangle4 = new Triangle3D(0, 0, 0, 1, 0, 0, 1, 1, 0);
            var rightTriangle5 = new Triangle3D(0, 0, 0, 0.5, 0, 0, 0.5, 0.5, 0);
            var rightTriangle6 = new Triangle3D(0, 0, 0, 0.3, 0, 0, 0, 0.3, 0);

            //Coplanar Edge on Edge
            Test(mt, mi, t, 0, rightTriangle.Translate(0.1, 0, 0), leftTriangle);
            Test(mt, mi, t, 0, rightTriangle.Rotate(0, 0, 1, -0.1), leftTriangle);
            Test(mt, mi, t, 0, rightTriangle, leftTriangle);
            Test(mt, mi, t, 1, rightTriangle2, leftTriangle);
            Test(mt, mi, t, 1, rightTriangle3, leftTriangle);
            Test(mt, mi, t, 1, shiftedRightTriangle, leftTriangle);
            Test(mt, mi, t, 1, shiftedRightTriangle2, leftTriangle);

            //Edge on Triangle
            Test(mt, mi, t, 0, rightTriangle, foldedRightTriangle);
            Test(mt, mi, t, 1, rightTriangle, foldedRightTriangle.Translate(0, 0.05, 0));
            Test(mt, mi, t, 1, rightTriangle, foldedRightTriangle.Translate(0.1, 0.05, 0));
            Test(mt, mi, t, 1, rightTriangle, foldedRightTriangle2.Translate(0.1, 0.05, 0));
            Test(mt, mi, t, 1, rightTriangle, foldedRightTriangle2.Translate(0.1, -0.05, 0));
            Test(mt, mi, t, 1, rightTriangle, foldedRightTriangle2.Translate(0.1, -0.10, 0));

            //Coplanar Triangle on Triangle
            Test(mt, mi, t, 4, rightTriangle, rightTriangle.Translate(Point3D.Zero - rightTriangle.Center).Rotate(0, 0, 1, Math.PI).Translate(rightTriangle.Center - Point3D.Zero).Translate(0.4, 0, 0));
            Test(mt, mi, t, 5, rightTriangle, rightTriangle.Translate(Point3D.Zero - rightTriangle.Center).Rotate(0, 0, 1, Math.PI / 3).Translate(rightTriangle.Center - Point3D.Zero).Translate(0.3, 0.2, 0));
            Test(mt, mi, t, 3, rightTriangle, rightTriangle.Translate(Point3D.Zero - rightTriangle.Center).Rotate(0, 0, 1, Math.PI / 3).Translate(rightTriangle.Center - Point3D.Zero).Translate(0.45, 0.3, 0));
            Test(mt, mi, t, 6, rightTriangle, rightTriangle.Translate(Point3D.Zero - rightTriangle.Center).Rotate(0, 0, 1, Math.PI).Translate(rightTriangle.Center - Point3D.Zero));
            Test(mt, mi, t, 3, rightTriangle, rightTriangle.Scale(0.25).Translate(0.1, 0.1, 0));
            Test(mt, mi, t, 3, rightTriangle4, rightTriangle5);
            Test(mt, mi, t, 4, rightTriangle, rightTriangle.Rotate(0, 0, 1, Math.PI / 6));
            Test(mt, mi, t, 3, rightTriangle, rightTriangle6.Rotate(0, 0, 1, Math.PI / 6));
            Test(mt, mi, t, 3, rightTriangle, rightTriangle6.Rotate(0, 0, 1, Math.PI / 6).Translate(0, 0.1, 0));
            Test(mt, mi, t, 4, rightTriangle, rightTriangle6.Rotate(0, 0, 1, Math.PI / 6).Translate(-0.05, 0.1, 0));
            Test(mt, mi, t, 3, rightTriangle, rightTriangle6.Rotate(0, 0, 1, Math.PI / 6).Translate(0.05, 0.1, 0));

            //Edge on Edge
            Test(mt, mi, t, 0, rightTriangle.Rotate(0, 1, 0, 0.5), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle.Translate(0, 0.1, 0).Rotate(0, 1, 0, 0.5), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle3.Rotate(0, 1, 0, 0.5), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle.Translate(0, -0.1, 0).Rotate(1, 0, 0, 0.7).Translate(0, 0.1, 0), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle.Translate(0, -0.1, 0).Rotate(1, 0, 0, 0.7).Translate(-0.05, 0.1, 0), leftTriangle);

            //Triangle on Triangle
            Test(mt, mi, t, 1, rightTriangle.Translate(0, 0.1, 0).Rotate(0, 1, 0, 0.5).Translate(-0.3, 0, 0.1), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle4.Translate(0, 0.1, 0).Rotate(0, 1, 0, 0.5).Translate(-0.3, 0, 0.1), leftTriangle);
            Test(mt, mi, t, 1, rightTriangle.Rotate(0, 1, 0, 0.5).Rotate(0, 0, 1, 0.3).Rotate(1, 0, 0, 0.1), leftTriangle);

            TestNextRow();
        }

        private void Test(IWireFrameMesh masterTriangleGrid, IWireFrameMesh masterIntersectionGrid, Transform t, int expectedIntersections, Triangle3D a, Triangle3D b)
        {
            var grid = WireFrameMesh.Create();
            var intersectionGrid = WireFrameMesh.Create();

            var aT = a.Transform(t);
            var bT = b.Transform(t);

            var intersections = Triangle3D.LineSegmentIntersections(aT, bT);

            grid.AddRangeTriangles([aT, bT], "", 0);
            intersectionGrid.AddRangeTriangles(intersections.Select(i => new Triangle3D(i.Start, i.Center, i.End)), "", 0);

            masterTriangleGrid.AddToMaster(grid, i, j);
            masterIntersectionGrid.AddToMaster(intersectionGrid, i, j);

            Console.Write($"Test {i}, {j} Intersections found: {intersections.Count()} Expected: {expectedIntersections} ");
            if (intersections.Count() == expectedIntersections) { Console.WriteLine("PASSED", ConsoleColor.Green); } else { Console.WriteLine("FAILED", ConsoleColor.Red); }
            ;

            i++;
        }

        private void TestNextRow()
        {
            i = 0;
            j++;
        }
    }

    public static class Extension
    {
        public static void AddToMaster(this IWireFrameMesh masterGrid, IWireFrameMesh grid, int i, int j)
        {
            var clone = grid.Clone();
            clone.Apply(Transform.Translation(i * 2.5, -j * 2.5, 0));
            masterGrid.AddGrid(clone);
        }
    }

}
