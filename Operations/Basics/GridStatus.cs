using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;
using System.Linq;

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

        public static void ShowVitals(this IWireFrameMesh mesh, int index = -1)
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
            var tags = mesh.Triangles.Where(t => t.AdjacentAnyCount < 3 && t.Triangle.MaxEdge.Length > 0.0);
            Console.WriteLine($"Tags {tags.Count()}");
            foreach(var e in tags)
            {
                Console.WriteLine($"{e.Id}  [{e.A.PositionObject.Id}, {e.B.PositionObject.Id}, {e.C.PositionObject.Id}] [{e.A.PositionObject.Point}, {e.B.PositionObject.Point}, {e.C.PositionObject.Point}] Length: {e.Triangle.MaxEdge.Length} Aspect: {e.Triangle.AspectRatio}", e.Triangle.MaxEdge.Length > 0.1 ? ConsoleColor.Red: ConsoleColor.Gray);
            }
            //Console.WriteLine(string.Join("\n", tags.Select(e => $"{e.Id}  [{e.A.PositionObject.Id}, {e.B.PositionObject.Id}, {e.C.PositionObject.Id}] [{e.A.PositionObject.Point}, {e.B.PositionObject.Point}, {e.C.PositionObject.Point}] Length: {e.Triangle.MaxEdge.Length} Aspect: {e.Triangle.AspectRatio}")));
            Console.WriteLine();

            if (index > -1)
            {
                var grid = WireFrameMesh.Create();
                grid.AddRangeTriangles(tags, "", 0);
                WavefrontFile.Export(grid, $"Wavefront/Tags-{index}");
            }
        }
    }
}
