using BaseObjects;
using BaseObjects.GeometricObjects.Interfaces;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class SegmentBridging
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles, BoxBucket<IntermeshPoint> pointsBucket, Combination2Dictionary<IntermeshSegment> segmentTable)
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

                System.Console.WriteLine($"{triangle.Id} Broken points {brokenEndPoints.Count()}");

                var brokenPointsConnected = new Dictionary<int, bool>();

                foreach (var brokenEndPoint in brokenEndPoints)
                {
                    if (brokenPointsConnected.ContainsKey(brokenEndPoint.Point.Id)) { continue; }
                    var allResults = new List<Result>();
                    allResults.AddRange(GetProjectSegmentPoints(triangle, brokenEndPoint));
                    allResults.AddRange(GetOtherPoints(triangle, brokenEndPoint));
                    var nearestResult = Point3D.GetNearestResult(allResults);

                    if(nearestResult.EndPoint is not null)
                    {
                        var key = new Combination2(brokenEndPoint.Point.Id, nearestResult.EndPoint.Id);
                        if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(brokenEndPoint.Point, nearestResult.EndPoint); }
                        var segment = segmentTable[key];

                        triangle.Add(segment);
                        brokenPointsConnected[nearestResult.EndPoint.Id] = true;
                        brokenPointsConnected[brokenEndPoint.Point.Id] = true;
                    }
                    else if (nearestResult.Segment is not null)
                    {
                        var i = LinkIntersections.FetchPointAt(nearestResult.Point, pointsBucket);
                        nearestResult.Segment.Add(i);
                        i.Add(nearestResult.Segment);
                        nearestResult.Segment.Add(triangle);
                        brokenPointsConnected[brokenEndPoint.Point.Id] = true;
                    }
                }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Segment Bridging. Broken points {count} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IEnumerable<Result> GetProjectSegmentPoints(IntermeshTriangle triangle, BrokenEndPoint brokenEndPoint)
        {
            var projectedPoints = triangle.Segments.
                Where(s => brokenEndPoint.Segments.Single().Id != s.Id).
                Select(s => new
                {
                    Point = s.Segment.ProjectionWithIn(brokenEndPoint.Point.Point),
                    Segment = s
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

        private static IEnumerable<Result> GetOtherPoints(IntermeshTriangle triangle, BrokenEndPoint brokenEndPoint)
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
            public IntermeshPoint Point { get; set; }
            public IntermeshSegment[] Segments { get; set; }
        }

        private struct Result : IResult
        {
            public Point3D Point { get; set; }
            public double Distance { get; set; }
            public IntermeshSegment Segment { get; set; }
            public IntermeshPoint EndPoint { get; set; }
        }
    }
}
