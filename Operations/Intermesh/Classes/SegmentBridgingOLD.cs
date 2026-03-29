using BaseObjects;
using BaseObjects.GeometricObjects.Interfaces;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class SegmentBridgingOLD
    {
        internal static void Action(IEnumerable<IntermeshTriangleOLD> intermeshTriangles, BoxBucket<IntermeshPointOLD> pointsBucket, Combination2Dictionary<IntermeshSegmentOLD> segmentTable)
        {
            DateTime start = DateTime.Now;
            int count = 0;
            foreach (var triangle in intermeshTriangles)
            {
                var brokenEndPoints = triangle.SegmentPoints.Select(p => new BrokenEndPoint()
                {
                    Point = p,
                    Segments = p.Segments.Where(d => d.Triangles.Any(t => t.Id == triangle.Id)).ToArray()
                }).
                Where(a => a.Segments.Count() == 1).ToArray();

                count += brokenEndPoints.Count();
                if (!brokenEndPoints.Any()) { continue; }

                System.Console.WriteLine($"{triangle.Id} Broken points [{string.Join(",", brokenEndPoints.Select(p => p.Point.Id))}]");

                var brokenPointsConnected = new Dictionary<int, bool>();

                foreach (var brokenEndPoint in brokenEndPoints.ToArray())
                {
                    if (brokenPointsConnected.ContainsKey(brokenEndPoint.Point.Id)) { continue; }
                    var allResults = new List<Result>();
                    allResults.AddRange(GetProjectSegmentPoints(triangle, brokenEndPoint));
                    allResults.AddRange(GetOtherPoints(triangle, brokenEndPoint));
                    var nearestResult = Point3D.GetNearestResult(allResults);

                    if (nearestResult.EndPoint is not null)
                    {
                        var key = new Combination2(brokenEndPoint.Point.Id, nearestResult.EndPoint.Id);
                        if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegmentOLD(brokenEndPoint.Point, nearestResult.EndPoint); }
                        var segment = segmentTable[key];

                        triangle.Add(segment);
                        //BaseObjects.Console.WriteLine($"Broken connect {segment.Key}");
                        brokenPointsConnected[nearestResult.EndPoint.Id] = true;
                        brokenPointsConnected[brokenEndPoint.Point.Id] = true;
                        var bridge = new IntermeshSegmentOLD(brokenEndPoint.Point, nearestResult.EndPoint);
                        //BaseObjects.Console.WriteLine($"Add bridge {bridge.Key}");
                        triangle.Add(bridge);
                    }
                    else if (nearestResult.Segment is not null)
                    {
                        var i = LinkIntersectionsOLD.FetchPointAt(nearestResult.Point, pointsBucket);
                        nearestResult.Segment.Add(i);
                        i.Add(nearestResult.Segment);
                        nearestResult.Segment.Add(triangle);
                        //BaseObjects.Console.WriteLine($"Broken connect {nearestResult.Segment.Key} add point {i.Id}");
                        brokenPointsConnected[brokenEndPoint.Point.Id] = true;
                        var bridge = new IntermeshSegmentOLD(brokenEndPoint.Point, i);
                        //BaseObjects.Console.WriteLine($"Add bridge {bridge.Key}");
                        triangle.Add(bridge);
                    }
                }
                
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Segment Bridging. Broken points {count} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IEnumerable<Result> GetProjectSegmentPoints(IntermeshTriangleOLD triangle, BrokenEndPoint brokenEndPoint)
        {
            var projectedPoints = triangle.Segments.
                Where(s => brokenEndPoint.Segments.Single().Id != s.Id).
                Select(s => new
                {
                    Point = s.Segment.ProjectionWithIn(brokenEndPoint.Point.Point),
                    Segment = (IntermeshSegmentOLD)s
                }).
                Where(p => p.Point is not null).
                Select(p => new Result()
                {
                    Point = p.Point,
                    Distance = Point3D.Distance(p.Point, brokenEndPoint.Point.Point),
                    Segment = p.Segment
                });

            return projectedPoints;
        }

        private static IEnumerable<Result> GetOtherPoints(IntermeshTriangleOLD triangle, BrokenEndPoint brokenEndPoint)
        {
            var otherPoints = triangle.SegmentPoints.Where(p => p.Id != brokenEndPoint.Point.Id).
                Select(b => new Result()
                {
                    Point = b.Point,
                    Distance = Point3D.Distance(b.Point, brokenEndPoint.Point.Point),
                    EndPoint = b
                });

            return otherPoints;
        }

        private struct BrokenEndPoint
        {
            public IntermeshPointOLD Point { get; set; }
            public IntermeshSegmentOLD[] Segments { get; set; }
        }

        private struct Result : IResult
        {
            public Point3D Point { get; set; }
            public double Distance { get; set; }
            public IntermeshSegmentOLD Segment { get; set; }
            public IntermeshPointOLD EndPoint { get; set; }
        }
    }
}
