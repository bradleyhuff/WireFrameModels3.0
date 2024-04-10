using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class SetIntersectionLinks
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var intersectionNodes = intermeshTriangles.SelectMany(t => t.Intersections).DistinctBy(t => t.Id).ToArray();

            DisableIntersectionNodes(intersectionNodes);

            var unlinkedVerticies = intersectionNodes.Where(i => !i.Disabled).SelectMany(l => l.VerticiesAB).Where(v => v.Vertex is null).ToList();
            //Console.WriteLine($"Unlinked verticies {unlinkedVerticies.Count()}");

            var bucket = new BoxBucket<IntermeshIntersection>(intersectionNodes.Where(i => !i.Disabled).ToArray());

            SegmentLengthNearestCheckLinking(ref unlinkedVerticies, bucket);
            BidirectionRadiusNearestCheckLinking(ref unlinkedVerticies, bucket);
            MonodirectionalRadiusNearestCheckLinking(ref unlinkedVerticies, bucket);

            var linkedVerticies = intersectionNodes.Where(i => !i.Disabled).SelectMany(l => l.VerticiesAB).Where(v => v.Vertex is not null).DistinctBy(v => v.Id).ToList();
            AllCheckRadiusLinking(linkedVerticies, bucket);

            SamePointDisableAndRemoval(intersectionNodes);

            foreach (var triangle in intermeshTriangles) { triangle.ClearDisabledIntersections(); }
            ConsoleLog.WriteLine($"Set intersection links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            //Console.WriteLine();
        }

        private static void DisableIntersectionNodes(IEnumerable<IntermeshIntersection> intersectionNodes)
        {
            foreach (var intersectionNode in intersectionNodes)
            {
                if (intersectionNode.Intersection.Length < 1e-9) { intersectionNode.Disabled = true; }
            }

            //Console.WriteLine($"Before removing multiples {intersectionNodes.Count(i => !i.Disabled)}");

            var bucket = new BoxBucket<IntermeshIntersection>(intersectionNodes.Where(i => !i.Disabled).ToArray());
            foreach (var intersectionNode in intersectionNodes.Where(i => !i.Disabled))
            {
                var matches = bucket.Fetch(intersectionNode);
                foreach (var match in matches)
                {
                    if (intersectionNode.Intersection.Equals(match.Intersection)) { intersectionNode.AddMultiple(match); }
                }
            }

            foreach (var intersectionNode in intersectionNodes)
            {
                if (intersectionNode.Disabled) { continue; }
                foreach (var multiple in intersectionNode.Multiples)
                {
                    multiple.Disabled = true;
                }
            }

            //Console.WriteLine($"Step 1 After removing multiples {intersectionNodes.Count(i => !i.Disabled)}");
        }

        private static void SamePointDisableAndRemoval(IEnumerable<IntermeshIntersection> intersectionNodes)
        {
            int count = 0;
            foreach (var intersectionNode in intersectionNodes.Where(i => !i.Disabled &&
                i.VertexA.Vertex is not null && i.VertexB.Vertex is not null &&
                i.VertexA.Vertex.Id == i.VertexB.Vertex.Id))
            {
                intersectionNode.Disabled = true; intersectionNode.VertexA.Delink(); intersectionNode.VertexB.Delink();

                count++;
            }
            //Console.WriteLine($"Step 6 Same point intersection removal {count}");
        }

        private static void SegmentLengthNearestCheckLinking(ref List<IntersectionVertexContainer> unlinkedVerticies, BoxBucket<IntermeshIntersection> bucket)
        {
            foreach (var vertex in unlinkedVerticies) { SegmentLengthNearestCheckLinking(vertex, bucket); }
            unlinkedVerticies = unlinkedVerticies.Where(v => v.Vertex is null).ToList();
            //Console.WriteLine($"Step 2 Segment length nearest check linking {unlinkedVerticies.Count()}");
        }

        private static void SegmentLengthNearestCheckLinking(IntersectionVertexContainer vertex, BoxBucket<IntermeshIntersection> bucket)
        {
            var unlinkedNeighbors = GetNeighbors(vertex, 3e-9, bucket).Where(n => n.Vertex is null);
            SegmentLengthNearestCheckLinking(vertex, 3e-9, unlinkedNeighbors);
        }

        private static void BidirectionRadiusNearestCheckLinking(ref List<IntersectionVertexContainer> unlinkedVerticies, BoxBucket<IntermeshIntersection> bucket)
        {
            bool applyLink = unlinkedVerticies.Any();
            while (applyLink)
            {
                applyLink = false;
                foreach (var vertex in unlinkedVerticies) { applyLink |= BidirectionRadiusNearestCheckLinking(vertex, bucket); }
                if (!applyLink) { break; }
                unlinkedVerticies = unlinkedVerticies.Where(v => v.Vertex is null).ToList();
                //Console.WriteLine($"Step 3 Bidirectional radius nearest check linking {unlinkedVerticies.Count()}");
            }
            //Console.WriteLine($"Step 3 Bidirectional radius nearest check linking {unlinkedVerticies.Count()}");
        }

        private static bool BidirectionRadiusNearestCheckLinking(IntersectionVertexContainer vertex, BoxBucket<IntermeshIntersection> bucket)
        {
            var unlinkedNeighbors = GetNeighbors(vertex, 3e-9, bucket).Where(n => n.Vertex is null);
            var nearestNeighbor = GetNearestNeighbor(vertex, unlinkedNeighbors);
            if (nearestNeighbor is null) { return false; }

            var unlinkedNeighbors2 = GetNeighbors(nearestNeighbor, 3e-9, bucket).Where(n => n.Vertex is null);
            var nearestNeighbor2 = GetNearestNeighbor(nearestNeighbor, unlinkedNeighbors2);

            if (vertex.Id == nearestNeighbor2.Id)
            {
                VertexCore.Link(vertex, nearestNeighbor);
                return true;
            }
            return false;
        }

        private static void MonodirectionalRadiusNearestCheckLinking(ref List<IntersectionVertexContainer> unlinkedVerticies, BoxBucket<IntermeshIntersection> bucket)
        {
            foreach (var vertex in unlinkedVerticies) { MonodirectionalRadiusNearestCheckLinking(vertex, bucket); }
            unlinkedVerticies = unlinkedVerticies.Where(v => v.Vertex is null).ToList();
            //Console.WriteLine($"Step 4 Monodirection radius nearest check linking {unlinkedVerticies.Count()}");
        }

        private static void MonodirectionalRadiusNearestCheckLinking(IntersectionVertexContainer vertex, BoxBucket<IntermeshIntersection> bucket)
        {
            var vertexPath = vertex.GetTreeUntil(v => Point3D.Distance(v.Point, vertex.Point) > 4e-9).ToArray();

            var neighbors = GetNeighbors(vertex, 3e-9, bucket);
            neighbors = neighbors.ExceptBy(vertexPath.Select(t => t.Id), c => c.Id);
            var nearestNeighbor = GetNearestNeighbor(vertex, neighbors);
            if (nearestNeighbor is null) { return; }

            VertexCore.Link(vertex, nearestNeighbor);
        }

        private static void AllCheckRadiusLinking(List<IntersectionVertexContainer> linkedVerticies, BoxBucket<IntermeshIntersection> bucket)
        {
            foreach (var vertex in linkedVerticies) { AllCheckRadiusLinking(vertex, bucket); }
            //Console.WriteLine($"Step 5 All check radius linking {linkedVerticies.Count()}");
        }

        private static void AllCheckRadiusLinking(IntersectionVertexContainer vertex, BoxBucket<IntermeshIntersection> bucket)
        {
            if (vertex.Vertex is null) { Console.WriteLine($"Null vertex {vertex.Id}"); return; }

            var vertexPath = vertex.GetTreeUntil(v => Point3D.Distance(v.Point, vertex.Point) > 4e-9).ToArray();
            var neighbors = GetNeighbors(vertex, 3e-9, bucket);
            neighbors = neighbors.ExceptBy(vertexPath.Select(t => t.Id), c => c.Id);

            var neighborhoods = NeighborhoodGrouping(neighbors);

            foreach (var neighbor in neighbors)
            {
                VertexCore.Link(vertex, neighbor);
            }
        }

        private static IEnumerable<IntersectionVertexContainer[]> NeighborhoodGrouping(IEnumerable<IntersectionVertexContainer> neighbors)
        {
            var ungroupedNeighbors = neighbors.ToList();
            var groups = new List<IntersectionVertexContainer[]>();
            while (ungroupedNeighbors.Any())
            {
                var first = ungroupedNeighbors.First();
                var neighborhood = first.GetTreeUntil(v => Point3D.Distance(v.Point, first.Point) > 8e-9).ToList();
                var neighborhoodOpposite = first.GetTreeUntil(v => Point3D.Distance(v.Point, first.Opposite.Point) > 8e-9);
                neighborhood.AddRange(neighborhoodOpposite);
                var group = ungroupedNeighbors.IntersectBy(neighborhood.Select(n => n.Id), v => v.Id);
                ungroupedNeighbors = ungroupedNeighbors.ExceptBy(group.Select(n => n.Id), v => v.Id).ToList();
                groups.Add(group.ToArray());
            }

            return groups;
        }

        private static IEnumerable<IntersectionVertexContainer> GetNeighbors(IntersectionVertexContainer vertex, double radius, BoxBucket<IntermeshIntersection> bucket)
        {
            return bucket.Fetch(vertex.Intersection).SelectMany(i => i.VerticiesAB).
                Where(l => Point3D.Distance(vertex.Point, l.Point) < radius && l.Intersection.Id != vertex.Intersection.Id);
        }

        private static IntersectionVertexContainer GetNearestNeighbor(IntersectionVertexContainer vertex, IEnumerable<IntersectionVertexContainer> neighbors)
        {
            IntersectionVertexContainer nearestNeighbor = null;
            double nearestDistance = double.MaxValue;
            foreach (var neighbor in neighbors)
            {
                double distance = Point3D.Distance(neighbor.Point, vertex.Point);
                if (distance < nearestDistance) { nearestNeighbor = neighbor; nearestDistance = distance; }
            }

            return nearestNeighbor;
        }

        private static void SegmentLengthNearestCheckLinking(IntersectionVertexContainer vertex, double confirmationRange, IEnumerable<IntersectionVertexContainer> links)
        {
            NearestInRangeLink(vertex, System.Math.Min(confirmationRange, vertex.Intersection.Intersection.Length * 0.99), links);
        }
        private static void NearestInRangeLink(IntersectionVertexContainer vertex, double radius, IEnumerable<IntersectionVertexContainer> links)
        {
            IntersectionVertexContainer nearestLink = null;
            double distance = radius;
            foreach (var link in links.Where(l => Point3D.Distance(vertex.Point, l.Point) < radius))
            {
                var linkDistance = Point3D.Distance(vertex.Point, link.Point);
                if (linkDistance < distance)
                {
                    distance = linkDistance;
                    nearestLink = link;
                }
            }
            if (nearestLink is not null)
            {
                VertexCore.Link(vertex, nearestLink);
            }
        }
    }
}
