using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
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
            //            Triangle
            //{ Triangle A: [X: 0.113999950709140 Y: 0.434181599758615 Z: 0.405515357986652 ] B: [X: 0.114443516250287 Y: 0.434066015928310 Z: 0.405515244270833 ] C: [X: 0.114528705752820 Y: 0.434043818779481 Z: 0.405515223469387 ]}

            //            Gathering
            //{ Triangle A: [X: 0.113999950709140 Y: 0.434181599758615 Z: 0.405515357986652 ] B: [X: 0.114440816380691 Y: 0.434066719367738 Z: 0.405515244898318 ] C: [X: 0.114443516250287 Y: 0.434066015928310 Z: 0.405515244270833 ]}

            /*
             Triangle A: [ X: 0.984170907604961 Y: 1.000000000000000 Z: 0.644805738059577 ] B: [ X: 0.985152556741039 Y: 1.000000000000000 Z: 0.604817785311423 ] C: [ X: 0.995000000000000 Y: 1.000000000000000 Z: 0.604938637688187 ] 
            Triangle A: [ X: 0.984048201460000 Y: 1.000000000000000 Z: 0.649804232153000 ] B: [ X: 0.985275262886000 Y: 1.000000000000000 Z: 0.599819291218000 ] C: [ X: 0.970559395404000 Y: 1.000000000000000 Z: 0.599277273723000 ]
             */

            //var triangle = new Triangle3D(new Point3D(0.113999950709140, 0.434181599758615, 0.405515357986652), new Point3D(0.114443516250287, 0.434066015928310, 0.405515244270833), new Point3D(0.114528705752820, 0.434043818779481, 0.405515223469387));
            //var gathering = new Triangle3D(new Point3D(0.113999950709140, 0.434181599758615, 0.405515357986652), new Point3D(0.114440816380691, 0.434066719367738, 0.405515244898318), new Point3D(0.114443516250287, 0.434066015928310, 0.405515244270833));

            var triangle = new Triangle3D(new Point3D(0.984170907604961, 1.000000000000000, 0.644805738059577), new Point3D(0.985152556741039, 1.000000000000000, 0.604817785311423), new Point3D(0.995000000000000, 1.000000000000000, 0.604938637688187));
            var gathering = new Triangle3D(new Point3D(0.984048201460000, 1.000000000000000, 0.649804232153000), new Point3D(0.985275262886000, 1.000000000000000, 0.599819291218000), new Point3D(0.970559395404000, 1.000000000000000, 0.599277273723000));

            var intersections = Triangle3D.LineSegmentIntersections(triangle, gathering).ToArray();
            //var intersections = Triangle3D.CoplanarIntersections(triangle, gathering).ToArray();
            Console.WriteLine($"Intersections \n{string.Join("\n", intersections.Select(i => i))}");

            var grid = WireFrameMesh.Create();
            grid.AddTriangle(triangle, "", 0);
            grid.AddTriangle(gathering, "", 0);
            WavefrontFile.Export(grid, "Wavefront/ProblemTriangles");

            foreach (var i in intersections.Select((ii, i) => new { intersection = ii, index = i})) {
                grid = WireFrameMesh.Create();
                grid.AddTriangle(i.intersection.Start, i.intersection.Center, i.intersection.End, "", 0);
                WavefrontFile.Export(grid, $"Wavefront/Intersections-{i.index}");
            }

            //var points = triangle.Vertices.ToArray();
            //var points2 = gathering.Vertices.ToArray();

            //var grid = WireFrameMesh.Create();
            //var point1 = triangle.MinimumHeightScale(points[0], 0.25 / triangle.AspectRatio);
            //var point2 = triangle.MinimumHeightScale(points[1], 0.25 / triangle.AspectRatio);
            //var point3 = triangle.MinimumHeightScale(points[2], 0.25 / triangle.AspectRatio);
            //var point1B = triangle.MinimumHeightScale(points2[0], 0.25 / triangle.AspectRatio);
            //var point2B = triangle.MinimumHeightScale(points2[1], 0.25 / triangle.AspectRatio);
            //var point3B = triangle.MinimumHeightScale(points2[2], 0.25 / triangle.AspectRatio);

            //grid.AddTriangle(point1, point2, point3, "", 0);
            ////grid.AddTriangle(point1B, point2B, point3B, "", 0);

            //grid.Apply(Transform.Translation(-new Vector3D(point1.X, point2.Y, point3.Z)));
            //grid.Apply(Transform.Scale(1000));

            //WavefrontFile.Export(grid, "Wavefront/ProblemTrianglesA");

            //grid = WireFrameMesh.Create();
            //grid.AddTriangle(point1B, point2B, point3B, "", 0);
            //grid.Apply(Transform.Translation(-new Vector3D(point1.X, point2.Y, point3.Z)));
            //grid.Apply(Transform.Scale(1000));

            //WavefrontFile.Export(grid, "Wavefront/ProblemTrianglesB");

            //grid = WireFrameMesh.Create();
            //foreach (var intersection in intersections)
            //{
            //    var start = triangle.MinimumHeightScale(intersection.Start, 0.25 / triangle.AspectRatio);
            //    var center = triangle.MinimumHeightScale(intersection.Center, 0.25 / triangle.AspectRatio);
            //    var end = triangle.MinimumHeightScale(intersection.End, 0.25 / triangle.AspectRatio);
            //    grid.AddTriangle(start, center, end, "", 0);
            //}
            //grid.Apply(Transform.Translation(-new Vector3D(point1.X, point2.Y, point3.Z)));
            //grid.Apply(Transform.Scale(1000));

            //WavefrontFile.Export(grid, "Wavefront/Intersections");
        }
    }
}
