using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Extensions;
using Operations.ParallelSurfaces.Internals;
using System.Net.Http.Headers;
using System.Xml.Linq;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    public class TriangleIntersections : ProjectBase
    {
        protected override void RunProject()
        {
            //var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var transform = Transform.Reflection(Vector3D.BasisY) * Transform.Rotation(Vector3D.BasisZ, -0.75) * Transform.Translation(new Vector3D(-0.1, -0.650, 0))/** Transform.Translation(new Vector3D(0.60, -1.4, 0))*/;
            //var triangleB = transform.Apply(triangle);

            var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            var transform = Transform.Reflection(Vector3D.BasisY) * Transform.Rotation(Vector3D.BasisZ, -0.25) * Transform.Translation(new Vector3D(0.60, -1.0, 0));
            var triangleB = transform.Apply(triangle);

            //var triangle = new Triangle3D(new Point3D(0, -1, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var transform = Transform.Reflection(Vector3D.BasisX) * Transform.Translation(new Vector3D(-0.5, 0, 0));
            //var triangleB = transform.Apply(triangle);

            //var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0.1, 0));
            //var triangleB = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, -0.1, 0));

            //var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
            //var triangleB = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));

            var fillA = new FillTriangle(triangle.A, triangle.Normal, triangle.B, triangle.Normal, triangle.C, triangle.Normal, "", 0);
            var fillB = new FillTriangle(triangleB.A, triangleB.Normal, triangleB.B, triangleB.Normal, triangleB.C, triangleB.Normal, "", 0);


            var intersections = Triangle3D.LineSegmentIntersections(triangle, triangleB);
            var differences = triangle.Edges.SelectMany(e => e.Difference(intersections));


            FillTriangle[] splitsA = fillA.CoplanarDivideFrom(fillB).ToArray();
            FillTriangle[] splitsB = fillB.CoplanarDivideFrom(fillA).ToArray();

            //for (int i = 0; i < 1000000; i++)
            //{
            //    splitsA = fillA.CoplanarDivideFrom(fillB).ToArray();
            //    splitsB = fillB.CoplanarDivideFrom(fillA).ToArray();
            //}


            //var endPoints = intersections.Concat(differences).SelectMany(s => s.Points);

            //var containers = new List<PointNode>();
            //var discretize = new Discretize<Point3D, PointNode>(p => new Rectangle3D(p, BoxBucket.MARGINS), (s, n) => n.Point == s, p => new PointNode(p));

            //foreach (var point in endPoints)
            //{
            //    containers.Add(discretize.Fetch(point));
            //}



            var grid = WireFrameMesh.Create();
            grid.AddRangeTriangles([triangle, triangleB], "", 0);
            WavefrontFile.Export(grid, "Wavefront/Triangles");

            grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(intersections.Select(i => new Triangle3D(i.Start, i.Center, i.End)), "", 0);
            WavefrontFile.Export(grid, "Wavefront/TriangleIntersections");

            grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(differences.Select(i => new Triangle3D(i.Start, i.Center, i.End)), "", 0);
            WavefrontFile.Export(grid, "Wavefront/TriangleDifferences");

            grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(splitsA.Select(i => new Triangle3D(i.Triangle.A, i.Triangle.B, i.Triangle.C)), "", 0);
            WavefrontFile.Export(grid, "Wavefront/TriangleSplitsA");

            grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(splitsB.Select(i => new Triangle3D(i.Triangle.A, i.Triangle.B, i.Triangle.C)), "", 0);
            WavefrontFile.Export(grid, "Wavefront/TriangleSplitsB");

            //var pointA = new Point3D(1, 2, 3);
            //var pointB = new Point3D(3, 2, 3);
            //var pointC = new Point3D(2, 2.01, 3);
            //var plane = new BasisPlane(pointA, pointB, pointC);

            ////
            //var surfaceA = plane.MapToSurfaceCoordinates(pointA);
            //var surfaceB = plane.MapToSurfaceCoordinates(pointB);
            //var surfaceC = plane.MapToSurfaceCoordinates(pointC);

            ////transform
            //surfaceA = new Point2D(surfaceA.X, surfaceA.Y * 0.5);
            //surfaceB = new Point2D(surfaceB.X, surfaceB.Y * 0.5);
            //surfaceC = new Point2D(surfaceC.X, surfaceC.Y * 0.5);

            //var imageA = plane.MapToSpaceCoordinates(surfaceA);
            //var imageB = plane.MapToSpaceCoordinates(surfaceB);
            //var imageC = plane.MapToSpaceCoordinates(surfaceC);

            //var imageTriangle = new Triangle3D(imageA, imageB, imageC);

            //var triangle = new Triangle3D(pointA, pointB, pointC);

            //var image = triangle.MinimumHeightScaleImage(0.5 / triangle.AspectRatio);

            //var grid = WireFrameMesh.Create();
            //grid.AddTriangle(triangle, "", 0);
            //grid.AddTriangle(new Triangle3D(triangle.Center, triangle.A, triangle.B), "", 0);
            //WavefrontFile.Export(grid, "Wavefront/OriginalTriangle");

            //grid = WireFrameMesh.Create();
            //grid.AddTriangle(image, "", 0);
            //grid.AddTriangle(new Triangle3D(image.Center, image.A, image.B), "", 0);
            //WavefrontFile.Export(grid, "Wavefront/ImageTriangle");
        }
    }
}
