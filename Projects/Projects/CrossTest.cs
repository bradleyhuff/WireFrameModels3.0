using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using FundamentalMeshes;
using Operations;
using Operations.SetOperators;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class CrossTest : ProjectBase
    {
        protected override void RunProject()
        {
            var part1 = Cuboid.Create(0.5, 1, 5, 10, 5, 10);
            //var part2 = part1.Clone(Transform.Rotation(Vector3D.BasisY, Math.PI / 2));
            var part2 = Cuboid.Create(0.5, 1, 4, 8, 4, 8);
            part2.Apply(Transform.Rotation(Vector3D.BasisY, Math.PI / 2));
            part2.Apply(Transform.Translation(new Vector3D(-1.50000, 0.6, 2.5)));
            //part2.Apply(Transform.Translation(new Vector3D(-1.50000, 0.1, 2.5)));

            //var part3 = part1.Clone(Transform.Rotation(Vector3D.BasisY, -Math.PI / 3));
            //part3.Apply(Transform.Translation(new Vector3D(1.10, -0.1, 2.55)));

            var part4 = Cuboid.Create(10, 1, 10, 1, 10, 1);
            part4.Apply(Transform.Translation(new Vector3D(-4.4, 3.50, -2.5)));

            var cross = part1.Union(part2);//.Union(part3);
            //var cross = WireFrameMesh.Create();
            //cross.AddGrid(part1);
            //cross.AddGrid(part2);
            //IntermeshOperation.Run(cross);
            //int count = cross.RemoveAllTriangles(cross.Triangles.Where(t => t.Id == 1176));
            //Console.WriteLine($"Triangles removed {count}");
            cross = cross.Difference(part4);
            //cross.AddGrid(part3);
            //var sum = WireFrameMesh.Create();
            //sum.AddGrid(part1);
            //sum.AddGrid(part2);

            WavefrontFile.Export(cross, "Wavefront/CrossTest");
            //{
            //    var test = WireFrameMesh.Create();
            //    var errorPoint = new Point3D(0.5, 3.5, 2.2);
            //    test.AddTriangle(errorPoint, errorPoint, errorPoint);
            //    WavefrontFile.Export(test, "Wavefront/CrossTestErrorPoint");
            //}
            //{
            //    var test = WireFrameMesh.Create();
            //    var checkPoint = new Point3D(0.5, 3.5, 2.4000000000000004);
            //    test.AddTriangle(checkPoint, checkPoint, checkPoint);

            //    var checkPoint2 = new Point3D(0.5, 3.5, 0);
            //    test.AddTriangle(checkPoint2, checkPoint2, checkPoint2);

            //    var checkPoint3 = new Point3D(0.5, 3.5, 2);
            //    test.AddTriangle(checkPoint3, checkPoint3, checkPoint3);

            //    WavefrontFile.Export(test, "Wavefront/CrossTestCheckPoints");
            //}
            {
                var test = WireFrameMesh.Create();
                var testSegment = new LineSegment3D(new Point3D(-1, 3.5, 2), new Point3D(0, 3.5, 2.5));
                Console.WriteLine($"Test segment {testSegment}");
                test.AddTriangle(testSegment.Start, testSegment.Center, testSegment.End);
                WavefrontFile.Export(test, "Wavefront/CrossTest_TestSegment");
            }
            //{
            //    var test = WireFrameMesh.Create();
            //    var collinear = new LineSegment3D(new Point3D(0.5, 3.5, 0), new Point3D(0.5, 3.5, 0.5));
            //    test.AddTriangle(collinear.Start, collinear.Center, collinear.End);

            //    var collinear2 = new LineSegment3D(new Point3D(0.5, 3.5, 0.5), new Point3D(0.5, 3.5, 1));
            //    test.AddTriangle(collinear2.Start, collinear2.Center, collinear2.End);

            //    var collinear3 = new LineSegment3D(new Point3D(0.5, 3.5, 1), new Point3D(0.5, 3.5, 1.5));
            //    test.AddTriangle(collinear3.Start, collinear3.Center, collinear3.End);

            //    var collinear4 = new LineSegment3D(new Point3D(0.5, 3.5, 1.5), new Point3D(0.5, 3.5, 2));
            //    test.AddTriangle(collinear4.Start, collinear4.Center, collinear4.End);

            //    WavefrontFile.Export(test, "Wavefront/CrossTest_Collinears");
            //}
            //{
            //    var test = WireFrameMesh.Create();
            //    var collinear = new LineSegment3D(new Point3D(0.5999999999999999, 3.5, 2.5), new Point3D(0, 3.5, 1.9000000000000001));
            //    test.AddTriangle(collinear.Start, collinear.Center, collinear.End);

            //    var collinear2 = new LineSegment3D(new Point3D(0, 3.5, 0), new Point3D(0.5, 3.5, 0));
            //    test.AddTriangle(collinear2.Start, collinear2.Center, collinear2.End);

            //    var collinear3 = new LineSegment3D(new Point3D(0.5, 3.5, 2), new Point3D(0.8999999999999999, 3.5, 2));
            //    test.AddTriangle(collinear3.Start, collinear3.Center, collinear3.End);

            //    WavefrontFile.Export(test, "Wavefront/CrossTest_Links");
            //}
        }
    }
}
