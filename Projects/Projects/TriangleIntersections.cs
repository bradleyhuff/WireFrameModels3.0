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
            //var triangle = new Triangle3D(new Point3D(0.348293811586750, 0.505460785186775, 0.987859743699044), new Point3D(0.340518189192871, 0.499999999999922, 0.998223810162009), new Point3D(0.339185606088167, 0.499064132782194, 1.000000000000000));
            //var triangle2 = new Triangle3D(new Point3D(0.348293811586750, 0.505460785186775, 0.987859743699044), new Point3D(0.339185606088167, 0.499064132782194, 1.000000000000000), new Point3D(0.340518189195709, 0.500000000000000, 0.998223810335592));
            //var border1 = new Triangle3D(new Point3D(0.340518189192871, 0.499999999999922, 0.998223810162009), new Point3D(0.340518189195709, 0.500000000000000, 0.998223810335592), new Point3D(0.341795075323959, 0.499145570216585, 1.000000000000000));
            //var border2 = new Triangle3D(new Point3D(0.340518189195709, 0.500000000000000, 0.998223810335592), new Point3D(0.340518189192871, 0.499999999999922, 0.998223810162009), new Point3D(0.336119990977729, 0.503039115443091, 1.000000000000000));
            /*
+		GatheringTriangle	{Triangle A: [ X: 0.501423803223381 Y: 0.158123450101038 Z: 0.465859624462612 ] 
            B: [ X: 0.500000000000000 Y: 0.152916092505751 Z: 0.466084099946972 ] 
            C: [ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ]}	BasicObjects.GeometricObjects.Triangle3D


-+		IntersectedTriangle	{Triangle A: [ X: 0.498375974084575 Y: 0.158062354287749 Z: 0.466095109770848 ] 
            B: [ X: 0.500000000000000 Y: 0.153053511144470 Z: 0.466037844847931 ] 
            C: [ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ]}	BasicObjects.GeometricObjects.Triangle3D
+		[0]	{[[ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ], [ X: 0.500000000000000 Y: 0.153053511144470 Z: 0.466037844847931 ]] 1.08622106133307E-12}	BasicObjects.GeometricObjects.LineSegment3D


            */
            var intersector = new Triangle3D(
                new Point3D(0.498375974084575, 0.158062354287749, 0.466095109770848), 
                new Point3D(0.500000000000000, 0.153053511144470, 0.466037844847931), 
                new Point3D(0.500000000000000, 0.153053511143441, 0.466037844848277));

            var gathering = new Triangle3D(
                new Point3D(0.501423803223381, 0.158123450101038, 0.465859624462612),
                new Point3D(0.500000000000000, 0.152916092505751, 0.466084099946972),
                new Point3D(0.500000000000000, 0.153053511143441, 0.466037844848277));

            var center = new Point3D(0.500000000000000, 0.153053511144470, 0.466037844847931);
            var magnification = 1e10;

            var zone = new Rectangle3D(center, 1 / magnification);
            WavefrontFile.Export(zone.LineSegments.Select(z => z.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Zone");

            WavefrontFile.Export(zone.Clip(intersector).Select(i => i.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Triangle-Intersector");
            WavefrontFile.Export(zone.Clip(gathering).Select(g => g.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Triangle-Gathering");

            var intersections = Triangle3D.LineSegmentIntersections(intersector, gathering).ToArray();
            Console.WriteLine($"Intersections \n{string.Join("\n", intersections.Select(i => i))}");

            WavefrontFile.Export(zone.Clip(intersections).Select(i => i.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Triangle-Intersections");
            /*
             * a = {[[ X: 0.500000000000000 Y: 0.153053511144470 Z: 0.466037844847931 ], [ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ]] 1.0855973416399235E-12}
b = {[[ X: 0.501423803223381 Y: 0.158123450101038 Z: 0.465859624462612 ], [ X: 0.500000000000000 Y: 0.152916092505751 Z: 0.466084099946972 ]] 0.00540316370168275}


            a = {[[ X: 0.500000000000000 Y: 0.153053511144470 Z: 0.466037844847931 ], [ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ]] 1.0855973416399235E-12}
b = {[[ X: 0.500000000000000 Y: 0.153053511143441 Z: 0.466037844848277 ], [ X: 0.501423803223381 Y: 0.158123450101038 Z: 0.465859624462612 ]] 0.005269085229765826}
            */
            var testA = new LineSegment3D(0.500000000000000, 0.153053511144470, 0.466037844847931, 0.500000000000000, 0.153053511143441, 0.466037844848277);
            var testB = new LineSegment3D(0.500000000000000, 0.153053511143441, 0.466037844848277, 0.501423803223381, 0.158123450101038, 0.465859624462612);

            WavefrontFile.Export(zone.Clip([testA]).Select(i => i.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Triangle-TestA");
            WavefrontFile.Export(zone.Clip([testB]).Select(g => g.TranslateToPointAndScale(center, magnification)), $"Wavefront/Surfaces/Triangle-TestB");


            //var intersections = Triangle3D.LineSegmentIntersections(triangle, border1).ToArray();
            //Console.WriteLine($"Intersections \n{string.Join("\n", intersections.Select(i => i))}");

            //var intersections2 = Triangle3D.LineSegmentIntersections(triangle, border2).ToArray();
            //Console.WriteLine($"Intersections \n{string.Join("\n", intersections2.Select(i => i))}");

            //var intersections3 = Triangle3D.LineSegmentIntersections(triangle2, border1).ToArray();
            //Console.WriteLine($"Intersections \n{string.Join("\n", intersections3.Select(i => i))}");

            //var intersections4  = Triangle3D.LineSegmentIntersections(triangle2, border2).ToArray();
            //Console.WriteLine($"Intersections \n{string.Join("\n", intersections4.Select(i => i))}");

            //var grid = WireFrameMesh.Create();
            //grid.AddTriangle(triangle, "", 0);
            //grid.AddTriangle(gathering, "", 0);
            //WavefrontFile.Export(grid, "Wavefront/ProblemTriangles");

            //foreach (var i in intersections.Select((ii, i) => new { intersection = ii, index = i})) {
            //    grid = WireFrameMesh.Create();
            //    grid.AddTriangle(i.intersection.Start, i.intersection.Center, i.intersection.End, "", 0);
            //    WavefrontFile.Export(grid, $"Wavefront/Intersections-{i.index}");
            //}


        }
    }
}
