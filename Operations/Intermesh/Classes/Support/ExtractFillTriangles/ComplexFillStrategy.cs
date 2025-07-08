using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using Operations.SurfaceSegmentChaining.Interfaces;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class ComplexFillStrategy : IFillStrategy
    {
        private int complexDivision = 0;

        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            complexDivision++;
            var surfaceSet = triangle.CreateSurfaceSegmentSet();
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);

            ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain;
            try
            {
                chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Chaining Error", ConsoleColor.Red);
                //Console.WriteLine($"Chaining Error {triangle.Segments.Count} Triangle {triangle.Id}");
                var strategy = new AbstractNearDegenerateFill<IntermeshPoint>(triangle.NonSpurDivisions.Select(d => (d.A, d.B)), p => p.Id, p => triangle.Verticies.Any(v => v.Id == p.Id));

                //Console.WriteLine($"Near degenerate {triangle.Id} Fills {strategy.GetFill().Count()} Min angle {triangle.Triangle.MinimumAngle}");
                foreach (var filling in strategy.GetFill())
                {
                    var fillTriangle = new FillTriangle(triangle, filling.Item1, filling.Item2, filling.Item3);
                    triangle.Fillings.Add(fillTriangle);
                }
                Console.WriteLine($"End error", ConsoleColor.Red);
                return;
            }

            var fillings = new SurfaceTriangleContainer<IntermeshPoint>[0];
            try
            {
                var planarFilling = new PlanarFilling<PlanarFillingGroup, IntermeshPoint>(chain, triangle.Id);
                fillings = planarFilling.Fillings.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            foreach (var filling in fillings)
            {
                var fillTriangle = new FillTriangle(triangle, filling.A.Reference.Point, filling.A.Normal,
                    filling.B.Reference.Point, filling.B.Normal, filling.C.Reference.Point, filling.C.Normal, filling.FillId, triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag);
                triangle.Fillings.Add(fillTriangle);
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return true;
        }
    }
}
