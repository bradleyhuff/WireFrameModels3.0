using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;
using E = BasicObjects.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using BaseObjects.Transformations;
using Operations.SetOperators;

namespace Operations.ParallelSurfaces
{
    public static class GridRounding
    {
        public static void FacePlatesRounding(this IWireFrameMesh mesh)
        {
            var positions = mesh.Positions.Where(p => p.PositionNormals.All(pn => pn.Triangles.Any(t => IsEdgeTriangle(t)))).ToArray();
            var edgePositions = positions.Where(p => p.Cardinality > 1);
            var cornerPositions = edgePositions.Where(p => p.Cardinality > 2);

            var surroundingPositions = new Dictionary<int, Position[]>();
            foreach (var edgePosition in edgePositions)
            {
                surroundingPositions[edgePosition.Id] = SurroundingPositions(edgePosition).ToArray();
            }

            var edgePairs = GetEdgePairs(edgePositions, surroundingPositions).ToArray();

            Console.WriteLine($"Edge positions {edgePositions.Count()} Corner positions {cornerPositions.Count()} Edge pairs {edgePairs.Length}");

            foreach (var edgePair in edgePairs) { PlotEdgeSegment(edgePair, surroundingPositions, mesh); }
            foreach (var corner in cornerPositions) { PlotCorner(corner, surroundingPositions, mesh); }

        }

        private static bool IsEdgeTriangle(PositionTriangle triangle)
        {
            return !string.IsNullOrEmpty(triangle.Trace) && triangle.Trace.Length > 0 && triangle.Trace[0] == 'E' || triangle.Trace[0] == 'F';
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

        private static void PlotEdgeSegment(Position[] edgePair, Dictionary<int, Position[]> surroundingPositions, IWireFrameMesh mesh)
        {
            var surfaceGroupsA = GetSurfacePoints(edgePair[0], surroundingPositions);
            var surfaceGroupsB = GetSurfacePoints(edgePair[1], surroundingPositions);

            //Console.WriteLine($"Surface groups A {edgePair[0].Id} {string.Join(",", surfaceGroupsA.Select(g => g.Key))} Surface groups B {edgePair[1].Id} {string.Join(",", surfaceGroupsB.Select(g => g.Key))}");

            GetMatchingGroups(surfaceGroupsA, surfaceGroupsB);
            //Console.WriteLine($"Surface groups A {edgePair[0].Id} {string.Join(",", surfaceGroupsA.Select(g => g.Key))} Surface groups B {edgePair[1].Id} {string.Join(",", surfaceGroupsB.Select(g => g.Key))}", ConsoleColor.Green);
            if (surfaceGroupsA.Count != 2) { return; }

            var keys = surfaceGroupsA.Keys.ToArray();
            var radiiA = Radii(edgePair[0].Point, surfaceGroupsA.Values.Select(v => v.Point).ToArray()).ToArray();
            var radiiB = Radii(edgePair[1].Point, surfaceGroupsB.Values.Select(v => v.Point).ToArray()).ToArray();

            if (!E.Double.IsEqual(radiiA[0], radiiA[1])) { return; }
            if (!E.Double.IsEqual(radiiB[0], radiiB[1])) { return; }

            var vectorA1 = (surfaceGroupsA[keys[0]].Point - edgePair[0].Point).Direction;
            var vectorA2 = (surfaceGroupsA[keys[1]].Point - edgePair[0].Point).Direction;

            var vectorB1 = (surfaceGroupsB[keys[0]].Point - edgePair[1].Point).Direction;
            var vectorB2 = (surfaceGroupsB[keys[1]].Point - edgePair[1].Point).Direction;

            int steps = 8;
            BuildEdgeSegment(mesh, steps, edgePair[0].Point,  radiiA[0], vectorA1, vectorA2, edgePair[1].Point, radiiA[1], vectorB1, vectorB2);
        }

        private static void PlotCorner(Position corner, Dictionary<int, Position[]> surroundingPositions, IWireFrameMesh mesh)
        {
            var surfaceGroups = GetSurfacePoints(corner, surroundingPositions);
            if (surfaceGroups.Count != 3) { return; }

            var radii = Radii(corner.Point, surfaceGroups.Values.Select(v => v.Point).ToArray()).ToArray();
            if (!E.Double.IsEqual(radii[0], radii[1])) { return; }
            if (!E.Double.IsEqual(radii[0], radii[2])) { return; }

            var keys = surfaceGroups.Keys.ToArray();
            var vector1 = (surfaceGroups[keys[0]].Point - corner.Point).Direction;
            var vector2 = (surfaceGroups[keys[1]].Point - corner.Point).Direction;
            var vector3 = (surfaceGroups[keys[2]].Point - corner.Point).Direction;

            int steps = 8;
            BuildCorner(mesh, radii[0], steps, corner.Point, vector1, vector2, vector3);
        }

        private static void BuildEdgeSegment(IWireFrameMesh mesh, int steps, Point3D pointA, double radiusA, Vector3D vectorA1, Vector3D vectorA2, Point3D pointB, double radiusB, Vector3D vectorB1, Vector3D vectorB2)
        {
            var arcA = VectorTransform3D.BarycentricArc(vectorA1, vectorA2, steps).ToArray();
            var arcB = VectorTransform3D.BarycentricArc(vectorB1, vectorB2, steps).ToArray();

            for (int i = 0; i < arcA.Length; i++)
            {
                mesh.AddPoint(pointA + radiusA * arcA[i], arcA[i]);
                mesh.AddPoint(pointB + radiusB * arcB[i], arcB[i]);
                mesh.EndRow();
            }
            mesh.EndGrid();
        }

        private static void BuildCorner(IWireFrameMesh mesh, double radius, int steps, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            var triangle = VectorTransform3D.BarycentricSteradian(n0, n1, n2, steps);

            for (int i = 0; i < triangle.Length - 1; i++)
            {
                var row = triangle[i];
                var nextRow = triangle[i + 1];

                for (int j = 0; j < row.Length - 1; j++)
                {
                    AddTriangle(mesh, radius, point, row[j], row[j + 1], nextRow[j]);
                    if (j < row.Length - 2) { AddTriangle(mesh, radius, point, row[j + 1], nextRow[j], nextRow[j + 1]); }
                }
            }
        }

        private static void AddTriangle(IWireFrameMesh mesh, double radius, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            mesh.AddTriangle(point + radius * n0.Direction, n0.Direction,
                point + radius * n1.Direction, n1.Direction,
                point + radius * n2.Direction, n2.Direction);
        }

        private static IEnumerable<double> Radii(Point3D origin, params Point3D[] positions)
        {
            foreach (var position in positions)
            {
                yield return Point3D.Distance(origin, position);
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

        private static void GetMatchingGroups(Dictionary<string, Position> a, Dictionary<string, Position> b)
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
