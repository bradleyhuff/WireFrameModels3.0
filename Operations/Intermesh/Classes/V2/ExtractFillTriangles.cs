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

namespace Operations.Intermesh.Classes.V2
{
    internal class ExtractFillTriangles
    {
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;
            ResetCounts();
            GetFillTriangles(triangles);

            //ConsoleLog.WriteLine($"No divisions {noDivisions}\nProspective Single Segments {singleSegment}\nSingle Segment One {singleSegmentOneDivision}\nSingle Segment Two {singleSegmentTwoDivision}\nProspective Double Segments {doubleSegment}\nDouble Segment One {doubleSegmentOneDivision}\nDouble Segment Two {doubleSegmentTwoDivision}\nComplex {complexDivision}");
            ConsoleLog.WriteLine($"Extract fill triangles. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static int noDivisions = 0;
        private static int singleSegment = 0;
        private static int singleSegmentOneDivision = 0;
        private static int singleSegmentTwoDivision = 0;
        private static int doubleSegment = 0;
        private static int doubleSegmentOneDivision = 0;
        private static int doubleSegmentTwoDivision = 0;
        private static int complexDivision = 0;

        private static void ResetCounts()
        {
            noDivisions = 0;
            singleSegment = 0;
            singleSegmentOneDivision = 0;
            singleSegmentTwoDivision = 0;
            doubleSegment = 0;
            doubleSegmentOneDivision = 0;
            doubleSegmentTwoDivision = 0;
            complexDivision = 0;
        }

        private static void GetFillTriangles(IEnumerable<IntermeshTriangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                if (!triangle.HasDivisions) { noDivisions++; triangle.Fillings.Add(new FillTriangle(triangle)); continue; }
                var internalSegments = triangle.InternalSegments.ToArray();
                if (internalSegments.Length == 1 && !internalSegments.Any(s => s.InternalDivisions > 0))
                {
                    singleSegment++;
                    if (SingleSegmentFill(triangle)) { continue; }
                }
                if (internalSegments.Length == 2 && !internalSegments.Any(s => s.InternalDivisions > 0))
                {
                    doubleSegment++;
                    if (DoubleSegmentFill(triangle)) { continue; }
                }

                ComplexSegmentFills(triangle);
            }
        }

        private static bool SingleSegmentFill(IntermeshTriangle triangle)
        {
            if (triangle.AB?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentOneDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.A, triangle.ABInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.B, triangle.ABInternalPoints[0]));
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentOneDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.B, triangle.BCInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.C, triangle.BCInternalPoints[0]));
                return true;
            }
            if (triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentOneDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.C, triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.A, triangle.CAInternalPoints[0]));
                return true;
            }

            if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[0], triangle.BCInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.BCInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.C, triangle.BCInternalPoints[0]));
                return true;
            }
            if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.C, triangle.CAInternalPoints[0]));
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0)
            {
                singleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[0], triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.BCInternalPoints[0], triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.A, triangle.BCInternalPoints[0]));
                return true;
            }
            //Console.WriteLine($"Single segment extra {(triangle.AB?.InternalDivisions) ?? 0} {(triangle.BC?.InternalDivisions) ?? 0} {(triangle.CA?.InternalDivisions) ?? 0}");
            return false;
        }

        private static bool DoubleSegmentFill(IntermeshTriangle triangle)
        {
            if (triangle.AB?.InternalDivisions == 2)
            {
                if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0], triangle.ABInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0], triangle.ABInternalPoints[1]));
                    return true;
                }
                if (triangle.BC?.InternalDivisions == 2 && (triangle.CA?.InternalDivisions ?? 0) == 0)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0], triangle.BCInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[1], triangle.BCInternalPoints[1], triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.BCInternalPoints[1], triangle.C));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.C, triangle.A));
                    return true;
                }
            }
            if (triangle.BC?.InternalDivisions == 2)
            {
                if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0], triangle.BCInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0], triangle.BCInternalPoints[1]));
                    return true;
                }
                if (triangle.CA?.InternalDivisions == 2 && (triangle.AB?.InternalDivisions ?? 0) == 0)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0], triangle.CAInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[1], triangle.CAInternalPoints[1], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.CAInternalPoints[1], triangle.A));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.A, triangle.B));
                    return true;
                }
            }
            if (triangle.CA?.InternalDivisions == 2)
            {
                if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0], triangle.CAInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0], triangle.CAInternalPoints[1]));
                    return true;
                }
                if (triangle.AB?.InternalDivisions == 2 && (triangle.BC?.InternalDivisions ?? 0) == 0)
                {
                    doubleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0], triangle.ABInternalPoints[1]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[1], triangle.ABInternalPoints[1], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.ABInternalPoints[1], triangle.B));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.B, triangle.C));
                    return true;
                }
            }

            var points = triangle.InternalSegments.SelectMany(s => s.DivisionPoints).ToArray();
            var groups = points.GroupBy(p => p.Id);
            var commonPoint = groups.SingleOrDefault(g => g.Count() == 2)?.FirstOrDefault();
            if (commonPoint is null) {
                //Console.WriteLine($"Double segment no common point {(triangle.AB?.InternalDivisions) ?? 0} {(triangle.BC?.InternalDivisions) ?? 0} {(triangle.CA?.InternalDivisions) ?? 0}");
                return false; 
            }

            if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                doubleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, commonPoint, triangle.ABInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, commonPoint, triangle.BCInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], commonPoint, triangle.A));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], commonPoint, triangle.C));
                triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.A, triangle.C));
                return true;
            }
            if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                doubleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, commonPoint, triangle.ABInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, commonPoint, triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], commonPoint, triangle.B));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], commonPoint, triangle.C));
                triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.B, triangle.C));
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0)
            {
                doubleSegmentTwoDivision++;
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, commonPoint, triangle.CAInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, commonPoint, triangle.BCInternalPoints[0]));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], commonPoint, triangle.B));
                triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], commonPoint, triangle.A));
                triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.A, triangle.B));
                return true;
            }

            //Console.WriteLine($"Double segment extra {(triangle.AB?.InternalDivisions) ?? 0} {(triangle.BC?.InternalDivisions) ?? 0} {(triangle.CA?.InternalDivisions) ?? 0}");

            return false;
        }

        public static int LoopError = 0;
        public static int SpurredLoopError = 0;

        private static void ComplexSegmentFills(IntermeshTriangle triangle)
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
