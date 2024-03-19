using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Operations.Intermesh.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Basics.Abstractions;
using Operations.SurfaceSegmentChaining.Chaining.Extensions;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal class SpurLoopingChaining<G, T> : SurfaceSegmentChaining<G, T> where G : TriangleFillingGroup
    {
        public static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentChaining<G, T> input)
        {
            return new SpurLoopingChaining<G, T>(input);
        }

        protected SpurLoopingChaining(ISurfaceSegmentChaining<G, T> input) : base(input.ReferenceArray, GetLinkedIndexSurfaceSegments(input))
        {
        }

        private static List<LinkedIndexSurfaceSegment<G, T>> GetLinkedIndexSurfaceSegments(ISurfaceSegmentChaining<G, T> input)
        {
            var output = SpurChaining.PullSegments(input).ToList();

            var protectedIndexLoops = ProtectedIndexedLoops.Create<SpurChaining.InternalProtectedIndexedLoops>(input.ProtectedIndexedLoops);
            var indexSpurredLoops = protectedIndexLoops.GetIndexSpurredLoops();
            var loops = protectedIndexLoops.GetIndexLoops().ToList();
            loops.AddRange(indexSpurredLoops);

            SpurChaining.GetSpurEndpoints(indexSpurredLoops, input, loops, (l) => new InternalLoopSegment(l),
                out List<SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> spurEndpoints,
                out List<OpenSpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> openSpurs);

            var spurEndpointTable = GetSpurEndpointTable(spurEndpoints);
            var addedSegments = new List<LinkedIndexSurfaceSegment<G, T>>();
            var addedLineSegments = new List<LineSegment3D>();

            EndPointToEndPointTests(spurEndpoints, ref addedSegments, ref addedLineSegments);
            EndPointToLoopPointsTests(spurEndpoints, spurEndpointTable, ref addedSegments, ref addedLineSegments);
            EndPointToSpurPointsTests(spurEndpoints, ref addedSegments, ref addedLineSegments);

            //if (ShowSpurChainingSegments)
            //{
            //    var testMesh = WireFrameMeshFactory.Create();
            //    Debugging.ShowDividingSegments(addedLineSegments, input.PerimeterLoopGroupObjects.First().Plane.Normal, testMesh);
            //    Wavefront.Export(testMesh, $"Wavefront/SpurChainingSegments");
            //}

            output.AddRange(addedSegments);

            return output;
        }

        public static bool ShowSpurChainingSegments { get; set; }

        private static void EndPointToEndPointTests(IEnumerable<SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> spurEndpoints, ref List<LinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            var spurEndpointsArray = spurEndpoints.ToArray();
            var segmentTable = new Combination2Dictionary<bool>();

            for (int i = 0; i < spurEndpointsArray.Length; i++)
            {
                var spurEndpointA = spurEndpointsArray[i];
                for (int j = 0; j < spurEndpointsArray.Length; j++)
                {
                    if (i == j) { continue; }
                    var spurEndpointB = spurEndpointsArray[j];
                    if (segmentTable.ContainsKey(spurEndpointA.SpurredLoop[spurEndpointA.Index], spurEndpointB.SpurredLoop[spurEndpointB.Index])) { continue; }
                    if (Intersects(spurEndpointA, addedLineSegments, spurEndpointA.SpurredLoop[spurEndpointA.Index], spurEndpointB.SpurredLoop[spurEndpointB.Index])) { continue; }

                    addedLineSegments.Add(new LineSegment3D(spurEndpointA.ReferenceArray[spurEndpointA.SpurredLoop[spurEndpointA.Index]].Point, spurEndpointB.ReferenceArray[spurEndpointB.SpurredLoop[spurEndpointB.Index]].Point));
                    output.Add(new LinkedIndexSurfaceSegment<G, T>(spurEndpointA.GroupKey,
                        spurEndpointA.Group, spurEndpointA.SpurredLoop[spurEndpointA.Index], spurEndpointB.SpurredLoop[spurEndpointB.Index], Rank.Dividing));
                    segmentTable[spurEndpointA.SpurredLoop[spurEndpointA.Index], spurEndpointB.SpurredLoop[spurEndpointB.Index]] = true;
                    spurEndpointA.Status.MatchScore += 1;
                    spurEndpointB.Status.MatchScore += 1;
                }
            }
        }

        private static void EndPointToLoopPointsTests(IEnumerable<SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> spurEndpoints, Dictionary<int, bool> spurEndpointTable, ref List<LinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            foreach (var spurEndpoint in spurEndpoints.Where(s => !s.Status.IsCompleted))
            {
                var indexCounts = GetIndexCounts(spurEndpoint.SpurredLoop);

                var testPoints = spurEndpoint.SpurredLoop.AlternatingUnwrap(spurEndpoint.Index).
                    Skip(1).Where(i => indexCounts[i] == 1 && !spurEndpointTable.ContainsKey(i)).ToArray();

                foreach (var testPoint in testPoints)
                {
                    if (Intersects(spurEndpoint, addedLineSegments, spurEndpoint.SpurredLoop[spurEndpoint.Index], testPoint)) { continue; }
                    addedLineSegments.Add(new LineSegment3D(spurEndpoint.ReferenceArray[spurEndpoint.SpurredLoop[spurEndpoint.Index]].Point, spurEndpoint.ReferenceArray[testPoint].Point));
                    output.Add(new LinkedIndexSurfaceSegment<G, T>(spurEndpoint.GroupKey, spurEndpoint.Group, spurEndpoint.SpurredLoop[spurEndpoint.Index], testPoint, Rank.Dividing));
                    spurEndpoint.Status.MatchScore += 2;
                    break;
                }
            }
        }

        private static void EndPointToSpurPointsTests(IEnumerable<SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> spurEndPoints, ref List<LinkedIndexSurfaceSegment<G, T>> output, ref List<LineSegment3D> addedLineSegments)
        {
            if (spurEndPoints.Any(s => !s.Status.IsCompleted))
            {
                throw new NotImplementedException("EndPoint to spur points not implemented.");
            }
        }

        private static Dictionary<int, bool> GetSpurEndpointTable(IEnumerable<SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T>> spurEndpoints)
        {
            var result = new Dictionary<int, bool>();
            foreach (var spurEndpoint in spurEndpoints)
            {
                result[spurEndpoint.SpurredLoop[spurEndpoint.Index]] = true;
            }
            return result;
        }

        private static Dictionary<int, int> GetIndexCounts(int[] indexSpurredLoop)
        {
            var indexCount = new Dictionary<int, int>();

            for (int i = 0; i < indexSpurredLoop.Length; i++)
            {
                if (!indexCount.ContainsKey(indexSpurredLoop[i])) { indexCount[indexSpurredLoop[i]] = 0; }
                indexCount[indexSpurredLoop[i]]++;
            }
            return indexCount;
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

        private static bool Intersects(SpurEndpoint<G, InternalLoopSegment, SpurEndpointStatus, T> spurEndpoint, IEnumerable<LineSegment3D> addedSegments, int spurIndex, int testIndex)
        {
            var segment = new LineSegment3D(spurEndpoint.ReferenceArray[spurIndex].Point, spurEndpoint.ReferenceArray[testIndex].Point);
            var matches = spurEndpoint.Bucket.Fetch(new InternalLoopSegment(segment));
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
    }
}
