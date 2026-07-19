using BasicObjects.GeometricObjects;
using FileExportImport;
using Operations.Diagnostics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using System.Runtime.CompilerServices;

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

        private void GetFillTriangles(IntermeshTriangle triangle, SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint> surfaceSet)
        {
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);
            try
            {
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);
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
                    var fillTriangle = new FillTriangle(triangle,
                        filling.A.Reference,
                        filling.B.Reference,
                        filling.C.Reference);
                    triangle.Fillings.Add(fillTriangle);
                }
            }
            catch (Exception e)
            {
                BaseObjects.Console.WriteLine($"Triangle: {triangle.Id} {e.Message}", ConsoleColor.Red);
                triangle.Show();
                BaseObjects.Console.WriteLine();
                BaseObjects.Console.WriteLine($"Perimeters {string.Join(", ", surfaceSet.PerimeterSegments.Select(s => $"[{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
                BaseObjects.Console.WriteLine($"Dividings {string.Join(", ", surfaceSet.DividingSegments.Select(s => $"[{s.A.Reference.Id}, {s.B.Reference.Id}]"))}");
                var pointCount = surfaceSet.PerimeterSegments.SelectMany(ss => ss.Points).GroupBy(g => g.Reference.Id);
                BaseObjects.Console.WriteLine($"Boundary points [{string.Join(",", pointCount.Where(g => g.Count() > 2).Select(g => g.Key))}]");
                //triangle.Dump(triangle.Triangle.Center, 1e2);


            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return true;
        }
    }
}
