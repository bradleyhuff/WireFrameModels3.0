using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class SimpleFillStrategy :IFillStrategy
    {
        private int noDivisions = 0;
        private int singleSegment = 0;
        private int singleSegmentOneDivision = 0;
        private int singleSegmentTwoDivision = 0;
        private int doubleSegment = 0;
        private int doubleSegmentOneDivision = 0;
        private int doubleSegmentTwoDivision = 0;
        private int complexDivision = 0;

        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            if (!triangle.HasDivisions) { noDivisions++; triangle.Fillings.Add(new FillTriangle(triangle)); return; }
            var internalSegments = triangle.InternalSegments.ToArray();
            if (internalSegments.Length == 1 && !internalSegments.Any(s => s.InternalDivisions > 0))
            {
                singleSegment++;
                if (SingleSegmentCase(triangle, true)) { return; }
            }
            if (internalSegments.Length == 2 && !internalSegments.Any(s => s.InternalDivisions > 0))
            {
                doubleSegment++;
                if (DoubleSegmentCase(triangle, true)) { return; }
            }

            throw new InvalidOperationException($"Incorrect triangle fill strategy used.");
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            if (triangle.IsNearDegenerate) { return false; }
            if (!triangle.HasDivisions) { return true; }
            var internalSegments = triangle.InternalSegments.ToArray();
            if (internalSegments.Length == 1 && !internalSegments.Any(s => s.InternalDivisions > 0))
            {
                if (SingleSegmentCase(triangle, false)) { return true; }
            }
            if (internalSegments.Length == 2 && !internalSegments.Any(s => s.InternalDivisions > 0))
            {
                if (DoubleSegmentCase(triangle, false)) { return true; }
            }

            return false;
        }

        private bool SingleSegmentCase(IntermeshTriangle triangle, bool applyFill)
        {
            if (triangle.AB?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.A, triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.B, triangle.ABInternalPoints[0]));
                }
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.B, triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.C, triangle.BCInternalPoints[0]));
                }
                return true;
            }
            if (triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentOneDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.C, triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.A, triangle.CAInternalPoints[0]));
                }
                return true;
            }

            if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[0], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.C, triangle.BCInternalPoints[0]));
                }
                return true;
            }
            if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.C, triangle.CAInternalPoints[0]));
                }
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    singleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.BCInternalPoints[0], triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.A, triangle.BCInternalPoints[0]));
                }
                return true;
            }
            return false;
        }

        private bool DoubleSegmentCase(IntermeshTriangle triangle, bool applyFill)
        {
            if (triangle.AB?.InternalDivisions == 2)
            {
                if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0], triangle.ABInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0], triangle.ABInternalPoints[1]));
                    }
                    return true;
                }
                if (triangle.BC?.InternalDivisions == 2 && (triangle.CA?.InternalDivisions ?? 0) == 0)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[1], triangle.BCInternalPoints[0], triangle.BCInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[1], triangle.BCInternalPoints[1], triangle.ABInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.BCInternalPoints[1], triangle.C));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.C, triangle.A));
                    }
                    return true;
                }
            }
            if (triangle.BC?.InternalDivisions == 2)
            {
                if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0], triangle.BCInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], triangle.CAInternalPoints[0], triangle.BCInternalPoints[1]));
                    }
                    return true;
                }
                if (triangle.CA?.InternalDivisions == 2 && (triangle.AB?.InternalDivisions ?? 0) == 0)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[1], triangle.CAInternalPoints[0], triangle.CAInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[1], triangle.CAInternalPoints[1], triangle.BCInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.CAInternalPoints[1], triangle.A));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.A, triangle.B));
                    }
                    return true;
                }
            }
            if (triangle.CA?.InternalDivisions == 2)
            {
                if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.BCInternalPoints[0], triangle.CAInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], triangle.ABInternalPoints[0], triangle.CAInternalPoints[1]));
                    }
                    return true;
                }
                if (triangle.AB?.InternalDivisions == 2 && (triangle.BC?.InternalDivisions ?? 0) == 0)
                {
                    if (applyFill)
                    {
                        doubleSegmentOneDivision++;
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[1], triangle.ABInternalPoints[0], triangle.ABInternalPoints[1]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[1], triangle.ABInternalPoints[1], triangle.CAInternalPoints[0]));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.ABInternalPoints[1], triangle.B));
                        triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], triangle.B, triangle.C));
                    }
                    return true;
                }
            }

            var points = triangle.InternalSegments.SelectMany(s => s.DivisionPoints).ToArray();
            var groups = points.GroupBy(p => p.Id);
            var commonPoint = groups.SingleOrDefault(g => g.Count() == 2)?.FirstOrDefault();
            if (commonPoint is null)
            {
                return false;
            }

            if (triangle.AB?.InternalDivisions == 1 && triangle.BC?.InternalDivisions == 1 && (triangle.CA?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    doubleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, commonPoint, triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.B, commonPoint, triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], commonPoint, triangle.A));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], commonPoint, triangle.C));
                    triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.A, triangle.C));
                }
                return true;
            }
            if (triangle.AB?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.BC?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    doubleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, commonPoint, triangle.ABInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.A, commonPoint, triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.ABInternalPoints[0], commonPoint, triangle.B));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], commonPoint, triangle.C));
                    triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.B, triangle.C));
                }
                return true;
            }
            if (triangle.BC?.InternalDivisions == 1 && triangle.CA?.InternalDivisions == 1 && (triangle.AB?.InternalDivisions ?? 0) == 0)
            {
                if (applyFill)
                {
                    doubleSegmentTwoDivision++;
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, commonPoint, triangle.CAInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.C, commonPoint, triangle.BCInternalPoints[0]));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.BCInternalPoints[0], commonPoint, triangle.B));
                    triangle.Fillings.Add(new FillTriangle(triangle, triangle.CAInternalPoints[0], commonPoint, triangle.A));
                    triangle.Fillings.Add(new FillTriangle(triangle, commonPoint, triangle.A, triangle.B));
                }
                return true;
            }

            return false;
        }
    }
}
