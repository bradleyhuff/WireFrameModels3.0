using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
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

        public static IWireFrameMesh Union(params IWireFrameMesh[] grids)
        {
            ConsoleLog.MaximumLevels = 1;
            DateTime start = DateTime.Now;
            string note = "Params Union";
            ConsoleLog.Push(note);

            var output = WireFrameMesh.Create();
            int index = 0;
            foreach(var grid in grids)
            {
                foreach(var triangle in output.AddGrid(grid))
                {
                    triangle.Tag = index;
                }
                index++;
            }
            //{
            //    WavefrontFile.Export(output, "Wavefront/BeforeIntermesh");
            //}
            var space = new Space(output.Triangles.ToArray());
            
            output.Intermesh2();
            //WavefrontFile.Export(output, "Wavefront/AfterIntermesh");
            //WavefrontFileGroups.ExportByFaces(output,"Wavefront/Faces");
            var groups = GroupingCollection.ExtractFaces(output.Triangles).ToArray();
            var remainingGroups = UnionTestAndRemoveGroups(output, groups, space);
            IncludedGroupInverts(remainingGroups);

            //ConsoleLog.MaximumLevels = 8;
            //output.RemoveShortSegments(1e-7);
            //output.RemoveCollinearEdgePoints();
            //output.RemoveCoplanarSurfacePoints();

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"{note}: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
            ConsoleLog.MaximumLevels = 1;
            return output;
        }

        public static IWireFrameMesh Sum(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Sum", gridA, gridB, (a, b) => true);
        }
        private static IWireFrameMesh Run(string note, IWireFrameMesh gridA, IWireFrameMesh gridB, Func<Region, Region, bool> includeGroup)
        {
            ConsoleLog.MaximumLevels = 8;
            DateTime start = DateTime.Now;
            ConsoleLog.Push(note);
            var sum = CombineAndMark(gridA, gridB, out Space space);
            sum.Intermesh2();
            var groups = GroupExtraction(sum);
            var remainingGroups = TestAndRemoveGroups(sum, groups, space, includeGroup);
            IncludedGroupInverts(remainingGroups);

            ConsoleLog.MaximumLevels = 8;
            //sum.RemoveShortSegments(1e-4);
            //sum.RemoveCollinearEdgePoints();
            //sum.RemoveCoplanarSurfacePoints();

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"{note}: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
            ConsoleLog.MaximumLevels = 1;
            return sum;
        }

        private static IWireFrameMesh CombineAndMark(IWireFrameMesh gridA, IWireFrameMesh gridB, out Space space)
        {
            var start = DateTime.Now;

            foreach (var triangle in gridA.Triangles) { triangle.Tag = 1; }
            foreach (var triangle in gridB.Triangles) { triangle.Tag = 2; }

            space = new Space(gridA.Triangles.Select(t => t).Concat(gridB.Triangles.Select(t => t)).ToArray());

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

        private static List<GroupingCollection> TestAndRemoveGroups(IWireFrameMesh grid, GroupingCollection[] groups, Space space, Func<Region, Region, bool> includeGroup)
        {
            var start = DateTime.Now;
            var remainingGroups = new List<GroupingCollection>();
            foreach (var group in groups)
            {
                var testPoint = GetTestPoint(group.Triangles);
                if (testPoint is null) {
                    //remainingGroups.Add(group);
                    continue;
                }

                var tag1Region = Region.OnBoundary;
                var tag2Rregion = Region.OnBoundary;
                var tag = group.Triangles.First().Tag;
                if (tag == 1) { tag2Rregion = space.RegionOfPoint(testPoint, 2); }
                if (tag == 2) { tag1Region = space.RegionOfPoint(testPoint, 1); }
                if (!includeGroup(tag1Region, tag2Rregion))
                {
                    grid.RemoveAllTriangles(group.Triangles);
                }
                else
                {
                    remainingGroups.Add(group);
                }
            }
            ConsoleLog.WriteLine($"Test and remove groups: Remaining groups {remainingGroups.Count} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return remainingGroups;
        }

        private static List<GroupingCollection> UnionTestAndRemoveGroups(IWireFrameMesh grid, GroupingCollection[] groups, Space space)
        {
            var remainingGroups = new List<GroupingCollection>();
            foreach (var group in groups)
            {
                var testPoint = GetTestPoint(group.Triangles);
                if (testPoint is null)
                {
                    //remainingGroups.Add(group);
                    continue;
                }

                var tag = group.Triangles.First().Tag;
                if (!space.PointIsExteriorAt(testPoint, tag))
                {
                    grid.RemoveAllTriangles(group.Triangles);
                }
                else
                {
                    remainingGroups.Add(group);
                }
            }
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
                if(interiorTestPoint is null)
                {
                    continue;
                }
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
            var internalTriangle = triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).FirstOrDefault();
            return internalTriangle?.Triangle.Center;
        }

        private static Point3D GetInternalTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var triangle = triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).FirstOrDefault();
            if(triangle is null)
            {
                return null;
            }

            var direction = Vector3D.Average([triangle.A.Normal, triangle.B.Normal, triangle.C.Normal]);
            return triangle.Triangle.Center + -1e-4 * direction;
        }
    }
}
