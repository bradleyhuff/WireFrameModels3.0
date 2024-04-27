using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes;
using Console = BaseObjects.Console;

namespace Operations
{
    public static partial class IntermeshOperation
    {
        public static void Run(IWireFrameMesh mesh)
        {
            DateTime start = DateTime.Now;
            ConsoleLog.Push("Intermesh");
            var collections = new IntermeshCollection(mesh);

            TriangleGathering.Action(collections.Triangles);
            CalculateIntersections.Action(collections.Triangles);
            SeparateProcesses(collections, out List<IntermeshTriangle> processTriangles);
            CalculateDivisions.Action(processTriangles);

            SetIntersectionLinks.Action(processTriangles);
            SetDivisionLinks.Action(processTriangles);
            var elasticLinks = BuildElasticLinks.Action(processTriangles);
            PullElasticLinks.Action(elasticLinks);
            var fillTriangles = ExtractFillTriangles.Action(elasticLinks);
            UpdateResultGrid(mesh, processTriangles, fillTriangles);

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void SeparateProcesses(IntermeshCollection collections, out List<IntermeshTriangle> processTriangles)
        {
            var start = DateTime.Now;
            processTriangles = collections.Triangles.Where(t => t.Intersections.Any()).ToList();
            ConsoleLog.WriteLine($"Separate processes: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void UpdateResultGrid(IWireFrameMesh mesh, IEnumerable<IntermeshTriangle> processTriangles, IEnumerable<FillTriangle> fillTriangles)
        {
            var start = DateTime.Now;

            var processPositionTriangles = processTriangles.Select(p => p.PositionTriangle).ToArray();
            var removalCount = mesh.RemoveAllTriangles(processPositionTriangles);
            foreach (var triangle in fillTriangles)
            {
                triangle.AddWireFrameTriangle(mesh);
            }
            ConsoleLog.WriteLine($"Update result grid: Triangle removals {removalCount} Fills {fillTriangles.Count()} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            var edges = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => new Combination2(s[0].PositionObject.Id, s[1].PositionObject.Id), new Combination2Comparer()).ToArray();
            var groups = edges.GroupBy(p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p[0].Position, p[1].Position)))).OrderBy(g => g.Key).ToArray();
            Console.WriteLine();
            foreach (var group in groups)
            {
                Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}");
            }
            Console.WriteLine();
        }
    }
}
