using BasicObjects.GeometricObjects;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class SetDivisionLinks
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;

            var intersections = intermeshTriangles.SelectMany(t => t.Intersections).DistinctBy(t => t.Id).ToArray();

            DisableAndClearDivisionNodes(intersections);
            LinkDivisionNodeEnds(intersections);
            DivisionInterlinks(intersections);
            AllCheckRadiusLinking(intermeshTriangles);
            SamePointDisableAndRemoval(intersections);

            var disabledNodes = intermeshTriangles.SelectMany(t => t.Intersections).DistinctBy(t => t.Id).SelectMany(i => i.Divisions).Where(d => d.Disabled).ToArray();
            var verticies = intersections.SelectMany(i => i.Divisions.SelectMany(d => d.VerticiesAB)).Where(v => v.Vertex is not null).Select(v => v.Vertex).DistinctBy(v => v.Id);
            Console.Write("Intermesh: ", ConsoleColor.Cyan);
            Console.WriteLine($"Set division links. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Magenta);
            //Console.WriteLine();
            //Notes.SetDivisionLinksNotes(intermeshTriangles, intersectionNodes, verticies, disabledNodes);
        }

        private static void DisableAndClearDivisionNodes(IntermeshIntersection[] intersectionNodes)
        {
            foreach (var intersection in intersectionNodes)
            {
                foreach (var division in intersection.Divisions)
                {
                    if (division.Length < 1e-9) { division.Disabled = true; }
                }
            }
            int disabledCount = intersectionNodes.Sum(i => i.Divisions.Count(d => d.Disabled));

            intersectionNodes.ClearDisabledDivisions();
            //Console.WriteLine($"Step 1 Division node removal {disabledCount}");
        }

        private static void SamePointDisableAndRemoval(IntermeshIntersection[] intersectionNodes)
        {
            foreach (var intersectionNode in intersectionNodes)
            {
                var samePointDivisions = intersectionNode.Divisions.Where(d => d.VertexA.Vertex.Id == d.VertexB.Vertex.Id).ToArray();
                if (!samePointDivisions.Any()) { continue; }
                if (intersectionNode.Divisions.Count > 1)
                {
                    foreach (var samePointDivision in samePointDivisions) { samePointDivision.Disabled = true; samePointDivision.VertexA.Delink(); samePointDivision.VertexB.Delink(); }
                }
            }
            int disabledCount = intersectionNodes.Sum(i => i.Divisions.Count(d => d.Disabled));
            intersectionNodes.ClearDisabledDivisions();
            //Console.WriteLine($"Step 5 Same point intersection removal {disabledCount}");
        }

        private static void ClearDisabledDivisions(this IEnumerable<IntermeshIntersection> list)
        {
            foreach (var element in list) { element.ClearDisabledDivisions(); }
        }

        private static void LinkDivisionNodeEnds(IntermeshIntersection[] intersectionNodes)
        {
            int count = 0;
            foreach (var intersectionNode in intersectionNodes)
            {
                VertexCore.Link(intersectionNode.VertexA, intersectionNode.Divisions.First().VertexA);
                VertexCore.Link(intersectionNode.VertexB, intersectionNode.Divisions.Last().VertexB);
                count++;
            }
            //Console.WriteLine($"Step 2 Division end links {count}");
        }

        private static void DivisionInterlinks(IntermeshIntersection[] intersectionNodes)
        {
            int count = 0;
            foreach (var intersectionNode in intersectionNodes.Where(i => i.Divisions.Count() > 1))
            {
                var divisionNodes = intersectionNode.Divisions;

                IntermeshDivision currentNode = null;

                foreach (var divisionNode in divisionNodes)
                {
                    if (currentNode is null) { currentNode = divisionNode; continue; }
                    VertexCore.Link(currentNode.VertexB, divisionNode.VertexA);
                    currentNode = divisionNode;
                    count++;
                }
            }
            //Console.WriteLine($"Step 3 Division interlinks {count}");
        }

        private static void AllCheckRadiusLinking(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            int count = 0;
            aCount = 0; bCount = 0; abCount = 0; divCount = 0;
            foreach (var triangle in intermeshTriangles)
            {
                var vertexContainers = triangle.Intersections.SelectMany(i => i.Divisions.SelectMany(d => d.VerticiesAB)).ToArray();
                foreach (var intersection in triangle.Intersections)
                {
                    var divisionNodes = intersection.Divisions.ToList();
                    for (int i = 0; i < divisionNodes.Count; i++)
                    {
                        var element = divisionNodes[i];

                        if (i == 0)
                        {
                            var vertexPath = element.VertexA.GetTreeUntil(v => Point3D.Distance(v.Point, element.VertexA.Point) > 4e-9).ToArray();
                            var containers = vertexContainers.Where(c => c.Id != element.VertexA.Id && c.Division.Length > 3e-9);
                            containers = containers.ExceptBy(vertexPath.Select(t => t.Id), c => c.Id);
                            AllCheckRadiusLinking(element.VertexA, 3e-9, containers);
                        }
                        {
                            var vertexPath = element.VertexB.GetTreeUntil(v => Point3D.Distance(v.Point, element.VertexB.Point) > 4e-9).ToArray();
                            var containers = vertexContainers.Where(c => c.Id != element.VertexB.Id && c.Division.Length > 3e-9);
                            containers = containers.ExceptBy(vertexPath.Select(t => t.Id), c => c.Id);
                            AllCheckRadiusLinking(element.VertexB, 3e-9, containers);
                        }
                    }
                }
            }

            //Console.WriteLine($"A count {aCount} B count {bCount} AB count {abCount} Div count {divCount} Grouping count {count} Average neighbor {totalNeighbors / (double)count} Average group {totalGroups / (double)count} Tree sum {treeSum}");
            //Console.WriteLine($"Step 4 All check radius linking {count}");
        }
        private static int count = 0;
        private static int totalNeighbors = 0;
        private static int totalGroups = 0;
        private static int treeSum = 0;
        private static IEnumerable<DivisionVertexContainer[]> NeighborhoodGrouping(IEnumerable<DivisionVertexContainer> neighbors)
        {
            var ungroupedNeighbors = neighbors.ToList();
            totalNeighbors += ungroupedNeighbors.Count;
            var groups = new List<DivisionVertexContainer[]>();
            while (ungroupedNeighbors.Any())
            {
                var first = ungroupedNeighbors.First();
                var neighborhoodA = first.GetTreeUntil(v => Point3D.Distance(v.Point, first.Point) > 8e-9).ToArray();
                var neighborhoodB = first.Opposite.GetTreeUntil(v => Point3D.Distance(v.Point, first.Opposite.Point) > 8e-9).ToArray();
                var neighborhood = neighborhoodA.Concat(neighborhoodB).ToArray();
                treeSum += neighborhood.Length;

                var group = ungroupedNeighbors.IntersectBy(neighborhood.Select(n => n.Id), v => v.Id).ToArray();
                ungroupedNeighbors = ungroupedNeighbors.ExceptBy(group.Select(n => n.Id), v => v.Id).ToList();
                groups.Add(group);
            }
            count++;
            totalGroups += groups.Count;
            return groups;
        }

        private static void NearestInRangeLinking(DivisionVertexContainer vertex, double radius, IEnumerable<DivisionVertexContainer> links)
        {
            DivisionVertexContainer nearestLink = null;
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
            if (nearestLink is not null && vertex.Vertex.Id != nearestLink.Vertex.Id) { PreferenceLink(vertex, nearestLink); }
        }

        private static void AllCheckRadiusLinking(DivisionVertexContainer vertex, double radius, IEnumerable<DivisionVertexContainer> links)
        {
            var inRangeLinks = links.Where(l => Point3D.Distance(vertex.Point, l.Point) < radius);
            var divisions = inRangeLinks.Select(p => p.Division).DistinctBy(d => d.Id);
            foreach (var division in divisions)
            {
                LinkDivision(vertex, division);
            }
        }

        private static void LinkDivision(DivisionVertexContainer vertex, IntermeshDivision division)
        {
            if (Point3D.Distance(vertex.Point, division.VertexA.Point) < Point3D.Distance(vertex.Point, division.VertexB.Point))
            {
                if (vertex.Vertex.Id != division.VertexA.Vertex.Id) { PreferenceLink(vertex, division.VertexA); }
            }
            else
            {
                if (vertex.Vertex.Id != division.VertexB.Vertex.Id) { PreferenceLink(vertex, division.VertexB); }
            }
        }

        private static int aCount = 0;
        private static int bCount = 0;
        private static int abCount = 0;
        private static int divCount = 0;

        private static void PreferenceLink(DivisionVertexContainer a, DivisionVertexContainer b)
        {
            if (a.Vertex.IntersectionContainers.Any() && !b.Vertex.IntersectionContainers.Any())
            {
                VertexCore.Link(a, b);
                aCount++;
                return;
            }

            if (!a.Vertex.IntersectionContainers.Any() && b.Vertex.IntersectionContainers.Any())
            {
                VertexCore.Link(b, a);
                bCount++;
                return;
            }
            if (a.Vertex.IntersectionContainers.Any() && b.Vertex.IntersectionContainers.Any())
            {
                abCount++;
                return;
            }
            VertexCore.Link(a, b);
            divCount++;
        }
    }
}
