using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal static class SegmentContactAssignments
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var allSegments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(i => i.Id).ToArray();
            foreach (var segment in allSegments) { segment.ClearContacts(); }
            // Triangle segment contact assignments
            var segmentBucket = new BoxBucket<IntermeshSegment>(allSegments.Where(s => !s.IsRemoved));
            foreach (var segment in allSegments.Where(s => !s.IsRemoved))
            {
                var matches = segmentBucket.Fetch(segment, 1e-5).Where(m => m.Id != segment.Id);
                segment.AddRangeContacts(matches.Where(m => LineSegment3D.Distance(m.Segment, segment.Segment) < GapConstants.Resolver));
            }

            //var contacts = intermeshTriangles.SelectMany(t => t.Segments.Where(s => !s.IsRemoved).SelectMany(s => s.Contacts.Where(s => !s.IsRemoved)).DistinctBy(c => c.Id));
            //BaseObjects.Console.WriteLine($"Contacts {contacts.Count()}");
        }
    }
}
