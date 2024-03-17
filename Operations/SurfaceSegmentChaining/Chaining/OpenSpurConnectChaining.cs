using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining.Abstractions;
using Operations.SurfaceSegmentChaining.Chaining.Extensions;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal class OpenSpurConnectChaining<G, T> : SurfaceSegmentChaining<G, T> where G : TriangleFillingGroup
    {
        public static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentChaining<G, T> input)
        {
            return new OpenSpurConnectChaining<G, T>(input);
        }

        protected OpenSpurConnectChaining(ISurfaceSegmentChaining<G, T> input) : base(input.ReferenceArray, GetLinkedIndexSurfaceSegments(input))
        {
        }

        private static List<InternalLinkedIndexSurfaceSegment<G, T>> GetLinkedIndexSurfaceSegments(ISurfaceSegmentChaining<G, T> input)
        {
            var output = SpurChaining.PullSegments(input).ToList();

            var protectedIndexLoops = ProtectedIndexedLoops.Create<SpurChaining.InternalProtectedIndexedLoops>(input.ProtectedIndexedLoops);
            var perimeterIndexLoops = protectedIndexLoops.GetPerimeterIndexLoops();
            var indexSpurredLoops = protectedIndexLoops.GetIndexSpurredLoops();
            var indexLoops = protectedIndexLoops.GetIndexLoops().ToList();
            var loops = protectedIndexLoops.GetIndexLoops().ToList();
            loops.AddRange(indexSpurredLoops);

            SpurChaining.GetSpurEndpoints(indexSpurredLoops, input, loops, (l) => new InternalLoopSegment(l), out List<SpurChaining.SpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> spurEndpoints, out List<SpurChaining.OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurs);
            if (!openSpurs.Any()) { return output; }

            var addedSegments = new List<InternalLinkedIndexSurfaceSegment<G, T>>();
            var addedLineSegments = new List<LineSegment3D>();

            var testLoops = indexLoops;
            testLoops.AddRange(perimeterIndexLoops);
            NearestLoopPointConnectionTests(openSpurs, testLoops, ref addedSegments, ref addedLineSegments);
            ConnectedOpenSpurToNearestOpenSpurConnectionTests(openSpurs, ref addedSegments, ref addedLineSegments);

            //if (ShowSpurChainingSegments)
            //{
            //    var testMesh = WireFrameMeshFactory.Create();
            //    Debugging.ShowDividingSegments(addedLineSegments, input.PerimeterLoopGroupObjects.First().Plane.Normal, testMesh);
            //    Wavefront.Export(testMesh, $"Wavefront/OpenSpurChainingSegments");
            //}

            output.AddRange(addedSegments);

            return output;
        }

        public static bool ShowSpurChainingSegments { get; set; }

        private static void NearestLoopPointConnectionTests(List<SpurChaining.OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurEndpoints, List<int[]> indexLoops, ref List<InternalLinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
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
                output.Add(new InternalLinkedIndexSurfaceSegment<G, T>(openSpurEndpoint.GroupKey, openSpurEndpoint.Group, nearest.IndexA, nearest.IndexB, Rank.Dividing));
                openSpurEndpoint.Status.IsLinked = true;
            }
        }

        private static void ConnectedOpenSpurToNearestOpenSpurConnectionTests(List<SpurChaining.OpenSpurEndpoint<G, InternalLoopSegment, OpenSpurStatus, T>> openSpurEndpoints, ref List<InternalLinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            if (openSpurEndpoints.All(s => !s.Status.IsLinked))
            {
                throw new InvalidOperationException("No open spurs were linked.");
            }

            while (openSpurEndpoints.Any(s => !s.Status.IsLinked))
            {
                throw new NotImplementedException("Connected open spur to nearest open spur connection tests not implemented.");
                foreach (var connectedOpenSpurEndpoint in openSpurEndpoints.Where(s => s.Status.IsLinked))
                {
                    //
                    //connectedOpenSpurEndpoint.IsLinked = true;
                }
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
                var intersection = Line3D.PointIntersection(segment, match);
                if (Line3D.PointIntersection(segment, match) is not null) { return true; }
            }
            foreach (var match in addedSegments.Where(m => LineSegment3D.IsNonLinking(m, segment)))
            {
                var intersection = Line3D.PointIntersection(segment, match);
                if (Line3D.PointIntersection(segment, match) is not null) { return true; }
            }
            return false;
        }

        private static int[] ReturnOpenSpur(int[] indexSpurredLoop)
        {
            var endpoints = GetEndpoints(indexSpurredLoop).ToArray();
            if (endpoints.Length != 2) { return null; }

            var segmentA = indexSpurredLoop.RotateToFirst((v, i) => v == endpoints[0]).TakeWhileIncluding((v, i) => v == endpoints[1]).ToArray();
            var segmentB = indexSpurredLoop.RotateToFirst((v, i) => v == endpoints[1]).TakeWhileIncluding((v, i) => v == endpoints[0]).Reverse().ToArray();
            if (!segmentA.IsEqualTo(segmentB)) { return null; }

            return segmentA;
        }

        private static IEnumerable<int> GetEndpoints(int[] indexSpurredLoop)
        {
            for (int i = 0; i < indexSpurredLoop.Length; i++)
            {
                if (SpurChaining.IsSpurEndpoint(indexSpurredLoop, i)) { yield return indexSpurredLoop[i]; }
            }
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
