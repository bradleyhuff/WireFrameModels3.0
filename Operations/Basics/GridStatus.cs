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
using Collections.Buckets;
using Operations.ParallelSurfaces.Internals;
using BaseObjects;
using Operations.Groupings.FileExportImport;

namespace Operations.Basics
{
    public static class GridStatus
    {
        public static void ShowSegmentLengths(this IWireFrameMesh mesh, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.WriteLine();
            Console.WriteLine("Segment lengths", ConsoleColor.Yellow);
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, Combination2Comparer.Comparer).ToArray();
            Console.WriteLine(segments.GroupCountAccumulates(
                p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p.A.Position, p.B.Position))), 
                (key, count, accumulate) => [Math.Pow(10, key / 3.0).ToString("E3") , count.ToString("#,##0"), accumulate.ToString("#,##0")])
                .DisplayByLine());
            Console.WriteLine();
        }

        public static void ShowVitals(this IWireFrameMesh mesh)
        {
            var clusters = GroupingCollection.ExtractClusters(mesh.Triangles);
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles);
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles);

            //var badTriangles = new List<PositionTriangle>();
            //foreach (var triangle in mesh.Triangles)
            //{
            //    if (triangle.ABadjacents.Count > 1)
            //    {
            //        //Console.WriteLine($"{triangle.Id} AB count {triangle.ABadjacents.Count} Parent tag {triangle.Tag} Adjacent tags {string.Join(",", triangle.ABadjacents.Select(t => t.Tag))}");
            //        //badTriangles.Add(triangle);
            //        badTriangles.AddRange(triangle.ABadjacents.Where(a => a.Tag != triangle.Tag));

            //    }
            //    if (triangle.BCadjacents.Count > 1)
            //    {
            //        //Console.WriteLine($"{triangle.Id} BC count {triangle.BCadjacents.Count} Parent tag {triangle.Tag} Adjacent tags {string.Join(",", triangle.BCadjacents.Select(t => t.Tag))}");
            //        //badTriangles.Add(triangle);
            //        badTriangles.AddRange(triangle.BCadjacents.Where(a => a.Tag != triangle.Tag));
            //    }
            //    if (triangle.CAadjacents.Count > 1)
            //    {
            //        //Console.WriteLine($"{triangle.Id} CA count {triangle.CAadjacents.Count} Parent tag {triangle.Tag} Adjacent tags {string.Join(",", triangle.CAadjacents.Select(t => t.Tag))}");
            //        //badTriangles.Add(triangle);
            //        badTriangles.AddRange(triangle.CAadjacents.Where(a => a.Tag != triangle.Tag));
            //    }
            //}

            //mesh.RemoveAllTriangles(badTriangles);

            //var test = mesh.Triangles.SingleOrDefault(t => t.Id == 318561);
            //if (test is not null)
            //{
            //    var adjacents = test.ABadjacents;
            //    WavefrontFile.Export([test], $"Wavefront/Parent-318561");
            //    WavefrontFile.Export(adjacents, $"Wavefront/Adjacents-318561");
            //}



            Console.WriteLine($"Clusters {clusters.Count()}  Surfaces {surfaces.Count()}  Faces {faces.Count()}", ConsoleColor.Yellow);
            Console.WriteLine();
            BaseObjects.Console.WriteLine("Position cardinalities", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(mesh.Positions.GroupCounts(g => g.Cardinality).DisplayByLine());
            BaseObjects.Console.WriteLine("AB Adjacency counts", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(mesh.Triangles.Select(t => t.ABadjacents).GroupCounts(g => g.Count).DisplayByLine());
            BaseObjects.Console.WriteLine("BC Adjacency counts", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(mesh.Triangles.Select(t => t.BCadjacents).GroupCounts(g => g.Count).DisplayByLine());
            BaseObjects.Console.WriteLine("CA Adjacency counts", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(mesh.Triangles.Select(t => t.CAadjacents).GroupCounts(g => g.Count).DisplayByLine());

            //WavefrontFile.Export(badTriangles, $"Wavefront/BadTriangles");

            var tags = mesh.Triangles.Where(t => t.AdjacentAnyCount < 3 && t.Triangle.MaxEdge.Length > 0.0);
            var openEdges = tags.Select(t => new { t, t.OpenEdges }).ToArray();
            if (openEdges.Length == 0) { Console.WriteLine("No open edges"); return; }
            Console.WriteLine($"Open edges {openEdges.Length}");
            foreach (var openEdge in openEdges)
            {
                Console.WriteLine($"Open edge Triangle {openEdge.t.Id} Length {openEdge.t.Triangle.MaxEdge.Length} Aspect {openEdge.t.Triangle.AspectRatio} Height {openEdge.t.Triangle.MinHeight}\n{string.Join("\n", openEdge.OpenEdges.Select(o => $"Key {o.Key} Segment {o.Segment}"))}\n", ConsoleColor.Red);
                //Console.WriteLine($"Open edges {string.Join("\n", openEdge.OpenEdges.Select(o => $"Key {o.Key} Segment {o.Segment}"))}\n", ConsoleColor.Red);
            }
        }

        public static void ShowSmallDistances(this IWireFrameMesh mesh)
        {
            var bucket = new BoxBucket<Position>(mesh.Positions);
            var counts = new Dictionary<int, int>();
            foreach(var position in mesh.Positions)
            {
                var nearbyPoints = bucket.Fetch(new Rectangle3D(position.Point, 1e-6)).Where(p => p.Id != position.Id && Point3D.Distance(p.Point, position.Point) < 1e-7).ToArray();

                if (!counts.ContainsKey(nearbyPoints.Length)) { counts[nearbyPoints.Length] = 0; }
                counts[nearbyPoints.Length]++;
            }

            foreach (var pair in counts.OrderBy(p => p.Key))
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }
    }
}
