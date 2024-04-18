using BaseObjects;
using BasicObjects.GeometricObjects;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class PullElasticLinks
    {
        internal static void Action(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            var start = DateTime.Now;
            AnchorPull(elasticTriangles);
            ConsoleLog.WriteLine($"Pull elastic links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            var elasticSegments = elasticTriangles.SelectMany(t => t.Segments).DistinctBy(s => s.Id).ToArray();
            ConsoleLog.WriteNextLine($"Total elastic segments {elasticSegments.Length}");
            //TableDisplays.ShowCountSpread("Elastic segment length counts", elasticSegments, p => (int)Math.Floor(Math.Log10(p.Length)));
            var groups = elasticSegments.GroupBy(p => (int)Math.Floor(3 * Math.Log10(p.Length))).OrderBy(g => g.Key).ToArray();
            foreach (var group in groups)
            {
                Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}");
            }
        }

        private static void AnchorPull(IEnumerable<ElasticTriangle> elasticTriangles)
        {
            int pulls = 0;

            var perimeterTable = GetPerimeterTable(elasticTriangles);

            foreach (var elasticTriangle in elasticTriangles)
            {
                var perimeterPoints = elasticTriangle.PerimeterEdges.SelectMany(p => p.PerimeterPoints).ToArray();
                foreach (var perimeter in perimeterPoints)
                {
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorA.Point) < GapConstants.Filler)
                    {
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                        pulls++;
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorB.Point) < GapConstants.Filler)
                    {
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                        pulls++;
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorC.Point) < GapConstants.Filler)
                    {
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                        pulls++;
                    }
                }
            }
            //Console.WriteLine($"Anchor pulls {pulls}");
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
