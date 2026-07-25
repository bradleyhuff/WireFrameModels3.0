using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal static class TriangleSegmentResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments).DistinctBy(s => s.Id).ToArray();
            if (!intersections.Any()) return;
            while (ResolveCycle(intermeshTriangles));

            InlineSlotSegmentReplacements(intermeshTriangles);
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

            ShortSegmentReplacements(intermeshTriangles, segments, ref pairs);
            NearParallelReplacements(intermeshTriangles,segments, ref pairs);

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

            var wasChanged = segments.Any(s => s.WasChanged);

            SegmentReplacements(intermeshTriangles);

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
            segment1.CapsuleSplit(point);
            segment2.CapsuleSplit(point);
        }

        private static void GapResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

            unresolvedPair.Item1.ExtendWith(linkSegment.A);
            unresolvedPair.Item1.ExtendWith(linkSegment.B);
        }

        public static void InlineSlotSegmentReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            var segments = slots.SelectMany(s => s.Segments).DistinctBy(s => s.Id).ToArray();
            var points = segments.Points().ToArray();

            var pointSlotsMap = new GroupingDictionary<int, List<IntermeshEdgeSlot>>(() => new List<IntermeshEdgeSlot>());
            foreach (var slot in slots)
            {
                foreach (var point in slot.Segments.Points())
                {
                    if (!pointSlotsMap[point.Id].Any(s => s.Id == slot.Id))
                    {
                        pointSlotsMap[point.Id].Add(slot);
                    }
                }
            }

            var inLineReplacements = new List<(IEnumerable<IntermeshEdgeSlot>ReplaceIn, IntermeshSegment ToBeReplaced, IEnumerable<IntermeshSegment> ReplaceWith)>();

            foreach (var segment in segments)
            {
                var slotsA = pointSlotsMap[segment.A.Id];
                var slotsB = pointSlotsMap[segment.B.Id];

                var commonSlots = slotsA.Concat(slotsB).GroupBy(g => g.Id).Where(g => g.Count() == 2).Select(g => g.First()).ToArray();
                if(commonSlots.Count() > 1)
                {
                    var slotsWithSegment = commonSlots.Where(s => s.Segments.Any(ss => ss.Key == segment.Key));
                    var replacements = commonSlots.Where(s => !s.Segments.Any(ss => ss.Key == segment.Key));
                    foreach (var replacement in replacements.Take(1))
                    {
                        var replaceWith = replacement.Segments.Between(segment);
                        inLineReplacements.Add((slotsWithSegment, segment, replaceWith));
                    }
                }
            }

            foreach (var inlineReplacement in inLineReplacements)
            {
                //BaseObjects.Console.WriteLine($"Segment replacement: <{string.Join(",", inlineReplacement.ReplaceIn.Select(s => s.Id))}> {inlineReplacement.ToBeReplaced.Key} => [{string.Join(",", inlineReplacement.ReplaceWith.Select(s => s.Key))}]", ConsoleColor.Cyan);
                foreach (var slot in inlineReplacement.ReplaceIn)
                {
                    var list = slot.Segments;
                    int index = list.IndexOf(inlineReplacement.ToBeReplaced);
                    if (index == -1) { continue; }
                    list.RemoveAt(index);                    
                    list.InsertRange(index, inlineReplacement.ReplaceWith);
                }                
            }
            //BaseObjects.Console.WriteLine($"InLineSlotSegmentReplacements Slots: {slots.Count()} Segments: {segments.Count()} Points: {points.Count()}   Elapsed Time {(DateTime.Now - start).TotalSeconds} seconds", ConsoleColor.Cyan);
        }
    }
}
