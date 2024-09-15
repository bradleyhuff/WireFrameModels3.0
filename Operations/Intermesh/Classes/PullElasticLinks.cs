using BaseObjects;
using BasicObjects.GeometricObjects;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal static class PullElasticLinks
    {
        internal static void Action(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var start = DateTime.Now;
            AnchorPull(elasticTriangles);
            ConsoleLog.WriteLine($"Pull elastic links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void AnchorPull(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var perimeterTable = GetPerimeterTable(elasticTriangles);

            foreach (var elasticTriangle in elasticTriangles)
            {
                var perimeterPoints = elasticTriangle.PerimeterEdges.SelectMany(p => p.PerimeterPoints).ToArray();
                foreach (var perimeter in perimeterPoints)
                {
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorA.Point) < GapConstants.Proximity)
                    {
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorB.Point) < GapConstants.Proximity)
                    {
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorC.Point) < GapConstants.Proximity)
                    {
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                    }
                }
            }
        }

        private static Dictionary<int, List<ElasticEdge>> GetPerimeterTable(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var perimeterTable = new Dictionary<int, List<ElasticEdge>>();
            foreach (var perimeterEdge in elasticTriangles.SelectMany(t => t.PerimeterEdges))
            {
                foreach (var point in perimeterEdge.PerimeterPoints)
                {
                    if (!perimeterTable.ContainsKey(point.Id)) { perimeterTable[point.Id] = new List<ElasticEdge>(); }
                    if (!perimeterTable[point.Id].Any(p => p.Id == perimeterEdge.Id))
                    {
                        perimeterTable[point.Id].Add(perimeterEdge);
                    }
                }
            }
            return perimeterTable;
        }

        private static void AnchorPull(ElasticVertexAnchor anchor, ElasticEdge edge, ElasticVertexCore vertex, Dictionary<int, List<ElasticEdge>> perimeterTable)
        {
            anchor.Link(vertex);
            edge.RemovePerimeterPoint(vertex);
            edge.RemovePerimeterPoint(anchor);
            if (perimeterTable.ContainsKey(vertex.Id))
            {
                var perimeterEdges = perimeterTable[vertex.Id];
                foreach (var perimeterEdge in perimeterEdges)
                {
                    perimeterEdge.ReplacePerimeterPoint(vertex, anchor);
                }
            }
        }
    }
}
