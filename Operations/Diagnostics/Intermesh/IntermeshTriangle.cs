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
                WavefrontFile.Export(clippedTriangle, $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}");
            }

            foreach (var segment in triangle.PerimeterDivisions)
            {
                Console.WriteLine($"{segment.Id} Parent {segment.ParentSegment?.Id} Perimeter segment {segment.Key} {segment.Segment}");
                var clip = zone.Clip(segment.Segment.Transform(directionalTransform));
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}-Perimeter-{segment.Key}");
                }
            }

            foreach (var segment in triangle.InternalDivisions)
            {
                Console.WriteLine($"{segment.Id} Parent {segment.ParentSegment?.Id} Internal segment {segment.Key} {segment.Segment}");
                var clip = zone.Clip(segment.Segment.Transform(directionalTransform));
                if (clip is not null)
                {
                    clip = clip.Transform(Transform.TranslateToPointAndScale(focusAt, magnification));
                    WavefrontFile.Export([clip], $"Wavefront/Triangle-{triangle.Id}/Graph-{triangle.Id}-Internal-{segment.Key}");
                }
            }
        }

        
    }
}
