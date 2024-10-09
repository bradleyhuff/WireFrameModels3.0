using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;
using E = BasicObjects.Math;
using BaseObjects.Transformations;
using Operations.Intermesh;

namespace Operations.ParallelSurfaces
{
    public static class GridRounding
    {
        public static void FacePlatesRounding(this IWireFrameMesh mesh)
        {
            var positions = mesh.Positions.
                Where(p => !p.PositionNormals.Any(pn => pn.Triangles.Any(t => t.Trace[0] == 'S'))).
                Where(p => p.PositionNormals.Any(pn => pn.Triangles.Any(t => IsEdgeTriangle(t)))).ToArray();
            var edgePositions = positions.Where(p => p.PositionNormals.Count(pn => !pn.Triangles.All(t => t.Trace[0] == 'B')) > 1);
            var cornerPositions = edgePositions.Where(p => p.PositionNormals.Count(pn => !pn.Triangles.All(t => t.Trace[0] == 'B')) > 2);

            var surroundingPositions = new Dictionary<int, Position[]>();
            foreach (var edgePosition in edgePositions)
            {
                surroundingPositions[edgePosition.Id] = SurroundingPositions(edgePosition).ToArray();
            }

            var edgePairs = GetEdgePairs(edgePositions, surroundingPositions).ToArray();

            Console.WriteLine($"Edge positions {edgePositions.Count()} Corner positions {cornerPositions.Count()} Edge pairs {edgePairs.Length}");

            foreach (var edgePair in edgePairs) { PlotEdgeSegment(mesh, edgePair, surroundingPositions); }
            foreach (var corner in cornerPositions) { PlotCornerBlock(mesh, corner, surroundingPositions); }

            mesh.Intermesh();

            //var test = mesh.CreateNewInstance();
            //test.AddRangeTriangles(mesh.Triangles.Where(t => !string.IsNullOrWhiteSpace(t.Trace) &&t.Trace.Any() && t.Trace == "F1"));
            //WavefrontFile.Export(test, "Wavefront/ElbowTriangles");
            //test = mesh.CreateNewInstance();
            //var sideTriangles = mesh.Triangles.Where(t => !string.IsNullOrWhiteSpace(t.Trace) && t.Trace.Any() && t.Trace == "E5");
            //test.AddRangeTriangles(mesh.Triangles.Where(t => !string.IsNullOrWhiteSpace(t.Trace) && t.Trace.Any() && t.Trace == "E5"));
            //WavefrontFile.Export(test, "Wavefront/SideTriangles");

        }

        private static bool IsEdgeTriangle(PositionTriangle triangle)
        {
            return !string.IsNullOrEmpty(triangle.Trace) && triangle.Trace.Length > 0 && (triangle.Trace[0] == 'E' || triangle.Trace[0] == 'F');
        }

        private static IEnumerable<Position> SurroundingPositions(Position position)
        {
            return position.Triangles.SelectMany(t => t.Positions.Select(pn => pn.PositionObject)).DistinctBy(p => p.Id).Where(p => p.Id != position.Id);
        }

        private static IEnumerable<Position[]> GetEdgePairs(IEnumerable<Position> edgePositions, Dictionary<int, Position[]> surroundingPositions)
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

        private static void PlotEdgeSegment(IWireFrameMesh mesh, Position[] edgePair, Dictionary<int, Position[]> surroundingPositions)
        {
            var surfaceGroupsA = GetSurfacePoints(edgePair[0], surroundingPositions);
            var surfaceGroupsB = GetSurfacePoints(edgePair[1], surroundingPositions);

            RemoveNonMatchingGroups(surfaceGroupsA, surfaceGroupsB);
            if (surfaceGroupsA.Count != 2) { return; }

            var keys = surfaceGroupsA.Keys.ToArray();
            var vectorA1 = (surfaceGroupsA[keys[0]].Point - edgePair[0].Point);
            var vectorA2 = (surfaceGroupsA[keys[1]].Point - edgePair[0].Point);

            var vectorB1 = (surfaceGroupsB[keys[0]].Point - edgePair[1].Point);
            var vectorB2 = (surfaceGroupsB[keys[1]].Point - edgePair[1].Point);

            int steps = 8;
            PlotEdgeSegment(mesh, steps, edgePair[0].Point, vectorA1, vectorA2, edgePair[1].Point, vectorB1, vectorB2);
        }

        private static void PlotEdgeSegment(IWireFrameMesh mesh, int steps, Point3D pointA, Vector3D vectorA1, Vector3D vectorA2, Point3D pointB, Vector3D vectorB1, Vector3D vectorB2)
        {
            var plotA = VectorTransform3D.PlanarArc(vectorA1, vectorA2, steps).Select(v => new Ray3D(pointA + v, v)).ToArray();
            var plotB = VectorTransform3D.PlanarArc(vectorB1, vectorB2, steps).Select(v => new Ray3D(pointB + v, v)).ToArray();

            for (int i = 0; i < plotA.Length; i++)
            {
                mesh.AddPoint(plotA[i].Point, plotA[i].Normal);
                mesh.AddPoint(plotB[i].Point, plotB[i].Normal);
                mesh.EndRow();
            }
            mesh.EndGrid();
        }

        private static void PlotCornerBlock(IWireFrameMesh mesh, Position corner, Dictionary<int, Position[]> surroundingPositions)
        {
            var surfaceGroups = GetSurfacePoints(corner, surroundingPositions);
            if (surfaceGroups.Count != 3) { return; }

            int steps = 8;

            var keys = surfaceGroups.Keys.ToArray();
            var vector1 = surfaceGroups[keys[0]].Point - corner.Point;
            var vector2 = surfaceGroups[keys[1]].Point - corner.Point;
            var vector3 = surfaceGroups[keys[2]].Point - corner.Point;

            PlotCornerBlock(mesh, steps, corner.Point, vector1, vector2, vector3);
        }

        private static void PlotCornerBlock(IWireFrameMesh mesh, int steps, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            var triangle = VectorTransform3D.CurvedSurfaceTriangle(n0, n1, n2, steps);

            for (int i = 0; i < triangle.Length - 1; i++)
            {
                var row = triangle[i];
                var nextRow = triangle[i + 1];

                for (int j = 0; j < row.Length - 1; j++)
                {
                    mesh.AddTriangle(point + row[j], row[j], point + row[j + 1], row[j + 1], point + nextRow[j], nextRow[j]);
                    if (j < row.Length - 2) { mesh.AddTriangle(point + row[j + 1], row[j + 1], point + nextRow[j], nextRow[j], point + nextRow[j + 1], nextRow[j + 1]); }
                }
            }
        }

        private static Dictionary<string, Position> GetSurfacePoints(Position position, Dictionary<int, Position[]> surroundingPositions)
        {
            var surfacePoints = surroundingPositions[position.Id].Where(p => p.Triangles.Any(t => t.Trace[0] == 'S'));
            var surfaceGroups = surfacePoints.GroupBy(p => p.Triangles.Where(t => t.Trace[0] == 'S').First().Trace).ToArray();

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
    }
}
