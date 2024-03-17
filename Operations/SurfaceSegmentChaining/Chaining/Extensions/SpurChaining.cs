using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets.Interfaces;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining.Extensions
{
    internal static class SpurChaining
    {
        public static void GetSpurEndpoints<G, I, S, T>(IEnumerable<int[]> indexSpurredLoops, ISurfaceSegmentChaining<G, T> input,
            IEnumerable<int[]> loops, Func<LineSegment3D, I> createNode,
            out List<SpurEndpoint<G, I, S, T>> spurEndPoints, out List<OpenSpurEndpoint<G, I, S, T>> openSpurs)
            where S : new() where I : IBox where G : TriangleFillingGroup
        {
            var groupArray = input.SpurredLoopGroupObjects.ToArray();
            var groupKeyArray = input.SpurredLoopGroupKeys.ToArray();
            var segmentTable = new Combination2Dictionary<bool>();

            var loopSegments = loops.SelectMany(l => PullLoopSegments(segmentTable, input.ReferenceArray, l)).ToArray();
            var bucket = new BoxBucket<I>(loopSegments.Select(p => createNode(p)).ToArray());

            spurEndPoints = new List<SpurEndpoint<G, I, S, T>>();
            openSpurs = new List<OpenSpurEndpoint<G, I, S, T>>();
            foreach (var pair in indexSpurredLoops.Select((l, i) => new { Loop = l, Index = i }))
            {
                var indexSpurredLoop = pair.Loop;

                var openSpur = ReturnOpenSpur(indexSpurredLoop);
                if (openSpur is not null)
                {
                    openSpurs.Add(new OpenSpurEndpoint<G, I, S, T>(openSpur, groupKeyArray[pair.Index], groupArray[pair.Index], bucket, input.ReferenceArray));
                    continue;
                }

                for (int j = 0; j < indexSpurredLoop.Length; j++)
                {
                    var isSpurEndpoint = SpurChaining.IsSpurEndpoint(indexSpurredLoop, j);
                    if (!isSpurEndpoint) { continue; }

                    spurEndPoints.Add(new SpurEndpoint<G, I, S, T>(indexSpurredLoop, j, groupKeyArray[pair.Index], groupArray[pair.Index], bucket, input.ReferenceArray));
                }
            }
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
                if (IsSpurEndpoint(indexSpurredLoop, i)) { yield return indexSpurredLoop[i]; }
            }
        }

        public class OpenSpurEndpoint<G, I, S, T> where I : IBox where S : new()
        {
            public OpenSpurEndpoint(int[] openSpur, int groupKey, G group, BoxBucket<I> bucket, IReadOnlyList<SurfaceRayContainer<T>> referenceArray)
            {
                OpenSpur = openSpur;
                GroupKey = groupKey;
                Group = group;
                Bucket = bucket;
                ReferenceArray = referenceArray;
            }
            public int[] OpenSpur { get; }
            public int GroupKey { get; }
            public G Group { get; }
            public BoxBucket<I> Bucket { get; }
            public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
            public S Status { get; } = new S();
        }

        public class SpurEndpoint<G, I, S, T> where G : TriangleFillingGroup where I : IBox where S : new()
        {
            public SpurEndpoint(int[] spurredLoop, int index, int groupKey, G group, BoxBucket<I> bucket, IReadOnlyList<SurfaceRayContainer<T>> referenceArray)
            {
                Index = index;
                SpurredLoop = spurredLoop;
                GroupKey = groupKey;
                Group = group;
                Bucket = bucket;
                ReferenceArray = referenceArray;
            }
            public int Index { get; }
            public int[] SpurredLoop { get; }
            public int GroupKey { get; }
            public G Group { get; }
            public BoxBucket<I> Bucket { get; }
            public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
            public S Status { get; } = new S();

            public override string ToString()
            {
                return $"Index: {SpurredLoop[Index]} Loop: {SpurredLoop.Length}";
            }
        }

        public static IEnumerable<InternalLinkedIndexSurfaceSegment<G, T>> PullSegments<G, T>(ISurfaceSegmentChaining<G, T> input) where G : TriangleFillingGroup
        {
            var protectedIndexLoops = ProtectedIndexedLoops.Create<InternalProtectedIndexedLoops>(input.ProtectedIndexedLoops);
            var indexLoops = protectedIndexLoops.GetIndexLoops();
            var indexSpurredLoops = protectedIndexLoops.GetIndexSpurredLoops();
            var perimeterIndexLoops = protectedIndexLoops.GetPerimeterIndexLoops();
            var segmentTable = new Combination2Dictionary<bool>();

            foreach (var segment in
                PullLoopSegments<G, T>(segmentTable, Rank.Perimeter,
                    perimeterIndexLoops, input.PerimeterLoopGroupKeys, input.PerimeterLoopGroupObjects))
            { yield return segment; }

            foreach (var segment in
                PullLoopSegments<G, T>(segmentTable, Rank.Dividing,
                indexLoops, input.LoopGroupKeys, input.LoopGroupObjects))
            { yield return segment; }

            foreach (var segment in
                PullLoopSegments<G, T>(segmentTable, Rank.Dividing,
                indexSpurredLoops, input.SpurredLoopGroupKeys, input.SpurredLoopGroupObjects))
            { yield return segment; }
        }

        private static IEnumerable<InternalLinkedIndexSurfaceSegment<G, T>> PullLoopSegments<G, T>(
            Combination2Dictionary<bool> segmentTable,
            Rank rank,
            IReadOnlyList<int[]> indexLoops,
            IReadOnlyList<int> loopGroupKeys,
            IReadOnlyList<G> loopGroupObjects
            ) where G : TriangleFillingGroup
        {
            for (int i = 0; i < indexLoops.Count; i++)
            {
                var groupObject = loopGroupObjects[i];
                var groupKey = loopGroupKeys[i];
                var indexLoop = indexLoops[i];

                for (int j = 0; j < indexLoop.Length; j++)
                {
                    int jPlusOne = (j + 1) % indexLoop.Length;
                    var indexA = indexLoop[j];
                    var indexB = indexLoop[jPlusOne];
                    var key = new Combination2(indexA, indexB);
                    if (!segmentTable.ContainsKey(key))
                    {
                        yield return new InternalLinkedIndexSurfaceSegment<G, T>(groupKey, groupObject, indexA, indexB, rank);
                        segmentTable[key] = true;
                    }
                }
            }
        }

        public static IEnumerable<LineSegment3D> PullLoopSegments<T>(
            Combination2Dictionary<bool> segmentTable,
            IReadOnlyList<SurfaceRayContainer<T>> referenceArray,
            int[] indexLoop)
        {
            for (int j = 0; j < indexLoop.Length; j++)
            {
                int jPlusOne = (j + 1) % indexLoop.Length;
                var indexA = indexLoop[j];
                var indexB = indexLoop[jPlusOne];
                var key = new Combination2(indexA, indexB);
                if (!segmentTable.ContainsKey(key))
                {
                    yield return new LineSegment3D(referenceArray[indexA].Point, referenceArray[indexB].Point);
                    segmentTable[key] = true;
                }
            }
        }
        internal class InternalProtectedIndexedLoops : ProtectedIndexedLoops
        {
            public IReadOnlyList<int[]> GetPerimeterIndexLoops()
            {
                return PerimeterIndexLoops;
            }
            public IReadOnlyList<int[]> GetIndexLoops()
            {
                return IndexLoops;
            }
            public IReadOnlyList<int[]> GetIndexSpurredLoops()
            {
                return IndexSpurredLoops;
            }
        }

        public static int NextIndex(int[] spurredLoop, int i, int next)
        {
            return (i + next + spurredLoop.Length) % spurredLoop.Length;
        }

        public static bool IsSpurEndpoint(int[] spurredLoop, int i)
        {
            return spurredLoop[NextIndex(spurredLoop, i, -1)] == spurredLoop[NextIndex(spurredLoop, i, 1)];
        }
    }
}
