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
            var perimeters = intermeshTriangles.SelectMany(t => t.PerimeterSegments).DistinctBy(p => p.Id).ToArray();
            foreach (var perimeter in perimeters) { perimeter.ClearContacts(); }
            var pointTable = new Dictionary<int, List<IntermeshSegment>>();
            foreach (var perimeter in perimeters)
            {
                if (!pointTable.ContainsKey(perimeter.A.Id)) { pointTable[perimeter.A.Id] = new List<IntermeshSegment>(); }
                if (!pointTable.ContainsKey(perimeter.B.Id)) { pointTable[perimeter.B.Id] = new List<IntermeshSegment>(); }
                pointTable[perimeter.A.Id].Add(perimeter);
                pointTable[perimeter.B.Id].Add(perimeter);
            }

            foreach (var perimeter in perimeters)
            {
                perimeter.AddRangeContacts(pointTable[perimeter.A.Id].Where(s => s.Id != perimeter.Id));
                perimeter.AddRangeContacts(pointTable[perimeter.B.Id].Where(s => s.Id != perimeter.Id));
            }

            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments).DistinctBy(i => i.Id).ToArray();
            foreach (var intersection in intersections) { intersection.ClearContacts(); }
            var allSegments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(i => i.Id).ToArray();

            // Triangle intersection contact assignments
            var segmentBucket = new BoxBucket<IntermeshSegment>(allSegments);
            foreach (var intersection in intersections)
            {
                var matches = segmentBucket.Fetch(intersection, 1e-5).Where(m => m.Id != intersection.Id);
                intersection.AddRangeContacts(matches.Where(m => LineSegment3D.Distance(m.Segment, intersection.Segment) < GapConstants.Resolver));
            }
        }
    }
}
