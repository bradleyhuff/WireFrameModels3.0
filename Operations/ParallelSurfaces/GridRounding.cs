using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using BaseObjects.Transformations;
using Operations.Intermesh;
using Operations.Groupings.Basics;

namespace Operations.ParallelSurfaces
{
    public static class GridRounding
    {
        public static void FacePlatesRounding(this IWireFrameMesh mesh)
        {
            var positions = mesh.Positions.
                Where(p => !p.PositionNormals.Any(pn => pn.Triangles.Any(t => IsSurfaceTriangle(t)))).
                Where(p => p.PositionNormals.Any(pn => pn.Triangles.Any(t => IsEdgeTriangle(t)))).ToArray();
 
            var surroundingPositions = new Dictionary<int, Position[]>();
            foreach (var position in positions)
            {
                surroundingPositions[position.Id] = SurroundingPositions(position).ToArray();
            }

            var edgePositions = positions.Where(p => p.PositionNormals.Any(pn => pn.Triangles.Any(t => !IsBaseTriangle(t))));
            var cornerPositions = edgePositions.Where(p => p.PositionNormals.Count(pn => pn.Triangles.Any(t => !IsBaseTriangle(t))) > 2);

            var edgePairs = PullEdgePairs(edgePositions, surroundingPositions).ToArray();

            var edgePlots = new Combination2Dictionary<EdgePlot>();
            var edgeSegments = PullEdgeSegments(edgePairs, surroundingPositions, edgePlots).ToArray();
            var cornerBlocks = PullCornerBlocks(cornerPositions, surroundingPositions, edgePlots).ToArray();

            Console.WriteLine($"Edge positions {edgePositions.Count()} Corner positions {cornerPositions.Count()} Edge pairs {edgePairs.Length}");

            foreach (var edgeSegment in edgeSegments.Where(e => e[0].IsPolar && e[1].IsPolar)) { DetermineSplitVectors(edgeSegment); }
            foreach (var edgeSegment in edgeSegments) { PlotEdgeSegment(mesh, edgeSegment); }
            foreach (var cornerBlock in cornerBlocks) { PlotCornerBlock(mesh, cornerBlock); }

            mesh.Intermesh();
            RemoveInternalSurfaces(mesh);
        }

        private static bool IsSurfaceTriangle(PositionTriangle triangle)
        {
            return IsSurfaceTriangle(triangle.Trace);
        }
        private static bool IsSurfaceTriangle(string trace)
        {
            return !string.IsNullOrEmpty(trace) && trace.Length > 0 && trace[0] == 'S';
        }
        private static bool IsBaseTriangle(PositionTriangle triangle)
        {
            return IsBaseTriangle(triangle.Trace);
        }
        private static bool IsBaseTriangle(string trace)
        {
            return !string.IsNullOrEmpty(trace) && trace.Length > 0 && trace[0] == 'B';
        }
        private static bool IsEdgeTriangle(PositionTriangle triangle)
        {
            return IsEdgeTriangle(triangle.Trace);
        }
        private static bool IsEdgeTriangle(string trace)
        {
            return !string.IsNullOrEmpty(trace) && trace.Length > 0 && (trace[0] == 'E' || trace[0] == 'F');
        }

        private static IEnumerable<Position> SurroundingPositions(Position position)
        {
            return position.Triangles.SelectMany(t => t.Positions.Select(pn => pn.PositionObject)).DistinctBy(p => p.Id).Where(p => p.Id != position.Id);
        }

        private static IEnumerable<Position[]> PullEdgePairs(IEnumerable<Position> edgePositions, Dictionary<int, Position[]> surroundingPositions)
        {
            var usedKeys = new Combination2Dictionary<bool>();
            var edgePositionTable = edgePositions.ToDictionary(p => p.Id, p => true);

            foreach (var edgePosition in edgePositions)
            {
                var adjacentEdgePositions = surroundingPositions[edgePosition.Id].Where(p => edgePositionTable.ContainsKey(p.Id));
                foreach (var adjacentEdgePosition in adjacentEdgePositions)
                {
                    var key = new Combination2(edgePosition.Id, adjacentEdgePosition.Id);
                    if (usedKeys.ContainsKey(key)) { continue; }

                    yield return new Position[] { edgePosition, adjacentEdgePosition };
                    usedKeys[key] = true;
                }
            }
        }

        private static IEnumerable<EdgePlot[]> PullEdgeSegments(IEnumerable<Position[]> edgePairs, Dictionary<int, Position[]> surroundingPositions, Combination2Dictionary<EdgePlot> edgePlots)
        {
            foreach (var edgePair in edgePairs)
            {
                var surfaceGroupsA = GetSurfacePoints(edgePair[0], surroundingPositions);
                var surfaceGroupsB = GetSurfacePoints(edgePair[1], surroundingPositions);

                RemoveNonMatchingGroups(surfaceGroupsA, surfaceGroupsB);
                if (surfaceGroupsA.Count != 2) { continue; }

                var keys = surfaceGroupsA.Keys.OrderBy(g => g).ToArray();

                var keyA = new Combination2(surfaceGroupsA[keys[0]].Id, surfaceGroupsA[keys[1]].Id);
                var keyB = new Combination2(surfaceGroupsB[keys[0]].Id, surfaceGroupsB[keys[1]].Id);

                if (!edgePlots.ContainsKey(keyA))
                {
                    var edgePlot = new EdgePlot(surfaceGroupsA[keys[0]], edgePair[0], surfaceGroupsA[keys[1]]);
                    edgePlots[keyA] = edgePlot;
                }

                if (!edgePlots.ContainsKey(keyB))
                {
                    var edgePlot = new EdgePlot(surfaceGroupsB[keys[0]], edgePair[1], surfaceGroupsB[keys[1]]);
                    edgePlots[keyB] = edgePlot;
                }

                yield return new EdgePlot[] { edgePlots[keyA], edgePlots[keyB] };
            }
            yield break;
        }

        private static IEnumerable<EdgePlot[]> PullCornerBlocks(IEnumerable<Position> corners, Dictionary<int, Position[]> surroundingPositions, Combination2Dictionary<EdgePlot> edgePlots)
        {
            foreach (var corner in corners)
            {
                var surfaceGroups = GetSurfacePoints(corner, surroundingPositions);
                if (surfaceGroups.Count < 3) { continue; }
                if (surfaceGroups.Count == 3)
                {
                    var surfacePositions = surfaceGroups.Values.ToArray();
                    var edgePlotA = edgePlots[surfacePositions[0].Id, surfacePositions[1].Id];
                    var edgePlotB = edgePlots[surfacePositions[1].Id, surfacePositions[2].Id];
                    var edgePlotC = edgePlots[surfacePositions[0].Id, surfacePositions[2].Id];
                    yield return new EdgePlot[] { edgePlotA, edgePlotB, edgePlotC };
                    continue;
                }

                var edgePositions = new Dictionary<string, Position>();
                foreach (var position in surfaceGroups.Values)
                {
                    var edgeTrace = position.Triangles.First(IsEdgeTriangle).Trace;
                    edgePositions[edgeTrace] = position;
                }

                var edgeRelations = new Dictionary<string, List<string>>();
                foreach (var group in corner.Triangles.GroupBy(t => t.Trace).Where(g => IsEdgeTriangle(g.Key)))
                {
                    var relations = group.SelectMany(g => g.AllAdjacents).Select(t => t.Trace).
                        Where(t => IsEdgeTriangle(t) && t != group.Key).Distinct().ToList();
                    edgeRelations[group.Key] = relations;
                }

                var orderedPositions = IterateEdges(edgeRelations).Select(e => edgePositions[e]).ToArray();
                var positionPairs = PullPositionPairs(orderedPositions);

                var cornerPlots = new List<EdgePlot>();
                foreach (var positionPair in positionPairs)
                {
                    var edgePlot = edgePlots[positionPair[0].Id, positionPair[1].Id];
                    cornerPlots.Add(edgePlot);
                }
                yield return cornerPlots.ToArray();
            }
            yield break;
        }

        private class EdgePlot
        {
            public EdgePlot(Position upperSurface, Position position, Position lowerSurface)
            {
                UpperSurface = upperSurface;
                Position = position;
                LowerSurface = lowerSurface;

                UpperVector = UpperSurface.Point - Position.Point;
                LowerVector = LowerSurface.Point - Position.Point;
                IsPolar = Vector3D.ArePolar(UpperVector, LowerVector);
            }

            private List<Vector3D> _splits = new List<Vector3D>();
            public Position UpperSurface { get; }
            public Position Position { get; }
            public Position LowerSurface { get; }
            public IEnumerable<Position> SurfacePositions { get { yield return UpperSurface; yield return LowerSurface; } }
            public Vector3D UpperVector { get; }
            public Vector3D LowerVector { get; }
            public bool IsPolar { get; }
            public IReadOnlyList<Vector3D> Splits { get { return _splits; } }
            public void AddSplit(Vector3D split)
            {
                if (_splits.Any(s => Vector3D.AreParallel(s, split))) { return; }
                _splits.Add(split);
            }

            public Vector3D GetSplit(Vector3D cross)
            {
                return Splits.MaxBy(s => Math.Abs(Vector3D.Dot(s, cross)));
            }
        }

        private static void PlotEdgeSegment(IWireFrameMesh mesh, EdgePlot[] edgeSegment)
        {
            var plotA = edgeSegment[0];
            var plotB = edgeSegment[1];

            int steps = 8;

            if (plotA.Splits.Any() && plotB.Splits.Any())
            {
                var cross = Vector3D.Cross(plotA.Position.Point - plotB.Position.Point, plotA.UpperVector);

                var splitA = plotA.GetSplit(cross);
                var splitB = plotB.GetSplit(cross);

                PlotEdgeSegment(mesh, steps, plotA.Position.Point, plotA.UpperVector, splitA, plotB.Position.Point, plotB.UpperVector, splitB);
                PlotEdgeSegment(mesh, steps, plotA.Position.Point, splitA, plotA.LowerVector, plotB.Position.Point, splitB, plotB.LowerVector);
                return;
            }
            if (plotA.IsPolar || plotB.IsPolar) { return; }
            PlotEdgeSegment(mesh, steps, plotA.Position.Point, plotA.UpperVector, plotA.LowerVector, plotB.Position.Point, plotB.UpperVector, plotB.LowerVector);
        }

        private static void DetermineSplitVectors(EdgePlot[] edgePlot)
        {
            if (edgePlot.Length != 2 || !edgePlot[0].IsPolar || !edgePlot[1].IsPolar) { return; }

            var crossVector = edgePlot[0].Position.Point - edgePlot[1].Position.Point;
            var splitA = Vector3D.Cross(edgePlot[0].UpperVector, crossVector).Direction;
            var splitB = Vector3D.Cross(edgePlot[1].UpperVector, crossVector).Direction;

            splitA = edgePlot[0].Position.PositionNormals.MaxBy(pn => Math.Abs(Vector3D.Dot(pn.Normal, splitA))).Normal.Direction;
            splitB = edgePlot[1].Position.PositionNormals.MaxBy(pn => Math.Abs(Vector3D.Dot(pn.Normal, splitB))).Normal.Direction;

            splitA = edgePlot[0].UpperVector.Magnitude * splitA;
            splitB = edgePlot[1].UpperVector.Magnitude * splitB;

            edgePlot[0].AddSplit(splitA);
            edgePlot[1].AddSplit(splitB);
        }

        private static void PlotCornerBlock(IWireFrameMesh mesh, EdgePlot[] cornerBlock)
        {
            if (cornerBlock.Length < 3) { return; }

            int steps = 8;

            var surfacePositions = cornerBlock.SelectMany(c => c.SurfacePositions).DistinctBy(p => p.Id);
            var surfaceVectors = surfacePositions.Select(s => s.Point - cornerBlock[0].Position.Point).ToArray();

            if (cornerBlock.Length == 3)
            {
                var polarEdgePlot = cornerBlock.SingleOrDefault(c => c.IsPolar);
                if (polarEdgePlot != null)
                {
                    if (polarEdgePlot.Splits.Any())
                    {
                        var baseVector = surfaceVectors.Single(s => !Vector3D.AreParallel(s, polarEdgePlot.UpperVector) && !Vector3D.AreParallel(s, polarEdgePlot.LowerVector));
                        PlotCornerBlock(mesh, steps, cornerBlock[0].Position.Point, polarEdgePlot.UpperVector, polarEdgePlot.Splits.Single(), baseVector);
                        PlotCornerBlock(mesh, steps, cornerBlock[0].Position.Point, polarEdgePlot.LowerVector, polarEdgePlot.Splits.Single(), baseVector);
                    }
                    else
                    {
                        var baseVector = surfaceVectors.Single(s => !Vector3D.AreParallel(s, polarEdgePlot.UpperVector) && !Vector3D.AreParallel(s, polarEdgePlot.LowerVector));
                        var crossA = Vector3D.Cross(baseVector.Direction, polarEdgePlot.UpperVector.Direction);
                        var crossB = -crossA;

                        var cross = crossA;
                        var basePosition = surfacePositions.Single(s => s.Id != polarEdgePlot.UpperSurface.Id && s.Id != polarEdgePlot.LowerSurface.Id);
                        var surfaceDirection = GetSurfaceDirection(basePosition);
                        if (Vector3D.Dot(crossB, surfaceDirection) < 0) { cross = crossB; }
                        var splitVector = baseVector.Magnitude * cross;
                        PlotCornerBlock(mesh, steps, cornerBlock[0].Position.Point, polarEdgePlot.UpperVector, splitVector, baseVector);
                        PlotCornerBlock(mesh, steps, cornerBlock[0].Position.Point, polarEdgePlot.LowerVector, splitVector, baseVector);
                    }
                    return;
                }
                {
                    PlotCornerBlock(mesh, steps, cornerBlock[0].Position.Point, surfaceVectors[0], surfaceVectors[1], surfaceVectors[2]);
                    return;
                }
            }
            {
                var radius = surfaceVectors.Select(v => v.Magnitude).Average();
                var poleVector = radius * Vector3D.Sum(surfaceVectors).Direction;

                foreach (var edgePlot in cornerBlock)
                {
                    PlotCornerBlock(mesh, 8, cornerBlock[0].Position.Point, poleVector, edgePlot.UpperVector, edgePlot.LowerVector);
                }
            }
        }

        private static void PlotEdgeSegment(IWireFrameMesh mesh, int steps,
            Point3D pointA, Vector3D vectorA1, Vector3D vectorA2,
            Point3D pointB, Vector3D vectorB1, Vector3D vectorB2)
        {
            var plotA = VectorTransform3D.PlanarArcPlot(vectorA1, vectorA2, steps).Select(v => new Ray3D(pointA + v, v)).ToArray();
            var plotB = VectorTransform3D.PlanarArcPlot(vectorB1, vectorB2, steps).Select(v => new Ray3D(pointB + v, v)).ToArray();

            for (int i = 0; i < plotA.Length; i++)
            {
                mesh.AddPoint(plotA[i].Point, plotA[i].Normal);
                mesh.AddPoint(plotB[i].Point, plotB[i].Normal);
                SetRadiusTrace(mesh.EndRow());
            }
            SetRadiusTrace(mesh.EndGrid());
        }

        private static void SetRadiusTrace(IEnumerable<PositionTriangle> traces)
        {
            foreach (var t in traces) { t.Trace = "R"; }
        }

        private static IEnumerable<string> IterateEdges(Dictionary<string, List<string>> edges)
        {
            if (!edges.Any()) { yield break; }

            var firstEdge = edges.Keys.First();
            var currentEdge = firstEdge;
            var lastEdge = string.Empty;

            do
            {
                yield return currentEdge;
                var nextEdge = edges[currentEdge].First(e => e != lastEdge);
                lastEdge = currentEdge;
                currentEdge = nextEdge;
            }
            while (firstEdge != currentEdge);
        }

        private static IEnumerable<Position[]> PullPositionPairs(Position[] orderedPositions)
        {
            if (!orderedPositions.Any()) { yield break; }
            for (int i = 0; i < orderedPositions.Length - 1; i++)
            {
                yield return [orderedPositions[i], orderedPositions[i + 1]];
            }
            yield return [orderedPositions[orderedPositions.Length - 1], orderedPositions[0]];
        }

        private static void PlotCornerBlock(IWireFrameMesh mesh, int steps, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            var triangle = VectorTransform3D.CurvedSurfacePlot(n0, n1, n2, steps);

            for (int i = 0; i < triangle.Length - 1; i++)
            {
                var row = triangle[i];
                var nextRow = triangle[i + 1];

                for (int j = 0; j < row.Length - 1; j++)
                {
                    var a = row[j];
                    var b = row[j + 1];
                    var c = nextRow[j];
                    mesh.AddTriangle(point + a, a, point + b, b, point + c, c, "R", 0);

                    if (j < row.Length - 2)
                    {
                        var d = nextRow[j + 1];
                        mesh.AddTriangle(point + b, b, point + c, c, point + d, d, "R", 0);
                    }
                }
            }
        }

        private static Dictionary<string, Position> GetSurfacePoints(Position position, Dictionary<int, Position[]> surroundingPositions)
        {
            var surfacePoints = surroundingPositions[position.Id].Where(p => p.Triangles.Any(t => IsSurfaceTriangle(t)));
            var surfaceGroups = surfacePoints.GroupBy(p => p.Triangles.Where(t => IsSurfaceTriangle(t)).First().Trace).ToArray();

            var nearestPointTable = new Dictionary<string, Position>();
            foreach (var group in surfaceGroups)
            {
                nearestPointTable[group.Key] = GetNearestPosition(position, group.ToArray());
            }

            return nearestPointTable;
        }

        private static Position GetNearestPosition(Position origin, params Position[] positions)
        {
            Position nearestPoint = null;
            double nearestDistance = double.MaxValue;

            foreach (var position in positions)
            {
                double distance = Point3D.Distance(origin.Point, position.Point);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = position;
                }
            }

            return nearestPoint;
        }

        private static void RemoveNonMatchingGroups(Dictionary<string, Position> a, Dictionary<string, Position> b)
        {
            var keysToRemove = new List<string>();
            foreach (var key in a.Keys)
            {
                if (!b.ContainsKey(key)) { keysToRemove.Add(key); }
            }
            foreach (var key in b.Keys)
            {
                if (!a.ContainsKey(key)) { keysToRemove.Add(key); }
            }

            foreach (var key in keysToRemove)
            {
                a.Remove(key);
                b.Remove(key);
            }
        }

        private static Vector3D GetSurfaceDirection(Position position)
        {
            var surfaceTriangles = position.Triangles.Where(t => t.Trace is not null && t.Trace.Any() && t.Trace[0] == 'S');
            if (!surfaceTriangles.Any()) { return null; }

            var aggregatePoint = Point3D.Average(surfaceTriangles.Select(t => t.Triangle.Center));

            return (aggregatePoint - position.Point).Direction;
        }

        private static void RemoveInternalSurfaces(IWireFrameMesh mesh)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();

            foreach(var surface in surfaces)
            {
                var isInternalSurface = surface.Triangles.Any(t => t.Trace[0] == 'R') && !surface.PerimeterEdges.Any(e => e.Triangles.Any(t => t.Trace[0] == 'S'));
                if (isInternalSurface)
                {
                    mesh.RemoveAllTriangles(surface.Triangles);
                }                
            }
        }
    }
}
