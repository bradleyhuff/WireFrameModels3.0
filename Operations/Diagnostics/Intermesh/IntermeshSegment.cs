using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Diagnostics.Intermesh
{
    internal class IntermeshSegment
    {
        internal static void Dump(IEnumerable<Operations.Intermesh.Basics.IntermeshSegment> segments, Point3D focusAt, double magnification)
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);

            foreach (var segment in segments)
            {
                var clip = zone.Clip(segment.Segment);
                if (clip is not null)
                {
                    clip = clip.TranslateToPointAndScale(focusAt, magnification);
                    WavefrontFile.Export([clip], $"Wavefront/IntermeshSegments/Graph-{segment.Id}-{segment.Key}");
                    Console.WriteLine($"Length {segment.Segment}");
                }
            }
        }
    }
}
