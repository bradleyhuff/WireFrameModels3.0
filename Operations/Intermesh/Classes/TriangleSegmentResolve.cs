using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using Microsoft.VisualBasic;
using Operations.Basics;
using Operations.Diagnostics;
using Operations.Diagnostics.Intermesh;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Interfaces;
using System.Net.Sockets;
using System.Xml.Linq;

namespace Operations.Intermesh.Classes
{
    internal static class TriangleSegmentResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            BaseObjects.Console.WriteLine("Triangle Segment Resolve", ConsoleColor.Yellow);
            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments).DistinctBy(s => s.Id).ToArray();
            if (!intersections.Any()) return;
            while (ResolveCycle(intermeshTriangles)) { }
        }

        private static Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> BuildPairsTable(IntermeshSegment[] segments)
        {
            var pairs = new Combination2Dictionary<(IntermeshSegment, IntermeshSegment)>();

            foreach (var segment in segments.Where(s => !s.IsRemoved))
            {
                foreach (var contact in segment.Contacts.Where(c => !c.IsRemoved))
                {
                    var key = new Combination2(segment.Id, contact.Id);
                    if (!pairs.ContainsKey(key)) { pairs[key] = (segment, contact); }
                }
            }

            return pairs;
        }

        private static bool ResolveCycle(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            var pairs = BuildPairsTable(segments);

            ShortSegmentReplacements(segments, ref pairs);
            NearParallelReplacements(segments, ref pairs);

            var unresolvedPairs = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            var unresolvedNearInlinePairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsNearInLineParallel(u.Value)).ToArray();
            var unresolvedCrossPairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsCross(u.Value)).ToArray();

            var bucket = new BoxBucket<IntermeshSegment>(segments);
            foreach (var unresolvedPair in unresolvedPairs)
            {
                var segment1 = unresolvedPair.Value.Item1.Segment;
                var segment2 = unresolvedPair.Value.Item2.Segment;
                var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
                var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);

                if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value, bucket); else GapResolve(unresolvedPair.Value, bucket);
            }
            //BaseObjects.Console.WriteLine($"Long pair distances {pairs.Count(p => LineSegment3D.Distance(p.Value.Item1.Segment, p.Value.Item2.Segment) > GapConstants.Resolver)}");
            //BaseObjects.Console.WriteLine($"Short segments {segments.SelectMany(s => s.Capsules).Count(c => c.Segment.Length < GapConstants.Resolver)}");
            //var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            //BaseObjects.Console.WriteLine($"Removed segments {segments.Count(s => s.IsRemoved)} Empty slots {slots.Count(s => !s.Segments.Any())}");
            var unresolvedPairsAfter = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            var wasChanged = segments.Any(s => s.WasChanged);
            var last = !wasChanged;
            //if(!lastResolve)
            BaseObjects.Console.WriteLine($"Segments {segments.Count()} Unresolved: {unresolvedPairs.Count()} Changed: {segments.Count(s => s.WasChanged)}  Left unresolved: {unresolvedPairsAfter.Count()} ");
            //if(lastResolve)
            //BaseObjects.Console.WriteLine($"Segments {segments.Count()} Unresolved: {unresolvedPairs.Count()} Changed: {segments.Count(s => s.WasChanged)}  Left unresolved: {unresolvedPairsAfter.Count()} ",
            //    !last ? ConsoleColor.Gray : (!unresolvedPairsAfter.Any() ? ConsoleColor.Black : ConsoleColor.White),
            //    !last ? System.Console.BackgroundColor : (!unresolvedPairsAfter.Any() ? ConsoleColor.Green : ConsoleColor.Red));

            SegmentReplacements(intermeshTriangles);
            return wasChanged;
        }

        private static void SegmentReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            var allSegments = slots.SelectMany(s => s.Segments).ToArray();
            //BaseObjects.Console.WriteLine($"Segment/Capsule counts", ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine(allSegments.GroupCounts(s => s.Capsules.Count()).DisplayByLine(), ConsoleColor.Cyan);

            var triangle6435 = intermeshTriangles.SingleOrDefault(t => t.Id == 6435);

            ReplaceEmptySlots(slots);
            RemoveEmptySlots(intermeshTriangles);

            var replacements = slots.Where(s => s.Segments.Any(ss => ss.Capsules.Count() != 1)).ToArray();
            var replacementTable = BuildReplacementTable(replacements);
            //BuildContacts(replacementTable);
            //BaseObjects.Console.WriteLine($"Replacement table", ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine(replacementTable.Values.GroupCounts(s => s.Replacement.Count()).DisplayByLine(), ConsoleColor.Cyan);
            ApplyReplacements(replacements, replacementTable);
            RemoveDuplicateIntersectionSlots(intermeshTriangles);
            ClearSegmentHistories(intermeshTriangles);

            //BaseObjects.Console.WriteLine($"Segment/Capsule counts", ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine(allSegments.GroupCounts(s => s.Capsules.Count()).DisplayByLine(), ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine($"Replacements finished.", ConsoleColor.Cyan);
        }

        private static void ReplaceEmptySlots(IEnumerable<IntermeshEdgeSlot> slots)
        {
            var removedSlots = slots.Where(s => s.Segments.Any(ss => ss.IsRemoved && ss.Replacement is not null)).ToArray();
            foreach (var removedSlot in removedSlots)
            {
                var toBeReplaced = removedSlot.Segments.Where(s => s.IsRemoved && s.Replacement is not null).ToArray();
                removedSlot.Segments.RemoveAll(s => s.IsRemoved && s.Replacement is not null);
                removedSlot.Segments.AddRange(toBeReplaced.Select(r => GetReplacement(r)));
            }
        }

        private static IntermeshSegment GetReplacement(IntermeshSegment input)
        {
            var replacement = input.Replacement;
            while (replacement.Replacement is not null)
            {
                replacement = replacement.Replacement;
            }
            return replacement;
        }

        private static void RemoveEmptySlots(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var slot in triangle.IntersectionSlots.Where(ss => !ss.Segments.Any(s => !s.IsRemoved)).ToArray())
                {
                    triangle.RemoveIntersectionSlot(slot);
                }
            }
        }

        private static void ClearSegmentHistories(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var segment in intermeshTriangles.SelectMany(t => t.Segments))
            {
                segment.ClearHistory();
            }
        }

        private static void RemoveDuplicateIntersectionSlots(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var triangle in intermeshTriangles)
            {
                triangle.RemoveIntersectionSlots(triangle.IntersectionSlots.Where(i => triangle.PerimeterSegments.Any(p => p.Key == i.Key)).ToArray());
            }
        }

        private static void ApplyReplacements(IEnumerable<IntermeshEdgeSlot> replacements, Dictionary<int, (IntermeshSegment Segment, List<IntermeshSegment> Replacement)> replacementTable)
        {
            foreach (var replacement in replacements)
            {
                var segments = new List<IntermeshSegment>();
                foreach (var element in replacement.Segments.Where(ss => ss.Capsules.Any()))
                {
                    if (replacementTable.ContainsKey(element.Id))
                    {
                        segments.AddRange(replacementTable[element.Id].Replacement);
                    }
                    else
                    {
                        segments.Add(element);
                    }
                }
                replacement.Segments = segments.DistinctBy(s => s.Key, new Combination2Comparer()).ToList();
            }
        }

        private static Dictionary<int, (IntermeshSegment Segment, List<IntermeshSegment> Replacement)> BuildReplacementTable(IEnumerable<IntermeshEdgeSlot> replacements)
        {
            var segmentTable = new Combination2Dictionary<IntermeshSegment>();
            var replacementTable = new Dictionary<int, (IntermeshSegment, List<IntermeshSegment>)>();
            foreach (var replacement in replacements)
            {
                var segments = replacement.Segments.Where(ss => ss.Capsules.Any()).ToArray();
                foreach (var element in segments.Where(ss => ss.Capsules.Count() > 1))
                {
                    replacementTable[element.Id] = (element, ApplyCapsules(element, segmentTable));
                }
            }
            return replacementTable;
        }

        private static List<IntermeshSegment> ApplyCapsules(IntermeshSegment segment, Combination2Dictionary<IntermeshSegment> segmentTable)
        {
            var output = new List<IntermeshSegment>();
            foreach (var capsule in segment.Capsules)
            {
                output.Add(FetchSegment(capsule, segmentTable));
            }

            return output;
        }

        private static IntermeshSegment FetchSegment(IntermeshCapsule capsule, Combination2Dictionary<IntermeshSegment> segments)
        {
            var key = new Combination2(capsule.A.Id, capsule.B.Id);
            if (!segments.ContainsKey(key)) { segments[key] = new IntermeshSegment(capsule.A, capsule.B); }
            return segments[key];
        }

        private static void ShortSegmentReplacements(IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            var bucket = new BoxBucket<IntermeshSegment>(segments);
            IntermeshSegment[] shortSegments;
            bool shortSegmentsRemoved = false;

            shortSegments = segments.Where(s => s.Segment.Length < GapConstants.Resolver && !s.IsRemoved).ToArray();
            while (true)
            {
                shortSegments = segments.Where(s => s.Segment.Length < GapConstants.Resolver && !s.IsRemoved).ToArray();
                if (!shortSegments.Any()) { break; }

                foreach (var shortSegment in shortSegments.NonAdjoining())
                {
                    var linksA = bucket.LinkingSegments(shortSegment.A).Count();
                    var linksB = bucket.LinkingSegments(shortSegment.B).Count();
                    var from = (linksA > linksB) || (linksA == linksB && shortSegment.A.Id > shortSegment.B.Id) ? shortSegment.A : shortSegment.B;
                    var to = (linksA < linksB) || (linksA == linksB && shortSegment.A.Id < shortSegment.B.Id) ? shortSegment.A : shortSegment.B;

                    bucket.PointTransferFromTo(from, to, shortSegment);

                    shortSegment.Remove();
                    shortSegmentsRemoved = true;
                }
            }
            if (shortSegmentsRemoved)
            {
                pairs = BuildPairsTable(segments);
            }
        }

        private static void NearParallelReplacements(IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            bool nearParallelRemoved = false;
            var nearParallelPairs = pairs.Where(p => IntermeshSegmentExtensions.IsNearParallel(p.Value)).ToArray();

            //BaseObjects.Console.WriteLine($"Near parallel pairs {nearParallelPairs.Count()}", ConsoleColor.Cyan);

            foreach (var pair in nearParallelPairs.Select(p => p.Value))
            {
                var toRemove = pair.Item1;
                var toAddTo = pair.Item2;
                if (pair.Item1.Contacts.Count > pair.Item2.Contacts.Count) { toRemove = pair.Item2; toAddTo = pair.Item1; }
                toAddTo.AddRangeContacts(toRemove.Contacts.Where(c => !c.IsRemoved));
                nearParallelRemoved = true;
                toRemove.Remove();
                toRemove.Replacement = toAddTo;
            }

            if (nearParallelRemoved)
            {
                pairs = BuildPairsTable(segments);
            }
        }

        private static void InLineResolve((IntermeshSegment, IntermeshSegment) unresolvedPair)
        {
            var pointsA = unresolvedPair.Item1.Capsules.Points().ToArray();
            var pointsB = unresolvedPair.Item2.Capsules.Points().ToArray();
            foreach (var point in pointsA)
            {
                unresolvedPair.Item2.SplitBy(point);
            }
            foreach (var point in pointsB)
            {
                unresolvedPair.Item1.SplitBy(point);
            }
        }

        private static void CrossResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var unresolvedSet = IntermeshSegmentExtensions.PointIntersection(unresolvedPair);
            CrossWithIntersectionResolve(unresolvedSet, bucket);
        }

        private static void CrossWithIntersectionResolve((Point3D, IntermeshCapsule, IntermeshCapsule, IntermeshSegment, IntermeshSegment) unresolvedSet, BoxBucket<IntermeshSegment> bucket)
        {
            var intersection = unresolvedSet.Item1;
            if (intersection is null) { return; }
            var capsule1 = unresolvedSet.Item2;
            var capsule2 = unresolvedSet.Item3;
            var segment1 = unresolvedSet.Item4;
            var segment2 = unresolvedSet.Item5;

            var point = IntermeshPointExtensions.Fetch(intersection);
            var wasSplit1 = segment1.SplitBy(point);
            var wasSplit2 = segment2.SplitBy(point);

            var nearbyPoints1 = segment1.Capsules.Points().Where(p => Point3D.Distance(p.Point, point.Point) < GapConstants.Resolver).ToArray();
            var nearbyPoints2 = segment2.Capsules.Points().Where(p => Point3D.Distance(p.Point, point.Point) < GapConstants.Resolver).ToArray();

            segment1.ResolvePoints(nearbyPoints1);
            segment1.ResolvePoints(nearbyPoints2);

            segment2.ResolvePoints(nearbyPoints1);
            segment2.ResolvePoints(nearbyPoints2);

            //var isResolved2 = IntermeshSegmentExtensions.IsResolved(segment1, segment2);
            //if (!isResolved2)
            //{
            //    BaseObjects.Console.WriteLine($"Failed cross {segment1.Id} {segment2.Id} Segment1 split {wasSplit1} Segment2 split {wasSplit2}", ConsoleColor.Red);
            //}
            //else
            //{
            //    BaseObjects.Console.WriteLine($"Successful cross {segment1.Id} {segment2.Id} Segment1 split {wasSplit1} Segment2 split {wasSplit2}", ConsoleColor.Green);
            //}
        }

        private static void GapResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

            unresolvedPair.Item1.ExtendWith(linkSegment.A);
            unresolvedPair.Item1.ExtendWith(linkSegment.B);
            unresolvedPair.Item2.ResolvePoint(linkSegment.A);
            unresolvedPair.Item2.ResolvePoint(linkSegment.B);

            //var isResolved2 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
            //if (!isResolved2)
            //{
            //    BaseObjects.Console.WriteLine($"Failed gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Red);
            //}
            //else
            //{
            //    BaseObjects.Console.WriteLine($"Successful gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Green);
            //}
        }
    }
}
