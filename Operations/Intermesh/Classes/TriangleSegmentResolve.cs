using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
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
            var segments = intermeshTriangles.SelectMany(t => t.Segments.SelectMany(i => i.Segments)).DistinctBy(s => s.Id).ToArray();
            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments.SelectMany(i => i.Segments)).DistinctBy(s => s.Id).ToArray();

            WavefrontFile.Export(intersections.Select(s => s.Segment), $"Wavefront/Intersections");

            var pairs = BuildPairsTable(segments);

            BaseObjects.Console.WriteLine($"Intersections {intersections.Count()} Segments {segments.Count()}\nSegment contact pairs {pairs.Count}\n", ConsoleColor.Magenta);

            ShortSegmentReplacements(segments, ref pairs);
            NearParallelReplacements(segments, ref pairs);

            var unresolvedPairs = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            var unresolvedNearInlinePairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsNearInLineParallel(u.Value)).ToArray();
            var unresolvedCrossPairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsCross(u.Value)).ToArray();
            BaseObjects.Console.WriteLine($"\nUnresolved pairs {unresolvedPairs.Count()} Unresolved near inline pairs {unresolvedNearInlinePairs.Count()} Unresolved cross pairs {unresolvedCrossPairs.Count()}", ConsoleColor.Magenta);
            int index = 0;
            foreach (var unresolvedPair in unresolvedPairs)
            {
                var segment1 = unresolvedPair.Value.Item1.Segment;
                var segment2 = unresolvedPair.Value.Item2.Segment;
                var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
                var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);
                var color = ConsoleColor.Magenta;
                if (inLine && isCross)
                {
                    color = ConsoleColor.Yellow;
                }
                if (!inLine && !isCross)
                {
                    color = ConsoleColor.Red;
                }
                BaseObjects.Console.WriteLine($"{unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2} Inline: {inLine} Cross: {isCross} Distance: {LineSegment3D.Distance(segment1, segment2)}", color);
                if(index == 0) Operations.Diagnostics.Intermesh.IntermeshSegment.Dump([unresolvedPair.Value.Item1, unresolvedPair.Value.Item2], unresolvedPair.Value.Item1.A.Point, 1e0);
                index++;
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment resolve. Segments {segments.Length} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
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

        private static IntermeshSegment[] _allSegments;
        private static IntermeshSegment[] _allInvolvedSegments;

        private static void SetSegmentstoDisplay(IntermeshSegment[] segments)
        {
            _allSegments = segments;
            var allContacts = _allSegments.SelectMany(s => s.Contacts.Where(c => !c.IsRemoved)).DistinctBy(s => s.Id).ToArray();
            _allInvolvedSegments = allContacts.UnionBy(_allSegments, s => s.Id).ToArray();
        }

        private static void ShortSegmentReplacements(IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            var bucket = new BoxBucket<IntermeshSegment>(segments);
            IntermeshSegment[] shortSegments;
            bool shortSegmentsRemoved = false;

            //SetSegmentstoDisplay(segments.Where(s => s.Segment.Length < 1e-9 && !s.IsRemoved).ToArray());
            //BaseObjects.Console.WriteLine($"All involved segbments {_allInvolvedSegments.Count()}", ConsoleColor.Magenta);
            //if(allSegments.Any()) Operations.Diagnostics.Intermesh.IntermeshSegment.Dump(allInvolvedSegments, allSegments.First().A.Point, 1e8);

            while (true)
            {
                shortSegments = segments.Where(s => s.Segment.Length < 1e-9 && !s.IsRemoved).ToArray();
                if (!shortSegments.Any()) { break; }

                foreach (var segment in shortSegments)
                {
                    BaseObjects.Console.WriteLine($"     {segment.ToString()} {segment.Segment.Length} Contacts {segment.Contacts.Count(c => !c.IsRemoved)} [{string.Join(",", segment.Contacts.Where(c => !c.IsRemoved).Select(c => c.Id))}]", ConsoleColor.Magenta);
                }

                foreach (var shortSegment in shortSegments.NonAdjoining())
                {
                    var transferSet = bucket.TransferSet(shortSegment);

                    foreach (var removal in transferSet.RemoveFrom.Links)
                    {
                        removal.ReplaceWith(
                            transferSet.RemoveFrom.ReplaceRemovalPoint(removal.A, transferSet.AddTo.Point),
                            transferSet.RemoveFrom.ReplaceRemovalPoint(removal.B, transferSet.AddTo.Point));
                    }

                    foreach (var segment in transferSet.AddTo.Links)
                    {
                        segment.AddRangeContacts(shortSegment.Contacts);
                    }

                    shortSegment.Remove();
                    shortSegment.ReplacementStatus = IntermeshSegment.ReplacementType.ShortSegment;
                    shortSegmentsRemoved = true;
                }

                BaseObjects.Console.WriteLine($"Short segments replaced {shortSegments.NonAdjoining().Count()}", ConsoleColor.Magenta);
            }
            if (shortSegmentsRemoved)
            {
                pairs = BuildPairsTable(segments);
            }
            BaseObjects.Console.WriteLine($"Segment contact pairs {pairs.Count}", ConsoleColor.Magenta);

            //if (allSegments.Any()) Operations.Diagnostics.Intermesh.IntermeshSegment.Dump(allInvolvedSegments.Where(c => !c.IsRemoved), allSegments.First().A.Point, 1e8);
        }

        private static ((IntermeshPoint Point, IEnumerable<IntermeshSegment> Links) AddTo,
            (IntermeshPoint Point, IEnumerable<IntermeshSegment> Links) RemoveFrom)
            TransferSet(this BoxBucket<IntermeshSegment> bucket, IntermeshSegment segment)
        {
            var linkingA = bucket.LinkingSegments(segment.A).Where(l => l.Id != segment.Id).ToArray();
            var linkingB = bucket.LinkingSegments(segment.B).Where(l => l.Id != segment.Id).ToArray();

            return (segment.MaximumLinkPoint(linkingA, linkingB), segment.MinimumLinkPoint(linkingA, linkingB));
        }

        private static IEnumerable<IntermeshSegment> LinkingSegments(this BoxBucket<IntermeshSegment> bucket, IntermeshPoint point)
        {
            return bucket.Fetch(point, 1e-5).Where(p => !p.IsRemoved && (p.A.Id == point.Id || p.B.Id == point.Id));
        }

        private static (IntermeshPoint Point, IEnumerable<IntermeshSegment> Links) MaximumLinkPoint(this IntermeshSegment s, IEnumerable<IntermeshSegment> a, IEnumerable<IntermeshSegment> b)
        {
            return a.Count() > b.Count() ? (s.A, a) : (s.B, b);
        }

        private static (IntermeshPoint Point, IEnumerable<IntermeshSegment> Links) MinimumLinkPoint(this IntermeshSegment s, IEnumerable<IntermeshSegment> a, IEnumerable<IntermeshSegment> b)
        {
            return a.Count() > b.Count() ? (s.B, b) : (s.A, a);
        }

        private static IntermeshPoint ReplaceRemovalPoint(this (IntermeshPoint Point, IEnumerable<IntermeshSegment> Links) pointLinks, IntermeshPoint removalPoint, IntermeshPoint addPoint)
        {
            return pointLinks.Point.Id == removalPoint.Id ? addPoint : removalPoint;
        }

        private static void NearParallelReplacements(IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            bool nearParallelRemoved = false;
            var nearParallelPairs = pairs.Where(p => IntermeshSegmentExtensions.IsNearParallel(p.Value)).ToArray();

            BaseObjects.Console.WriteLine($"Near parallel pairs {nearParallelPairs.Count()}", ConsoleColor.Magenta);

            foreach (var pair in nearParallelPairs.Select(p => p.Value))
            {
                BaseObjects.Console.WriteLine($"     {pair.Item1} Contacts: {pair.Item1.Contacts.Count(c => !c.IsRemoved)} {pair.Item2} Contacts: {pair.Item2.Contacts.Count(c => !c.IsRemoved)}  Length: {pair.Item1.Segment.Length} Distance: {LineSegment3D.Distance(pair.Item1.Segment, pair.Item2.Segment)}", ConsoleColor.Magenta);
            }

            foreach (var pair in nearParallelPairs.Select(p => p.Value))
            {
                var toRemove = pair.Item1;
                var toAddTo = pair.Item2;
                if (pair.Item1.Contacts.Count > pair.Item2.Contacts.Count) { toRemove = pair.Item2; toAddTo = pair.Item1; }
                toAddTo.AddRangeContacts(toRemove.Contacts.Where(c => !c.IsRemoved));
                nearParallelRemoved = true;
                toRemove.Remove();
                toRemove.ReplacementStatus = IntermeshSegment.ReplacementType.NearParallelSegment;
                BaseObjects.Console.WriteLine($"Near parallel removed: {toRemove} added: {toAddTo} Contacts: {toAddTo.Contacts.Count(c => !c.IsRemoved)}", ConsoleColor.Magenta);
            }

            BaseObjects.Console.WriteLine($"Near parallel segments removed {segments.Count(s => s.ReplacementStatus == IntermeshSegment.ReplacementType.NearParallelSegment)}", ConsoleColor.Magenta);

            if (nearParallelRemoved)
            {
                pairs = BuildPairsTable(segments);
            }
            BaseObjects.Console.WriteLine($"Segment contact pairs {pairs.Count}", ConsoleColor.Magenta);

            //if (_allSegments.Any()) Operations.Diagnostics.Intermesh.IntermeshSegment.Dump(_allInvolvedSegments.Where(c => !c.IsRemoved), _allSegments.First().A.Point, 1e8);
        }



        private static void VertexReassigns(IntermeshSegment[] segments)
        {
            //var elements = segments.Where(s => s.IsRemoved && (s.VertexAContacts.Any() || s.VertexBContacts.Any()));

            //foreach (var element in elements)
            //{
            //    var distanceAA = Point3D.Distance(element.A.Point, element.ReplacedBy.A.Point);
            //    var distanceAB = Point3D.Distance(element.A.Point, element.ReplacedBy.B.Point);
            //    var distanceBA = Point3D.Distance(element.B.Point, element.ReplacedBy.A.Point);
            //    var distanceBB = Point3D.Distance(element.B.Point, element.ReplacedBy.B.Point);

            //    var isReversedA = distanceAA > distanceAB;
            //    var isReversedB = distanceBA < distanceBB;
            //    if (isReversedA != isReversedB) throw new InvalidOperationException($"Parity between {element.Id} and {element.ReplacedBy.Id} can not be determined.");

            //    VertexReassigns(element, isReversedA);

            //    BaseObjects.Console.WriteLine($"Reassign verticies A [{string.Join(",", element.ReplacedBy.VertexAContacts.Select(c => c.Id))}], verticies B [{string.Join(",", element.ReplacedBy.VertexBContacts.Select(c => c.Id))}] to segment {element.Id}  Reversed A {isReversedA} Reversed B {isReversedB}", ConsoleColor.Cyan);
            //}

        }

        private static void VertexReassigns(IntermeshSegment wasRemoved, bool isReversed)
        {
            //var assignToA = isReversed ? wasRemoved.VertexBContacts : wasRemoved.VertexAContacts;
            //var assignToB = isReversed ? wasRemoved.VertexAContacts : wasRemoved.VertexBContacts;

            //foreach (var a in assignToA.Where(s => !s.IsRemoved))
            //{
            //    if (wasRemoved.A.Id == a.A.Id)
            //    {
            //        var capsule = a.Capsules.Single();
            //        a.Capsules.Clear();
            //        a.Capsules.Add(IntermeshCapsuleExtensions.Fetch(wasRemoved.ReplacedBy.A, capsule.B));
            //    }
            //    else
            //    {
            //        var capsule = a.Capsules.Single();
            //        a.Capsules.Clear();
            //        a.Capsules.Add(IntermeshCapsuleExtensions.Fetch(capsule.A, wasRemoved.ReplacedBy.B));
            //    }
            //    wasRemoved.ReplacedBy.AddVertexAContact(a);
            //}

            //foreach (var b in assignToB.Where(s => !s.IsRemoved))
            //{
            //    if (wasRemoved.B.Id == b.A.Id)
            //    {
            //        var capsule = b.Capsules.Single();
            //        b.Capsules.Clear();
            //        b.Capsules.Add(IntermeshCapsuleExtensions.Fetch(wasRemoved.ReplacedBy.A, capsule.B));
            //    }
            //    else
            //    {
            //        var capsule = b.Capsules.Single();
            //        b.Capsules.Clear();
            //        b.Capsules.Add(IntermeshCapsuleExtensions.Fetch(capsule.A, wasRemoved.ReplacedBy.B));
            //    }
            //    wasRemoved.ReplacedBy.AddVertexBContact(b);
            //}
        }

        private static void InteriorNearParallelAssigns(IntermeshSegment[] segments)
        {
            int count = 0;
            foreach (var segment in segments.Where(s => !s.IsRemoved))
            {
                //foreach (var contact in segment.ContactsWithRemovedRecursion(c => c.LocalContacts).Where(c => !c.IsRemoved && c.Capsules.Any(c => segment.Capsules.Any(s => s.IsInteriorNearParallel(c)))))
                foreach (var contact in segment.Contacts.Where(c => !c.IsRemoved && c.Capsules.Any(c => segment.Capsules.Any(s => s.IsInteriorNearParallel(c)))))
                {
                    BaseObjects.Console.WriteLine($"Segment {segment.Id} Contact {contact.Id} has interior near parallels.", ConsoleColor.Magenta);
                    count++;

                    var contactSplit = contact.Capsules.SplitBy(segment.Capsules);
                    var segmentSplit = segment.Capsules.SplitBy(contact.Capsules);

                    BaseObjects.Console.WriteLine($"Contact Splits {contactSplit.Count()} Short lengths {contactSplit.Count(c => c.Segment.Length < 1e-9)}", ConsoleColor.Magenta);
                    BaseObjects.Console.WriteLine($"Segment Splits {segmentSplit.Count()} Short lengths {segmentSplit.Count(c => c.Segment.Length < 1e-9)}", ConsoleColor.Magenta);

                    segment.Capsules = segmentSplit.ToList();
                    contact.Capsules = contactSplit.ToList();
                }
            }
            BaseObjects.Console.WriteLine($"Interior near parallels {count}.", ConsoleColor.Magenta);
        }

        private static void CrossReplacements(IntermeshSegment[] segments)
        {
            //var allContacts = segments.Select(s => (Segment: s, Contacts: s.ContactsWithRemovedRecursion(c => c.LocalContacts))).Where(p => !p.Segment.IsRemoved && p.Contacts.Any(c => !c.IsRemoved)).ToList();
            //var nearParallels = segments.Select(s => (Segment: s, Contacts: s.ContactsWithRemovedRecursion(c => c.LocalContacts)
            //        .Where(c => IntermeshSegmentExtensions.IsNearParallel(s, c)))).Where(p => !p.Segment.IsRemoved && p.Contacts.Any(c => !c.IsRemoved)).ToList();

            //var crosses = segments.Select(s => (Segment: s, Contacts: s.ContactsWithRemovedRecursion(c => c.LocalContacts)
            //    .Where(c => IntermeshSegmentExtensions.IsCross(s, c)))).Where(p => !p.Segment.IsRemoved && p.Contacts.Any(c => !c.IsRemoved)).ToList();
            //var leftOver = allContacts.ExceptBy(nearParallels.Select(e => e.Segment.Id), e => e.Segment.Id).ExceptBy(crosses.Select(e => e.Segment.Id), e => e.Segment.Id);

            //BaseObjects.Console.WriteLine($"All contacts {allContacts.Count()} Near parallel contacts {nearParallels.Count()} Cross contacts {crosses.Count()} Left overs {leftOver.Count()}", ConsoleColor.Magenta);

            int count = 0;
            int count2 = 0;
            int success = 0;
            int failure = 0;
            foreach (var segment in segments.Where(s => !s.IsRemoved))
            {
                bool isNew = true;
                //foreach (var contact in segment.ContactsWithRemovedRecursion(c => c.LocalContacts).Where(c => !c.IsRemoved && IntermeshSegmentExtensions.IsCross(segment, c)))
                foreach (var contact in segment.Contacts.Where(c => !c.IsRemoved && IntermeshSegmentExtensions.IsCross(segment, c)))
                {
                    //BaseObjects.Console.WriteLine($"Segment {segment.Id} Contact {contact.Id} has interior near parallels.", ConsoleColor.Magenta);
                    count++;
                    if (isNew) { count2++; isNew = false; }

                    var intersection = Line3D.Intersection(segment.Segment.LineExtension, contact.Segment.LineExtension);
                    if (intersection is null) { BaseObjects.Console.WriteLine($"Intersection is null", ConsoleColor.Red); continue; }
                    if (intersection.Length > 1e-9) { BaseObjects.Console.WriteLine($"Intersection is long at {intersection.Length}", ConsoleColor.Blue); }

                    var point = IntermeshPointExtensions.Fetch(intersection.Center);

                    var contactSplit = contact.Capsules.SplitBy(point).ToList();
                    var segmentSplit = segment.Capsules.SplitBy(point).ToList();

                    if (contact.Capsules.Count() < contactSplit.Count && segment.Capsules.Count < segmentSplit.Count)
                    {
                        success++;
                    }
                    else
                    {
                        failure++;
                    }

                    //BaseObjects.Console.WriteLine($"Contact Splits {contactSplit.Count()} Short lengths {contactSplit.Count(c => c.Segment.Length < 1e-9)}", ConsoleColor.Magenta);
                    //BaseObjects.Console.WriteLine($"Segment Splits {segmentSplit.Count()} Short lengths {segmentSplit.Count(c => c.Segment.Length < 1e-9)}", ConsoleColor.Magenta);

                    //segment.Capsules = segmentSplit.ToList();
                    //contact.Capsules = contactSplit.ToList();
                }
            }
            if (failure > 100)
            {

            }
            BaseObjects.Console.WriteLine($"Cross success: {success} failure {failure}", ConsoleColor.Magenta);
            BaseObjects.Console.WriteLine($"All Segment {segments.Count(s => !s.IsRemoved)} Interior crosses Segments {count2} Contacts {count}.", ConsoleColor.Magenta);

        }
    }
}
