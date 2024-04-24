using BaseObjects;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Chaining.Diagnostics;
using Operations.SurfaceSegmentChaining.Interfaces;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class ExtractFillTriangles
    {
        internal static IEnumerable<FillTriangle> Action(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var start = DateTime.Now;

            var fillTriangles = GetFillTriangles(elasticTriangles).ToArray();

            //Console.WriteLine($"Fill triangles {fillTriangles.Length}");
            //Console.WriteLine($"Loop error {LoopError} Spurred loop error {SpurredLoopError} Fill error {InternalLoop.FillLoopError}");
            ConsoleLog.WriteLine($"Extract fill triangles. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            //Console.WriteLine();
            return fillTriangles;
        }

        private static IEnumerable<FillTriangle> GetFillTriangles(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            //int simpleTriangles0 = 0;
            //int simpleTriangles1 = 0;
            //int simpleTriangles2 = 0;
            //int simpleTriangles3 = 0;
            //int complexTriangles = 0;
            //int needComplexFill = 0;
            foreach (var triangle in elasticTriangles)
            {
                var segments = triangle.SegmentsCount;
                switch (segments)
                {
                    case 0:
                        {
                            var fills = NoSegmentFills(triangle);
                            if (fills.Any()) { /*simpleTriangles0++;*/ foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    case 1:
                        {
                            var fills = SingleSegmentFills(triangle);
                            if (fills.Any()) { /*simpleTriangles1++;*/ foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    case 2:
                        {
                            var fills = DoubleSegmentFills(triangle);
                            if (fills.Any()) { /*simpleTriangles2++;*/ foreach (var fill in fills) { yield return fill; } break; }
                            goto default;
                        }
                    default:
                        {
                            //needComplexFill++;
                            var fills = ComplexSegmentFills(triangle).ToArray();
                            if (fills.Any())
                            { /*complexTriangles++;*/ foreach (var fill in fills) { yield return fill; } }
                        }
                        break;
                }
            }
            //Console.WriteLine($"Needs complex fill {needComplexFill}");
            //Console.WriteLine($"Simple triangles0 {simpleTriangles0} Simple triangles1 {simpleTriangles1} Simple triangles2 {simpleTriangles2}  Simple triangles3 {simpleTriangles3} Complex triangles {complexTriangles}");
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
            if (triangle.PerimeterPointsCount == 1 && triangle.VertexPointsCount == 1)
            {

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
            var collection = new SurfaceElasticSegmentCollections<TriangleFillingGroup>(surfaceSet);

            ISurfaceSegmentChaining<TriangleFillingGroup, int> chain;
            try
            {
                chain = SurfaceSegmentChaining<TriangleFillingGroup, int>.Create(collection);
            }
            catch (ChainingException<int> e)
            {
                Console.WriteLine($"Chaining Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                WavefrontFileChaining.Export(triangle, e, $"Wavefront/SurfaceChainingError");
                LoopError++;
                yield break;
            }
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
                    chain = OpenSpurConnectChaining<TriangleFillingGroup, int>.Create(chain);
                    chain = SpurLoopingChaining<TriangleFillingGroup, int>.Create(chain);
                }
                catch (SpurLoopChainingException<TriangleFillingGroup, int> e)
                {
                    Console.WriteLine($"Spurred Loop Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                    WavefrontFileChaining.Export(triangle, e, $"Wavefront/SpurLoopingChainingError", 5e-4);
                    SpurredLoopError++;
                    yield break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Spurred Loop Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                    SpurredLoopError++;
                    yield break;
                }
            }

            var planarFilling = new PlanarFilling<TriangleFillingGroup, int>(chain, triangle.Id);
            var fillings = planarFilling.Fillings.ToArray();
            var lookup = triangle.VertexLookup;

            foreach (var filling in fillings)
            {
                yield return new FillTriangle(lookup[filling.A.Reference], filling.A.Normal,
                    lookup[filling.B.Reference], filling.B.Normal, lookup[filling.C.Reference], filling.C.Normal, triangle.Trace);
            }

            yield break;
        }
    }
}
