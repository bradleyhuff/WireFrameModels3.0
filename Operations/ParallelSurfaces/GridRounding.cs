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

            foreach (var edgePair in edgePairs) { PlotEdgeSegment(edgePair, surroundingPositions, mesh); }
            foreach (var corner in cornerPositions) { PlotCorner(corner, surroundingPositions, mesh); }

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

        private static void PlotEdgeSegment(Position[] edgePair, Dictionary<int, Position[]> surroundingPositions, IWireFrameMesh mesh)
        {
            var surfaceGroupsA = GetSurfacePoints(edgePair[0], surroundingPositions);
            var surfaceGroupsB = GetSurfacePoints(edgePair[1], surroundingPositions);

            //Console.WriteLine($"Surface groups A {edgePair[0].Id} {string.Join(",", surfaceGroupsA.Select(g => g.Key))} Surface groups B {edgePair[1].Id} {string.Join(",", surfaceGroupsB.Select(g => g.Key))}");

            GetMatchingGroups(surfaceGroupsA, surfaceGroupsB);
            //Console.WriteLine($"Surface groups A {edgePair[0].Id} {string.Join(",", surfaceGroupsA.Select(g => g.Key))} Surface groups B {edgePair[1].Id} {string.Join(",", surfaceGroupsB.Select(g => g.Key))}", ConsoleColor.Green);
            if (surfaceGroupsA.Count != 2) { /*Console.WriteLine($"Surface groups {surfaceGroupsA.Count} {surfaceGroupsB.Count}");*/ return; }

            var keys = surfaceGroupsA.Keys.ToArray();
            var vectorA1 = (surfaceGroupsA[keys[0]].Point - edgePair[0].Point);
            var vectorA2 = (surfaceGroupsA[keys[1]].Point - edgePair[0].Point);

            var vectorB1 = (surfaceGroupsB[keys[0]].Point - edgePair[1].Point);
            var vectorB2 = (surfaceGroupsB[keys[1]].Point - edgePair[1].Point);

            int steps = 8;
            BuildEdgeSegment(mesh, steps, edgePair[0].Point, vectorA1, vectorA2, edgePair[1].Point, vectorB1, vectorB2);
        }

        private static void PlotCorner(Position corner, Dictionary<int, Position[]> surroundingPositions, IWireFrameMesh mesh)
        {
            var surfaceGroups = GetSurfacePoints(corner, surroundingPositions);
            if (surfaceGroups.Count != 3) { return; }

            int steps = 8;

            var radii = Radii(corner.Point, surfaceGroups.Values.Select(v => v.Point).ToArray()).ToArray();
            if (!E.Double.IsEqual(radii[0], radii[1]) ||
                !E.Double.IsEqual(radii[0], radii[2])) {
                EllipsoidSection(mesh, steps, corner.Point, surfaceGroups.Values.Select(v => v.Point - corner.Point).ToArray());
                return; 
            }

            var keys = surfaceGroups.Keys.ToArray();
            var vector1 = (surfaceGroups[keys[0]].Point - corner.Point).Direction;
            var vector2 = (surfaceGroups[keys[1]].Point - corner.Point).Direction;
            var vector3 = (surfaceGroups[keys[2]].Point - corner.Point).Direction;

            
            BuildCorner(mesh, radii[0], steps, corner.Point, vector1, vector2, vector3);
        }

        private static void BuildEdgeSegment(IWireFrameMesh mesh, int steps, Point3D pointA, Vector3D vectorA1, Vector3D vectorA2, Point3D pointB, Vector3D vectorB1, Vector3D vectorB2)
        {
            if (!E.Double.IsEqual(vectorA1.Magnitude, vectorA2.Magnitude) || 
                !E.Double.IsEqual(vectorB1.Magnitude, vectorB2.Magnitude))
            {
                var plotA = EllipticalSection(mesh, steps, pointA, vectorA1, vectorA2);
                var plotB = EllipticalSection(mesh, steps, pointB, vectorB1, vectorB2);
                for (int i = 0; i < plotA.Length; i++)
                {
                    mesh.AddPoint(plotA[i].Point, plotA[i].Normal);
                    mesh.AddPoint(plotB[i].Point, plotB[i].Normal);
                    mesh.EndRow();
                }
                mesh.EndGrid();
                return;
            }

            var arcA = VectorTransform3D.BarycentricArc(vectorA1.Direction, vectorA2.Direction, steps).ToArray();
            var arcB = VectorTransform3D.BarycentricArc(vectorB1.Direction, vectorB2.Direction, steps).ToArray();

            for (int i = 0; i < arcA.Length; i++)
            {
                mesh.AddPoint(pointA + vectorA1.Magnitude * arcA[i], arcA[i]);
                mesh.AddPoint(pointB + vectorB1.Magnitude * arcB[i], arcB[i]);
                mesh.EndRow();
            }
            mesh.EndGrid();
        }

        private static int i = 0;
        private static Ray3D[] EllipticalSection(IWireFrameMesh mesh, int steps, Point3D c, Vector3D v1, Vector3D v2)
        {
            Console.WriteLine($"{c} Elliptical section Radii {v1.Magnitude} {v2.Magnitude}");

            var p1 = c + v1;
            var p2 = c + v2;

            var basisPlane = new BasisPlane(c, p1, p2);

            var cc = basisPlane.MapToSurfaceCoordinates(c);
            var pp1 = basisPlane.MapToSurfaceCoordinates(p1);
            var pp2 = basisPlane.MapToSurfaceCoordinates(p2);

            var a = v1.Magnitude;

            var vv1 = pp1 - cc;
            var vv2 = pp2 - cc;

            var rr = v2.Magnitude / v1.Magnitude;

            var vvv1 = vv1;
            var vvv2 = new Vector2D(vv2.X, Math.Sqrt(a * a - vv2.X * vv2.X));

            var r = vv2.Y / vvv2.Y;
            var b = a * r;

            var arc = VectorTransform2D.BarycentricArc(vvv1.Direction, vvv2.Direction, steps).ToArray();
            var arcEllipse = arc.Select(p => new Ray2D(new Point2D(a * p.X, b * p.Y), new Vector2D(a * p.X, b * p.Y).Direction)).ToArray();
            var plot = arcEllipse.Select(basisPlane.MapToSpaceCoordinates).ToArray();


            //var test = mesh.CreateNewInstance();
            ////test.AddTriangle(new Triangle3D(c, p1, p2));
            //foreach (var p in plot) { test.AddPoint(p.Point); }
            //test.EndRow();
            //test.AddPoint(c);
            //test.EndRow();
            //test.EndGrid();
            //WavefrontFile.Export(test, $"Wavefront/EllipticalPlane-{i}");

            //test = mesh.CreateNewInstance();
            //foreach (var p in plot) { test.AddTriangle(p.Point, p.Point + 0.05 * p.Normal, p.Point + 0.1 * p.Normal); }

            //WavefrontFile.Export(test, $"Wavefront/EllipticalPlaneNormals-{i}");

            //i++;

            return plot;
        }

        private static Ray3D[] EllipsoidSection(IWireFrameMesh mesh, int steps, Point3D c, params Vector3D[] v)
        {
            Console.WriteLine($"{c} Ellipsoid section Radii {string.Join(' ', v.Select(v => v.Magnitude))}");

            return null;
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
