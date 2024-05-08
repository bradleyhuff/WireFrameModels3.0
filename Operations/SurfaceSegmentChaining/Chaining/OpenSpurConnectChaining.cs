using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Basics.Abstractions;
using Operations.SurfaceSegmentChaining.Chaining.Extensions;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal class OpenSpurConnectChaining<G, T> : SurfaceSegmentChaining<G, T> where G : PlanarFillingGroup
    {
        public static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentChaining<G, T> input)
        {
            var chaining = new OpenSpurConnectChaining<G, T>();
            chaining.Run(input);
            return chaining;
        }

        protected void Run(ISurfaceSegmentChaining<G, T> input)
        {
            var linkedSegments = GetLinkedIndexSurfaceSegments(input);
            base.Run(input.ReferenceArray, linkedSegments);
        }

        private List<LinkedIndexSurfaceSegment<G, T>> GetLinkedIndexSurfaceSegments(ISurfaceSegmentChaining<G, T> input)
        {
            var output = SpurChaining.PullSegments(input).ToList();

            var protectedIndexLoops = ProtectedIndexedLoops.Create<SpurChaining.InternalProtectedIndexedLoops>(input.ProtectedIndexedLoops);
            var perimeterIndexLoops = protectedIndexLoops.GetPerimeterIndexLoops();
            var indexSpurredLoops = protectedIndexLoops.GetIndexSpurredLoops();
            var indexLoops = protectedIndexLoops.GetIndexLoops().ToList();
            var loops = protectedIndexLoops.GetIndexLoops().ToList();
            loops.AddRange(indexSpurredLoops);

            SpurChaining.GetSpurEndpoints(indexSpurredLoops, input, loops, (l) => new InternalLoopSegment(l), out List<SpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> spurEndpoints, out List<OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurs);
            if (!openSpurs.Any()) { return output; }

            var addedSegments = new List<LinkedIndexSurfaceSegment<G, T>>();
            var addedLineSegments = new List<LineSegment3D>();

            var testLoops = indexLoops;
            testLoops.AddRange(perimeterIndexLoops);
            NearestLoopPointConnectionTests(openSpurs, testLoops, ref addedSegments, ref addedLineSegments);
            ConnectedOpenSpurToNearestOpenSpurConnectionTests(openSpurs, ref addedSegments, ref addedLineSegments);

            output.AddRange(addedSegments);

            return output;
        }

        private static void NearestLoopPointConnectionTests(List<OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurEndpoints, List<int[]> indexLoops, ref List<LinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            var testPoints = indexLoops.SelectMany(l => l).ToArray();
            foreach (var openSpurEndpoint in openSpurEndpoints)
            {
                var indexA = openSpurEndpoint.OpenSpur.First();
                var sweepA = PointDistanceSweep(indexA, testPoints, openSpurEndpoint.Bucket, openSpurEndpoint.ReferenceArray, addedLineSegments);
                var indexB = openSpurEndpoint.OpenSpur.Last();
                var sweepB = PointDistanceSweep(indexB, testPoints, openSpurEndpoint.Bucket, openSpurEndpoint.ReferenceArray, addedLineSegments);

                var sweep = sweepA.Concat(sweepB);
                if (!sweep.Any()) continue;

                var nearest = sweep.MinBy(s => s.Distance);

                addedLineSegments.Add(new LineSegment3D(openSpurEndpoint.ReferenceArray[nearest.IndexA].Point, openSpurEndpoint.ReferenceArray[nearest.IndexB].Point));
                output.Add(new LinkedIndexSurfaceSegment<G, T>(openSpurEndpoint.GroupKey, openSpurEndpoint.Group, nearest.IndexA, nearest.IndexB, Rank.Dividing));
                openSpurEndpoint.Status.IsLinked = true;
            }
        }

        private static void ConnectedOpenSpurToNearestOpenSpurConnectionTests(List<OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurEndpoints, ref List<LinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            if (openSpurEndpoints.All(s => !s.Status.IsLinked))
            {
                throw new InvalidOperationException("No open spurs were linked.");
            }

            while (openSpurEndpoints.Any(s => !s.Status.IsLinked))
            {
                throw new NotImplementedException("Connected open spur to nearest open spur connection tests not implemented.");
            }
        }
        private struct PointDistance
        {
            public int IndexA;
            public int IndexB;
            public double Distance;
        }
        private static IEnumerable<PointDistance> PointDistanceSweep(int focusIndex, int[] testIndicies, BoxBucket<InternalLoopSegment> bucket, IReadOnlyList<SurfaceRayContainer<T>> referenceArray, IEnumerable<LineSegment3D> addedSegments)
        {
            foreach (int testIndex in testIndicies)
            {
                var testSegment = new LineSegment3D(referenceArray[focusIndex].Point, referenceArray[testIndex].Point);
                if (Intersects(testSegment, bucket, addedSegments)) continue;

                yield return new PointDistance() { IndexA = focusIndex, IndexB = testIndex, Distance = testSegment.Length };
            }
            yield break;
        }

        private static bool Intersects(LineSegment3D segment, BoxBucket<InternalLoopSegment> bucket, IEnumerable<LineSegment3D> addedSegments)
        {
            var matches = bucket.Fetch(new InternalLoopSegment(segment));
            foreach (var match in matches.Select(m => m.Segment).Where(m => LineSegment3D.IsNonLinking(m, segment)))
            {
                var intersection = LineSegment3D.PointIntersection(segment, match);
                if (LineSegment3D.PointIntersection(segment, match) is not null) { return true; }
            }
            foreach (var match in addedSegments.Where(m => LineSegment3D.IsNonLinking(m, segment)))
            {
                var intersection = LineSegment3D.PointIntersection(segment, match);
                if (LineSegment3D.PointIntersection(segment, match) is not null) { return true; }
            }
            return false;
        }

        private class OpenSpurStatus
        {
            public bool IsLinked { get; set; }
        }
        private class SpurEndpointStatus
        {
            public bool IsCompleted { get { return MatchScore >= 2; } }
            public int MatchScore { get; set; }
        }
        private class InternalLoopSegment : BaseSegment
        {
            public InternalLoopSegment(LineSegment3D segment) : base(segment) { }
        }
    }
}
