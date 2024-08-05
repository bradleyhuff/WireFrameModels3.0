using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal static class Chaining
    {
        public static IEnumerable<ISurfaceSegmentChaining<G, T>> SplitByPerimeterLoops<G, T>(ISurfaceSegmentChaining<G, T> chain) 
            where G : class
            //where T : ProtectedIndexedLoops, new()
        {
            var protectedIndexedLoops = ProtectedIndexedLoops.SplitByPerimeterIndexLoops<ProtectedIndexedLoops>(chain.ProtectedIndexedLoops).ToArray();

            for (int i = 0; i < chain.PerimeterLoops.Count; i++)
            {
                yield return new SplitChain<G, T>(
                    chain.ReferenceArray,
                    protectedIndexedLoops[i],
                    [chain.PerimeterLoopGroupKeys[i]],
                    chain.LoopGroupKeys,
                    chain.SpurredLoopGroupKeys,
                    chain.SpurGroupKeys,
                    [chain.PerimeterLoopGroupObjects[i]],
                    chain.LoopGroupObjects,
                    chain.SpurredLoopGroupObjects,
                    chain.SpurGroupObjects,
                    [chain.PerimeterLoops[i]],
                    chain.Loops,
                    chain.SpurredLoops,
                    chain.Spurs
                    );
            }
        }
    }

    internal class SplitChain<G, T> : ISurfaceSegmentChaining<G, T> where G : class
    {
        public SplitChain(
            IReadOnlyList<SurfaceRayContainer<T>> referenceArray,
            ProtectedIndexedLoops protectedIndexedLoops,
            List<int> perimeterLoopGroupKeys,
            IReadOnlyList<int> loopGroupKeys,
            IReadOnlyList<int> spurredLoopGroupKeys,
            IReadOnlyList<int> spurGroupKeys,
            List<G> perimeterLoopGroupObjects,
            IReadOnlyList<G> loopGroupObjects,
            IReadOnlyList<G> spurredLoopGroupObjects,
            IReadOnlyList<G> spurGroupObjects,
            IReadOnlyList<SurfaceRayContainer<T>[]> perimeterLoops,
            IReadOnlyList<SurfaceRayContainer<T>[]> loops,
            IReadOnlyList<SurfaceRayContainer<T>[]> spurredLoops,
            IReadOnlyList<SurfaceRayContainer<T>[]> spurs
            )
        {
            ReferenceArray = referenceArray;
            ProtectedIndexedLoops = protectedIndexedLoops;
            PerimeterLoopGroupKeys = perimeterLoopGroupKeys;
            LoopGroupKeys = loopGroupKeys;
            SpurredLoopGroupKeys = spurredLoopGroupKeys;
            SpurGroupKeys = spurGroupKeys;
            PerimeterLoopGroupObjects = perimeterLoopGroupObjects;
            LoopGroupObjects = loopGroupObjects;
            SpurredLoopGroupObjects = spurredLoopGroupObjects;
            SpurGroupObjects = spurGroupObjects;
            PerimeterLoops = perimeterLoops;
            Loops = loops;
            SpurredLoops = spurredLoops;
            Spurs = spurs;
        }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }

        public ProtectedIndexedLoops ProtectedIndexedLoops { get; }

        public IReadOnlyList<int> LoopGroupKeys { get; }

        public IReadOnlyList<int> SpurredLoopGroupKeys { get; }

        public IReadOnlyList<int> SpurGroupKeys { get; }

        public List<G> PerimeterLoopGroupObjects { get; }

        public IReadOnlyList<G> LoopGroupObjects { get; }

        public IReadOnlyList<G> SpurredLoopGroupObjects { get; }

        public IReadOnlyList<G> SpurGroupObjects { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> PerimeterLoops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> Loops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> SpurredLoops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> Spurs { get; }

        public List<int> PerimeterLoopGroupKeys { get; }
    }
}
