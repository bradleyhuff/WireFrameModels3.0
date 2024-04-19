using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    public static class Operations
    {
        public static IWireFrameMesh Intermesh(IWireFrameMesh mesh)
        {
            DateTime start = DateTime.Now;
            ConsoleLog.Push("Intermesh");
            var collections = new IntermeshCollection(mesh);

            TriangleGathering.Action(collections.Triangles);
            CalculateIntersections.Action(collections.Triangles);
            SeparateProcesses(collections, out List<IntermeshTriangle> byPassTriangles, out List<IntermeshTriangle> processTriangles);
            CalculateDivisions.Action(processTriangles);

            SetIntersectionLinks.Action(processTriangles);
            SetDivisionLinks.Action(processTriangles);
            var elasticLinks = BuildElasticLinks.Action(processTriangles);
            PullElasticLinks.Action(elasticLinks);
            var fillTriangles = ExtractFillTriangles.Action(elasticLinks);
            var output = BuildResultGrid(byPassTriangles, fillTriangles);

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            return output;
        }

        private static void SeparateProcesses(IntermeshCollection collections, out List<IntermeshTriangle> byPassTriangles, out List<IntermeshTriangle> processTriangles)
        {
            var start = DateTime.Now;
            processTriangles = collections.Triangles.Where(t => t.Intersections.Any()).ToList();
            byPassTriangles = collections.Triangles.Where(t => !t.Intersections.Any()).ToList();
            ConsoleLog.WriteLine($"Separate processes: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IWireFrameMesh BuildResultGrid(IEnumerable<IntermeshTriangle> byPassTriangles, IEnumerable<FillTriangle> fillTriangles)
        {
            var start = DateTime.Now;
            var output = WireFrameMesh.CreateMesh();
            foreach (var triangle in byPassTriangles)
            {
                triangle.AddWireFrameTriangle(output);
            }
            foreach (var triangle in fillTriangles)
            {
                triangle.AddWireFrameTriangle(output);
            }
            ConsoleLog.WriteLine($"Build result grid: Bypasses {byPassTriangles.Count()} Fills {fillTriangles.Count()} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            var edges = output.Triangles.SelectMany(t => t.Edges).DistinctBy(s => new Combination2(s[0].PositionObject.Id, s[1].PositionObject.Id), new Combination2Comparer()).ToArray();
            Console.WriteLine($"Total edges {edges.Length} Collinears {output.Triangles.Count(t => t.Triangle.IsCollinear)}");
            //TableDisplays.ShowCountSpread("Edge length counts", edges, p => (int)Math.Floor(Math.Log10(Point3D.Distance(p[0].Position, p[1].Position))));
            var groups = edges.GroupBy(p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p[0].Position, p[1].Position)))).OrderBy(g => g.Key).ToArray();
            foreach (var group in groups)
            {
                Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}");
            }
            //var test = edges.Where(p => (int)Math.Log10(Point3D.Distance(p[0].Position, p[1].Position)) == 0);
            //Console.WriteLine($"{string.Join(",", test.Select(p => Point3D.Distance(p[0].Position, p[1].Position)))}");
            return output;
        }
    }
}
