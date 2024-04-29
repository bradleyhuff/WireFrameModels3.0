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
            var a = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            var b = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0));
            b = b.Transform(Transform.Translation(new Vector3D(0, 0.25, 0)));

            var intersections = Triangle3D.LineSegmentIntersections(b, a).ToArray();
            Console.WriteLine($"Intersections {string.Join(",", intersections.Select(i => i))}");

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
