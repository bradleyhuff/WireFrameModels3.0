using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Diagnostics
{
    internal static class IntermeshSegmentDiagnostics
    {
        internal static void Dump(IEnumerable<Operations.Intermesh.Basics.IntermeshSegment> segments, IEnumerable<LineSegment3D> lineSegments, Point3D focusAt, double magnification,string text = "")
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);

            foreach (var segment in segments)
            {
                var clip = zone.Clip(segment.Segment);
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/IntermeshSegments{text}/Graph-{segment.Id}-{segment.Key}");
                    BaseObjects.Console.WriteLine($"Length {segment.Segment}");
                }
            }
            int index = 0;
            foreach (var segment in lineSegments)
            {
                var clip = zone.Clip(segment);
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/IntermeshSegments{text}/Graph-LineSegment-{index}");
                    BaseObjects.Console.WriteLine($"Length {segment}");
                }
                index++;
            }
        }

        internal static void Dump(this IEnumerable<Operations.Intermesh.Basics.IntermeshSegment> segments, Point3D focusAt, double magnification, string text = "")
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);

            foreach (var segment in segments)
            {
                var clip = zone.Clip(segment.Segment);
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/IntermeshSegments{text}/Graph-{segment.Id}-{segment.Key}");
                    BaseObjects.Console.WriteLine($"Length {segment.Segment}");
                }
            }
        }

        internal static void PointCountDisplay(this IEnumerable<Operations.Intermesh.Basics.IntermeshSegment> segments)
        {
            var points = segments.Where(s => !s.IsRemoved).SelectMany(s => s.Points);
            BaseObjects.Console.WriteLine($"\nPoint/Segment counts", ConsoleColor.Cyan);
            var pointCounts = points.GroupCounts(s => s.Id).Select(o => (int.Parse(o[1].ToString()), int.Parse(o[0].ToString())));
            foreach (var line in pointCounts.GroupCounts(g => g.Item1))
            {
                BaseObjects.Console.WriteLine(line.DisplayRow(), ConsoleColor.Cyan, int.Parse(line[0].ToString()) == 1 ? ConsoleColor.DarkRed: System.Console.BackgroundColor);
            }
        }

        internal static void ShowSingleSegmentPoints(this IEnumerable<Operations.Intermesh.Basics.IntermeshSegment> segments)
        {
            var points = segments.Where(s => !s.IsRemoved).SelectMany(s => s.Points);
            var pointCounts = points.GroupCounts(s => s.Id).Select(o => (int.Parse(o[1].ToString()), int.Parse(o[0].ToString())));
            var singlePoints = pointCounts.Where(p => p.Item1 == 1);
            if(singlePoints.Any()) BaseObjects.Console.WriteLine($"\nSingle points {string.Join(",", pointCounts.Where(p => p.Item1 == 1).Select(p => p.Item2))} ", ConsoleColor.Cyan, ConsoleColor.DarkRed);
        }
    }
}
