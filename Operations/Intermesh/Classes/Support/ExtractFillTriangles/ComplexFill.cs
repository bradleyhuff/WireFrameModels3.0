using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using Operations.Cover;
using Operations.Diagnostics;
using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class ComplexFill
    {
        public void GetFillings(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            GetFillChains(intermeshTriangles);
            ProcessFillChains(intermeshTriangles);
            GetFillTriangles(intermeshTriangles);
        }

        private void GetFillChains(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var triangle in intermeshTriangles)
            {
                var surfaceSets = triangle.CreateSurfaceSegmentSets();

                foreach (var surfaceSet in surfaceSets)
                {
                    var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);
                    try
                    {
                        triangle.FillChains.Add(SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection));
                    }
                    catch (Exception e)
                    {
                        BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Red);
                        triangle.Show();
                    }
                }
            }
        }


        private void ProcessFillChains(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime now = DateTime.Now;

            var coverCount = 0;
            var coverSize = 0;

            var coveringLoops = new CombinationDictionary<Rank>();

            Covers.Build(intermeshTriangles,
                (pass) =>
                {
                    //var key = Covers.Combination(l);
                    //Console.WriteLine($"{key.Array.Length}:{t.Id}:{key} => Perimeter {string.Join(", ", c.Select(c => $"{string.Join(", ", c.Loops.Select(cc => $"{c.Triangle.Id}:{Covers.Combination(cc)}"))}").ToList())}");
                    //coverCount++;
                    //if (key.Array.Length > coverSize) { coverSize = key.Array.Length; }
                },
                (pass) =>
                {
                    var key = Covers.Combination(pass.CoverLoop.Loop);
                    //Console.WriteLine($"{key.Array.Length}:{t.Id}:{p.Id}:{key} => Dividing {string.Join(", ", c.Select(c => $"{string.Join(", ", c.Loops.Select(cc => $"{c.Triangle.Id}:{cc.FillGroup.Id}:{Covers.Combination(cc.Loop)}"))}").ToList())}");
                    coverCount++;
                    if (key.Array.Length > coverSize) { coverSize = key.Array.Length; }

                    var modifiedChain = pass.Chain.DividingLoopSplitBy(pass.CoveredLoops.SelectMany(cc => cc.Loops), coveringLoops);
                    pass.Triangle.ModifiedFillChains.Add(modifiedChain);
                    //BaseObjects.Console.WriteLine($"After split {key.Array.Length}:{t.Id}:{p.Id}:{key} => Dividing {string.Join(", ", Covers.CombineDividingLoops(sc).Select(cc => $"{cc.FillGroup.Id}:{Covers.Combination(cc.Loop)}"))}", ConsoleColor.Cyan);
                }
                );

            BaseObjects.Console.WriteLine($"Covering loops {coveringLoops.Count}", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine($"{string.Join("\n", coveringLoops.Select(kp => kp.Key))}", ConsoleColor.Yellow);

            var loopTable = new CombinationDictionary<List<(PlanarFillingGroup, Rank)>>();

            foreach (var triangle in intermeshTriangles)
            {
                foreach (var chain in triangle.FillChains)
                {
                    for (int i = 0; i < chain.PerimeterLoops.Count; i++)
                    {
                        var loop = chain.PerimeterLoops[i];
                        var key = Covers.Combination(loop);
                        if (!loopTable.ContainsKey(key)) { loopTable[key] = new List<(PlanarFillingGroup, Rank)>(); }
                        loopTable[key].Add((chain.PerimeterLoopGroupObjects[i], Rank.Perimeter));
                    }
                    for (int i = 0; i < chain.DividingLoops.Count; i++)
                    {
                        var loop = chain.DividingLoops[i];
                        var key = Covers.Combination(loop);
                        if (!loopTable.ContainsKey(key)) { loopTable[key] = new List<(PlanarFillingGroup, Rank)>(); }
                        loopTable[key].Add((chain.DividingLoopGroupObjects[i], Rank.Dividing));
                    }
                }
            }

            foreach (var kv in loopTable)
            {
                var list = kv.Value;
                foreach (var element in list) { element.Item1.Disabled = true; }
                var perimeter = list.FirstOrDefault(e => e.Item2 == Rank.Perimeter);
                if (perimeter.Item1 is not null) { perimeter.Item1.Disabled = false; } else
                {
                    var dividing = list.First(e => e.Item2 == Rank.Dividing);
                    dividing.Item1.Disabled = false;
                }
            }

            foreach (var key in coveringLoops)
            {
                if (!loopTable.ContainsKey(key.Key)) { Console.WriteLine($"Covering {key.Key} not found.");  continue; }
                var elements = loopTable[key.Key].Where(e => e.Item2 == Rank.Dividing);
                foreach (var element in elements) { element.Item1.Disabled = true; }                
            }

            Console.WriteLine($"Process Fill Chain Covers: {coverCount} Max Cover Size: {coverSize} Loop Table: {loopTable.Count} Loop Table Nodes: {loopTable.Count(kv => kv.Value.Count() > 1)}  Elapsed time {(DateTime.Now - now).TotalSeconds}");



        }

        private void GetFillTriangles(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var chain in triangle.FillChains)
                {
                    var fillings = new SurfaceTriangleContainer<IntermeshPoint>[0];
                    try
                    {
                        var planarFilling = new PlanarFilling<PlanarFillingGroup, IntermeshPoint>(chain, triangle.Id);
                        fillings = planarFilling.GetFillTriangles().ToArray();
                    }
                    catch (Exception e)
                    {
                        BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Yellow);
                        triangle.Show();
                        return;
                    }

                    foreach (var filling in fillings)
                    {
                        var fillTriangle = new FillTriangle(triangle,
                            filling.A.Reference,
                            filling.B.Reference,
                            filling.C.Reference);
                        //fillTriangle.LoopKey = new Combination(filling.Loop.Select(p => p.Reference.Id));
                        //fillTriangle.Loop = filling.Loop.Select(p => p.Reference).ToArray();

                        triangle.Fillings.Add(fillTriangle);
                    }
                }
            }
        }

        private void ShowSlotInfo(IntermeshTriangle triangle, SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint> surfaceSet)
        {
            var slotTable = new Combination2Dictionary<int>();
            foreach (var slot in triangle.EdgeSlots)
            {
                foreach (var segment in slot.Segments)
                {
                    slotTable[segment.Key] = slot.Id;
                }
            }

            BaseObjects.Console.WriteLine();
            BaseObjects.Console.WriteLine($"Perimeters {string.Join(", ", surfaceSet.PerimeterSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
            BaseObjects.Console.WriteLine($"Dividings {string.Join(", ", surfaceSet.IntersectionSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
            var pointCount = surfaceSet.PerimeterSegments.SelectMany(ss => ss.Points).GroupBy(g => g.Reference.Id);
            BaseObjects.Console.WriteLine($"Boundary points [{string.Join(",", pointCount.Where(g => g.Count() > 2).Select(g => g.Key))}]");
            //var center = triangle.Segments.SelectMany(s => s.Points).FirstOrDefault(p => p.Id == 1752);
            //var center = triangle.Triangle.Center;
            //triangle.Dump(center, 1e0);
            //WavefrontFile.Export([triangle.Triangle], $"Wavefront/Trim/ErrorTriangle-{triangle.Id}");
        }
    }
}
