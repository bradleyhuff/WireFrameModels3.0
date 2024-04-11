using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class TriangleIntersections : ProjectBase
    {
        protected override void RunProject()
        {
            var a = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            var b = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0));
            b = b.Translate(new Vector3D(0.1, 0, 7e-13));
            //b = b.Transform(Transform.Scale(0.5) * Transform.Rotation(new Vector3D(1, 1, 1), 0.3));

            var intersections = Triangle3D.SegmentIntersections(a, b);
            Console.WriteLine($"Intersections {string.Join(",", intersections)}");

            var mesh = WireFrameMesh.CreateMesh();
            mesh.AddTriangle(a);
            mesh.AddTriangle(b);

            WavefrontFile.Export(mesh, "Wavefront/TriangleIntersections");

        }
    }
}
