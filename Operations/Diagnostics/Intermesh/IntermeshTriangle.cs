using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Diagnostics.Intermesh
{
    public static class IntermeshTriangle
    {
        internal static void Dump(Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification, double directionalScale)
        {
            var directionalTransform = Transform.DirectionalScaling(triangle.Triangle.Center, triangle.Triangle.MinimumHeight.Direction, directionalScale);
            Dump(triangle, focusAt, magnification, directionalTransform);
        }

        internal static void Dump(Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification)
        {
            Dump(triangle, focusAt, magnification, Transform.Identity());
        }
        internal static void Dump(Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification, Transform directionalTransform)
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);
            WavefrontFile.Export(zone.LineSegments.Select(z => z.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Triangle-{triangle.Id}/Zone-{triangle.Id}");

            var clippedTriangle = zone.Clip(triangle.Triangle.Transform(directionalTransform));
            if (clippedTriangle.Any())
            {
                clippedTriangle = clippedTriangle.Select(c => c.TranslateToPointAndScale(focusAt, magnification));
                WavefrontFile.Export(clippedTriangle, $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}-{triangle.Key}");
            }

            foreach (var segment in triangle.PerimeterDivisions)
            {
                var clip = zone.Clip(segment.Segment.Transform(directionalTransform));
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}-Perimeter-{segment.Key}");
                    Console.WriteLine($"Length {segment.Segment}");
                }
            }

            foreach (var segment in triangle.InternalDivisions)
            {
                var clip = zone.Clip(segment.Segment.Transform(directionalTransform));
                if (clip is not null)
                {
                    clip = clip.Transform(Transform.TranslateToPointAndScale(focusAt, magnification));
                    WavefrontFile.Export([clip], $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}-Internal-{segment.Key}");
                    Console.WriteLine($"Length {segment.Segment}");
                }
            }
        }

        internal static void GraphIntersectingTriangles(Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification, Transform directionalTransform)
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);
            WavefrontFile.Export(zone.LineSegments.Select(z => z.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/TriangleIntersectors-{triangle.Id}/Zone-{triangle.Id}");
            {
                var clippedTriangle = zone.Clip(triangle.Triangle.Transform(directionalTransform));
                if (clippedTriangle.Any())
                {
                    clippedTriangle = clippedTriangle.Select(c => c.TranslateToPointAndScale(focusAt, magnification));
                    WavefrontFile.Export(clippedTriangle, $"Wavefront/TriangleIntersectors-{triangle.Id}/Graph-{triangle.Id}");
                }
            }

            foreach (var intersector in triangle.IntersectingTriangles)
            {
                var clippedTriangle = zone.Clip(intersector.Triangle.Transform(directionalTransform));
                if (clippedTriangle.Any())
                {
                    clippedTriangle = clippedTriangle.Select(c => c.TranslateToPointAndScale(focusAt, magnification));
                    WavefrontFile.Export(clippedTriangle, $"Wavefront/TriangleIntersectors-{triangle.Id}/Intersector-{intersector.Id}");
                }

                var intersections = triangle.GatheringSets[intersector.Id]?.Intersections ?? Enumerable.Empty<LineSegment3D>();
                foreach (var intersection in intersections)
                {
                    int index = 0;
                    var clip = zone.Clip(intersection.Transform(directionalTransform));
                    if (clip is not null)
                    {
                        clip = clip.Transform(Transform.TranslateToPointAndScale(focusAt, magnification));
                        Console.WriteLine($"{triangle.Id} Intersector {intersector.Id} Intersection {intersection}");
                        WavefrontFile.Export([clip], $"Wavefront/TriangleIntersectors-{triangle.Id}/Intersector_{intersector.Id}-Intersection{index}");
                        index++;
                    }
                }

            }
        }        
    }
}
