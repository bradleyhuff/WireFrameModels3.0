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
using System.Net.Sockets;

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

            SegmentReplacements(intermeshTriangles);
            segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            pairs = BuildPairsTable(segments);
            ResolveCycle(segments, ref pairs);

            SegmentReplacements(intermeshTriangles);
            segments = intermeshTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            pairs = BuildPairsTable(segments);
            ResolveCycle(segments, ref pairs);

            //ResolveCycle(segments, ref pairs);
            //ResolveCycle(segments, ref pairs);
            //ResolveCycle(segments, ref pairs);

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
 
            //var shortCapsules = segments.SelectMany(s => s.Capsules).Where(c => c.Segment.Length < 1e-9);
            //if(shortCapsules.Any()) BaseObjects.Console.WriteLine($"Short capsules {shortCapsules.Count()}", ConsoleColor.Cyan);

            BaseObjects.Console.WriteLine($"Was unresolved: {unresolvedPairsBefore.Count()} WasChanged: {segments.Count(s => s.WasChanged)}  Left unresolved: {unresolvedPairsAfter.Count()} ", 
                !unresolvedPairsBefore.Any() ? ConsoleColor.Gray : (!unresolvedPairsAfter.Any() ? ConsoleColor.Green : ConsoleColor.White), 
                !unresolvedPairsAfter.Any() ? System.Console.BackgroundColor : ConsoleColor.Red);


            //var bucket = new BoxBucket<IntermeshSegment>(segments);
            //foreach (var unresolvedPair in unresolvedPairsAfter)
            //{
            //    var segment1 = unresolvedPair.Value.Item1.Segment;
            //    var segment2 = unresolvedPair.Value.Item2.Segment;
            //    BaseObjects.Console.WriteLine($"{unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2} Distance: {LineSegment3D.Distance(segment1, segment2)}", ConsoleColor.Red);
            //    var isResolved0 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Value);
            //    if (isResolved0) { BaseObjects.Console.WriteLine($"Resolve attempt already resolved", ConsoleColor.Yellow); continue; }
            //    //var magnification = 1e0;
            //    //IntermeshSegmentDiagnostics.Dump([unresolvedPair.Value.Item1, unresolvedPair.Value.Item2], unresolvedPair.Value.Item1.A.Point, magnification, $"-Unresolved-{unresolvedPair.Value.Item1.Key}-{unresolvedPair.Value.Item2.Key}");

            //    var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
            //    var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);

            //    if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value, bucket); else GapResolve(unresolvedPair.Value, bucket);

            //    var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Value);
            //    BaseObjects.Console.WriteLine($"Resolve attempt {isResolved}", isResolved ? ConsoleColor.Green: ConsoleColor.Red);
            //}

            //var unresolvedPairsAfter2 = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            //if (unresolvedPairsAfter2.Any()) BaseObjects.Console.WriteLine($"Resolve attempt Is resolved: {unresolvedPairsAfter2.Count()}", ConsoleColor.Red);

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
            BaseObjects.Console.WriteLine($"(1) Long pair distances {pairs.Count(p => LineSegment3D.Distance(p.Value.Item1.Segment, p.Value.Item2.Segment) > 1e-9)}");
            ShortSegmentReplacements(segments, ref pairs);
            BaseObjects.Console.WriteLine($"(2) Long pair distances {pairs.Count(p => LineSegment3D.Distance(p.Value.Item1.Segment, p.Value.Item2.Segment) > 1e-9)}");
            NearParallelReplacements(segments, ref pairs);
            BaseObjects.Console.WriteLine($"(3) Long pair distances {pairs.Count(p => LineSegment3D.Distance(p.Value.Item1.Segment, p.Value.Item2.Segment) > 1e-9)}");
            BaseObjects.Console.WriteLine($"(3B) Short segments {segments.SelectMany(s => s.Capsules).Count(c => c.Segment.Length < 1e-9)}");

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

                if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value, bucket); else GapResolve(unresolvedPair.Value, bucket);

                //var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Value);
                //if (!isResolved)
                //{
                //    BaseObjects.Console.WriteLine($"{(inLine ? "LINE" : (isCross ? "CROSS" : "GAP"))} {(isCross && withIntersection ? " INTERSECTION" : "")} Resolve failed {unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2}", ConsoleColor.Red);
                //}
                //else
                //{
                //    BaseObjects.Console.WriteLine($"{(inLine ? "LINE" : (isCross ? "CROSS" : "GAP"))} {(isCross && withIntersection ? " INTERSECTION" : "")} Resolve succeeded {unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2}", ConsoleColor.Green);
                //}
                    index++;
            }
            BaseObjects.Console.WriteLine($"(4) Long pair distances {pairs.Count(p => LineSegment3D.Distance(p.Value.Item1.Segment, p.Value.Item2.Segment) > 1e-9)}");
            BaseObjects.Console.WriteLine($"(4B) Short segments {segments.SelectMany(s => s.Capsules).Count(c => c.Segment.Length < 1e-9)}");
            //shortSegments = segments.Where(s => s.Segment.Length < 1e-9 && !s.IsRemoved).ToArray();

            //unresolvedPairs = pairs.Where(p => !IntermeshSegmentExtensions.IsResolved(p.Value)).ToArray();
            //foreach (var unresolvedPair in unresolvedPairs)
            //{
            //    var segment1 = unresolvedPair.Value.Item1.Segment;
            //    var segment2 = unresolvedPair.Value.Item2.Segment;
            //    var inLine = IntermeshSegmentExtensions.IsNearInLineParallel(unresolvedPair.Value);
            //    var isCross = IntermeshSegmentExtensions.IsCross(unresolvedPair.Value);
            //    if (inLine) InLineResolve(unresolvedPair.Value); else if (isCross) CrossResolve(unresolvedPair.Value, bucket); else GapResolve(unresolvedPair.Value, bucket);
            //    var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Value);
            //    if (!isResolved)
            //    {
            //        BaseObjects.Console.WriteLine($"{(inLine ? "LINE" : (isCross ? "CROSS" : "GAP"))} {(isCross && withIntersection ? " INTERSECTION" : "")} Resolve failed {unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2}", ConsoleColor.Red);
            //    }
            //    else
            //    {
            //        BaseObjects.Console.WriteLine($"{(inLine ? "LINE" : (isCross ? "CROSS" : "GAP"))} {(isCross && withIntersection ? " INTERSECTION" : "")} Resolve succeeded {unresolvedPair.Value.Item1} {unresolvedPair.Value.Item2}", ConsoleColor.Green);
            //    }
            //    index++;
            //}
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

        private static bool withIntersection = false;
        private static void CrossResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {//Failed cross 13639 13060 Code 10 Segment1 split False Segment2 split False
            //if (unresolvedPair.Item1.Id == 13605 && unresolvedPair.Item2.Id == 13616)
            //{
            //    var magnification = 1e10;
                
            //    Operations.Diagnostics.IntermeshSegmentDiagnostics.Dump([unresolvedPair.Item1, unresolvedPair.Item2], unresolvedPair.Item1.A.Point, magnification, $"-CrossResolve-{unresolvedPair.Item1.Key}-{unresolvedPair.Item2.Key}");

            //}

            var unresolvedSet = IntermeshSegmentExtensions.PointIntersection(unresolvedPair);
            //var intersection = LineSegment3D.PointIntersection(unresolvedPair.Item1.Segment, unresolvedPair.Item2.Segment, 1e-15);
            var intersection = unresolvedSet.Item1;
            //if (intersection is not null)
            {
                //CrossWithIntersectionResolve(unresolvedPair, intersection, bucket);
                IntermeshSegmentExtensions.CrossWithIntersectionResolve(unresolvedSet, bucket);
                withIntersection = true;
            }
            //else
            //{
            //    CrossWithoutIntersectionResolve(unresolvedPair, bucket);
            //    withIntersection = false;
            //}
        }
        //private static void CrossWithoutIntersectionResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        //{
        //    var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

        //    var extensionA = LineSegment3D.SegmentExtensionToLine(unresolvedPair.Item1.Segment, unresolvedPair.Item2.Segment, 1e-15);
        //    var extensionB = LineSegment3D.SegmentExtensionToLine(unresolvedPair.Item2.Segment, unresolvedPair.Item1.Segment, 1e-15);

        //    if (extensionA is not null && extensionB is not null)
        //    {
        //        var extension = extensionA.Length > extensionB.Length ? extensionB : extensionA;
        //        var to = IntermeshPointExtensions.Fetch(extension.Start);
        //        var from = IntermeshPointExtensions.Fetch(extension.End);

        //        var points = unresolvedPair.Item1.Capsules.SelectMany(c => c.Points);
        //        var nearestPoint = points.NearestPoint(to);
        //        if (Point3D.Distance(nearestPoint.Point, to.Point) < 1e-9) { to = nearestPoint; } else { unresolvedPair.Item1.SplitBy(to); }

        //        //bucket.PointTransferFromTo(from, to);

        //        //unresolvedPair.Item2.ExtendWith(to);
        //        //unresolvedPair.Item1.ResolvePoint(to);

        //        var isResolved2 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
        //        if (!isResolved2)
        //        {
        //            BaseObjects.Console.WriteLine($"Failed ExtentionAB gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Red);
        //        }
        //        else
        //        {
        //            BaseObjects.Console.WriteLine($"Successful ExtentionAB gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Green);
        //        }
        //    }

        //    else if (extensionA is not null && extensionA.Length < linkSegment.Distance)
        //    {
        //        //BaseObjects.Console.WriteLine($"Extension A split {unresolvedPair.Item1} {unresolvedPair.Item2}  {extensionA.Length}", ConsoleColor.Red);
        //        var to = IntermeshPointExtensions.Fetch(extensionA.Start);
        //        var from = IntermeshPointExtensions.Fetch(extensionA.End);

        //        var points = unresolvedPair.Item1.Capsules.SelectMany(c => c.Points);
        //        var nearestPoint = points.NearestPoint(to);
        //        if (Point3D.Distance(nearestPoint.Point, to.Point) < 1e-9) { to = nearestPoint; } else { unresolvedPair.Item1.SplitBy(to); }

        //        //bucket.PointTransferFromTo(from, to);

        //        //unresolvedPair.Item2.ExtendWith(to);
        //        //unresolvedPair.Item1.ResolvePoint(to);

        //        var isResolved2 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
        //        if (!isResolved2)
        //        {
        //            BaseObjects.Console.WriteLine($"Failed ExtentionA gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Red);
        //        }
        //        else
        //        {
        //            BaseObjects.Console.WriteLine($"Successful ExtentionA gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Green);
        //        }

        //        //var isResolved = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
        //        //BaseObjects.Console.WriteLine($"Is resolved: {isResolved}  Short capsules {unresolvedPair.Item1.Capsules.Count(c => c.Segment.Length < 1e-9)}");

        //        //var magnification = 1e10;
        //        //Operations.Diagnostics.IntermeshSegmentDiagnostics.Dump([unresolvedPair.Item1, unresolvedPair.Item2], unresolvedPair.Item1.A.Point, magnification, $"-Extension-A-{unresolvedPair.Item1.Key}-{unresolvedPair.Item2.Key}");

        //    }
        //    else if (extensionB is not null && extensionB.Length < linkSegment.Distance)
        //    {
        //        //BaseObjects.Console.WriteLine($"Extension B split {unresolvedPair.Item1} {unresolvedPair.Item2}  {extensionB.Length}", ConsoleColor.Red);
        //        var to = IntermeshPointExtensions.Fetch(extensionB.Start);
        //        var from = IntermeshPointExtensions.Fetch(extensionB.End);

        //        var points = unresolvedPair.Item2.Capsules.SelectMany(c => c.Points);
        //        var nearestPoint = points.NearestPoint(to);
        //        if (Point3D.Distance(nearestPoint.Point, to.Point) < 1e-9) { to = nearestPoint; } else { unresolvedPair.Item2.SplitBy(to); }

        //        //unresolvedPair.Item1.ExtendWith(to);
        //        //bucket.PointTransferFromTo(from, to);

        //        var isResolved2 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
        //        if (!isResolved2)
        //        {
        //            BaseObjects.Console.WriteLine($"Failed ExtentionB gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Red);
        //        }
        //        else
        //        {
        //            BaseObjects.Console.WriteLine($"Successful ExtentionB gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Green);
        //        }
        //        //BaseObjects.Console.WriteLine($"Is resolved: {isResolved}  Short capsules {unresolvedPair.Item2.Capsules.Count(c => c.Segment.Length < 1e-9)}");

        //        //var magnification = 1e10;
        //        //Operations.Diagnostics.IntermeshSegmentDiagnostics.Dump([unresolvedPair.Item1, unresolvedPair.Item2], unresolvedPair.Item1.A.Point, magnification, $"-Extension-B-{unresolvedPair.Item1.Key}-{unresolvedPair.Item2.Key}");
        //    }
        //    else
        //    {
        //        GapResolve(unresolvedPair, bucket);
        //    }
        //}

        private static void GapResolve((IntermeshSegment, IntermeshSegment) unresolvedPair, BoxBucket<IntermeshSegment> bucket)
        {
            var linkSegment = IntermeshSegmentExtensions.ShortestLink((unresolvedPair.Item1, unresolvedPair.Item2));

            unresolvedPair.Item1.ExtendWith(linkSegment.A);
            unresolvedPair.Item1.ExtendWith(linkSegment.B);
            unresolvedPair.Item2.ResolvePoint(linkSegment.A);
            unresolvedPair.Item2.ResolvePoint(linkSegment.B);

            var isResolved2 = IntermeshSegmentExtensions.IsResolved(unresolvedPair.Item1, unresolvedPair.Item2);
            if (!isResolved2)
            {
                BaseObjects.Console.WriteLine($"Failed gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Red);
            }
            //else
            //{
            //    BaseObjects.Console.WriteLine($"Successful gap {unresolvedPair.Item1.Id} {unresolvedPair.Item2.Id}", ConsoleColor.Green);
            //}
        }
    }
}
