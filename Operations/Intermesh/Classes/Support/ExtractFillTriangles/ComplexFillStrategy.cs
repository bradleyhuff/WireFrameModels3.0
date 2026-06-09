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

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class ComplexFillStrategy : IFillStrategy
    {
        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            var surfaceSet = triangle.CreateSurfaceSegmentSet();
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);

            //if (triangle.Id == 13267)
            //{
            //    foreach (var element in surfaceSet.PerimeterSegments)
            //    {
            //        WavefrontFile.Export([element.Segment.Segment], $"Wavefront/SurfaceSet-{triangle.Id}/Perimeter-Segment-{element.A.Index}-{element.B.Index}");
            //    }
            //    foreach (var element in surfaceSet.DividingSegments)
            //    {
            //        WavefrontFile.Export([element.Segment.Segment], $"Wavefront/SurfaceSet-{triangle.Id}/Dividing-Segment-{element.A.Index}-{element.B.Index}");
            //    }
            //}

            try
            {
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);

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

                //triangle.Dump(triangle.Triangle.Center, 1e0);

            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return true;
        }
    }
}
