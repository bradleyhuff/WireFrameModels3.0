using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class SurfaceBoundaryResolve
    {
        internal static void Action(IWireFrameMesh mesh)
        {
            //var surfaceBorders = intermesh.Triangles.Where(t => t.ABadjacents.Count > 1 || t.BCadjacents.Count > 1 || t.CAadjacents.Count > 1).ToArray();


            //var borderSegments = surfaceBorders.SelectMany(t => t.Edges.Where(e => e.Triangles.Count(et => et.Id != t.Id) > 1)).DistinctBy(b => b.Key, Combination2Comparer.Comparer).ToArray();

            //WavefrontFile.Export(borderSegments, $"Wavefront/Set/BorderSegments-{index}");

            var pointCount = new Dictionary<int, List<PositionEdge>>();
            var edges = new Combination2Dictionary<PositionEdge>();
            foreach (var triangle in mesh.Triangles)
            {
                foreach (var edge in triangle.Edges.Where(e => e.Triangles.Count(t => t.Id != triangle.Id) > 1))
                {
                    if (!edges.ContainsKey(edge.Key)) { edges[edge.Key] = edge; }
                }
            }

            var gapSegments = edges.Values.Where(e => e.Key == new Combination2(153209, 153210) || e.Key == new Combination2(153211, 153304));
            //if (gapSegments.Any())
            //{
            //    var magnification = 1e3;
            //    var focusAt = gapSegments.SelectMany(s => s.Positions).First(p => p.PositionObject.Id == 153211);
            //    var zone = new Rectangle3D(focusAt.Position, 1 / magnification);

            //    WavefrontFile.Export(edges.Values.Select(e => zone.Clip(e.Segment))
            //        .Where(c => c is not null)
            //        .Select(c => c.TranslateToPointAndScale(focusAt.Position, magnification)), $"Wavefront/Set/BorderSegments");
            //}

            var gapTriangles = mesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153210, 153211));

            if (gapTriangles.Any())
            {
                //    var magnification = 1e3;
                var focusAt = gapSegments.SelectMany(s => s.Positions).First(p => p.PositionObject.Id == 153211);
                var focusAtOther = gapSegments.SelectMany(s => s.Positions).First(p => p.PositionObject.Id == 153210);
                //    var zone = new Rectangle3D(focusAt.Position, 1 / magnification);

                //    WavefrontFile.Export(gapTriangles.SelectMany(e => e.Triangles.SelectMany(t => t.Edges)).Select(e => zone.Clip(e.Segment))
                //        .Where(c => c is not null)
                //        .Select(c => c.TranslateToPointAndScale(focusAt.Position, magnification)), $"Wavefront/Set/GapTriangles");

                var bucket = new BoxBucket<Position>(mesh.Positions);

                {
                    var matches = bucket.Fetch(focusAt, 1e-6).Where(m => m.Id != focusAt.PositionObject.Id);
                    foreach (var match in matches)
                    {
                        Console.WriteLine($"Match of 153211: {match.Id} Distance {Point3D.Distance(match.Point, focusAt.Position)}");
                    }
                }
                {
                    var matches = bucket.Fetch(focusAtOther, 1e-6).Where(m => m.Id != focusAtOther.PositionObject.Id);
                    foreach (var match in matches)
                    {
                        Console.WriteLine($"Match of 153210: {match.Id} Distance {Point3D.Distance(match.Point, focusAtOther.Position)}");
                    }
                }
                //    {
                //        var gapEdges = intermesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153974, 153210));

                //        WavefrontFile.Export(gapEdges.SelectMany(e => e.Triangles.SelectMany(t => t.Edges)).Select(e => zone.Clip(e.Segment))
                //            .Where(c => c is not null)
                //            .Select(c => c.TranslateToPointAndScale(focusAt.Position, magnification)), $"Wavefront/Set/GapTriangles2");


                //        var gapEdges2 = intermesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153974, 153211));
                //    }
            }


            foreach (var edge in edges.Values)
            {
                if (!pointCount.ContainsKey(edge.A.PositionObject.Id)) { pointCount[edge.A.PositionObject.Id] = new List<PositionEdge>(); }
                if (!pointCount.ContainsKey(edge.B.PositionObject.Id)) { pointCount[edge.B.PositionObject.Id] = new List<PositionEdge>(); }
                pointCount[edge.A.PositionObject.Id].Add(edge);
                pointCount[edge.B.PositionObject.Id].Add(edge);
            }

            var summary = new Dictionary<int, int>();

            foreach (var pair in pointCount)
            {
                if (!summary.ContainsKey(pair.Value.Count)) { summary[pair.Value.Count] = 0; }
                summary[pair.Value.Count]++;
            }

            if (pointCount.Any())
            {
                BaseObjects.Console.WriteLine("Surface boundary [Cardinality, Count]", ConsoleColor.Yellow);
                BaseObjects.Console.WriteLine(string.Join("\n", summary.OrderBy(s => s.Key).Select(s => $"{s.Key}: {s.Value}")), ConsoleColor.Yellow);
                BaseObjects.Console.WriteLine();
                BaseObjects.Console.WriteLine("Deviants", ConsoleColor.Yellow);
                foreach (var point in pointCount.OrderBy(p => p.Key).Where(p => p.Value.Count != 2))
                {
                    BaseObjects.Console.WriteLine($"{point.Key} <{string.Join(",", point.Value.Select(p => $"{p.Key} {p.Segment.Length.ToString("E2")}"))}>", ConsoleColor.Gray, point.Value.Count != 2 ? ConsoleColor.DarkRed : ConsoleColor.Black);
                }
            }
        }
    }
}
