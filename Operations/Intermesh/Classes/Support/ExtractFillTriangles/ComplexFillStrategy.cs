using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using FileExportImport;
using Operations.Diagnostics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class ComplexFillStrategy : IFillStrategy
    {
        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            var surfaceSets = triangle.CreateSurfaceSegmentSets();

            foreach (var surfaceSet in surfaceSets)
            {
                GetFillTriangles(triangle, surfaceSet);
            }
        }

        private CombinationDictionary<bool> _usedLoops = new CombinationDictionary<bool>();
        //private CombinationDictionary<List<FillTriangle>> _usedLoops = new CombinationDictionary<List<FillTriangle>>();
        //public CombinationDictionary<List<FillTriangle>> UsedLoops { get { return _usedLoops; } }

        private void GetFillTriangles(IntermeshTriangle triangle, SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint> surfaceSet)
        {
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);
            try
            {
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);

                chain = chain.WhereLoop(loop =>
                {
                    var key = new Combination(loop.Select(p => p.Reference.Id));
                    var isUsed = _usedLoops.ContainsKey(key);
                    _usedLoops[key] = true;
                    return !isUsed;
                });

                if (chain.Spurs.Any())
                {
                    foreach (var spur in chain.Spurs)
                    {
                        BaseObjects.Console.WriteLine($"{triangle.Id} Spurs [{string.Join(",", spur.Select(s => s.Reference.Id))}]", ConsoleColor.Red);
                    }
                }

                var fillings = new SurfaceTriangleContainer<IntermeshPoint>[0];
                try
                {
                    var planarFilling = new PlanarFilling<PlanarFillingGroup, IntermeshPoint>(chain, triangle.Id);
                    fillings = planarFilling.Fillings.ToArray();
                }
                catch (Exception e)
                {
                    BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Yellow);
                    return;
                }

                foreach (var filling in fillings)
                {
                    //var loopKey = new Combination(filling.Loop.Select(p => p.Reference.Id));
                    //if (filling.Loop.Length < 3)
                    //{
                    //    var isUsed = _usedLoops.ContainsKey(loopKey);

                    //    if (isUsed) { continue; }
                    //}


                    var fillTriangle = new FillTriangle(triangle,
                        filling.A.Reference,
                        filling.B.Reference,
                        filling.C.Reference);
                    fillTriangle.LoopKey = new Combination(filling.Loop.Select(p => p.Reference.Id));
                    fillTriangle.Loop = filling.Loop.Select(p => p.Reference).ToArray();

                    //if (!_usedLoops.ContainsKey(fillTriangle.LoopKey)) { _usedLoops[fillTriangle.LoopKey] = new List<FillTriangle>(); }
                    ////if (fillTriangle.LoopKey.Array.Length < 4 && _usedLoops[fillTriangle.LoopKey].Any()) { continue; }
                    //_usedLoops[fillTriangle.LoopKey].Add(fillTriangle);
                    //Console.WriteLine($"Triangle {triangle.Id} Fill {fillTriangle.Key}");
                    triangle.Fillings.Add(fillTriangle);
                    //_usedLoops[loopKey] = true;
                }
            }
            catch (Exception e)
            {
                BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Red);
                triangle.Show();

                //var slotTable = new Combination2Dictionary<int>();
                //foreach (var slot in triangle.EdgeSlots)
                //{
                //    foreach (var segment in slot.Segments)
                //    {
                //        slotTable[segment.Key] = slot.Id;
                //    }
                //}

                //BaseObjects.Console.WriteLine();
                //BaseObjects.Console.WriteLine($"Perimeters {string.Join(", ", surfaceSet.PerimeterSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
                //BaseObjects.Console.WriteLine($"Dividings {string.Join(", ", surfaceSet.DividingSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
                //var pointCount = surfaceSet.PerimeterSegments.SelectMany(ss => ss.Points).GroupBy(g => g.Reference.Id);
                //BaseObjects.Console.WriteLine($"Boundary points [{string.Join(",", pointCount.Where(g => g.Count() > 2).Select(g => g.Key))}]");
                //var center = triangle.Segments.SelectMany(s => s.Points).FirstOrDefault(p => p.Id == 1752);
                //var center = triangle.Triangle.Center;
                //triangle.Dump(center, 1e0);
                ////WavefrontFile.Export([triangle.Triangle], $"Wavefront/Trim/ErrorTriangle-{triangle.Id}");
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return true;
        }
    }
}
