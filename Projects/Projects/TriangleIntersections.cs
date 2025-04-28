using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
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
            var pointA = new Point3D(1, 2, 3);
            var pointB = new Point3D(3, 2, 3);
            var pointC = new Point3D(2, 2.01, 3);
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

            var triangle = new Triangle3D(pointA, pointB, pointC);

            var image = triangle.MinimumHeightScaleImage(0.5 / triangle.AspectRatio);

            var grid = WireFrameMesh.Create();
            grid.AddTriangle(triangle, "", 0);
            grid.AddTriangle(new Triangle3D(triangle.Center, triangle.A, triangle.B), "", 0);
            WavefrontFile.Export(grid, "Wavefront/OriginalTriangle");

            grid = WireFrameMesh.Create();
            grid.AddTriangle(image, "", 0);
            grid.AddTriangle(new Triangle3D(image.Center, image.A, image.B), "", 0);
            WavefrontFile.Export(grid, "Wavefront/ImageTriangle");
        }
    }
}
