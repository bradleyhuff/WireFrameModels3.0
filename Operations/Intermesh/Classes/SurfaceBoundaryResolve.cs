using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Basics;

namespace Operations.Intermesh.Classes
{
    internal class SurfaceBoundaryResolve
    {
        internal static void Action(IWireFrameMesh mesh)
        {
            DateTime start = DateTime.Now;
            var surfaceBoundaryEdges = GetSurfaceBoundaryEdges(mesh.Triangles);
            var points = mesh.Positions.Select(p => new KeyValuePair<int, Position>(p.Id, p)).ToDictionary();
            var pointsBucket = new BoxBucket<Position>(mesh.Positions);
            //return;
            //var magnification = 1e4;
            //int id = 153974;
            //var focusAt = mesh.Positions.FirstOrDefault(p => p.Id == id);
            //if (focusAt is not null)
            //{
            //    var zone = new Rectangle3D(focusAt.Point, 1 / magnification);

            //    WavefrontFile.Export(edges.Values.Select(e => zone.Clip(e.Segment))
            //        .Where(c => c is not null)
            //        .Select(c => c.TranslateToPointAndScale(focusAt.Point, magnification)), $"Wavefront/Set/BorderSegments");
            //    var bucket = new BoxBucket<Position>(mesh.Positions);

            //    var matches = bucket.Fetch(focusAt, 1e-4).Where(m => m.Id != focusAt.Id);
            //    var gapTriangles2 = new List<PositionTriangle>();
            //    foreach (var match in matches)
            //    {
            //        var gapTriangles = mesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(focusAt.Id, match.Id)).SelectMany(s => s.Triangles);
            //        Console.WriteLine($"Match of {id}: {match.Id} Distance {Point3D.Distance(match.Point, focusAt.Point).ToString("E2")} Triangles {string.Join(",", gapTriangles.Select(t => t.Key))}");
            //        gapTriangles2.AddRange(gapTriangles);
            //    }

            //    WavefrontFile.Export(gapTriangles2.SelectMany(t => t.Edges).Select(e => zone.Clip(e.Segment))
            //    .Where(c => c is not null)
            //    .Select(c => c.TranslateToPointAndScale(focusAt.Point, magnification)), $"Wavefront/Set/GapTriangles-{id}");

            //    var missingEdge1 = mesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153102, 153103)).First();
            //    var missingEdge2 = mesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153210, 153211)).First();
            //    var missingEdge3 = mesh.Triangles.SelectMany(t => t.Edges).Where(e => e.Key == new Combination2(153197, 153199)).First();
            //    WavefrontFile.Export([
            //        zone.Clip(missingEdge1.Segment).TranslateToPointAndScale(focusAt.Point, magnification),
            //        zone.Clip(missingEdge2.Segment).TranslateToPointAndScale(focusAt.Point, magnification),
            //        zone.Clip(missingEdge3.Segment).TranslateToPointAndScale(focusAt.Point, magnification),
            //        ], $"Wavefront/Set/MissingEdges");
            //    Console.WriteLine($"Missing edge: {missingEdge1.Key} Length {missingEdge1.Segment.Length}");
            //    Console.WriteLine($"Missing edge: {missingEdge2.Key} Length {missingEdge2.Segment.Length}");
            //    Console.WriteLine($"Missing edge: {missingEdge3.Key} Length {missingEdge3.Segment.Length}");
            //}

            var pointCount = GetPointCount(surfaceBoundaryEdges);
            if (!pointCount.Any()) {
                if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Surface Boundary resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
                return; }
            var linkingPairs = GetLinkingPairs(pointCount);
            if (!linkingPairs.Any()) {
                if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Surface Boundary resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
                return; }
            var linkingEdges = linkingPairs.Select(l => (Key: l, A: points[l.A], B: points[l.B])).ToArray();
            //BaseObjects.Console.WriteLine("Linking pairs:");
            foreach (var p in linkingEdges)
            {
                var commonTriangles = p.A.Triangles.IntersectBy(p.B.Triangles.Select(t => t.Id), t => t.Id).ToArray();
                //BaseObjects.Console.WriteLine($"Common triangles [{string.Join(", ", commonTriangles.Select(t => t.Id))}]");
                var segment = new LineSegment3D(p.A.Point, p.B.Point);

                var matches = pointsBucket.Fetch(Rectangle3D.Containing([segment]), 1e-6)
                    .Where(pp => segment.Distance(pp.Point) < 1e-6 && !pointCount.ContainsKey(pp.Id)).ToArray();
                var orderedLinkingPositions = GetOrderedLinkingPositions(p.A, matches, p.B).ToArray();
                var apply = orderedLinkingPositions.Any() && orderedLinkingPositions.Last().Id == p.B.Id;
                BaseObjects.Console.WriteLine($"Ordered Linking Positions: [{string.Join(", ", orderedLinkingPositions.Select(l => l.Id))}]", apply ? ConsoleColor.Green : ConsoleColor.Red);

                if (apply)
                {
                    TriangleReplacement(mesh, commonTriangles, orderedLinkingPositions);
                }
            }

            ShowSurfaceBoundary(mesh);
            //mesh.ShowVitals();
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Surface Boundary resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void TriangleReplacement(IWireFrameMesh mesh, IEnumerable<PositionTriangle> commonTriangles, Position[] orderedLinkedSequence)
        {
            var A = orderedLinkedSequence.First();
            var B = orderedLinkedSequence.Last();

            foreach (var commonTriangle in commonTriangles)
            {
                var triangle = PositionTriangle.GetSurfaceTriangle(commonTriangle);
                var verticies = commonTriangle.Positions.Where(p => p.PositionObject?.Id != A.Id && p.PositionObject?.Id != B.Id);
                if (verticies.Count() != 1) { continue; }
                mesh.RemoveTriangle(commonTriangle);
                var rayC = PositionNormal.GetRay(verticies.Single());

                for (int i = 0; i < orderedLinkedSequence.Length - 1; i++)
                {
                    var a = orderedLinkedSequence[i];
                    var b = orderedLinkedSequence[i + 1];

                    var rayA = triangle.RayFromProjectedPoint(a.Point);
                    var rayB = triangle.RayFromProjectedPoint(b.Point);
                    var added = mesh.AddTriangle(rayA, rayB, rayC, commonTriangle.Trace, commonTriangle.Tag);
                    //BaseObjects.Console.WriteLine($"Add triangles {added.Id} {added.Key}");
                }
            }
        }

        private static IEnumerable<Position> GetOrderedLinkingPositions(Position A, IEnumerable<Position> matches, Position B)
        {
            var pointsToLink = CombinePoints(A, matches, B).ToArray();
            var linkedSets = pointsToLink.Select(l => (Point: l, Links: GetLinkTosFromPoint(l, pointsToLink))).ToArray();

            //var segment = new LineSegment3D(A.Point, B.Point);
            //var Key = new Combination2(A.Id, B.Id);
            //BaseObjects.Console.WriteLine($"{Key} {segment.Length.ToString("E2")}  Near points [{string.Join(", ",
            //    matches.Select(m => $"{m.Id}: {BasicObjects.Math.Math.Min(Point3D.Distance(m.Point, segment.Start), Point3D.Distance(m.Point, segment.End)).ToString("E2")}"))}]\nLinked Sets:\n{string.Join("\n", linkedSets.Select(l => $"{l.Point.Id} => [{string.Join(", ", l.Links.Select(ll => ll.Id))}]"))}");

            return GetLinkedSequence(A, B, linkedSets);
        }

        private static void ShowSurfaceBoundary(IWireFrameMesh mesh)
        {
            var surfaceBoundaryEdges = GetSurfaceBoundaryEdges(mesh.Triangles);
            var pointCount = GetPointCount(surfaceBoundaryEdges);
            var summary = new Dictionary<int, int>();

            foreach (var pair in pointCount)
            {
                if (!summary.ContainsKey(pair.Value.Count)) { summary[pair.Value.Count] = 0; }
                summary[pair.Value.Count]++;
            }
            BaseObjects.Console.WriteLine("Surface boundary [Cardinality, Count]", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(string.Join("\n", summary.OrderBy(s => s.Key).Select(s => $"{s.Key}: {s.Value}")), ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine();
        }

        private static IEnumerable<Position> CombinePoints(Position A, IEnumerable<Position> matches, Position B)
        {
            yield return A;
            foreach (var match in matches)
            {
                yield return match;
            }
            yield return B;
        }

        private static IEnumerable<Position> GetLinkedSequence(Position A, Position B, IEnumerable<(Position Point, IEnumerable<Position> Links)> linkedSets)
        {
            var deadEnds = new Dictionary<int, bool>();
            foreach (var set in linkedSets.Where(l => l.Links.Count() < 2))
            {
                deadEnds[set.Point.Id] = true;
            }

            var usableLinks = linkedSets.Where(l => !deadEnds.ContainsKey(l.Point.Id)).ToDictionary(p => p.Point.Id, p => p.Links);

            var currentPoint = A;
            var nextPoint = A;
            var lastPoint = B;

            while (usableLinks.Any())
            {
                yield return currentPoint;
                if (!usableLinks.ContainsKey(currentPoint.Id) ||
                    usableLinks[currentPoint.Id].Count(p => p.Id != lastPoint.Id && !deadEnds.ContainsKey(p.Id)) != 1) { yield break; }
                nextPoint = usableLinks[currentPoint.Id].Single(p => p.Id != lastPoint.Id && !deadEnds.ContainsKey(p.Id));
                usableLinks.Remove(currentPoint.Id);
                lastPoint = currentPoint;
                currentPoint = nextPoint;
            }

            yield break;
        }

        private static IEnumerable<Position> GetLinkTosFromPoint(Position point, IEnumerable<Position> linkTos)
        {
            var perimeterPoints = new Dictionary<int, bool>();
            foreach (var triangle in point.Triangles)
            {
                perimeterPoints[triangle.A.PositionObject.Id] = true;
                perimeterPoints[triangle.B.PositionObject.Id] = true;
                perimeterPoints[triangle.C.PositionObject.Id] = true;
            }

            return linkTos.Where(l => l.Id != point.Id && perimeterPoints.ContainsKey(l.Id));
        }

        private static Combination2Dictionary<PositionEdge> GetSurfaceBoundaryEdges(IEnumerable<PositionTriangle> triangles)
        {
            var edges = new Combination2Dictionary<PositionEdge>();
            foreach (var triangle in triangles)
            {
                foreach (var edge in triangle.Edges.Where(e => e.Triangles.Count(t => t.Id != triangle.Id) > 1))
                {
                    if (!edges.ContainsKey(edge.Key)) { edges[edge.Key] = edge; }
                }
            }

            return edges;
        }

        private static Dictionary<int, List<PositionEdge>> GetPointCount(Combination2Dictionary<PositionEdge> edges)
        {
            var pointCount = new Dictionary<int, List<PositionEdge>>();
            foreach (var edge in edges.Values)
            {
                if (!pointCount.ContainsKey(edge.A.PositionObject.Id)) { pointCount[edge.A.PositionObject.Id] = new List<PositionEdge>(); }
                if (!pointCount.ContainsKey(edge.B.PositionObject.Id)) { pointCount[edge.B.PositionObject.Id] = new List<PositionEdge>(); }
                pointCount[edge.A.PositionObject.Id].Add(edge);
                pointCount[edge.B.PositionObject.Id].Add(edge);
            }

            return pointCount;
        }

        private static IEnumerable<Combination2> GetLinkingPairs(Dictionary<int, List<PositionEdge>> pointCount)
        {
            var summary = new Dictionary<int, int>();

            foreach (var pair in pointCount)
            {
                if (!summary.ContainsKey(pair.Value.Count)) { summary[pair.Value.Count] = 0; }
                summary[pair.Value.Count]++;
            }
            if (summary.Keys.All(v => v != 1)) { return Enumerable.Empty<Combination2>(); }
            BaseObjects.Console.WriteLine("Surface boundary [Cardinality, Count]", ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine(string.Join("\n", summary.OrderBy(s => s.Key).Select(s => $"{s.Key}: {s.Value}")), ConsoleColor.Yellow);
            BaseObjects.Console.WriteLine();
            //BaseObjects.Console.WriteLine("Deviants", ConsoleColor.Yellow);
            //foreach (var point in pointCount.OrderBy(p => p.Key).Where(p => p.Value.Count == 1))
            //{
            //    BaseObjects.Console.WriteLine($"{point.Key} <{string.Join(",", point.Value.Select(p => $"{p.Key} {p.Segment.Length.ToString("E2")}"))}>");
            //}
            //BaseObjects.Console.WriteLine();

            var singlePoints = pointCount.Where(p => p.Value.Count == 1).Select(p => p.Value.Single().Positions.Single(s => s.PositionObject.Id == p.Key).PositionObject).ToArray();
            var singlePointKeys = singlePoints.Select(p => p.Id).Select(v => new KeyValuePair<int, bool>(v, true)).ToDictionary(p => p.Key, p => p.Value);

            var linkingPairs = new Combination2Dictionary<bool>();

            foreach (var singlePoint in singlePoints)
            {
                var edgeKeys = singlePoint.Triangles.SelectMany(t => t.Edges).Select(e => e.Key)
                    .Where(k => k.Indicies.Any(i => i == singlePoint.Id)).ToArray();
                var matchingKeys = edgeKeys.Where(k => singlePointKeys.ContainsKey(k.A) && singlePointKeys.ContainsKey(k.B));
                foreach (var matchingKey in matchingKeys) { linkingPairs[matchingKey] = true; }
            }

            return linkingPairs.Keys;
        }
    }
}
