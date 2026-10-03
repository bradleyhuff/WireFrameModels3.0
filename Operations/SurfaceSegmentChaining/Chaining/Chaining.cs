using BasicObjects.MathExtensions;
using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Basics.Abstractions;
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
            where G : LoopGroupObjects
        {
            var protectedIndexedLoops = ProtectedIndexedLoops.SplitByPerimeterIndexLoops<ProtectedIndexedLoops>(chain.ProtectedIndexedLoops).ToArray();

            for (int i = 0; i < chain.PerimeterLoops.Count; i++)
            {
                yield return new ModifiedChain<G, T>(
                    chain.ReferenceArray,
                    chain.BackReference,
                    protectedIndexedLoops[i],
                    [chain.PerimeterLoopGroupKeys[i]],
                    chain.DividingLoopGroupKeys,
                    chain.SpurredLoopGroupKeys,
                    chain.SpurGroupKeys,
                    [chain.PerimeterLoopGroupObjects[i]],
                    chain.DividingLoopGroupObjects,
                    chain.SpurredLoopGroupObjects,
                    chain.SpurGroupObjects,
                    [chain.PerimeterLoops[i]],
                    chain.DividingLoops,
                    chain.SpurredLoops,
                    chain.Spurs
                    );
            }
        }
        public static ISurfaceSegmentChaining<G, T> WhereLoop<G, T>(this ISurfaceSegmentChaining<G, T> chain, Func<SurfaceRayContainer<T>[], bool> includeLoop)
            where G : LoopGroupObjects
        {
            var includedLoopsIndicies = new List<int>();
            for (int i = 0; i < chain.DividingLoops.Count; i++)
            {
                if (includeLoop(chain.DividingLoops[i]))
                {
                    includedLoopsIndicies.Add(i);
                }
            }

            var includedLoopsKeys = new List<int>();
            var includedLoopObjects = new List<G>();
            var includedLoops = new List<SurfaceRayContainer<T>[]>();

            for (int i = 0; i < includedLoopsIndicies.Count; i++)
            {
                includedLoopsKeys.Add(chain.DividingLoopGroupKeys[includedLoopsIndicies[i]]);
                includedLoopObjects.Add(chain.DividingLoopGroupObjects[includedLoopsIndicies[i]]);
                includedLoops.Add(chain.DividingLoops[includedLoopsIndicies[i]]);
            }

            return new ModifiedChain<G, T>(
                chain.ReferenceArray,
                chain.BackReference,
                ProtectedIndexedLoops.IncludeLoopsByIndex<ProtectedIndexedLoops>(chain.ProtectedIndexedLoops, includedLoopsIndicies),
                chain.PerimeterLoopGroupKeys,
                includedLoopsKeys,//
                chain.SpurredLoopGroupKeys,
                chain.SpurGroupKeys,
                chain.PerimeterLoopGroupObjects,
                includedLoopObjects,//
                chain.SpurredLoopGroupObjects,
                chain.SpurGroupObjects,
                chain.PerimeterLoops,
                includedLoops,//
                chain.SpurredLoops,
                chain.Spurs
                );
        }

        public static ISurfaceSegmentChaining<G, T> DividingLoopSplitBy<G, T>(this ISurfaceSegmentChaining<G, T> chain, IEnumerable<(G FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank)> splits, CombinationDictionary<Rank> coveringLoops)
            where G : LoopGroupObjects
        {
            var planarFillGroups = new CombinationDictionary<G>();
            foreach(var split in splits)
            {
                planarFillGroups[new Combination(split.Loop.Select(l => l.Reference.Id))] = split.FillGroup;
            }

            var splitDividingLoopKeys = new List<int>();
            var splitDividingLoopObjects = new List<G>();
            var splitDividingLoops = new List<SurfaceRayContainer<T>[]>();
            int splitCount = 0;

            var protectedLoops = ProtectedIndexedLoops.DividingLoopSplitBy<ProtectedIndexedLoops>(chain.ProtectedIndexedLoops,
                splits.Select(s => s.Loop.Select(s => chain.BackReference[s.Index]).ToArray()),
                (gi, s) =>
                {
                    var loop = s.Select(p => chain.ReferenceArray[p]).ToArray();
                    var key = new Combination(loop.Select(l => l.Index));
                    splitDividingLoopKeys.Add(chain.DividingLoopGroupKeys[gi]);
                    splitDividingLoopObjects.Add(planarFillGroups.ContainsKey(key) ? planarFillGroups[key] : chain.DividingLoopGroupObjects[gi]);//
                    splitDividingLoops.Add(loop);
                },
                (gi, l, sgi, s) =>
                {
                    var coverLoop = l.Select(p => chain.ReferenceArray[p]).ToArray();
                    var coverKey = new Combination(coverLoop.Select(l => l.Index));
                    var coverLoopObject = planarFillGroups.ContainsKey(coverKey) ? planarFillGroups[coverKey] : chain.DividingLoopGroupObjects[sgi];

                    var loop = s.Select(p => chain.ReferenceArray[p]).ToArray();
                    var key = new Combination(loop.Select(l => l.Index));

                    var loopKey = chain.DividingLoopGroupKeys[gi];
                    var loopObject = planarFillGroups.ContainsKey(key) ? planarFillGroups[key] : chain.DividingLoopGroupObjects[sgi];

                    if (coverLoop.Length > loop.Length)
                    {
                        coveringLoops[coverKey] = Rank.Dividing;
                    }
                    
                    //BaseObjects.Console.WriteLine($"Covering {coverLoopObject.Id}:[{string.Join(", ", coverLoop.Select(l => l.Index))}] split {loopObject.Id}:[{string.Join(", ", loop.Select(l => l.Index))}] ", ConsoleColor.Yellow);
                    splitCount++;
                }
                );

            //if (splitCount > 0) { BaseObjects.Console.WriteLine(); }

            return new ModifiedChain<G, T>(
                chain.ReferenceArray,
                chain.BackReference,
                protectedLoops,
                chain.PerimeterLoopGroupKeys,
                splitDividingLoopKeys,//
                chain.SpurredLoopGroupKeys,
                chain.SpurGroupKeys,
                chain.PerimeterLoopGroupObjects,
                splitDividingLoopObjects,//
                chain.SpurredLoopGroupObjects,
                chain.SpurGroupObjects,
                chain.PerimeterLoops,
                splitDividingLoops,//
                chain.SpurredLoops,
                chain.Spurs
                );
        }
    }

    internal class ModifiedChain<G, T> : ISurfaceSegmentChaining<G, T> where G : LoopGroupObjects
    {
        public ModifiedChain(
            IReadOnlyList<SurfaceRayContainer<T>> referenceArray,
            IReadOnlyDictionary<int, int> backReference,
            ProtectedIndexedLoops protectedIndexedLoops,
            IReadOnlyList<int> perimeterLoopGroupKeys,
            IReadOnlyList<int> loopGroupKeys,
            IReadOnlyList<int> spurredLoopGroupKeys,
            IReadOnlyList<int> spurGroupKeys,
            IReadOnlyList<G> perimeterLoopGroupObjects,
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
            BackReference = backReference;
            ProtectedIndexedLoops = protectedIndexedLoops;
            PerimeterLoopGroupKeys = perimeterLoopGroupKeys;
            DividingLoopGroupKeys = loopGroupKeys;
            SpurredLoopGroupKeys = spurredLoopGroupKeys;
            SpurGroupKeys = spurGroupKeys;
            PerimeterLoopGroupObjects = perimeterLoopGroupObjects;
            DividingLoopGroupObjects = loopGroupObjects;
            SpurredLoopGroupObjects = spurredLoopGroupObjects;
            SpurGroupObjects = spurGroupObjects;
            PerimeterLoops = perimeterLoops;
            DividingLoops = loops;
            SpurredLoops = spurredLoops;
            Spurs = spurs;
        }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        public IReadOnlyDictionary<int, int> BackReference { get; }

        public ProtectedIndexedLoops ProtectedIndexedLoops { get; }

        public IReadOnlyList<int> DividingLoopGroupKeys { get; }

        public IReadOnlyList<int> SpurredLoopGroupKeys { get; }

        public IReadOnlyList<int> SpurGroupKeys { get; }

        public IReadOnlyList<G> PerimeterLoopGroupObjects { get; }

        public IReadOnlyList<G> DividingLoopGroupObjects { get; }

        public IReadOnlyList<G> SpurredLoopGroupObjects { get; }

        public IReadOnlyList<G> SpurGroupObjects { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> PerimeterLoops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> DividingLoops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> SpurredLoops { get; }

        public IReadOnlyList<SurfaceRayContainer<T>[]> Spurs { get; }

        public IReadOnlyList<int> PerimeterLoopGroupKeys { get; }
    }
}


