using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;

namespace Operations.Basics
{
    public static class GridStatus
    {
        public static void ShowSegmentLengths(this IWireFrameMesh mesh, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.WriteLine();
            Console.WriteLine("Segment lengths", ConsoleColor.Yellow);
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var groups = segments.GroupBy(p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p.A.Position, p.B.Position)))).OrderBy(g => g.Key).ToArray();
            foreach (var group in groups)
            {
                Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}", color);
            }
            Console.WriteLine();
        }

        public static void ShowVitals(this IWireFrameMesh mesh)
        {
            var clusters = GroupingCollection.ExtractClusters(mesh.Triangles);
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles);
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles);

            Console.WriteLine($"Clusters {clusters.Count()}  Surfaces {surfaces.Count()}  Faces {faces.Count()}", ConsoleColor.Yellow);
            Console.WriteLine();
            TableDisplays.ShowCountSpread("Position cardinalities", mesh.Positions, p => p.Cardinality);
            TableDisplays.ShowCountSpread("AB Adjacency counts", mesh.Triangles.Select(t => t.ABadjacents), l => l.Count);
            TableDisplays.ShowCountSpread("BC Adjacency counts", mesh.Triangles.Select(t => t.BCadjacents), l => l.Count);
            TableDisplays.ShowCountSpread("CA Adjacency counts", mesh.Triangles.Select(t => t.CAadjacents), l => l.Count);
            Console.WriteLine($"Collinears {mesh.Triangles.Count(t => t.Triangle.IsCollinear)} Tags {mesh.Triangles.Count(t => t.AdjacentAnyCount <= 1)}");
            Console.WriteLine();
        }
    }
}
