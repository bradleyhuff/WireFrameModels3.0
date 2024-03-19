using BaseObjects.Transformations;
using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    public class WireMeshTest : ProjectBase
    {
        protected override void RunProject()
        {
            var wireMesh = WireFrameMesh.CreateMesh();

            wireMesh.AddPoint(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(1, 0, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(2, 0, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(3, 0, 0), new Vector3D(0, 0, 1));
            wireMesh.EndRow();
            wireMesh.AddPoint(new Point3D(0, 1, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(1, 1, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(2, 1, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(3, 1, 0), new Vector3D(0, 0, 1));
            wireMesh.EndRow();
            wireMesh.AddPoint(new Point3D(0, 2, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(1, 2, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(2, 2, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(3, 2, 0), new Vector3D(0, 0, 1));
            wireMesh.EndRow();
            wireMesh.AddPoint(new Point3D(0, 3, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(1, 3, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(2, 3, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(3, 3, 0), new Vector3D(0, 0, 1));
            wireMesh.EndRow();
            wireMesh.AddPoint(new Point3D(0, 4, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(1, 4, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(2, 4, 0), new Vector3D(0, 0, 1));
            wireMesh.AddPoint(new Point3D(3, 4, 0), new Vector3D(0, 0, 1));
            wireMesh.EndRow();
            wireMesh.EndGrid();

            wireMesh.AddPoint(new Point3D(0, 0, 0), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 1, 0), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 2, 0), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 3, 0), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 4, 0), new Vector3D(-1, 0, 0));
            wireMesh.EndRow();
            wireMesh.AddPoint(new Point3D(0, 0, 1), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 1, 1), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 2, 1), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 3, 1), new Vector3D(-1, 0, 0));
            wireMesh.AddPoint(new Point3D(0, 4, 1), new Vector3D(-1, 0, 0));
            wireMesh.EndRow();
            wireMesh.EndGrid();

            var triangles = wireMesh.Triangles;
            var positions = wireMesh.Positions;

            Console.WriteLine($"Triangles {triangles.Count} Positions {positions.Count}");
            TableDisplays.ShowCountSpread("Position normal triangle counts", positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", positions, p => p.PositionNormals.Count);

            var clone = wireMesh.Clone();
            Console.WriteLine($"Clone Triangles {clone.Triangles.Count} Positions {clone.Positions.Count}");
            TableDisplays.ShowCountSpread("Position normal triangle counts", clone.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", clone.Positions, p => p.PositionNormals.Count);

            WavefrontFile.Export(wireMesh, "Wavefront/WireMeshTest");
            PntFile.Export(wireMesh, "PositionNormalTriangle/WireMeshTest");

            var import = PntFile.Import(() => WireFrameMesh.CreateMesh(), "PositionNormalTriangle/WireMeshTest");
            WavefrontFile.Export(import, "PositionNormalTriangle/WireMeshImportExportTest");
            TableDisplays.ShowCountSpread("Position normal triangle counts", import.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", import.Positions, p => p.PositionNormals.Count);

            var wireMesh2 = WireFrameMesh.CreateMesh();
            wireMesh2.AddPoint(new Point3D(0, 0, 0), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(1, 0, 0), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(2, 0, 0), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(3, 0, 0), new Vector3D(0, -1, 0));
            wireMesh2.EndRow();
            wireMesh2.AddPoint(new Point3D(0, 0, 1), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(1, 0, 1), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(2, 0, 1), new Vector3D(0, -1, 0));
            wireMesh2.AddPoint(new Point3D(3, 0, 1), new Vector3D(0, -1, 0));
            wireMesh2.EndRow();
            wireMesh2.EndGrid();
            WavefrontFile.Export(wireMesh2, "PositionNormalTriangle/WireMeshTest2");

            wireMesh.AddGrid(wireMesh2);
            //var reflection = Transform.ShearXY(1, 1).Reflect(new Vector3D(1, 1, 1)).AtPoint(new Point3D(0.1, 0.1, 0.1));
            //wireMesh.Transformation(p => reflection.Apply(p));
            //var rotation = Transform.Rotation(new Vector3D(1, 1, 1), 0.1);
            wireMesh.Apply(Transform.Rotation(new Vector3D(1, 1, 1), 0.1));
            Console.WriteLine($"Triangles {wireMesh.Triangles.Count} Positions {wireMesh.Positions.Count}");
            TableDisplays.ShowCountSpread("Position normal triangle counts", wireMesh.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", wireMesh.Positions, p => p.PositionNormals.Count);
            PntFile.Export(wireMesh, "PositionNormalTriangle/WireMeshTest3");
            WavefrontFile.Export(wireMesh, "PositionNormalTriangle/WireMeshImportExportTest3");
        }
    }
}
