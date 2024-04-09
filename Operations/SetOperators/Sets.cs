using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using Operations.Regions;
using Console = BaseObjects.Console;

namespace Operations.SetOperators
{
    public static class Sets
    {
        public static IWireFrameMesh Difference(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Difference", gridA, gridB, (a, b) => a == Region.OnBoundary && b != Region.Interior || a != Region.Exterior && b == Region.OnBoundary);
        }

        public static IWireFrameMesh Intersection(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Interesection", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Exterior) || (a != Region.Exterior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Union(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Union", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Interior) || (a != Region.Interior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh All(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("All", gridA, gridB, (a, b) => true);
        }
        private static IWireFrameMesh Run(string note, IWireFrameMesh gridA, IWireFrameMesh gridB, Func<Region, Region, bool> includeGroup)
        {
            DateTime start = DateTime.Now;

            var sum = CloneAndMark(note, gridA, gridB, out Space spaceA, out Space spaceB);
            var intermesh = Intermesh.ElasticIntermeshOperations.Operations.Intermesh(sum);
            var groups = GroupExtraction(note, intermesh);
            var includedGroupGrids = TestAndIncludeGroups(note, groups, spaceA, spaceB, includeGroup);
            IncludedGroupInverts(note, includedGroupGrids);
            var result = BuildResultGrid(note, intermesh, includedGroupGrids);

            Console.WriteLine($"{note}: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n", ConsoleColor.Yellow);
            return result;
        }

        private static IWireFrameMesh CloneAndMark(string note, IWireFrameMesh gridA, IWireFrameMesh gridB, out Space spaceA, out Space spaceB)
        {
            var start = DateTime.Now;
            gridB = gridB.Clone();

            foreach (var triangle in gridA.Triangles) { triangle.Trace = "A"; }
            foreach (var triangle in gridB.Triangles) { triangle.Trace = "B"; }

            spaceA = new Space(gridA.Triangles.Select(t => t.Triangle).ToArray());
            spaceB = new Space(gridB.Triangles.Select(t => t.Triangle).ToArray());

            var result = gridA.CreateNewInstance();
            result.AddGrid(gridA);
            result.AddGrid(gridB);
            Console.Write($"{note}: ", ConsoleColor.Yellow);
            Console.WriteLine($"Clone and mark: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.", ConsoleColor.Cyan);
            return result;
        }

        private static GroupingCollection[] GroupExtraction(string note, IWireFrameMesh intermesh)
        {
            var start = DateTime.Now;
            var groups = GroupingCollection.ExtractSurfaces(intermesh.Triangles).ToArray();
            Console.Write($"{note}: ", ConsoleColor.Yellow);
            Console.WriteLine($"Group extraction: Groups {groups.Length} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.", ConsoleColor.Cyan);
            return groups;
        }

        private static List<(int Id, IWireFrameMesh Grid)> TestAndIncludeGroups(string note, GroupingCollection[] groups, Space spaceA, Space spaceB, Func<Region, Region, bool> includeGroup)
        {
            var start = DateTime.Now;
            var groupGrids = groups.Select(g => new { g.Id, Grid = g.CreateMesh() });
            var includedGroupGrids = new List<(int Id, IWireFrameMesh Grid)>();
            foreach (var groupGrid in groupGrids)
            {
                var testPoint = GetTestPoint(groupGrid.Grid);
                var spaceARegion = Region.OnBoundary;
                var spaceBRegion = Region.OnBoundary;
                var trace = groupGrid.Grid.Triangles.First().Trace;
                if (trace == "A") { spaceBRegion = spaceB.RegionOfPoint(testPoint); }
                if (trace == "B") { spaceARegion = spaceA.RegionOfPoint(testPoint); }

                if (includeGroup(spaceARegion, spaceBRegion))
                {
                    includedGroupGrids.Add((groupGrid.Id, groupGrid.Grid));
                }
            }
            Console.Write($"{note}: ", ConsoleColor.Yellow);
            Console.WriteLine($"Test and include groups: Included groups {includedGroupGrids.Count} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.", ConsoleColor.Cyan);
            return includedGroupGrids;
        }

        private static void IncludedGroupInverts(string note, List<(int Id, IWireFrameMesh Grid)> includedGroupGrids)
        {
            var start = DateTime.Now;
            var resultShell = new Space(includedGroupGrids.SelectMany(t => t.Grid.Triangles.Select(t => t.Triangle)).ToArray());
            var inverted = new Dictionary<int, bool>();
            foreach (var group in includedGroupGrids)
            {
                var interiorTestPoint = GetInternalTestPoint(group.Grid);
                var region = resultShell.RegionOfPoint(interiorTestPoint);
                if (region == Region.Exterior)
                {
                    foreach (var triangle in group.Grid.Triangles)
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
                }
            }
            Console.Write($"{note}: ", ConsoleColor.Yellow);
            Console.WriteLine($"Included group and invert: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.", ConsoleColor.Cyan);
        }

        private static IWireFrameMesh BuildResultGrid(string note, IWireFrameMesh intermesh, List<(int Id, IWireFrameMesh Grid)> includedGroupGrids)
        {
            var start = DateTime.Now;
            var result = intermesh.CreateNewInstance();
            result.AddGrids(includedGroupGrids.Select(g => g.Grid));
            Console.Write($"{note}: ", ConsoleColor.Yellow);
            Console.WriteLine($"Build result grid: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.", ConsoleColor.Cyan);
            return result;
        }

        private static Point3D GetTestPoint(IWireFrameMesh mesh)
        {
            var internalTriangle = mesh.Triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).First();
            return internalTriangle.Triangle.Center;
        }

        private static Point3D GetInternalTestPoint(IWireFrameMesh mesh)
        {
            var triangle = mesh.Triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).First();

            var direction = Vector3D.Average([triangle.A.Normal, triangle.B.Normal, triangle.C.Normal]);
            return triangle.Triangle.Center + -1e-4 * direction;
        }
    }
}
