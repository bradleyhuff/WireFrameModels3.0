using BasicObjects.GeometricObjects;
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

            Console.WriteLine($"Pull elastic links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Yellow);
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
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorA.Point) < 3e-9)
                    {
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorA, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                        pulls++;
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorB.Point) < 3e-9)
                    {
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorB, elasticTriangle.PerimeterEdgeAB, perimeter, perimeterTable);
                        pulls++;
                    }
                    if (Point3D.Distance(perimeter.Point, elasticTriangle.AnchorC.Point) < 3e-9)
                    {
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeCA, perimeter, perimeterTable);
                        AnchorPull(elasticTriangle.AnchorC, elasticTriangle.PerimeterEdgeBC, perimeter, perimeterTable);
                        pulls++;
                    }
                }
            }

            //int blindPulls = 0;
            //foreach (var elasticTriangle in elasticTriangles)
            //{
            //    var segmentPoints = elasticTriangle.Segments.SelectMany(ps => ps.VerticiesAB).ToArray();

            //    var notMatchedPointsA = segmentPoints.Where(p => p.Vertex.Point == elasticTriangle.AnchorA.Point && p.Vertex.Id != elasticTriangle.AnchorA.Id &&
            //        !elasticTriangle.PerimeterEdgeAB.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id) &&
            //        !elasticTriangle.PerimeterEdgeCA.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id));

            //    foreach (var point in notMatchedPointsA)
            //    {
            //        point.Vertex.Delink(point);
            //        elasticTriangle.AnchorA.Link(point);
            //        blindPulls++;
            //    }

            //    if (notMatchedPointsA.Any()) { Console.WriteLine($"{elasticTriangle.Id} Not matched points A {notMatchedPointsA.Count()}"); }

            //    var notMatchedPointsB = segmentPoints.Where(p => p.Vertex.Point == elasticTriangle.AnchorB.Point && p.Vertex.Id != elasticTriangle.AnchorB.Id &&
            //        !elasticTriangle.PerimeterEdgeAB.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id) &&
            //        !elasticTriangle.PerimeterEdgeBC.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id));

            //    foreach (var point in notMatchedPointsB)
            //    {
            //        point.Vertex.Delink(point);
            //        elasticTriangle.AnchorB.Link(point);
            //        blindPulls++;
            //    }

            //    if (notMatchedPointsB.Any()) { Console.WriteLine($"{elasticTriangle.Id} Not matched points B {notMatchedPointsB.Count()}"); }

            //    var notMatchedPointsC = segmentPoints.Where(p => p.Vertex.Point == elasticTriangle.AnchorC.Point && p.Vertex.Id != elasticTriangle.AnchorC.Id &&
            //        !elasticTriangle.PerimeterEdgeBC.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id) &&
            //        !elasticTriangle.PerimeterEdgeCA.PerimeterPoints.Any(ab => ab.Id == p.Vertex.Id));

            //    foreach (var point in notMatchedPointsC)
            //    {
            //        point.Vertex.Delink(point);
            //        elasticTriangle.AnchorC.Link(point);
            //        blindPulls++;
            //    }

            //    if (notMatchedPointsC.Any()) { Console.WriteLine($"{elasticTriangle.Id} Not matched points C {notMatchedPointsC.Count()}"); }
            //}
            //Console.WriteLine($"Anchor pulls {pulls} Blind vertex pulls {blindPulls}");
            Console.WriteLine($"Anchor pulls {pulls}");
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
