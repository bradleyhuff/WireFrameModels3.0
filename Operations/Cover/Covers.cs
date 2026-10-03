using BasicObjects.MathExtensions;
using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Cover
{
    internal static class Covers
    {
        public static void 
            Build(IEnumerable<IntermeshTriangle> intermeshTriangles,
            Action<CoverBuildPass> perimeterCovers,
            Action<CoverBuildPass> dividingCovers)
        {
            var vertexTable = new Dictionary<int, List<IntermeshTriangle>>();

            foreach (var triangle in intermeshTriangles)
            {
                foreach (var chain in triangle.FillChains)
                {
                    foreach (var loop in chain.PerimeterLoops)
                    {
                        foreach (var point in loop)
                        {
                            if (!vertexTable.ContainsKey(point.Reference.Id)) { vertexTable[point.Reference.Id] = new List<IntermeshTriangle>(); }
                            if (!vertexTable[point.Reference.Id].Any(t => t.Id == triangle.Id))
                            {
                                vertexTable[point.Reference.Id].Add(triangle);
                            }
                        }
                    }
                    foreach (var loop in chain.DividingLoops)
                    {
                        foreach (var point in loop)
                        {
                            if (!vertexTable.ContainsKey(point.Reference.Id)) { vertexTable[point.Reference.Id] = new List<IntermeshTriangle>(); }
                            if (!vertexTable[point.Reference.Id].Any(t => t.Id == triangle.Id))
                            {
                                vertexTable[point.Reference.Id].Add(triangle);
                            }
                        }
                    }
                }
            }

            foreach (var triangle in intermeshTriangles)
            {
                foreach (var chain in triangle.FillChains)
                {
                    foreach (var loop in CombinePerimeterLoops(chain))
                    {
                        var key = Combination(loop.Loop);

                        var checks = new List<IntermeshTriangle>();
                        foreach (var point in loop.Loop)
                        {
                            checks.AddRange(vertexTable[point.Reference.Id]);
                        }

                        checks = checks.DistinctBy(c => c.Id).Where(c => c.Id != triangle.Id).ToList();

                        var sets = checks.Select(t => new CoverSet(t, chain, loop, t.FillChains.SelectMany(c => Combine(c)))).ToArray();
                        var covers = sets.Select(c => new CoverSet(c.Triangle, c.Chain, c.CoverLoop, c.Loops.Where(l => key.Covers(Combination(l.Loop))))).Where(a => a.Loops.Any()).ToArray();
                        if (covers.Any())
                        {
                            var pass = new CoverBuildPass(chain, (loop.Loop, Rank.Perimeter), triangle, covers);
                            perimeterCovers(pass);
                        }
                    }
                    foreach (var loop in CombineDividingLoops(chain))
                    {
                        var key = Combination(loop.Loop);

                        var checks = new List<IntermeshTriangle>();
                        foreach (var point in loop.Loop)
                        {
                            checks.AddRange(vertexTable[point.Reference.Id]);
                        }

                        checks = checks.DistinctBy(c => c.Id).Where(c => c.Id != triangle.Id).ToList();

                        var sets = checks.Select(t => new CoverSet(t, chain, loop, t.FillChains.SelectMany(c => Combine(c)))).ToArray();
                        var covers = sets.Select(c => new CoverSet(c.Triangle, c.Chain, c.CoverLoop, c.Loops.Where(l => key.Covers(Combination(l.Loop))))).Where(a => a.Loops.Any()).ToArray();
                        if (covers.Any())
                        {
                            var pass = new CoverBuildPass(chain, (loop.Loop, Rank.Dividing), triangle, covers);
                            dividingCovers(pass);
                        }
                    }
                }
            }
            foreach (var triangle in intermeshTriangles)
            {
                if (triangle.ModifiedFillChains.Any())
                {
                    triangle.FillChains = triangle.ModifiedFillChains.ToList();
                    triangle.ModifiedFillChains.Clear();
                }
            }
        }

        private static IEnumerable<(PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank)> Combine(ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain)
        {
            for (int i = 0; i < chain.PerimeterLoopGroupObjects.Count; i++)
            {
                yield return (chain.PerimeterLoopGroupObjects[i], chain.PerimeterLoops[i], Rank.Perimeter);
            }
            for (int i = 0; i < chain.DividingLoopGroupObjects.Count; i++)
            {
                yield return (chain.DividingLoopGroupObjects[i], chain.DividingLoops[i], Rank.Dividing);
            }
        }

        public static IEnumerable<(PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank)> CombinePerimeterLoops(ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain)
        {
            for (int i = 0; i < chain.PerimeterLoopGroupObjects.Count; i++)
            {
                yield return (chain.PerimeterLoopGroupObjects[i], chain.PerimeterLoops[i], Rank.Perimeter);
            }
        }

        public static IEnumerable<(PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank)> CombineDividingLoops(ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain)
        {
            for (int i = 0; i < chain.DividingLoopGroupObjects.Count; i++)
            {
                yield return (chain.DividingLoopGroupObjects[i], chain.DividingLoops[i], Rank.Dividing);
            }
        }

        public static Combination Combination(IEnumerable<SurfaceRayContainer<IntermeshPoint>> loop)
        {
            return new Combination(loop.Select(l => l.Reference.Id));
        }
    }
}
