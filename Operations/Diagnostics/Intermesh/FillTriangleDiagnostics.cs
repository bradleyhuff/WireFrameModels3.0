using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Diagnostics.Intermesh
{
    internal static class FillTriangleDiagnostics
    {
        internal static void Dump(this Operations.Intermesh.Basics.FillTriangle fillTriangle, Point3D focusAt, double magnification, string text = "")
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);

            var loop = new List<LineSegment3D>();
            for (int i = 0; i < fillTriangle.Loop.Length - 1; i++)
            {
                var segment = new LineSegment3D(fillTriangle.Loop[i].Point, fillTriangle.Loop[i + 1].Point);
                var clip = zone.Clip(segment);
                if (clip is not null) { loop.Add(clip); 
                    WavefrontFile.Export([clip], $"Wavefront/FillTriangle-{fillTriangle.Id}{text}/Loop-Segment-{new Combination2(fillTriangle.Loop[i].Id, fillTriangle.Loop[i + 1].Id)}"); }
            }
            {
                var segment = new LineSegment3D(fillTriangle.Loop.Last().Point, fillTriangle.Loop.First().Point);
                var clip = zone.Clip(segment);
                if (clip is not null) { loop.Add(clip); 
                    WavefrontFile.Export([clip], $"Wavefront/FillTriangle-{fillTriangle.Id}{text}/Loop-Segment-{new Combination2(fillTriangle.Loop.First().Id, fillTriangle.Loop.Last().Id)}"); }
            }

            WavefrontFile.Export(loop, $"Wavefront/FillTriangle-{fillTriangle.Id}{text}/Loop-{fillTriangle.LoopKey}");

            {
                var clip = zone.Clip(fillTriangle.Triangle);
                WavefrontFile.Export(clip, $"Wavefront/FillTriangle-{fillTriangle.Id}{text}/Triangle-{fillTriangle.Key}");
            }

            WavefrontFile.Export(zone.LineSegments.Select(z => z.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/FillTriangle-{fillTriangle.Id}/Zone");
        }
    }
}
