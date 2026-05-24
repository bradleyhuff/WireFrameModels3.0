using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Diagnostics
{
    internal static class IntermeshTriangleDiagnotics
    {
        internal static void Dump(this Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification, string text = "")
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);
            {
                var clips = zone.Clip(triangle.PerimeterSegments.Select(s => s.Segment));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/Perimeter");
            }
            {
                var clips = zone.Clip(triangle.IntersectionSegments.Select(s => s.Segment));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/Intersections");
            }
            {
                var clips = zone.Clip(triangle.IntersectingTriangles.Select(t => t.Triangle).SelectMany(t => t.Edges));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/IntersectingTriangles");
            }

            foreach(var segment in triangle.PerimeterSegments) {
                var clip = zone.Clip(segment.Segment);
                clip = clip.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export([clip], $"Wavefront/IntermeshTriangle-{triangle.Id}/Perimeter-Segment-{segment.Id}");
            }
            foreach (var segment in triangle.IntersectionSegments)
            {
                var clip = zone.Clip(segment.Segment);
                clip = clip.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export([clip], $"Wavefront/IntermeshTriangle-{triangle.Id}/Intersection-Segment-{segment.Id}");
            }

            foreach (var filling in triangle.Fillings)
            {
                var clips = zone.Clip(filling.Triangle);
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/FillTriangle-{filling.Id}");
            }
        }
    }
}
