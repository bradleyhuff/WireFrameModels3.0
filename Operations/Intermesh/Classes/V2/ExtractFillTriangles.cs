using BaseObjects;
using Operations.Intermesh.Basics.V2;
using Operations.Intermesh.Elastics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using Operations.SurfaceSegmentChaining.Interfaces;
using Console = BaseObjects.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Classes.V2
{
    internal class ExtractFillTriangles
    {
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;

            GetFillTriangles(triangles);

            ConsoleLog.WriteLine($"Extract fill triangles. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void GetFillTriangles(IEnumerable<IntermeshTriangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                if (!triangle.HasDivisions) { triangle.Fillings.Add(new FillTriangle(triangle)); continue; }
                var internalSegments = triangle.InternalSegments.ToArray();
                if (internalSegments.Length == 1 && internalSegments.All(s => s.InternalDivisions == 0))
                {
                    if (SingleSegmentFill(triangle)) { continue; }
                }
                //if(internalDivisions == 2)
                //{
                //    if (DoubleSegmentFill(triangle)) { return; }
                //}

                //var segments = triangle.SegmentsCount;
                //switch (segments)
                //{
                //    case 0:
                //        {
                //            FillTriangle[] fills = [];
                //            try
                //            {
                //                fills = NoSegmentFills(triangle).ToArray();
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine(ex.Message);
                //            }

                //            if (fills.Any())
                //            {
                //                foreach (var fill in fills) { yield return fill; }
                //                break;
                //            }
                //            goto default;
                //        }
                //    case 1:
                //        {
                //            FillTriangle[] fills = [];
                //            try
                //            {
                //                fills = SingleSegmentFills(triangle).ToArray();
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine(ex.Message);
                //            }

                //            if (fills.Any())
                //            {
                //                foreach (var fill in fills) { yield return fill; }
                //                break;
                //            }
                //            goto default;
                //        }
                //    case 2:
                //        {
                //            FillTriangle[] fills = [];
                //            try
                //            {
                //                fills = DoubleSegmentFills(triangle).ToArray();
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine(ex.Message);
                //            }

                //            if (fills.Any())
                //            {
                //                foreach (var fill in fills) { yield return fill; }
                //                break;
                //            }
                //            goto default;
                //        }
                //    default:
                //        {
                //            var fills = ComplexSegmentFills(triangle);
                //            if (fills.Any()) { foreach (var fill in fills) { yield return fill; } }
                //        }
                //        break;
                //}

                ComplexSegmentFills(triangle);
            }
        }

        //private static IEnumerable<FillTriangle> NoSegmentFills(IntermeshTriangle triangle)
        //{
        //    var perimeter = triangle.PerimeterPointsCount;
        //    if (perimeter == 0)
        //    {
        //        yield return new FillTriangle(triangle);
        //    }
        //    yield break;
        //}

        private static bool SingleSegmentFill(IntermeshTriangle triangle)
        {
            //if (triangle.AB.InternalDivisions == 1 && triangle.BC.InternalDivisions == 1 && triangle.CA.InternalDivisions == 0)
            //{
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.AB.DivisionPoints[1], triangle.BC.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.AB.DivisionPoints[1], triangle.BC.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.C, triangle.BC.DivisionPoints[1]));
            //    return true;
            //}
            //if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 0)
            //{
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.AB.DivisionPoints[1], triangle.CA.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.AB.DivisionPoints[1], triangle.CA.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.C, triangle.CA.DivisionPoints[1]));
            //    return true;
            //}
            //if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && triangle.AB?.InternalDivisions == 0)
            //{
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BC.DivisionPoints[1], triangle.CA.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.BC.DivisionPoints[1], triangle.CA.DivisionPoints[1]));
            //    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.A, triangle.BC.DivisionPoints[1]));
            //    return true;
            //}
            return false;
        }

        //private static IEnumerable<FillTriangle> SingleSegmentFills(IntermeshTriangle triangle)
        //{
        //    if (triangle.PerimeterPointsCount == 2 && triangle.VertexPointsCount == 0)
        //    {
        //        var divisionsAB = triangle.ABInternalPoints.ToArray();
        //        var divisionsBC = triangle.BCInternalPoints.ToArray();
        //        var divisionsCA = triangle.CAInternalPoints.ToArray();
        //        if (divisionsAB.Length == 1 && divisionsBC.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.B, divisionsAB[0], divisionsBC[0]);
        //            yield return new FillTriangle(triangle, triangle.A, divisionsAB[0], divisionsBC[0]);
        //            yield return new FillTriangle(triangle, triangle.A, triangle.C, divisionsBC[0]);

        //            yield break;
        //        }
        //        if (divisionsAB.Length == 1 && divisionsCA.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.A, divisionsAB[0], divisionsCA[0]);
        //            yield return new FillTriangle(triangle, triangle.B, divisionsAB[0], divisionsCA[0]);
        //            yield return new FillTriangle(triangle, triangle.B, triangle.C, divisionsCA[0]);

        //            yield break;
        //        }
        //        if (divisionsBC.Length == 1 && divisionsCA.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.C, divisionsBC[0], divisionsCA[0]);
        //            yield return new FillTriangle(triangle, triangle.A, divisionsBC[0], divisionsCA[0]);
        //            yield return new FillTriangle(triangle, triangle.B, triangle.A, divisionsBC[0]);

        //            yield break;
        //        }
        //    }

        //    yield break;
        //}

        private static bool DoubleSegmentFill(IntermeshTriangle triangle)
        {


            return false;
        }

        //private static IEnumerable<FillTriangle> DoubleSegmentFills(IntermeshTriangle triangle)
        //{
        //    if (triangle.PerimeterPointsCount == 2 && triangle.VertexPointsCount == 0)
        //    {
        //        var X = triangle.Segments[0].VerticiesAB.Select(v => v.Vertex).Intersect(triangle.Segments[1].VerticiesAB.Select(v => v.Vertex)).SingleOrDefault();
        //        if (X is null) yield break;
        //        var divisionsAB = triangle.ABInternalPoints.ToArray();
        //        var divisionsBC = triangle.BCInternalPoints.ToArray();
        //        var divisionsCA = triangle.CAInternalPoints.ToArray();
        //        if (divisionsAB.Length == 1 && divisionsBC.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.B, X, divisionsAB[0]);
        //            yield return new FillTriangle(triangle, triangle.B, X, divisionsBC[0]);
        //            yield return new FillTriangle(triangle, divisionsAB[0], X, triangle.A);
        //            yield return new FillTriangle(triangle, divisionsBC[0], X, triangle.C);
        //            yield return new FillTriangle(triangle, X, triangle.A, triangle.C);

        //            yield break;
        //        }
        //        if (divisionsAB.Length == 1 && divisionsCA.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.A, X, divisionsAB[0]);
        //            yield return new FillTriangle(triangle, triangle.A, X, divisionsCA[0]);
        //            yield return new FillTriangle(triangle, divisionsAB[0], X, triangle.B);
        //            yield return new FillTriangle(triangle, divisionsCA[0], X, triangle.C);
        //            yield return new FillTriangle(triangle, X, triangle.B, triangle.C);

        //            yield break;
        //        }
        //        if (divisionsBC.Length == 1 && divisionsCA.Length == 1)
        //        {
        //            yield return new FillTriangle(triangle, triangle.C, X, divisionsCA[0]);
        //            yield return new FillTriangle(triangle, triangle.C, X, divisionsBC[0]);
        //            yield return new FillTriangle(triangle, divisionsBC[0], X, triangle.B);
        //            yield return new FillTriangle(triangle, divisionsCA[0], X, triangle.A);
        //            yield return new FillTriangle(triangle, X, triangle.A, triangle.B);

        //            yield break;
        //        }
        //    }
        //    yield break;
        //}

        public static int LoopError = 0;
        public static int SpurredLoopError = 0;

        private static void ComplexSegmentFills(IntermeshTriangle triangle)
        {
            var surfaceSet = triangle.CreateSurfaceSegmentSet();
            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, IntermeshPoint>(surfaceSet);

            ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain;
            try
            {
                chain = SurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint>.Create(collection);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Chaining Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                LoopError++;
                return;
            }

            if (chain.SpurredLoops.Any())
            {
                try
                {
                    chain = OpenSpurConnectChaining<PlanarFillingGroup, IntermeshPoint>.Create(chain);
                    chain = SpurLoopingChaining<PlanarFillingGroup, IntermeshPoint>.Create(chain);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Spurred Loop Error {triangle.Segments.Count} Triangle {triangle.Id} {e.Message}");
                    SpurredLoopError++;
                    return;
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
                Console.WriteLine(e.Message);
                return;
            }

            foreach (var filling in fillings)
            {
                var fillTriangle = new FillTriangle(filling.A.Reference, filling.A.Normal,
                    filling.B.Reference, filling.B.Normal, filling.C.Reference, filling.C.Normal, triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag);
                triangle.Fillings.Add(fillTriangle);
            }
        }
    }
}
