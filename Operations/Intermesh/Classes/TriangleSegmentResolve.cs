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

namespace Operations.Intermesh.Classes
{
    internal static class TriangleSegmentResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            var segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            var intersections = intermeshTriangles.SelectMany(t => t.IntersectionSegments).DistinctBy(s => s.Id).ToArray();
            if (!intersections.Any()) return;
            //WavefrontFile.Export(intersections.Select(s => s.Segment), $"Wavefront/Intersections");

            //segments.PointCountDisplay();
            //segments.ShowSingleSegmentPoints();

            var pairs = BuildPairsTable(segments);

            //BaseObjects.Console.WriteLine($"\nIntersections {intersections.Count()} Segments {segments.Count()}\nSegment contact pairs {pairs.Count}", ConsoleColor.Cyan);
            var unresolvedPairsBefore = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            ResolveCycle(segments, ref pairs);

            //SegmentReplacements(intermeshTriangles);
            //segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            //pairs = BuildPairsTable(segments);
            //ResolveCycle(segments, ref pairs);
            //SegmentReplacements(intermeshTriangles);
            //segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            //pairs = BuildPairsTable(segments);
            //ResolveCycle(segments, ref pairs);

            var unresolvedPairsAfter = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            //BaseObjects.Console.WriteLine($"Unresolved pairs {unresolvedPairs.Count()} ", ConsoleColor.Cyan, unresolvedPairs.Any() ? ConsoleColor.DarkRed : System.Console.BackgroundColor);
            foreach (var unresolvedPair in unresolvedPairsAfter)
            {
                var segment1 = unresolvedPair.Value.Item1.Segment;
                var segment2 = unresolvedPair.Value.Item2.Segment;
                BaseObjects.Console.WriteLine($"{unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2} Distance: {LineSegment3D.Distance(segment1, segment2)}", ConsoleColor.Magenta);
                var magnification = 1e3;
                IntermeshSegmentDiagnostics.Dump([unresolvedPair.Value.Item1, unresolvedPair.Value.Item2], unresolvedPair.Value.Item1.A.Point, magnification, $"-Unresolved-{unresolvedPair.Value.Item1.Key}-{unresolvedPair.Value.Item2.Key}");
            }
            //var shortCapsules = segments.SelectMany(s => s.Capsules).Where(c => c.Segment.Length < 1e-9);
            //if(shortCapsules.Any()) BaseObjects.Console.WriteLine($"Short capsules {shortCapsules.Count()}", ConsoleColor.Cyan);

            BaseObjects.Console.WriteLine($"Was unresolved: {unresolvedPairsBefore.Count()} WasChanged: {segments.Count(s => s.WasChanged)}  Left unresolved: {unresolvedPairsAfter.Count()} ", 
                !unresolvedPairsBefore.Any() ? ConsoleColor.Gray : (!unresolvedPairsAfter.Any() ? ConsoleColor.Green : ConsoleColor.White), 
                !unresolvedPairsAfter.Any() ? System.Console.BackgroundColor : ConsoleColor.Red);

            //if(unresolvedPairs.Any()) segments.PointCountDisplay();
            //if (unresolvedPairs.Any()) segments.ShowSingleSegmentPoints();
            SegmentReplacements(intermeshTriangles);

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

        private static void ResolveCycle(IntermeshSegment[] segments, ref Combination2Dictionary<(IntermeshSegment, IntermeshSegment)> pairs)
        {
            ShortSegmentReplacements(segments, ref pairs);
            NearParallelReplacements(segments, ref pairs);

            var unresolvedPairs = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            var unresolvedNearInlinePairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsNearInLineParallel(u.Value)).ToArray();
            var unresolvedCrossPairs = unresolvedPairs.Where(u => IntermeshSegmentExtensions.IsCross(u.Value)).ToArray();
            //BaseObjects.Console.WriteLine($"Unresolved pairs {unresolvedPairs.Count()} Unresolved near inline pairs {unresolvedNearInlinePairs.Count()} Unresolved cross pairs {unresolvedCrossPairs.Count()}", ConsoleColor.Cyan);
            int index = 0;

            var bucket = new BoxBucket<IntermeshSegment>(segments);

            foreach (var unresolvedPair in unresolvedPairs)
            {
                var segment1 = unresolvedPair.Value.Item1.Segment;
                var segment2 = unresolvedPair.Value.Item2.Segment;
                var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
                var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);

                if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value, bucket, segments); else GapResolve(unresolvedPair.Value, bucket);

                index++;
            }
        }

        private static void SegmentReplacements(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var slots = intermeshTriangles.SelectMany(t => t.EdgeSlots).DistinctBy(s => s.Id).ToArray();
            //BaseObjects.Console.WriteLine($"Segment slots: {slots.Count()}", ConsoleColor.Cyan);

            var replacements = slots.Where(s => s.Segments.Any(ss => ss.Capsules.Count() != 1));
            //BaseObjects.Console.WriteLine($"Replacements: {replacements.Count()}", ConsoleColor.Cyan);

            var segmentTable = new Combination2Dictionary<IntermeshSegment>();

            foreach (var replacement in replacements)
            {
                var segments = new List<IntermeshSegment>();
                foreach (var element in replacement.Segments.Where(ss => ss.Capsules.Any()))
                {
                    if (element.Capsules.Count() == 1) { segments.Add(element); continue; }
                    foreach (var capsule in element.Capsules)
                    {
                        segments.Add(FetchSegment(capsule, segmentTable));
                    }
                }

                replacement.Segments = segments;
            }

            //var allSegments = slots.SelectMany(s => s.Segments).ToArray();
            //BaseObjects.Console.WriteLine($"Segment/Capsule counts", ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine(allSegments.GroupCounts(s => s.Capsules.Count()).DisplayByLine(), ConsoleColor.Cyan);
            //BaseObjects.Console.WriteLine($"Replacements finished.", ConsoleColor.Cyan);
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

            shortSegments = segments.Where(s => s.Segment.Length < 1e-9 && !s.IsRemoved).ToArray();
            //BaseObjects.Console.WriteLine($"Short segments {shortSegments.Count()}", ConsoleColor.Cyan);
            while (true)
            {
                shortSegments = segments.Where(s => s.Segment.Length < 1e-9 && !s.IsRemoved).ToArray();
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
        private static void CrossResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket, IEnumerable<IntermeshSegment> segments)
        {
            var intersection = LineSegment3D.PointIntersection(unresolvedPair.Item1.Segment, unresolvedPair.Item2.Segment, 1e-15);
            if (intersection is not null)
            {
                CrossWithIntersectionResolve(unresolvedPair, intersection, bucket, segments);
            }
            else
            {
                CrossWithoutIntersectionResolve(unresolvedPair, bucket);
            }
        }
        private static void CrossWithoutIntersectionResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

            var extensionA = LineSegment3D.SegmentExtensionToLine(unresolvedPair.Item1.Segment, unresolvedPair.Item2.Segment, 1e-15);
            var extensionB = LineSegment3D.SegmentExtensionToLine(unresolvedPair.Item2.Segment, unresolvedPair.Item1.Segment, 1e-15);
            if (extensionA is not null && extensionA.Length < linkSegment.Distance)
            {
                //BaseObjects.Console.WriteLine($"Extension A split {unresolvedPair.Item1} {unresolvedPair.Item2}  {extensionA.Length}", ConsoleColor.Red);
                var to = IntermeshPointExtensions.Fetch(extensionA.Start);
                var from = IntermeshPointExtensions.Fetch(extensionA.End);

                var points = unresolvedPair.Item1.Capsules.SelectMany(c => c.Points);
                var nearestPoint = points.NearestPoint(to);
                if (Point3D.Distance(nearestPoint.Point, to.Point) < 1e-9) { to = nearestPoint; } else { unresolvedPair.Item1.SplitBy(to); }

                bucket.PointTransferFromTo(from, to);

                //var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
                //BaseObjects.Console.WriteLine($"Is resolved: {isResolved}  Short capsules {unresolvedPair.Item1.Capsules.Count(c => c.Segment.Length < 1e-9)}");

                //var magnification = 1e10;
                //Operations.Diagnostics.IntermeshSegmentDiagnostics.Dump([unresolvedPair.Item1, unresolvedPair.Item2], unresolvedPair.Item1.A.Point, magnification, $"-Extension-A-{unresolvedPair.Item1.Key}-{unresolvedPair.Item2.Key}");

            }
            else if (extensionB is not null && extensionB.Length < linkSegment.Distance)
            {
                //BaseObjects.Console.WriteLine($"Extension B split {unresolvedPair.Item1} {unresolvedPair.Item2}  {extensionB.Length}", ConsoleColor.Red);
                var to = IntermeshPointExtensions.Fetch(extensionB.Start);
                var from = IntermeshPointExtensions.Fetch(extensionB.End);

                var points = unresolvedPair.Item2.Capsules.SelectMany(c => c.Points);
                var nearestPoint = points.NearestPoint(to);
                if (Point3D.Distance(nearestPoint.Point, to.Point) < 1e-9) { to = nearestPoint; } else { unresolvedPair.Item2.SplitBy(to); }
                bucket.PointTransferFromTo(from, to);

                //var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
                //BaseObjects.Console.WriteLine($"Is resolved: {isResolved}  Short capsules {unresolvedPair.Item2.Capsules.Count(c => c.Segment.Length < 1e-9)}");

                //var magnification = 1e10;
                //Operations.Diagnostics.IntermeshSegmentDiagnostics.Dump([unresolvedPair.Item1, unresolvedPair.Item2], unresolvedPair.Item1.A.Point, magnification, $"-Extension-B-{unresolvedPair.Item1.Key}-{unresolvedPair.Item2.Key}");
            }
            else
            {
                var linksA = bucket.LinkingSegments(linkSegment.A).Count();
                var linksB = bucket.LinkingSegments(linkSegment.B).Count();
                var from = (linksA > linksB) || (linksA == linksB && linkSegment.A.Id > linkSegment.B.Id) ? linkSegment.A : linkSegment.B;
                var to = (linksA < linksB) || (linksA == linksB && linkSegment.A.Id < linkSegment.B.Id) ? linkSegment.A : linkSegment.B;
                bucket.PointTransferFromTo(from, to);
            }
        }
        private static void CrossWithIntersectionResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, Point3D intersection, BoxBucket<IntermeshSegment> bucket, IEnumerable<IntermeshSegment> segments)
        {
            var distanceAA = Point3D.Distance(intersection, unresolvedPair.Item1.A.Point);
            var distanceAB = Point3D.Distance(intersection, unresolvedPair.Item1.B.Point);
            var distanceBA = Point3D.Distance(intersection, unresolvedPair.Item2.A.Point);
            var distanceBB = Point3D.Distance(intersection, unresolvedPair.Item2.B.Point);

            var code = (distanceBB < 1e-9 ? 8 : 0) | (distanceBA < 1e-9 ? 4 : 0) | (distanceAB < 1e-9 ? 2 : 0) | (distanceAA < 1e-9 ? 1 : 0);
            switch (code)
            {
                case 0:
                    {
                        var point = IntermeshPointExtensions.Fetch(intersection);
                        unresolvedPair.Item1.SplitBy(point);
                        unresolvedPair.Item2.SplitBy(point);
                    }
                    break;
                case 1:
                    {
                        //unresolvedPair.Value.Item2.SplitBy(unresolvedPair.Value.Item1.A);
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        unresolvedPair.Item2.SplitBy(to);
                        var from = unresolvedPair.Item1.A;
                        bucket.PointTransferFromTo(from, to);
                    }
                    break;
                case 2:
                    {
                        //unresolvedPair.Value.Item2.SplitBy(unresolvedPair.Value.Item1.B);
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        unresolvedPair.Item2.SplitBy(to);
                        var from = unresolvedPair.Item1.B;
                        bucket.PointTransferFromTo(from, to);
                    }
                    break;
                case 3:
                    {
                        // Two ...
                        BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
                    }
                    break;
                case 4:
                    {
                        //unresolvedPair.Value.Item1.SplitBy(unresolvedPair.Value.Item2.A);
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        unresolvedPair.Item1.SplitBy(to);
                        var from = unresolvedPair.Item2.A;
                        bucket.PointTransferFromTo(from, to);
                    }
                    break;
                case 5:
                    {
                        // Two
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        bucket.PointTransferFromTo(unresolvedPair.Item1.A, to);
                        bucket.PointTransferFromTo(unresolvedPair.Item2.A, to);
                    }
                    break;
                case 6:
                    {
                        // Two
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        bucket.PointTransferFromTo(unresolvedPair.Item1.B, to);
                        bucket.PointTransferFromTo(unresolvedPair.Item2.A, to);
                    }
                    break;
                case 7:
                    BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
                    break;
                case 8:
                    {
                        //unresolvedPair.Value.Item1.SplitBy(unresolvedPair.Value.Item2.B);
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        unresolvedPair.Item1.SplitBy(to);
                        var from = unresolvedPair.Item2.B;
                        bucket.PointTransferFromTo(from, to);
                    }
                    break;
                case 9:
                    {
                        // Two
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        bucket.PointTransferFromTo(unresolvedPair.Item1.A, to);
                        bucket.PointTransferFromTo(unresolvedPair.Item2.B, to);
                    }
                    break;
                case 10:
                    {
                        // Two
                        var to = IntermeshPointExtensions.Fetch(intersection);
                        bucket.PointTransferFromTo(unresolvedPair.Item1.B, to);
                        bucket.PointTransferFromTo(unresolvedPair.Item2.B, to);
                    }
                    break;
                case 11:
                    BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
                    break;
                case 12:
                    {
                        // Two ...
                        BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
                    }
                    break;
                case 13: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
                case 14: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
                case 15: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
            }
        }

        private static void GapResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));
            var linksA = bucket.LinkingSegments(linkSegment.A).Count();
            var linksB = bucket.LinkingSegments(linkSegment.B).Count();
            var from = (linksA > linksB) || (linksA == linksB && linkSegment.A.Id > linkSegment.B.Id) ? linkSegment.A : linkSegment.B;
            var to = (linksA < linksB) || (linksA == linksB && linkSegment.A.Id < linkSegment.B.Id) ? linkSegment.A : linkSegment.B;
            bucket.PointTransferFromTo(from, to);
        }
    }
}
