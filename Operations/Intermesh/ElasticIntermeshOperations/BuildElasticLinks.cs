using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class BuildElasticLinks
    {
        internal static IEnumerable<ElasticTriangle> Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;

            var anchorTable = new Dictionary<int, ElasticVertexAnchor>();
            var triangleTable = new Dictionary<int, ElasticTriangle>();
            var segmentTable = new Dictionary<int, ElasticSegment>();
            var edgeTable = new Combination2Dictionary<ElasticEdge>();

            SetAllDivisions(intermeshTriangles, triangleTable, anchorTable, segmentTable, edgeTable);
            var containerTable = BuildContainerTable(intermeshTriangles, segmentTable);

            var anchorBucket = new BoxBucket<ElasticVertexAnchor>(anchorTable.Values);
            LinkAllDivisions(intermeshTriangles, containerTable, anchorBucket);

            var elasticTriangles = GetAllTriangles(intermeshTriangles, triangleTable).ToArray();
            var verticies = elasticTriangles.SelectMany(t => t.Segments.SelectMany(s => s.VerticiesAB)).DistinctBy(v => v.Id).ToArray();
            foreach (var vertex in verticies) { vertex.VertexFill(anchorBucket); }

            SetPerimeterPoints(intermeshTriangles, triangleTable, containerTable);
            SetAdjacents(intermeshTriangles, triangleTable);
            Console.WriteLine($"Build elastic links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Yellow);

            //Notes.BuildElasticLinksNotes(elasticTriangles, verticies, anchorTable, segmentTable);

            return elasticTriangles;
        }

        private static void SetAdjacents(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable)
        {
            foreach (var triangle in triangles)
            {
                var elasticTriangle = triangleTable[triangle.Id];
                var adjacentsAB = triangle.ABadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);
                var adjacentsBC = triangle.BCadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);
                var adjacentsCA = triangle.CAadjacents.Where(t => triangleTable.ContainsKey(t.Id)).Select(t => triangleTable[t.Id]);

                elasticTriangle.SetAdjacents(adjacentsAB, adjacentsBC, adjacentsCA);
            }
        }

        private static void SetAllDivisions(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticTriangle> triangleTable, Dictionary<int, ElasticVertexAnchor> anchorTable,
            Dictionary<int, ElasticSegment> segmentTable, Combination2Dictionary<ElasticEdge> edgeTable)
        {
            foreach (var triangle in triangles)
            {
                var elasticTriangle = GetTriangle(triangle, triangleTable, anchorTable, edgeTable);
                elasticTriangle.SetSegments(GetSegments(triangle.Divisions, segmentTable));
            }
        }

        private static Dictionary<int, ElasticVertexContainer> BuildContainerTable(IEnumerable<IntermeshTriangle> triangles, Dictionary<int, ElasticSegment> segmentTable)
        {
            var containerTable = new Dictionary<int, ElasticVertexContainer>();

            foreach (var triangle in triangles)
            {
                var containers = triangle.Divisions.SelectMany(s => s.VerticiesAB.SelectMany(v => v.Vertex.DivisionContainers)).DistinctBy(c => c.Id);
                foreach (var container in containers)
                {
                    var tag = container.Tag;
                    var division = segmentTable[container.Division.Id];
                    if (tag == 'a')
                    {
                        containerTable[container.Id] = division.VertexA;
                    }
                    else
                    {
                        containerTable[container.Id] = division.VertexB;
                    }
                }
            }

            return containerTable;
        }

        private static void LinkAllDivisions(IEnumerable<IntermeshTriangle> triangles,
            Dictionary<int, ElasticVertexContainer> containerTable, IBoxBucket<ElasticVertexAnchor> anchors)
        {
            foreach (var triangle in triangles)
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
                triangleTable[triangle.Id] = new ElasticTriangle(
                    anchors[0], triangle.A.Normal, anchors[1], triangle.B.Normal, anchors[2], triangle.C.Normal,
                    edgeAB, edgeBC, edgeCA, triangle.Trace);
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

        private static IEnumerable<ElasticSegment> GetSegments(IEnumerable<IntermeshDivision> divisionNodes, Dictionary<int, ElasticSegment> segmentTable)
        {
            foreach (var divisionNode in divisionNodes)
            {
                if (!segmentTable.ContainsKey(divisionNode.Id)) { segmentTable[divisionNode.Id] = new ElasticSegment(divisionNode.VertexA.Point, divisionNode.VertexB.Point); }

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
                var elasticTriangle = triangleTable[triangle.Id];

                var lineAB = elasticTriangle.SurfaceTriangle.Triangle.EdgeAB.LineExtension;
                var lineBC = elasticTriangle.SurfaceTriangle.Triangle.EdgeBC.LineExtension;
                var lineCA = elasticTriangle.SurfaceTriangle.Triangle.EdgeCA.LineExtension;

                var collinearsAB = elasticTriangle.Segments.Where(s => lineAB.SegmentIsOnLine(s.Segment, 3e-9)).ToArray();
                var collinearsBC = elasticTriangle.Segments.Where(s => lineBC.SegmentIsOnLine(s.Segment, 3e-9)).ToArray();
                var collinearsCA = elasticTriangle.Segments.Where(s => lineCA.SegmentIsOnLine(s.Segment, 3e-9)).ToArray();

                var perimeterABsegments = elasticTriangle.PerimeterEdgeAB.Segments.Concat(collinearsAB).DistinctBy(s => s.Id).ToArray();
                var perimeterBCsegments = elasticTriangle.PerimeterEdgeBC.Segments.Concat(collinearsBC).DistinctBy(s => s.Id).ToArray();
                var perimeterCAsegments = elasticTriangle.PerimeterEdgeCA.Segments.Concat(collinearsCA).DistinctBy(s => s.Id).ToArray();

                var intermeshPerimeterVerticiesAB = triangle.EdgeAB.GetPerimeterPoints().Select(c => c.Vertex).ToArray();
                var intermeshPerimeterVerticiesBC = triangle.EdgeBC.GetPerimeterPoints().Select(c => c.Vertex).ToArray();
                var intermeshPerimeterVerticiesCA = triangle.EdgeCA.GetPerimeterPoints().Select(c => c.Vertex).ToArray();

                var elasticPerimeterVerticiesAB = SetPerimeterPoints(intermeshPerimeterVerticiesAB, containerTable).ToArray();
                var elasticPerimeterVerticiesBC = SetPerimeterPoints(intermeshPerimeterVerticiesBC, containerTable).ToArray();
                var elasticPerimeterVerticiesCA = SetPerimeterPoints(intermeshPerimeterVerticiesCA, containerTable).ToArray();

                var perimetersAB = elasticTriangle.PerimeterEdgeAB.PerimeterPoints.Concat(elasticPerimeterVerticiesAB).DistinctBy(p => p.Id);
                var perimetersBC = elasticTriangle.PerimeterEdgeBC.PerimeterPoints.Concat(elasticPerimeterVerticiesBC).DistinctBy(p => p.Id);
                var perimetersCA = elasticTriangle.PerimeterEdgeCA.PerimeterPoints.Concat(elasticPerimeterVerticiesCA).DistinctBy(p => p.Id);

                var exclusiveA = triangle.AexclusiveVerticies;
                var exclusiveB = triangle.BexclusiveVerticies;
                var exclusiveC = triangle.CexclusiveVerticies;

                elasticTriangle.PerimeterEdgeAB.SetPerimeterPoints(perimetersAB, perimeterABsegments);
                elasticTriangle.PerimeterEdgeBC.SetPerimeterPoints(perimetersBC, perimeterBCsegments);
                elasticTriangle.PerimeterEdgeCA.SetPerimeterPoints(perimetersCA, perimeterCAsegments);

            }
        }

        private static IEnumerable<ElasticVertexCore> SetPerimeterPoints(IEnumerable<VertexCore> input, Dictionary<int, ElasticVertexContainer> containerTable)
        {
            var elasticPerimeter = new List<ElasticVertexContainer>();
            foreach (var vertex in input)
            {
                var divisions = vertex.DivisionContainers.Select(c => containerTable[c.Id]).ToArray();
                elasticPerimeter.AddRange(divisions);
            }
            return elasticPerimeter.Select(p => p.Vertex).DistinctBy(v => v.Id);
        }
    }
}
