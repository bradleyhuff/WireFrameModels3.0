using BaseObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Chaining.Diagnostics;
using Operations.SurfaceSegmentChaining.Collections;
using Operations.SurfaceSegmentChaining.Interfaces;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal static class ExtractFillTriangles
    {
        internal static IEnumerable<FillTriangle> Action(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var start = DateTime.Now;

            var fillTriangles = GetFillTriangles(elasticTriangles).ToArray();

            ConsoleLog.WriteLine($"Extract fill triangles. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            return fillTriangles;
        }

        private static IEnumerable<FillTriangle> GetFillTriangles(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            foreach (var triangle in elasticTriangles)
            {
                var segments = triangle.SegmentsCount;
                switch (segments)
                {
                    case 0:
                        {
                            FillTriangle[] fills = [];
                            try
                            {
                                fills = NoSegmentFills(triangle).ToArray();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            if (fills.Any()) { foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    case 1:
                        {
                            FillTriangle[] fills = [];
                            try
                            {
                                fills = SingleSegmentFills(triangle).ToArray();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            if (fills.Any()) { foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    case 2:
                        {
                            FillTriangle[] fills = [];
                            try
                            {
                                fills = DoubleSegmentFills(triangle).ToArray();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            if (fills.Any()) { foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    default:
                        {
                            var fills = ComplexSegmentFills(triangle);
                            if (fills.Any()) { foreach (var fill in fills) { yield return fill; } }
                        }
                        break;
                }
            }
        }

        private static IEnumerable<FillTriangle> NoSegmentFills(ElasticTriangle triangle)
        {
            var perimeter = triangle.PerimeterPointsCount;
            if (perimeter == 0)
            {
                yield return new FillTriangle(triangle);
            }
            yield break;
        }

        private static IEnumerable<FillTriangle> SingleSegmentFills(ElasticTriangle triangle)
        {
            if (triangle.PerimeterPointsCount == 2 && triangle.VertexPointsCount == 0)
            {
                var perimeterAB = triangle.PerimeterEdgeAB.PerimeterPoints;
                var perimeterBC = triangle.PerimeterEdgeBC.PerimeterPoints;
                var perimeterCA = triangle.PerimeterEdgeCA.PerimeterPoints;
                if (perimeterAB.Count == 1 && perimeterBC.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorB, perimeterAB[0], perimeterBC[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorA, perimeterAB[0], perimeterBC[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorA, triangle.AnchorC, perimeterBC[0]);

                    yield break;
                }
                if (perimeterAB.Count == 1 && perimeterCA.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorA, perimeterAB[0], perimeterCA[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorB, perimeterAB[0], perimeterCA[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorB, triangle.AnchorC, perimeterCA[0]);

                    yield break;
                }
                if (perimeterBC.Count == 1 && perimeterCA.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorC, perimeterBC[0], perimeterCA[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorA, perimeterBC[0], perimeterCA[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorB, triangle.AnchorA, perimeterBC[0]);

                    yield break;
                }
            }

            yield break;
        }

        private static IEnumerable<FillTriangle> DoubleSegmentFills(ElasticTriangle triangle)
        {
            if (triangle.PerimeterPointsCount == 2 && triangle.VertexPointsCount == 0)
            {
                var X = triangle.Segments[0].VerticiesAB.Select(v => v.Vertex).Intersect(triangle.Segments[1].VerticiesAB.Select(v => v.Vertex)).SingleOrDefault();
                if (X is null) yield break;
                var perimeterAB = triangle.PerimeterEdgeAB.PerimeterPoints;
                var perimeterBC = triangle.PerimeterEdgeBC.PerimeterPoints;
                var perimeterCA = triangle.PerimeterEdgeCA.PerimeterPoints;
                if (perimeterAB.Count == 1 && perimeterBC.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorB, X, perimeterAB[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorB, X, perimeterBC[0]);
                    yield return new FillTriangle(triangle, perimeterAB[0], X, triangle.AnchorA);
                    yield return new FillTriangle(triangle, perimeterBC[0], X, triangle.AnchorC);
                    yield return new FillTriangle(triangle, X, triangle.AnchorA, triangle.AnchorC);

                    yield break;
                }
                if (perimeterAB.Count == 1 && perimeterCA.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorA, X, perimeterAB[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorA, X, perimeterCA[0]);
                    yield return new FillTriangle(triangle, perimeterAB[0], X, triangle.AnchorB);
                    yield return new FillTriangle(triangle, perimeterCA[0], X, triangle.AnchorC);
                    yield return new FillTriangle(triangle, X, triangle.AnchorB, triangle.AnchorC);

                    yield break;
                }
                if (perimeterBC.Count == 1 && perimeterCA.Count == 1)
                {
                    yield return new FillTriangle(triangle, triangle.AnchorC, X, perimeterCA[0]);
                    yield return new FillTriangle(triangle, triangle.AnchorC, X, perimeterBC[0]);
                    yield return new FillTriangle(triangle, perimeterBC[0], X, triangle.AnchorB);
                    yield return new FillTriangle(triangle, perimeterCA[0], X, triangle.AnchorA);
                    yield return new FillTriangle(triangle, X, triangle.AnchorA, triangle.AnchorB);

                    yield break;
                }
            }
            yield break;
        }

        public static int LoopError = 0;
        public static int SpurredLoopError = 0;

        private static IEnumerable<FillTriangle> ComplexSegmentFills(ElasticTriangle triangle)
        {
            var surfaceSet = triangle.CreateSurfaceSegmentSet();
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, ElasticVertexCore>(surfaceSet);

            ISurfaceSegmentChaining<PlanarFillingGroup, ElasticVertexCore> chain;
            try
            {
                chain = SurfaceSegmentChaining<PlanarFillingGroup, ElasticVertexCore>.Create(collection);
            }
            //catch (ChainingException<ElasticVertexCore> e)
            //{
            //    Console.WriteLine($"Chaining Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
            //    WavefrontFileChaining.Export(triangle, e, $"Wavefront/SurfaceChainingError");
            //    LoopError++;
            //    yield break;
            //}
            catch (Exception e)
            {
                Console.WriteLine($"Chaining Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                LoopError++;
                yield break;
            }

            if (chain.SpurredLoops.Any())
            {
                try
                {
                    chain = OpenSpurConnectChaining<PlanarFillingGroup, ElasticVertexCore>.Create(chain);
                    chain = SpurLoopingChaining<PlanarFillingGroup, ElasticVertexCore>.Create(chain);
                }
                //catch (SpurLoopChainingException<PlanarFillingGroup, ElasticVertexCore> e)
                //{
                //    Console.WriteLine($"Spurred Loop Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                //    WavefrontFileChaining.Export(triangle, e, $"Wavefront/SpurLoopingChainingError", 5e-4);
                //    SpurredLoopError++;
                //    yield break;
                //}
                catch (Exception e)
                {
                    Console.WriteLine($"Spurred Loop Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                    SpurredLoopError++;
                    yield break;
                }
            }

            var fillings = new SurfaceTriangleContainer<ElasticVertexCore>[0];
            try
            {
                var planarFilling = new PlanarFilling<PlanarFillingGroup, ElasticVertexCore>(chain, triangle.Id);
                fillings = planarFilling.Fillings.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            foreach (var filling in fillings)
            {
                yield return new FillTriangle(filling.A.Reference, filling.A.Normal,
                    filling.B.Reference, filling.B.Normal, filling.C.Reference, filling.C.Normal, triangle.Trace);
            }

            yield break;
        }
    }
}
