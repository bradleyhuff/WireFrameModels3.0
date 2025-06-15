using BaseObjects;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;
using Collections.WireFrameMesh.Basics;
using BasicObjects.MathExtensions;
using BasicObjects.GeometricObjects;
using Collections.Buckets;

namespace Operations.Intermesh.Classes
{
    internal class OpenEdgesFill
    {
        internal static void Action(IWireFrameMesh mesh)
        {
            DateTime start = DateTime.Now;

            var tags = mesh.Triangles.Where(t => t.AdjacentAnyCount < 3 && t.Triangle.MaxEdge.Length > 0.0).ToArray();
            var openEdges = tags.Select(t => new { t, t.OpenEdges }).ToArray();
            if (!openEdges.Any()) {
                ConsoleLog.WriteLine($"Open edges fill. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
                return;
            }
            var edges = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(t => t.Key, new Combination2Comparer()).ToArray();
            var fullTags = tags.Where(t => !t.Triangle.IsCollinear).ToArray();
            var bucket = new BoxBucket<PositionEdge>(edges);
            var fillers = new List<SurfaceTriangle>();
            var removals = new List<PositionTriangle>();

            var matchCount = 0;
            foreach (var fullTag in fullTags)
            {
                foreach(var openEdge in fullTag.OpenEdges)
                {
                    var matches = bucket.Fetch(openEdge).Where(m => m.Key != openEdge.Key && m.Segment.Length > 1e-9 && 
                        openEdge.Segment.PointIsOnSegment(m.A.Position) && openEdge.Segment.PointIsOnSegment(m.B.Position));

                    if (matches.Any())
                    {
                        matchCount++;
                        var groups = matches.SelectMany(m => m.Positions).GroupBy(m => m.PositionObject.Id);
                        var center = groups.Single(g => g.Count() > 1).FirstOrDefault();
                        if (center is null) { Console.WriteLine("Center is null"); continue; }
                        var wings = groups.Where(g => g.Count() == 1).Select(w => w.First()).ToArray();
                        if (wings.Length != 2) { Console.WriteLine($"Wings length {wings.Length}"); continue; }

                        var opposite = fullTag.Positions.Single(p => p.PositionObject.Id != wings[0].PositionObject.Id && p.PositionObject.Id != wings[1].PositionObject.Id);

                        removals.Add(fullTag);
                        fillers.Add(new SurfaceTriangle(PositionNormal.GetRay(wings[0]), PositionNormal.GetRay(center), PositionNormal.GetRay(opposite)));
                        fillers.Add(new SurfaceTriangle(PositionNormal.GetRay(wings[1]), PositionNormal.GetRay(center), PositionNormal.GetRay(opposite)));
                    }
                }
            }

            removals.AddRange(tags.Where(t => t.Triangle.IsCollinear));

            mesh.AddRangeTriangles(fillers, "", 0);
            mesh.RemoveAllTriangles(removals);

            ConsoleLog.WriteLine($"Open edges fill. Matches {matchCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
