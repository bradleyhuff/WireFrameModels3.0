using BaseObjects.Transformations;
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
                ////triangle.Triangle.Center = {[ X: 0.359042643241032 Y: 0.513114487684745 Z: 0.997142645207350 ]}
                ////triangle.Triangle.AspectRatio = 4.970489647027372E-07
                ////triangle.Triangle.MinimumHeight.Direction = {[ X: 0.810476 Y: 0.585696 Z: 0.009453 ]}
                //var center = new Point3D(0.359046687017550, 0.513143470268609, 0.995000000000000);
                //var aspectRatio = 4.970489647027372E-07;
                //var direction = new Vector3D(0.810476, 0.585696, 0.009453);

                //44243 Parent 43592 Internal segment[16569, 16579] [[X: 0.359046687017550 Y: 0.513143470268609 Z: 0.995000000000000 ], [X: 0.359046688059896 Y: 0.513143471022016 Z: 0.995000000000000 ]] 1.286121816911512E-09

                //var directionalTransform = Transform.Identity();//Transform.DirectionalScaling(center, direction, 1.0 / aspectRatio);
                //var center = new Point3D(0.359046687017550, 0.513143470268609, 0.995000000000000);

                //Diagnostics.Intermesh.IntermeshTriangle.Dump(triangle, center, 1e6, directionalTransform);

                //foreach(var adjacent in triangle.IntersectingTriangles)
                //{
                //    Console.WriteLine($"Intersecting {adjacent.Id} of {triangle.Id}", ConsoleColor.Yellow);
                //    Diagnostics.Intermesh.IntermeshTriangle.Dump(adjacent, center, 1e6, directionalTransform);
                //}

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
