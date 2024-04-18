using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    public class TriangleIntersections : ProjectBase
    {
        protected override void RunProject()
        {
            //var a = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var b = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0));
            //b = b.Translate(new Vector3D(0.1, 0, 8e-13));
            //b = b.Rotate(new Vector3D(1, 1, 1), 0.3);
            //b = b.Transform(Transform.Scale(0.5) * Transform.Rotation(new Vector3D(1, 1, 1), 0.3));

            //var a = new Triangle3D(new Point3D(-1.4357233674983728E-19, 0.45519067551317516, 0.5855611248639351), new Point3D(-3.0614280859714992E-15, 0.4229436946792091, 0.5744788307056741), new Point3D(-5.195736337412959E-17, 0.4242640687119284, 0.5757359312880714));
            //var b = new Triangle3D(new Point3D(0.026563750415736824, 0.4592824384142292, 0.556244565377968), new Point3D(0, 0.4588926826460842, 0.5559247010854482), new Point3D(0, 0.43775472796354853, 0.5830104868552968));

            //Triangle A: [X: 4.62557172473802E-19 Y: 0.5800951867416362 Z: 0.42841442492440973 ] B: [X: -1.7943359023263948E-17 Y: 0.5770918735429689 Z: 0.446295142667704 ] C: [X: 3.3616378585859354E-14 Y: 0.5753407655771429 Z: 0.4444558964538835 ]
            //Triangle A: [X: 0.026563750415736824 Y: 0.5562445653779678 Z: 0.45928243841422933 ] B: [X: 0 Y: 0.5559247010854481 Z: 0.4588926826460843 ] C: [X: 0 Y: 0.5830104868552966 Z: 0.43775472796354875 ]
            //var a = new Triangle3D(new Point3D(4.62557172473802E-19, 0.5800951867416362, 0.42841442492440973), new Point3D(-1.7943359023263948E-17, 0.5770918735429689, 0.446295142667704), new Point3D(3.3616378585859354E-14, 0.5753407655771429, 0.4444558964538835));
            //var b = new Triangle3D(new Point3D(0.026563750415736824, 0.5562445653779678, 0.45928243841422933), new Point3D(0, 0.5559247010854481, 0.4588926826460843), new Point3D(0, 0.5830104868552966, 0.43775472796354875));
            //b = b.Translate(-0.001 * a.Normal);

            //var a = new Triangle3D(new Point3D(-1, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var a = new Triangle3D(new Point3D(-1, 1, 0), new Point3D(0, 0, 0), new Point3D(1, 1, 0));
            //a = a.Translate(new Vector3D(0, 0, 1e-13));
            //var b = new Triangle3D(new Point3D(0, 1, 1), new Point3D(0, 0, 0), new Point3D(0, 1, -1));
            //var b = new Triangle3D(new Point3D(0, 0, 1), new Point3D(0, 1, 0), new Point3D(0, 0, -1));
            //b = b.Translate(new Vector3D(1e-13, 1e-13, 0));
            //b = b.Rotate(Vector3D.BasisZ, 0.78539816339744830961566084581988);
            //b = b.Translate(new Vector3D(0, 1e-1, 0));

            //+		Triangle	{Triangle A: [ X: 0.5050252531694168 Y: 0.4949747468305832 Z: 0 ] B: [ X: 0.4813342122515286 Y: 0.4700912683929129 Z: 0 ] C: [ X: 0.5053236320259562 Y: 0.4946763679740436 Z: -0.02430189161396661 ]}	BasicObjects.GeometricObjects.Triangle3D
            //+		Triangle	{Triangle A: [ X: 0.4946763679740437 Y: 0.4946763679740437 Z: 0.02430189161396655 ] B: [ X: 0.5186657877484714 Y: 0.4700912683929129 Z: 0 ] C: [ X: 0.5 Y: 0.4896965583660738 Z: 0 ]}	BasicObjects.GeometricObjects.Triangle3D

            var a = new Triangle3D(new Point3D(0.5050252531694168, 0.4949747468305832, 0), new Point3D(0.4813342122515286, 0.4700912683929129, 0), new Point3D(0.5053236320259562, 0.4946763679740436, -0.02430189161396661));
            var b = new Triangle3D(new Point3D(0.4946763679740437, 0.4946763679740437, 0.02430189161396655), new Point3D(0.5186657877484714, 0.4700912683929129, 0), new Point3D(0.5, 0.4896965583660738, 0));


            var intersections = Triangle3D.LineSegmentIntersections(a, b).ToArray();
            Console.WriteLine($"Intersections {string.Join(",", intersections.Select(i => i))}");
            //{
            //    var vA = Vector3D.BasisX;
            //    var vB = new Vector3D(1, 1e-15, 0);
            //    Console.WriteLine($"Cross {Vector3D.Cross(vA, vB).Magnitude}");
            //}

            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green: ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(1.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    var c = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(1, 0, 0));
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(-0.5, 0, 0), new Point3D(0.5, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(0.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(1.5, 0, 0), new Point3D(2.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i is null ? ConsoleColor.Green : ConsoleColor.Red);
            //}


            //{
            //    var sA = new LineSegment3D(new Point3D(1, 0, 0), new Point3D(0, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(-0.5, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(0.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(1, 0, 0), new Point3D(0, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(-0.5, 0, 0), new Point3D(0.5, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(0.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(-0.5, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(0.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}


            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(0.65, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(0.65, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0.7, 0, 0), new Point3D(0.9, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(0.5, 0, 0), new Point3D(1.11, 0, 0));
            //    var c = new LineSegment3D(new Point3D(0.7, 0, 0), new Point3D(0.9, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i == c ? ConsoleColor.Green : ConsoleColor.Red);
            //}
            //{
            //    var sA = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1.5, 0, 0));
            //    var sB = new LineSegment3D(new Point3D(1.5, 0, 0), new Point3D(2.5, 0, 0));
            //    var i = LineSegment3D.LineSegmentIntersection(sA, sB);
            //    Console.WriteLine($"Intersection {i}", i is null ? ConsoleColor.Green : ConsoleColor.Red);
            //}

            {
                var mesh = WireFrameMesh.CreateMesh();
                mesh.AddTriangle(a);
                mesh.AddTriangle(b);

                WavefrontFile.Export(mesh, "Wavefront/TriangleIntersections");
            }
            {
                var mesh = WireFrameMesh.CreateMesh();
                //mesh.AddTriangle(a.Center, a.Center + 0.005 * a.Normal, a.Center + 0.01 * a.Normal);
                foreach (var intersection in intersections)
                {
                    mesh.AddTriangle(intersection.Start, intersection.Center, intersection.End);
                }
                WavefrontFile.Export(mesh, "Wavefront/TriangleIntersectionSegments");
            }

        }
    }
}
