using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using Operations.Intermesh;
using Operations.PositionRemovals;
using Operations.Regions;
using Console = BaseObjects.Console;

namespace Operations.SetOperators
{
    public static class Sets
    {
        public static IWireFrameMesh Difference(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Difference", gridA, gridB, (a, b) => (a == Region.OnBoundary && b == Region.Exterior) || (a == Region.Interior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Intersection(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Interesection", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Exterior) || (a != Region.Exterior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Union(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Union", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Interior) || (a != Region.Interior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Sum(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Sum", gridA, gridB, (a, b) => true);
        }
        private static IWireFrameMesh Run(string note, IWireFrameMesh gridA, IWireFrameMesh gridB, Func<Region, Region, bool> includeGroup)
        {
            DateTime start = DateTime.Now;
            ConsoleLog.Push(note);
            var sum = CombineAndMark(gridA, gridB, out Space spaceA, out Space spaceB);
            sum.Intermesh();
            var groups = GroupExtraction(sum);
            var remainingGroups = TestAndRemoveGroups(sum, groups, spaceA, spaceB, includeGroup);
            IncludedGroupInverts(remainingGroups);

            sum.RemoveShortSegments(3e-4);
            sum.RemoveCollinearEdgePoints();
            sum.RemoveCoplanarSurfacePoints();

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"{note}: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
            return sum;
        }

        private static IWireFrameMesh CombineAndMark(IWireFrameMesh gridA, IWireFrameMesh gridB, out Space spaceA, out Space spaceB)
        {
            var start = DateTime.Now;

            foreach (var triangle in gridA.Triangles) { triangle.Trace = "A"; }
            foreach (var triangle in gridB.Triangles) { triangle.Trace = "B"; }

            spaceA = new Space(gridA.Triangles.Select(t => t.Triangle).ToArray());
            spaceB = new Space(gridB.Triangles.Select(t => t.Triangle).ToArray());

            var result = gridA.Clone();
            result.AddGrid(gridB);
            ConsoleLog.WriteLine($"Combine and mark: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return result;
        }

        private static GroupingCollection[] GroupExtraction(IWireFrameMesh intermesh)
        {
            var start = DateTime.Now;
            var groups = GroupingCollection.ExtractSurfaces(intermesh.Triangles).ToArray();

            ConsoleLog.WriteLine($"Group extraction: Groups {groups.Length} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return groups;
        }

        private static List<GroupingCollection> TestAndRemoveGroups(IWireFrameMesh grid, GroupingCollection[] groups, Space spaceA, Space spaceB, Func<Region, Region, bool> includeGroup)
        {
            var start = DateTime.Now;
            var remainingGroups = new List<GroupingCollection>();
            foreach (var group in groups)
            {
                var triangles = group.Triangles.ToArray();
                var testPoint = GetTestPoint(triangles);
                var spaceAregion = Region.OnBoundary;
                var spaceBregion = Region.OnBoundary;
                var trace = triangles.First().Trace;
                if (trace == "A") { spaceBregion = spaceB.RegionOfPoint(testPoint); }
                if (trace == "B") { spaceAregion = spaceA.RegionOfPoint(testPoint); }
                if (!includeGroup(spaceAregion, spaceBregion))
                {
                    grid.RemoveAllTriangles(triangles);
                }
                else
                {
                    remainingGroups.Add(group);
                }
            }
            ConsoleLog.WriteLine($"Test and remove groups: Remaining groups {remainingGroups.Count} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return remainingGroups;
        }

        private static void IncludedGroupInverts(List<GroupingCollection> remainingGroups)
        {
            var start = DateTime.Now;
            var resultShell = new Space(remainingGroups.SelectMany(g => g.Triangles.Select(t => t.Triangle)).ToArray());
            var inverted = new Dictionary<int, bool>();
            int invertedGroups = 0;
            foreach (var group in remainingGroups)
            {
                var interiorTestPoint = GetInternalTestPoint(group.Triangles);
                var region = resultShell.RegionOfPoint(interiorTestPoint);
                if (region == Region.Exterior)
                {
                    foreach (var triangle in group.Triangles)
                    {
                        if (!inverted.ContainsKey(triangle.A.Id))
                        {
                            triangle.A.Normal = -triangle.A.Normal;
                            inverted[triangle.A.Id] = true;
                        }

                        if (!inverted.ContainsKey(triangle.B.Id))
                        {
                            triangle.B.Normal = -triangle.B.Normal;
                            inverted[triangle.B.Id] = true;
                        }

                        if (!inverted.ContainsKey(triangle.C.Id))
                        {
                            triangle.C.Normal = -triangle.C.Normal;
                            inverted[triangle.C.Id] = true;
                        }
                    }
                    invertedGroups++;
                }
            }
            ConsoleLog.WriteLine($"Included group and invert: Groups inverted {invertedGroups} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
        }

        private static Point3D GetTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var internalTriangle = triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).First();
            return internalTriangle.Triangle.Center;
        }

        private static Point3D GetInternalTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var triangle = triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).First();

            var direction = Vector3D.Average([triangle.A.Normal, triangle.B.Normal, triangle.C.Normal]);
            return triangle.Triangle.Center + -1e-4 * direction;
        }
    }
}
