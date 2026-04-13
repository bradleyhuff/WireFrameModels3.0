using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal static class TriangleSegmentResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            var segments = intermeshTriangles.SelectMany(t => t.IntersectionSegments.SelectMany(i => i.Segments)).DistinctBy(s => s.Id).ToArray();

            ShortSegmentReplacements(segments);
            NearParallelReplacements(segments);

            BaseObjects.Console.WriteLine($"Max stack {IntermeshSegment.MaxStack}");

            //var segment = segments.FirstOrDefault(s => s.Contacts.Count(c => !s.Capsules.First().IsResolved(c.Capsules.First())) == 6);
            //if (segment != null)
            //{
            //    var focusAt = segment.A.Point;
            //    var magnification = 1e12;
            //    var zone = new Rectangle3D(focusAt, 1 / magnification);
            //    var clippedSegment = zone.Clip(segment.Capsules.First().Segment);
            //    if (clippedSegment is not null)
            //    {
            //        clippedSegment = clippedSegment.TranslateToPointAndScale(focusAt, magnification);
            //        WavefrontFile.Export([clippedSegment], $"Wavefront/Resolve11-{segment.Id}/Segment-{segment.Key}");
            //    }

            //    //WavefrontFile.Export([segment11.Capsules.First().Segment], $"Wavefront/Resolve11-{segment11.Id}/Segment");
            //    foreach (var contact in segment.Contacts.Where(c => !segment.Capsules.First().IsResolved(c.Capsules.First())))
            //    {
            //        clippedSegment = zone.Clip(contact.Capsules.First().Segment);
            //        if (clippedSegment is not null)
            //        {
            //            clippedSegment = clippedSegment.TranslateToPointAndScale(focusAt, magnification);
            //            WavefrontFile.Export([clippedSegment], $"Wavefront/Resolve11-{segment.Id}/Contact-{contact.Id}-{contact.Key}");
            //        }
            //        //WavefrontFile.Export([contact.Capsules.First().Segment], $"Wavefront/Resolve11-{segment11.Id}/Contact-{contact.Id}");
            //    }
            //}

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment resolve. Segments {segments.Length} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        internal static void NearParallelReplacements(IntermeshSegment[] segments)
        {
            var nearParallels = segments.Select(s => (Segment: s, Contacts: s.ContactsWithRemovedRecursion(c => c.LocalContacts)
                .Where(c => IntermeshCapsule.IsNearParallel(s.Capsules.FirstOrDefault(), c.Capsules.FirstOrDefault())))).Where(p => p.Contacts.Any()).ToArray();
            foreach (var nearParallel in nearParallels)
            {

            }
            BaseObjects.Console.WriteLine($"Near parallels replaced {nearParallels.Count()}");
        }

        internal static void ShortSegmentReplacements(IntermeshSegment[] segments)
        {
            var shortSegments = segments.SelectMany(s => s.ContactsWithRemovedRecursion(c => c.LocalContacts))
                .Where(s => s.Segment.Length < 1e-9).ToArray();

            foreach (var shortSegment in shortSegments) { shortSegment.Remove(); }
            BaseObjects.Console.WriteLine($"Short segments replaced {shortSegments.Count()}");
        }
    }
}
