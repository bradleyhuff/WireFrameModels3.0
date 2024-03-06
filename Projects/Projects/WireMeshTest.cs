using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
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
        }
    }
}
