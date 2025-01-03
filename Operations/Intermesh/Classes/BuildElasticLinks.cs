﻿using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Console = BaseObjects.Console;
using Double = BasicObjects.Math.Double;

namespace Operations.Intermesh.Classes
{
    internal static class BuildElasticLinks
    {
        internal static IEnumerable<ElasticTriangle> Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;

            var anchorTable = new Dictionary<int, ElasticVertexAnchor>();
            var triangleTable = new Dictionary<int, ElasticTriangle>();
            var containerTable = new Dictionary<int, ElasticVertexContainer>();
            var segmentTable = new Dictionary<int, ElasticSegment>();
            var edgeTable = new Combination2Dictionary<ElasticEdge>();

            BuildContainerTable(intermeshTriangles, containerTable);

            SetAllDivisions(intermeshTriangles, triangleTable, anchorTable, containerTable, segmentTable, edgeTable);

            var anchorBucket = new BoxBucket<ElasticVertexAnchor>(anchorTable.Values);
            LinkAllDivisions(intermeshTriangles, containerTable, anchorBucket);

            var elasticTriangles = GetAllTriangles(intermeshTriangles, triangleTable).ToArray();
            var verticies = elasticTriangles.SelectMany(t => t.Segments.SelectMany(s => s.VerticiesAB)).DistinctBy(v => v.Id).ToArray();
            foreach (var vertex in verticies) { vertex.VertexFill(anchorBucket); }

            SetPerimeterPoints(intermeshTriangles, triangleTable, containerTable);
            SetAdjacents(intermeshTriangles, triangleTable);
            ConsoleLog.WriteLine($"Build elastic links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            return elasticTriangles;
        }

        private static void SetAdjacents(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable)
        {
            foreach (var triangle in triangles)
            {
                try
                {
                    var elasticTriangle = triangleTable[triangle.Id];
                    var adjacentsAB = triangle.ABadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);
                    var adjacentsBC = triangle.BCadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);
                    var adjacentsCA = triangle.CAadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);

                    elasticTriangle.SetAdjacents(adjacentsAB, adjacentsBC, adjacentsCA);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }

        private static void SetAllDivisions(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable, Dictionary<int, ElasticVertexAnchor> anchorTable,
            Dictionary<int, ElasticVertexContainer> containerTable,
            Dictionary<int, ElasticSegment> segmentTable, Combination2Dictionary<ElasticEdge> edgeTable)
        {
            foreach (var triangle in triangles)
            {
                try
                {
                    var elasticTriangle = GetTriangle(triangle, triangleTable, anchorTable, edgeTable);
                    elasticTriangle.SetSegments(GetSegments(triangle.Divisions, containerTable, segmentTable));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void BuildContainerTable(IEnumerable<IntermeshTriangle> triangles, Dictionary<int, ElasticVertexContainer> containerTable)
        {
            foreach (var triangle in triangles)
            {
                var containers = triangle.Divisions.SelectMany(s => s.VerticiesAB.SelectMany(v => v.Vertex.DivisionContainers)).DistinctBy(c => c.Id);
                foreach (var container in containers)
                {
                    if (!containerTable.ContainsKey(container.Id)) { containerTable[container.Id] = new ElasticVertexContainer(container.Point); }
                }
            }
        }

        private static void LinkAllDivisions(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticVertexContainer> containerTable, IBoxBucket<ElasticVertexAnchor> anchors)
        {
            foreach (var triangle in triangles)
            {
                try
                {
                    foreach (var division in triangle.Divisions)
                    {
                        var containersA = division.VertexA?.Vertex?.DivisionContainers ?? Enumerable.Empty<DivisionVertexContainer>();
                        var elasticContainersA = containersA.Select(c => containerTable[c.Id]).ToArray();
                        LinkContainers(elasticContainersA, anchors);

                        var containersB = division.VertexB?.Vertex?.DivisionContainers ?? Enumerable.Empty<DivisionVertexContainer>();
                        var elasticContainersB = containersB.Select(c => containerTable[c.Id]).ToArray();
                        LinkContainers(elasticContainersB, anchors);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }

        private static void LinkContainers(IEnumerable<ElasticVertexContainer> containers, IBoxBucket<ElasticVertexAnchor> anchors)
        {
            if (!containers.Any()) { return; }
            var first = containers.First();
            foreach (var container in containers.Skip(1))
            {
                first.Link(container, anchors);
            }
        }

        private static IEnumerable<ElasticTriangle> GetAllTriangles(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable)
        {
            foreach (var triangle in triangles)
            {
                yield return triangleTable[triangle.Id];
            }
        }

        private static ElasticTriangle GetTriangle(IntermeshTriangle triangle,
            Dictionary<int, ElasticTriangle> triangleTable, Dictionary<int, ElasticVertexAnchor> anchorTable,
            Combination2Dictionary<ElasticEdge> edgeTable)
        {
            if (!triangleTable.ContainsKey(triangle.Id))
            {
                var anchors = GetAnchors(triangle, anchorTable).ToArray();
                var edgeAB = GetPerimeterEdge(anchors[0], anchors[1], edgeTable);
                var edgeBC = GetPerimeterEdge(anchors[1], anchors[2], edgeTable);
                var edgeCA = GetPerimeterEdge(anchors[2], anchors[0], edgeTable);
                triangleTable[triangle.Id] = new ElasticTriangle(triangle,
                    anchors[0], triangle.A.Normal, anchors[1], triangle.B.Normal, anchors[2], triangle.C.Normal,
                    edgeAB, edgeBC, edgeCA, triangle.Trace, triangle.Tag);
            }
            return triangleTable[triangle.Id];
        }

        private static IEnumerable<ElasticVertexAnchor> GetAnchors(IntermeshTriangle triangle, Dictionary<int, ElasticVertexAnchor> anchors)
        {
            foreach (var vertex in triangle.Positions)
            {
                if (!anchors.ContainsKey(vertex.PositionObject.Id)) { anchors[vertex.PositionObject.Id] = new ElasticVertexAnchor(vertex.Position); }
                yield return anchors[vertex.PositionObject.Id];
            }
        }

        private static IEnumerable<ElasticSegment> GetSegments(IEnumerable<IntermeshDivision> divisionNodes,
            Dictionary<int, ElasticVertexContainer> containerTable, Dictionary<int, ElasticSegment> segmentTable)
        {
            foreach (var divisionNode in divisionNodes)
            {
                if (!segmentTable.ContainsKey(divisionNode.Id))
                {
                    segmentTable[divisionNode.Id] = new ElasticSegment(containerTable[divisionNode.VertexA.Id], containerTable[divisionNode.VertexB.Id]);
                }

                yield return segmentTable[divisionNode.Id];
            }
        }

        private static ElasticEdge GetPerimeterEdge(ElasticVertexAnchor anchorA, ElasticVertexAnchor anchorB, Combination2Dictionary<ElasticEdge> edgeTable)
        {
            var key = new Combination2(anchorA.Id, anchorB.Id);
            if (!edgeTable.ContainsKey(key)) { edgeTable[key] = new ElasticEdge(anchorA, anchorB); }
            return edgeTable[key];
        }

        private static void SetPerimeterPoints(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable, Dictionary<int, ElasticVertexContainer> containerTable)
        {
            foreach (var triangle in triangles)
            {
                try
                {
                    var elasticTriangle = triangleTable[triangle.Id];

                    var edgeABPoints = triangle.EdgeAB.GetPerimeterPoints().Select(c => c.Vertex).ToArray();
                    var edgeBCPoints = triangle.EdgeBC.GetPerimeterPoints().Select(c => c.Vertex).ToArray();
                    var edgeCAPoints = triangle.EdgeCA.GetPerimeterPoints().Select(c => c.Vertex).ToArray();

                    var perimeterABpoints = MapPerimeterPoints(edgeABPoints, containerTable);
                    var perimeterBCpoints = MapPerimeterPoints(edgeBCPoints, containerTable);
                    var perimeterCApoints = MapPerimeterPoints(edgeCAPoints, containerTable);

                    var perimeterABsegments = new List<ElasticSegment>();
                    var perimeterBCsegments = new List<ElasticSegment>();
                    var perimeterCAsegments = new List<ElasticSegment>();

                    ApplyCollinears(elasticTriangle,
                        perimeterABsegments, perimeterBCsegments, perimeterCAsegments,
                        perimeterABpoints, perimeterBCpoints, perimeterCApoints);

                    ApplyFreePoints(elasticTriangle, perimeterABpoints, perimeterBCpoints, perimeterCApoints);

                    elasticTriangle.PerimeterEdgeAB.AddPerimeterPoints(perimeterABpoints, perimeterABsegments);
                    elasticTriangle.PerimeterEdgeBC.AddPerimeterPoints(perimeterBCpoints, perimeterBCsegments);
                    elasticTriangle.PerimeterEdgeCA.AddPerimeterPoints(perimeterCApoints, perimeterCAsegments);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void ApplyCollinears(ElasticTriangle triangle,
            List<ElasticSegment> perimeterABsegments,
            List<ElasticSegment> perimeterBCsegments,
            List<ElasticSegment> perimeterCAsegments,
            List<ElasticVertexCore> perimeterABpoints,
            List<ElasticVertexCore> perimeterBCpoints,
            List<ElasticVertexCore> perimeterCApoints)
        {
            var lineAB = triangle.SurfaceTriangle.Triangle.EdgeAB.LineExtension;
            var lineBC = triangle.SurfaceTriangle.Triangle.EdgeBC.LineExtension;
            var lineCA = triangle.SurfaceTriangle.Triangle.EdgeCA.LineExtension;

            var collinearsAB = triangle.Segments.Where(s => lineAB.SegmentIsOnLine(s.Segment, GapConstants.Proximity)).ToArray();
            var collinearsBC = triangle.Segments.Where(s => lineBC.SegmentIsOnLine(s.Segment, GapConstants.Proximity)).ToArray();
            var collinearsCA = triangle.Segments.Where(s => lineCA.SegmentIsOnLine(s.Segment, GapConstants.Proximity)).ToArray();

            var collinears = collinearsAB.Concat(collinearsBC).Concat(collinearsCA).DistinctBy(s => s.Id).ToArray();
            foreach (var collinear in collinears)
            {
                var nearestLineA = NearestLine(collinear.VertexA.Point, lineAB, lineBC, lineCA);
                var nearestLineB = NearestLine(collinear.VertexB.Point, lineAB, lineBC, lineCA);
                if (nearestLineA == Line.None || nearestLineB == Line.None) { continue; }

                if (LineCheck.HasAB(nearestLineA) && LineCheck.HasAB(nearestLineB)) { perimeterABsegments.Add(collinear); }
                if (LineCheck.HasBC(nearestLineA) && LineCheck.HasBC(nearestLineB)) { perimeterBCsegments.Add(collinear); }
                if (LineCheck.HasCA(nearestLineA) && LineCheck.HasCA(nearestLineB)) { perimeterCAsegments.Add(collinear); }

                if (LineCheck.HasAB(nearestLineA)) { perimeterABpoints.Add(collinear.VertexA.Vertex); }
                if (LineCheck.HasBC(nearestLineA)) { perimeterBCpoints.Add(collinear.VertexA.Vertex); }
                if (LineCheck.HasCA(nearestLineA)) { perimeterCApoints.Add(collinear.VertexA.Vertex); }

                if (LineCheck.HasAB(nearestLineB)) { perimeterABpoints.Add(collinear.VertexB.Vertex); }
                if (LineCheck.HasBC(nearestLineB)) { perimeterBCpoints.Add(collinear.VertexB.Vertex); }
                if (LineCheck.HasCA(nearestLineB)) { perimeterCApoints.Add(collinear.VertexB.Vertex); }
            }
        }

        private static void ApplyFreePoints(ElasticTriangle triangle,
            List<ElasticVertexCore> perimeterABpoints,
            List<ElasticVertexCore> perimeterBCpoints,
            List<ElasticVertexCore> perimeterCApoints)
        {
            var lineAB = triangle.SurfaceTriangle.Triangle.EdgeAB.LineExtension;
            var lineBC = triangle.SurfaceTriangle.Triangle.EdgeBC.LineExtension;
            var lineCA = triangle.SurfaceTriangle.Triangle.EdgeCA.LineExtension;

            var freePoints = triangle.Segments.SelectMany(s => s.VerticiesAB)
                .Where(v => lineAB.PointIsOnLine(v.Vertex.Point, GapConstants.Proximity) || lineBC.PointIsOnLine(v.Vertex.Point, GapConstants.Proximity) || lineCA.PointIsOnLine(v.Vertex.Point, GapConstants.Proximity))
                .Where(v => !triangle.PerimeterEdgeAB.PerimeterPoints.Any(e => e.Id == v.Vertex.Id) &&
                    !triangle.PerimeterEdgeBC.PerimeterPoints.Any(e => e.Id == v.Vertex.Id) &&
                    !triangle.PerimeterEdgeCA.PerimeterPoints.Any(e => e.Id == v.Vertex.Id)).Select(v => v.Vertex).DistinctBy(v => v.Id).ToArray();

            foreach (var freePoint in freePoints)
            {
                double distanceAB = lineAB.Distance(freePoint.Point);
                double distanceBC = lineBC.Distance(freePoint.Point);
                double distanceCA = lineCA.Distance(freePoint.Point);

                if (distanceAB <= distanceBC && distanceAB <= distanceCA) { perimeterABpoints.Add(freePoint); }
                if (distanceBC <= distanceAB && distanceBC <= distanceCA) { perimeterBCpoints.Add(freePoint); }
                if (distanceCA <= distanceAB && distanceCA <= distanceBC) { perimeterCApoints.Add(freePoint); }
            }
        }

        private enum Line
        {
            None,
            LineAB,
            LineBC,
            LineCA,
            LineABBC,
            LineABCA,
            LineBCCA
        }
        private static class LineCheck
        {
            public static bool HasAB(Line line)
            {
                return line == Line.LineAB || line == Line.LineABBC || line == Line.LineABCA;
            }
            public static bool HasBC(Line line)
            {
                return line == Line.LineBC || line == Line.LineABBC || line == Line.LineBCCA;
            }
            public static bool HasCA(Line line)
            {
                return line == Line.LineCA || line == Line.LineBCCA || line == Line.LineABCA;
            }
        }

        private static Line NearestLine(Point3D point, Line3D lineAB, Line3D lineBC, Line3D lineCA)
        {
            double distanceAB = lineAB.Distance(point);
            double distanceBC = lineBC.Distance(point);
            double distanceCA = lineCA.Distance(point);
            if (distanceAB < Double.DifferenceError && distanceBC < Double.DifferenceError && distanceCA < Double.DifferenceError)
            {
                return Line.None;
            }
            if (distanceAB < Double.DifferenceError && distanceBC < Double.DifferenceError) { return Line.LineABBC; }
            if (distanceAB < Double.DifferenceError && distanceCA < Double.DifferenceError) { return Line.LineABCA; }
            if (distanceBC < Double.DifferenceError && distanceCA < Double.DifferenceError) { return Line.LineBCCA; }

            if (distanceAB < distanceBC && distanceAB < distanceCA) { return Line.LineAB; }
            if (distanceBC < distanceAB && distanceBC < distanceCA) { return Line.LineBC; }
            if (distanceCA < distanceAB && distanceCA < distanceBC) { return Line.LineCA; }
            return Line.None;
        }

        private static List<ElasticVertexCore> MapPerimeterPoints(IEnumerable<VertexCore> input, Dictionary<int, ElasticVertexContainer> containerTable)
        {
            var elasticPerimeter = new List<ElasticVertexContainer>();
            foreach (var vertex in input)
            {
                var divisions = vertex.DivisionContainers.Select(c => containerTable[c.Id]).ToArray();
                elasticPerimeter.AddRange(divisions);
            }
            return elasticPerimeter.Select(p => p.Vertex).DistinctBy(v => v.Id).ToList();
        }
    }
}
