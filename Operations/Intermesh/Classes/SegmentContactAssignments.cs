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
            DateTime start = DateTime.Now;

            //Triangle perimeter contact assignments
            var perimeters = intermeshTriangles.SelectMany(t => t.PerimeterSegments).DistinctBy(p => p.Id).ToArray();
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
                perimeter.VertexAContacts.AddRange(pointTable[perimeter.A.Id].Where(s => s.Id != perimeter.Id));
                perimeter.VertexBContacts.AddRange(pointTable[perimeter.B.Id].Where(s => s.Id != perimeter.Id));
            }

            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments.SelectMany(i => i.Segments)).DistinctBy(i => i.Id).ToArray();

            // Triangle intersection contact assignments
            var segmentBucket = new BoxBucket<IntermeshSegment>(intersections);
            foreach (var intersection in intersections)
            {
                var matches = segmentBucket.Fetch(intersection, 1e-5).Where(m => m.Id != intersection.Id);
                intersection.LocalContacts.AddRange(matches.Where(m => LineSegment3D.Distance(m.Capsules.Single().Segment, intersection.Capsules.Single().Segment) < 1e-9));
            }

            //BaseObjects.Console.WriteLine("Vertex A contact count", ConsoleColor.Magenta);
            //BaseObjects.Console.WriteLine(perimeters.GroupCounts(p => p.QueriedContacts(s => s.VertexAContacts).Count()).DisplayByLine(), ConsoleColor.Gray);
            //BaseObjects.Console.WriteLine("Vertex B contact count", ConsoleColor.Red);
            //BaseObjects.Console.WriteLine(perimeters.GroupCounts(p => p.QueriedContacts(s => s.VertexBContacts).Count()).DisplayByLine(), ConsoleColor.Gray);
            //BaseObjects.Console.WriteLine("Vertex contact count", ConsoleColor.Blue);
            //BaseObjects.Console.WriteLine(perimeters.GroupCounts(p => p.QueriedContacts(s => s.VertexAContacts.Concat(s.VertexBContacts)).Count()).DisplayByLine(), ConsoleColor.Gray);

            BaseObjects.Console.WriteLine("Intersection contact count", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(intersections.GroupCounts(s => s.LocalContacts.Count(c => !s.Capsules.First().IsResolved(c.Capsules.First()))).DisplayByLine(), ConsoleColor.Gray);

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Segment contact assignments. Intersections {intersections.Count()} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
