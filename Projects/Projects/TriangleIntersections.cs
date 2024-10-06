using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using System.Xml.Linq;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    public class TriangleIntersections : ProjectBase
    {
        protected override void RunProject()
        {
            //var a = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var b = a.Reflect(Vector3D.BasisX);
            ////b = b.Transform(Transform.Rotation(Vector3D.BasisZ, -0.7));
            //b = b.Transform(Transform.Translation(new Vector3D(0.00, -0.25, 0)));

            //var a = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 0, 1));
            //var b = new Triangle3D(new Point3D(0.5, 0, 0.5), new Point3D(1.5, 0, 0.5), new Point3D(1.5, 0, 1.5));
            ////node.Triangle = { Triangle A: [X: 0 Y: 0 Z: 0 ] B: [X: 1 Y: 0 Z: 0 ] C: [X: 1 Y: 0 Z: 1 ]}
            ////gathering.Triangle = { Triangle A: [X: 0.5 Y: 0 Z: 0.5 ] B: [X: 1.5 Y: 0 Z: 0.5 ] C: [X: 1.5 Y: 0 Z: 1.5 ]}

            //var intersections = Triangle3D.LineSegmentIntersections(b, a).ToArray();
            //Console.WriteLine($"Intersections\n{string.Join("\n", intersections.Select(i => i))}");

            //{
            //    var mesh = WireFrameMesh.Create();
            //    mesh.AddTriangle(a);
            //    mesh.AddTriangle(b);

            //    WavefrontFile.Export(mesh, "Wavefront/TriangleIntersections");
            //}
            //{
            //    var mesh = WireFrameMesh.Create();
            //    //mesh.AddTriangle(a.Center, a.Center + 0.005 * a.Normal, a.Center + 0.01 * a.Normal);
            //    foreach (var intersection in intersections)
            //    {
            //        mesh.AddTriangle(intersection.Start, intersection.Center, intersection.End);
            //    }
            //    WavefrontFile.Export(mesh, "Wavefront/TriangleIntersectionSegments");
            //}

            //var a = new Rectangle3D(new Point3D(0.19999899999999998, 0.079999,-1E-06), new Point3D(0.200001, 1.000001, 0.400001));
            //var b = new Rectangle3D(new Point3D(-1E-06, 0.079999, 0.19999899999999998), new Point3D(0.400001, 1.000001, 0.200001));
            //Console.WriteLine($"Overlaps {Rectangle3D.Overlaps(a, b)}");

            var plane = new BasisPlane(new Point3D(3, 3, 3), new Point3D(4, 5, 6), new Point3D(5, 2, 1));

            //
            //var space = plane.MapToSpaceCoordinates(new Point2D(-0.5, 8.4));
            var surface = plane.MapToSurfaceCoordinates(new Point3D(4, 5, 6));
            var space = plane.MapToSpaceCoordinates(surface);

            //Rectangle3D.Overlaps(input.Box, n.Box)

        }
    }
}
