using BasicObjects.GeometricObjects;
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
        private static int count = 0;

        public static int Count
        {
            get { return count; }
        }

        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            if (triangle.Id == 26704)
            {
                //var center = new Point3D(0.342936847021717, 0.501698612686442, 0.995000000000000);
                //Diagnostics.Intermesh.IntermeshTriangle.Dump(triangle, center, 1e1);
                //Diagnostics.Intermesh.IntermeshTriangle.Dump(triangle, center, 1e2, 1.0 / triangle.Triangle.AspectRatio);
                //var center = new Point3D(0.138852640831677, 0.499999999962244, 0.610672223875145);
                //var center = new Point3D(0.138892118133143, 0.499999999956937, 0.610513756229592);
                //
                //var center = new Point3D(0.359046687017550, 0.513143470268609, 0.995000000000000);
                //0.341133814535993 Y: 0.500461593446060 Z: 0.995000000000000
                //+		Point	{[ X: 0.342936847021717 Y: 0.501698612686442 Z: 0.995000000000000 ]}	BasicObjects.GeometricObjects.Point3D

                //var center = new Point3D(0.342936847021717, 0.501698612686442, 0.995000000000000);
                ////var center = triangle.Triangle.Center;

                //var segment13 = new LineSegment3D(new Point3D(0.507624338476000, 0.384037467368000, 0.500000000000000), new Point3D(0.503841430117926, 0.386987901996097, 0.496158569882069));
                //var segment312 = new LineSegment3D(new Point3D(0.503841430117926, 0.386987901996097, 0.496158569882069), new Point3D(0.506250363869848, 0.385108891263991, 0.498604863277479));
                //var segment1214 = new LineSegment3D(new Point3D(0.506250363869848, 0.385108891263991, 0.498604863277479), new Point3D(0.506250363869688, 0.385108881111974, 0.498604868229159));
                //var segment141 = new LineSegment3D(new Point3D(0.506250363869688, 0.385108881111974, 0.498604868229159), new Point3D(0.507624338476000, 0.384037467368000, 0.500000000000000));
                //WavefrontFile.Export(triangle.ExportSegment(segment13), $"Wavefront/Triangle-{triangle.Id}/Segment13", 1e2);
                //WavefrontFile.Export(triangle.ExportSegment(segment312), $"Wavefront/Triangle-{triangle.Id}/Segment312", 1e2);
                //WavefrontFile.Export(triangle.ExportSegment(segment1214), $"Wavefront/Triangle-{triangle.Id}/Segment1214", 1e2);
                //WavefrontFile.Export(triangle.ExportSegment(segment141), $"Wavefront/Triangle-{triangle.Id}/Segment141", 1e2);
            }
            complexDivision++;
            count++;
            var surfaceSet = triangle.CreateSurfaceSegmentSet();
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);

            ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain;
            try
            {
                chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);
                if (chain.Spurs.Any())
                {
                    foreach (var spur in chain.Spurs)
                    {
                        Console.WriteLine($"Spurs [{string.Join(",", spur.Select(s => s.Reference.Id))}]");
                    }

                    throw new Exception($"Spurs found {chain.SpurredLoops.Count()}");
                }
                //chain = OpenSpurConnectChaining<PlanarFillingGroup, IntermeshPoint>.Create(chain);
                //chain = SpurLoopingChaining<PlanarFillingGroup, IntermeshPoint>.Create(chain);
            }
            catch (Exception e)
            {
                var intersectionSets = triangle.GatheringSets.Where(s => s.Value.Intersections.Any());

                Console.WriteLine($"Chaining Error {e.Message} {triangle.Id} Min Height {triangle.Triangle.MinHeight.ToString("e1")} Near degenerate {triangle.IsNearDegenerate}", ConsoleColor.Red);

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
                var fillTriangle = new FillTriangle(triangle,
                    filling.A.Reference.Point, filling.A.Normal, filling.A.Reference.Id,
                    filling.B.Reference.Point, filling.B.Normal, filling.B.Reference.Id,
                    filling.C.Reference.Point, filling.C.Normal, filling.C.Reference.Id,
                    filling.FillId, triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag);
                triangle.Fillings.Add(fillTriangle);
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return true;
        }
    }
}
