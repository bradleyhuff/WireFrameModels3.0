using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.Math;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class TriangleSegmentContactResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            int count = 0;
            var usedSegments = new Combination2Dictionary<bool>();
            while (true)
            {
                Assign(intermeshTriangles);
                var wasChanged = Resolve(intermeshTriangles, count, usedSegments);

                count++;

                BaseObjects.Console.WriteLine($"{count} Was changed {wasChanged}",
                    count > 19 ? ConsoleColor.White : ConsoleColor.Gray,
                    count > 19 ? ConsoleColor.Red : ConsoleColor.Black);
                if (!wasChanged || count > 19) { break; }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment contact resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        internal static void Assign(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var allSegments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(i => i.Id).Where(s => !s.IsRemoved).ToArray();
            foreach (var segment in allSegments) { segment.ClearContacts(); }

            var segmentBucket = new BoxBucket<IntermeshSegment>(allSegments);
            foreach (var segment in allSegments)
            {
                var matches = segmentBucket.Fetch(segment, 1e-5).Where(m => m.Id != segment.Id).ToArray();
                segment.AddRangeContacts(matches.Where(m => LineSegment3D.Distance(m.Segment, segment.Segment) < GapConstants.Resolver).ToArray());
            }

            //var contacts = intermeshTriangles.SelectMany(t => t.Segments.Where(s => !s.IsRemoved).SelectMany(s => s.Contacts.Where(s => !s.IsRemoved)).DistinctBy(c => c.Id));
            //BaseObjects.Console.WriteLine($"Contacts {contacts.Count()}");
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

        private static bool Resolve(IEnumerable<IntermeshTriangle> intermeshTriangles, int count, Combination2Dictionary<bool> usedSegments)
        {
            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments).DistinctBy(s => s.Id).ToArray();
            if (!intersections.Any()) return false;
            var wasChanged = ResolveCycle(intermeshTriangles, count, usedSegments);
            if (!wasChanged) return false;
            while (ResolveCycle(intermeshTriangles, count, usedSegments)) ;
            InlineMultiSlotSegmentResolve(intermeshTriangles);
            JunctionSlotResolve(intermeshTriangles);
            CoplanarResolve(intermeshTriangles);
            return true;
        }

        private static bool ResolveCycle(IEnumerable<IntermeshTriangle> intermeshTriangles, int count, Combination2Dictionary<bool> usedSegments)
        {
            var segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            var pairs = BuildPairsTable(segments);

            ShortSegmentReplacements(intermeshTriangles, segments, ref pairs);
            NearParallelReplacements(intermeshTriangles, segments, ref pairs);

            var unresolvedPairs = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();

            //BaseObjects.Console.WriteLine($"Unresolved pairs {unresolvedPairs.Length}");

            foreach (var unresolvedPair in unresolvedPairs)
            {
                var segment1 = unresolvedPair.Value.Item1.Segment;
                var segment2 = unresolvedPair.Value.Item2.Segment;
                var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
                var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);

                if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value); else GapResolve(unresolvedPair.Value);
            }

            var changedSegments = segments.Where(s => s.WasChanged && !usedSegments.ContainsKey(s.Key)).ToArray();
            if (count > 3) { foreach (var changedSegment in changedSegments) { usedSegments[changedSegment.Key] = true; } }
            var wasChanged = changedSegments.Any();

            //BaseObjects.Console.WriteLine($"{count}    Was changed: {string.Join(", ", changedSegments.Take(5).Select(c => $"{c.Key} {c.Segment.Length.ToString("E3")} {c.Id}"))}");

            if (wasChanged) SegmentReplacements(intermeshTriangles);

            return wasChanged;
        }

        private static void SegmentReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            ReplaceEmptySegments(slots);
            RemoveEmptySlots(intermeshTriangles);

            var replacements = slots.Where(s => s.Segments.Any(ss => ss.Capsules.Count() != 1)).ToArray();
            var replacementTable = BuildReplacementTable(replacements);
            ApplyReplacements(replacements, replacementTable);
            RemoveDuplicateIntersectionSlots(intermeshTriangles);
            ClearSegmentHistories(intermeshTriangles);
        }

        private static void ReplaceEmptySegments(IEnumerable<IntermeshEdgeSlot> slots)
        {
            var emptySegmentSlots = slots.Where(s => s.Segments.Any(ss => ss.IsRemoved && ss.Replacement is not null)).ToArray();
            foreach (var emptySegmentSlot in emptySegmentSlots)
            {
                var toBeReplaced = emptySegmentSlot.Segments.Where(s => s.IsRemoved && s.Replacement is not null).ToArray();
                emptySegmentSlot.Segments.RemoveAll(s => s.IsRemoved && s.Replacement is not null);
                emptySegmentSlot.Segments.AddRange(toBeReplaced.Select(GetReplacement));
            }
        }

        private static IntermeshSegment GetReplacement(IntermeshSegment input)
        {
            var replacement = input.Replacement;
            while (replacement.Replacement is not null && !replacement.Replacement.IsRemoved)
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
                replacement.Segments = segments.DistinctBy(s => s.Key, Combination2Comparer.Comparer).ToList();
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
            foreach (var capsule in segment.Capsules.ToArray())
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

        private static void ShortSegmentReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles, IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            var bucket = new BoxBucket<IntermeshSegment>(segments);
            IntermeshSegment[] shortSegments;
            bool shortSegmentsRemoved = false;

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

        private static void NearParallelReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles, IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            bool nearParallelRemoved = false;
            var nearParallelPairs = pairs.Where(p => IntermeshSegmentExtensions.IsNearParallel(p.Value)).ToArray();

            foreach (var pair in nearParallelPairs.Select(p => p.Value))
            {
                var toRemove = pair.Item1;
                var toAddTo = pair.Item2;
                if (pair.Item1.Contacts.Count > pair.Item2.Contacts.Count) { toRemove = pair.Item2; toAddTo = pair.Item1; }
                if (pair.Item1.Contacts.Count == pair.Item2.Contacts.Count && pair.Item1.Id > pair.Item2.Id) { toRemove = pair.Item2; toAddTo = pair.Item1; }

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
                unresolvedPair.Item2.CapsuleSplit(point);
            }
            foreach (var point in pointsB)
            {
                unresolvedPair.Item1.CapsuleSplit(point);
            }
        }

        private static void CrossResolve((IntermeshSegment, IntermeshSegment) unresolvedPair)
        {
            var unresolvedSet = IntermeshSegmentExtensions.PointIntersection(unresolvedPair);
            CrossWithIntersectionResolve(unresolvedSet);
        }

        private static void CrossWithIntersectionResolve((Point3D, IntermeshCapsule, IntermeshCapsule, IntermeshSegment, IntermeshSegment) unresolvedSet)
        {
            var intersection = unresolvedSet.Item1;
            if (intersection is null) { return; }
            var capsule1 = unresolvedSet.Item2;
            var capsule2 = unresolvedSet.Item3;
            var segment1 = unresolvedSet.Item4;
            var segment2 = unresolvedSet.Item5;

            var point = IntermeshPointExtensions.Fetch(intersection);
            segment1.CapsuleSplit(point);
            segment2.CapsuleSplit(point);
        }

        private static void GapResolve((IntermeshSegment, IntermeshSegment) unresolvedPair)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

            unresolvedPair.Item1.ExtendWith(linkSegment.A);
            unresolvedPair.Item1.ExtendWith(linkSegment.B);
        }

        private static void JunctionSlotResolve(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var junctionSlots = intermeshTriangles.SelectMany(s => s.EdgeSlots).Where(s => s.JunctionPoints.Any()).DistinctBy(j => j.Id).ToArray();
            foreach (var junctionSlot in junctionSlots)
            {
                var junctionPoints = junctionSlot.JunctionPoints.ToArray();
                foreach (var junctionPoint in junctionPoints)
                {
                    var overlaps = junctionPoint.Segments.Where(s => junctionPoints.Any(j => j.Junction.Id == s.A.Id && j.Junction.Id == s.B.Id));
                    foreach (var overlap in overlaps)
                    {
                        overlap.Remove();
                    }
                }

                junctionPoints = junctionSlot.JunctionPoints.ToArray();
                foreach (var junctionPoint in junctionPoints)
                {
                    foreach (var wayward in junctionPoint.Waywards)
                    {
                        var segments = junctionPoint.Segments.Where(s => s.Id != wayward.Id);
                        var waywardPoint = wayward.Points.Single(w => !segments.SelectMany(s => s.Points).Any(s => s.Id == w.Id));
                        segments.CapsuleSplit(waywardPoint);
                    }
                }
            }

            var replacements = junctionSlots.Where(s => s.Segments.Any(ss => ss.Capsules.Count() != 1)).ToArray();
            var replacementTable = BuildReplacementTable(replacements);
            ApplyReplacements(replacements, replacementTable);

            //BaseObjects.Console.WriteLine($"InLineSingleSlotSegmentRemovals Slots: {slots.Count()} Slots with junctions: {count} Mismatched slots {count2}   Elapsed Time {(DateTime.Now - start).TotalSeconds} seconds", ConsoleColor.Yellow);
        }

        private static void InlineMultiSlotSegmentResolve(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            var segments = slots.SelectMany(s => s.Segments.Where(ss => !ss.IsRemoved)).DistinctBy(s => s.Id).ToArray();
            var points = segments.Points().ToArray();

            var pointSlotsMap = new GroupingDictionary<int, List<IntermeshEdgeSlot>>(() => new List<IntermeshEdgeSlot>());
            foreach (var slot in slots)
            {
                foreach (var point in slot.Segments.Where(ss => !ss.IsRemoved).Points())
                {
                    if (!pointSlotsMap[point.Id].Any(s => s.Id == slot.Id))
                    {
                        pointSlotsMap[point.Id].Add(slot);
                    }
                }
            }

            var inLineReplacements = new List<(IEnumerable<IntermeshEdgeSlot> ReplaceIn, IntermeshSegment ToBeReplaced, IEnumerable<IntermeshSegment> ReplaceWith)>();

            foreach (var segment in segments)
            {
                var slotsA = pointSlotsMap[segment.A.Id];
                var slotsB = pointSlotsMap[segment.B.Id];

                var commonSlots = slotsA.Concat(slotsB).GroupBy(g => g.Id).Where(g => g.Count() == 2).Select(g => g.First()).ToArray();
                if (commonSlots.Count() > 1)
                {
                    var slotsWithSegment = commonSlots.Where(s => s.Segments.Where(ss => !ss.IsRemoved).Any(ss => ss.Key == segment.Key));
                    var replacements = commonSlots.Where(s => !s.Segments.Where(ss => !ss.IsRemoved).Any(ss => ss.Key == segment.Key));
                    foreach (var replacement in replacements.Take(1))
                    {
                        var replaceWith = replacement.Segments.Where(ss => !ss.IsRemoved).Between(segment);
                        inLineReplacements.Add((slotsWithSegment, segment, replaceWith));
                    }
                }
            }

            foreach (var inlineReplacement in inLineReplacements)
            {
                foreach (var slot in inlineReplacement.ReplaceIn)
                {
                    var list = slot.Segments;
                    int index = list.IndexOf(inlineReplacement.ToBeReplaced);
                    if (index == -1) { continue; }
                    list.RemoveAt(index);
                    list.InsertRange(index, inlineReplacement.ReplaceWith);
                }
            }
            //BaseObjects.Console.WriteLine($"InLineMultiSlotSegmentReplacements Slots: {slots.Count()} Segments: {segments.Count()} Points: {points.Count()}   Elapsed Time {(DateTime.Now - start).TotalSeconds} seconds", ConsoleColor.Cyan);
        }
        private static void CoplanarResolve(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var kinkTriangles = intermeshTriangles.Where(t => t.CoplanarDeviation > GapConstants.Resolver);
            foreach (var kinkTriangle in kinkTriangles)
            {
                var kinkPoints = kinkTriangle.Segments.Points().Where(p => kinkTriangle.Triangle.Plane.Distance(p.Point) > GapConstants.Resolver).ToArray();
                var kinkPoints2 = kinkPoints.Select(k => k.Id).ToArray();
                //var adjacents = kinkTriangle.PositionTriangle.AllAdjacents.Select(a => a.Id).ToArray();
                //var adjacents2 = intermeshTriangles.Where(t => adjacents.Any(a => a == t.PositionTriangle.Id)).ToArray();
                //var adjacentsWithKink = adjacents2.Where(a => a.Segments.Points().Any(k => kinkPoints2.Any(kk => kk == k.Id))).ToArray();
                //var adjacentsWithKink2 = adjacents2.Where(a => a.Segments.Points().Any(k => kinkPoints2.Any(kk => kk == k.Id))).Select(t => (Triangle: t, KinkPoints: kinkPoints.Where(k => t.Segments.Points().Any(pp => pp.Id == k.Id)))).ToArray();

                BaseObjects.Console.WriteLine($"Kink triangle {kinkTriangle.Id}  {kinkTriangle.CoplanarDeviation.ToString("E3")} Kink points {string.Join(", ", kinkPoints.Select(k => k.Id))}", ConsoleColor.Black, ConsoleColor.Yellow);
                //BaseObjects.Console.WriteLine($"Kink triangle {kinkTriangle.Id}  {kinkTriangle.CoplanarDeviation.ToString("E3")} Angle {kinkTriangle.Triangle.Plane.AngleFromSurface(s.Segment.Segment).ConvertToDegrees().ToString("##0")}", ConsoleColor.White, ConsoleColor.Red);
                //BaseObjects.Console.WriteLine($"Base Triangle {kinkTriangle.Id}");

                //var kinkPointTable = new Dictionary<int, IntermeshPoint>();
                //var basePointTable = new Dictionary<int, (IntermeshTriangle Triangle, IntermeshPoint[] Bases)>();
                var segmentsToMoveTable = new Dictionary<int, List<(IntermeshTriangle Triangle, IntermeshSegment Segment)>>();

                foreach (var kinkPoint in kinkPoints)
                {
                    //kinkPointTable[kinkPoint.Id] = kinkPoint;
                    var containingSegments = kinkTriangle.Segments.Where(s => s.Key.Indicies.Any(i => i == kinkPoint.Id)).DistinctBy(s => s.Key, Combination2Comparer.Comparer).Select(s => (Triangle: kinkTriangle, Segment: s));
                    segmentsToMoveTable[kinkPoint.Id] = [.. containingSegments];
                    //BaseObjects.Console.WriteLine($"    Kink point {kinkPoint.Id}\n{string.Join("\n", containingSegments.Select(s => $"        {s.Segment.Key} {s.Segment.Segment.Length.ToString("E3")} " +
                    //    $"[{kinkTriangle.Triangle.Plane.Distance(s.Segment.KeyPointA.Point).ToString("E3")}, {kinkTriangle.Triangle.Plane.Distance(s.Segment.KeyPointB.Point).ToString("E3")}] " +
                    //    $"Angle {kinkTriangle.Triangle.Plane.AngleFromSurface(s.Segment.Segment).ConvertToDegrees().ToString("##0")}"))}");
                }
                //foreach (var adjacentWithKink in adjacentsWithKink2)
                //{
                //    BaseObjects.Console.WriteLine($"Next Triangle {adjacentWithKink.Triangle.Id}");
                //    foreach (var kinkPoint in adjacentWithKink.KinkPoints)
                //    {
                //        //var containingSegments0 = adjacentWithKink.Triangle.Segments.Where(s => s.Key.Indicies.Any(i => i == kinkPoint.Id));
                //        var containingSegments = adjacentWithKink.Triangle.Segments.Where(s => s.Key.Indicies.Any(i => i == kinkPoint.Id)).DistinctBy(s => s.Key, Combination2Comparer.Comparer).Select(s => (Triangle: adjacentWithKink.Triangle, Segment: s));
                //        segmentsToMoveTable[kinkPoint.Id].AddRange(containingSegments);
                //        BaseObjects.Console.WriteLine($"    Kink point {kinkPoint.Id}\n{string.Join("\n", containingSegments.Select(s => $"        {s.Segment.Key} {s.Segment.Segment.Length.ToString("E3")} " +
                //        $"[{adjacentWithKink.Triangle.Triangle.Plane.Distance(s.Segment.KeyPointA.Point).ToString("E3")}, {adjacentWithKink.Triangle.Triangle.Plane.Distance(s.Segment.KeyPointB.Point).ToString("E3")}] " +
                //        $"Angle {kinkTriangle.Triangle.Plane.AngleFromSurface(s.Segment.Segment).ConvertToDegrees().ToString("##0")}"))}");
                //    }
                //}

                //foreach (var adjacentWithKink in adjacentsWithKink)
                //{
                //    foreach (var kinkPoint in kinkPoints)
                //    {
                //        var kinkSegments = segmentsToMoveTable[kinkPoint.Id].Where(s => s.Triangle.Id == kinkTriangle.Id);
                //        var potentialBasePoints = kinkSegments.Select(k => k.Segment).Points().Where(p => p.Id != kinkPoint.Id).ToArray();
                //        var basePoints = potentialBasePoints.Where(p => adjacentWithKink.Segments.Points().Any(pp => p.Id == pp.Id)).ToArray();
                //        basePointTable[kinkPoint.Id] = (Triangle: adjacentWithKink, Bases: basePoints);
                //    }
                //}
                foreach (var kinkPoint in kinkPoints)
                {
                    var segments = segmentsToMoveTable[kinkPoint.Id].Where(s => s.Triangle.Id == kinkTriangle.Id).Select(k => k.Segment);
                    var angles = segments.Select(s => kinkTriangle.Triangle.Plane.AngleFromSurface(s.Segment)).ToArray();
                    if (BasicObjects.Math.Math.Max(angles) < 0.5) { continue; }
                    //var bases = basePointTable[kinkPoint.Id];
                    var endPoints = segments.Points().Where(p => p.Id != kinkPoint.Id).ToArray();
                    if (endPoints.Length != 2) { continue; }
                    var slots = kinkTriangle.EdgeSlots.Where(s => s.Segments.Points().Any(p => p.Id == kinkPoint.Id)).ToArray();
                    foreach (var slot in slots)
                    {
                        foreach (var removal in segments)
                        {
                            slot.Segments.Remove(removal);
                        }
                        slot.Segments.Add(new IntermeshSegment(endPoints[0], endPoints[1]));
                        BaseObjects.Console.WriteLine($"Kink point {kinkPoint.Id} removed from slot {slot.Id} in triangle {kinkTriangle.Id}", ConsoleColor.Yellow);
                    }
                }
            }
        }
    }
}
