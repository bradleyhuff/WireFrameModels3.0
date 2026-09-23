//using BasicObjects.GeometricObjects;
//using BasicObjects.MathExtensions;
//using FileExportImport;
//using Operations.Diagnostics;
//using Operations.Intermesh.Basics;
//using Operations.PlanarFilling.Basics;
//using Operations.PlanarFilling.Filling;
//using Operations.SurfaceSegmentChaining.Basics;
//using Operations.SurfaceSegmentChaining.Chaining;
//using Operations.SurfaceSegmentChaining.Collections;
//using System.Reflection.Metadata.Ecma335;
//using System.Runtime.CompilerServices;
//using System.Text.Json;

//namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
//{
//    internal class ComplexFill
//    {
//        public void GetFillings(IEnumerable<IntermeshTriangle> intermeshTriangles)
//        {
//            GetFillChains(intermeshTriangles);
//            ProcessFillChains(intermeshTriangles);
//            GetFillTriangles(intermeshTriangles);
//        }

//        private void GetFillChains(IEnumerable<IntermeshTriangle> intermeshTriangles)
//        {
//            foreach (var triangle in intermeshTriangles)
//            {
//                var surfaceSets = triangle.CreateSurfaceSegmentSets();

//                foreach (var surfaceSet in surfaceSets)
//                {
//                    var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);
//                    try
//                    {
//                        triangle.FillChain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);
//                    }
//                    catch (Exception e)
//                    {
//                        BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Red);
//                        triangle.Show();
//                    }
//                }
//            }
//        }

//        private CombinationDictionary<bool> _usedLoops = new CombinationDictionary<bool>();

//        private void ProcessFillChains(IEnumerable<IntermeshTriangle> intermeshTriangles)
//        {
//            //
//            //foreach (var triangle in intermeshTriangles)
//            //{
//            //    triangle.FillChain = triangle.FillChain.WhereLoop(loop =>
//            //    {
//            //        var key = new Combination(loop.Select(p => p.Reference.Id));
//            //        var isUsed = _usedLoops.ContainsKey(key);
//            //        _usedLoops[key] = true;
//            //        return !isUsed;
//            //    });
//            //}
//        }

//        private void GetFillTriangles(IEnumerable<IntermeshTriangle> intermeshTriangles)
//        {
//            foreach (var triangle in intermeshTriangles)
//            {
//                var fillings = new SurfaceTriangleContainer<IntermeshPoint>[0];
//                try
//                {
//                    var planarFilling = new PlanarFilling<PlanarFillingGroup, IntermeshPoint>(triangle.FillChain, triangle.Id);
//                    fillings = planarFilling.GetFillTriangles().ToArray();
//                }
//                catch (Exception e)
//                {
//                    BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Yellow);
//                    triangle.Show();
//                    return;
//                }

//                foreach (var filling in fillings)
//                {
//                    var fillTriangle = new FillTriangle(triangle,
//                        filling.A.Reference,
//                        filling.B.Reference,
//                        filling.C.Reference);
//                    //fillTriangle.LoopKey = new Combination(filling.Loop.Select(p => p.Reference.Id));
//                    //fillTriangle.Loop = filling.Loop.Select(p => p.Reference).ToArray();

//                    triangle.Fillings.Add(fillTriangle);
//                }
//            }
//        }

//        private void ShowSlotInfo(IntermeshTriangle triangle, SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint> surfaceSet)
//        {
//            var slotTable = new Combination2Dictionary<int>();
//            foreach (var slot in triangle.EdgeSlots)
//            {
//                foreach (var segment in slot.Segments)
//                {
//                    slotTable[segment.Key] = slot.Id;
//                }
//            }

//            BaseObjects.Console.WriteLine();
//            BaseObjects.Console.WriteLine($"Perimeters {string.Join(", ", surfaceSet.PerimeterSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
//            BaseObjects.Console.WriteLine($"Dividings {string.Join(", ", surfaceSet.IntersectionSegments.Select(s => $"{slotTable[new Combination2(s.A.Reference.Id, s.B.Reference.Id)]}: [{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
//            var pointCount = surfaceSet.PerimeterSegments.SelectMany(ss => ss.Points).GroupBy(g => g.Reference.Id);
//            BaseObjects.Console.WriteLine($"Boundary points [{string.Join(",", pointCount.Where(g => g.Count() > 2).Select(g => g.Key))}]");
//            //var center = triangle.Segments.SelectMany(s => s.Points).FirstOrDefault(p => p.Id == 1752);
//            //var center = triangle.Triangle.Center;
//            //triangle.Dump(center, 1e0);
//            //WavefrontFile.Export([triangle.Triangle], $"Wavefront/Trim/ErrorTriangle-{triangle.Id}");
//        }
//    }
//}
