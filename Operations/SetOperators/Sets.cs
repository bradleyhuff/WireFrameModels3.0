using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
using Operations.Intermesh;
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
            return Run("Intersection", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Exterior) || (a != Region.Exterior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Union(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Union", gridA, gridB, (a, b) => (a == Region.OnBoundary && b != Region.Interior) || (a != Region.Interior && b == Region.OnBoundary));
        }

        private static int count = 1;

        public static IWireFrameMesh Sum(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run("Sum", gridA, gridB, (a, b) => true);
        }
        private static IWireFrameMesh Run(string note, IWireFrameMesh gridA, IWireFrameMesh gridB, Func<Region, Region, bool> includeGroup)
        {
            ConsoleLog.MaximumLevels = 8;

            DateTime start = DateTime.Now;
            if (!Mode.ThreadedRun) ConsoleLog.Push(note);
            var sum = CombineAndMark(gridA, gridB, out Space space);
            if (Mode.ThreadedRun)
            {
                sum.IntermeshSingle();
            }
            else
            {
                sum.Intermesh();
            }

            var surfaces = GroupingCollection.ExtractSurfaces(sum.Triangles).ToArray();

            Console.WriteLine($"Surfaces {surfaces.Count()} [{string.Join(",", surfaces.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);

            //WavefrontFileGroups.ExportBySurfaces(sum,$"Wavefront/OpenEdges/Set-{count}/Surface");            
            {
                //var tags = sum.Triangles.Where(t => t.AdjacentAnyCount < 3 && t.Triangle.MaxEdge.Length > 0.0);
                //var openEdges = tags.Select(t => new { t, t.OpenEdges }).ToArray();
                //Console.WriteLine($"Open edges {openEdges.Length}");
                //WavefrontFile.Export(tags.SelectMany(t => t.OpenEdges).DistinctBy(e => e.Id), $"Wavefront/OpenEdges/Set-{count}/OpenEdges");

                //int twoGroupId = 301633;
                //var twoGroupTriangle = sum.Triangles.SingleOrDefault(t => t.Id == twoGroupId);
                //if (twoGroupTriangle is not null)
                //{
                //    WavefrontFile.Export([twoGroupTriangle], $"Wavefront/OpenEdges/TwoGroupTriangle-{twoGroupId}");
                //    WavefrontFile.Export(twoGroupTriangle.ABadjacents, $"Wavefront/OpenEdges/TwoGroupTriangleABadjacents-{twoGroupId}");
                //    WavefrontFile.Export(twoGroupTriangle.BCadjacents, $"Wavefront/OpenEdges/TwoGroupTriangleBCadjacents-{twoGroupId}");
                //    WavefrontFile.Export(twoGroupTriangle.CAadjacents, $"Wavefront/OpenEdges/TwoGroupTriangleCAadjacents-{twoGroupId}");

                //    var triangle = twoGroupTriangle.Triangle;
                //    var adjacents = twoGroupTriangle.BCadjacents;

                //    foreach (var adjacent in adjacents)
                //    {
                //        var intersections = Triangle3D.LineSegmentIntersections(triangle, adjacent.Triangle);
                //        Console.WriteLine($"Adjacent {adjacent.Id} Intersections {intersections.Count()}");
                //        WavefrontFile.Export(intersections, $"Wavefront/OpenEdges/TwoGroupTriangleBCadjacentIntersections-{twoGroupId}");
                //    }
                //}
            }
            count++;

            var groups = SurfaceExtraction(sum);
            var remainingGroups = TestAndRemoveSurfaces(sum, groups, space, includeGroup);
            IncludedGroupInverts(remainingGroups);

            //ConsoleLog.MaximumLevels = 8;
            //sum.RemoveShortSegments(1e-6);
            //sum.RemoveCollinearEdgePoints();
            //sum.RemoveCoplanarSurfacePoints();

            FoldPrimming(sum);
            int removedTags = RemoveTags(sum);
            //if (removedTags > 0) { BaseObjects.Console.WriteLine($"Tags removed {removedTags}", ConsoleColor.White, ConsoleColor.Red); }

            int removedSpikes = RemoveSpikes(sum);
            //if (removedSpikes > 0) { BaseObjects.Console.WriteLine($"Spikes removed {removedSpikes}", ConsoleColor.Black, ConsoleColor.Yellow); }

            if (!Mode.ThreadedRun) ConsoleLog.Pop();
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"{note}: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
            ConsoleLog.MaximumLevels = 1;
            return sum;
        }

        public static bool OutputGroups { get; set; } = false;

        private static IWireFrameMesh CombineAndMark(IWireFrameMesh gridA, IWireFrameMesh gridB, out Space space)
        {
            var start = DateTime.Now;

            foreach (var triangle in gridA.Triangles) { triangle.Tag = 1; }
            foreach (var triangle in gridB.Triangles) { triangle.Tag = 2; }

            space = new Space(gridA.Triangles.Select(t => t).Concat(gridB.Triangles.Select(t => t)).ToArray());

            var result = gridA.Clone();
            result.AddGrid(gridB);
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Combine and mark: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return result;
        }

        private static GroupingCollection[] SurfaceExtraction(IWireFrameMesh intermesh)
        {
            var start = DateTime.Now;
            var surfaces = GroupingCollection.ExtractSurfaces(intermesh.Triangles).ToArray();
            ConsoleLog.WriteLine($"Surface extraction: Groups {surfaces.Length} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return surfaces;
        }

        private static GroupingCollection[] ClusterExtraction(IWireFrameMesh intermesh)
        {
            var start = DateTime.Now;
            var clusters = GroupingCollection.ExtractClusters(intermesh.Triangles).ToArray();
            ConsoleLog.WriteLine($"Cluster extraction: Groups {clusters.Length} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return clusters;
        }

        private static List<GroupingCollection> TestAndRemoveSurfaces(IWireFrameMesh grid, GroupingCollection[] surfaces, Space space, Func<Region, Region, bool> includeGroup)
        {
            var start = DateTime.Now;
            var remainingSurfaces = new List<GroupingCollection>();
            foreach (var surface in surfaces)
            {
                var testPoint = GetTestPoint(surface.Triangles);
                if (testPoint is null)
                {
                    continue;
                }

                var tag1Region = Region.OnBoundary;
                var tag2Rregion = Region.OnBoundary;
                var tag = surface.Triangles.First().Tag;
                if (tag == 1) { tag2Rregion = space.RegionOfPoint(testPoint, 2); }
                if (tag == 2) { tag1Region = space.RegionOfPoint(testPoint, 1); }
                if (!includeGroup(tag1Region, tag2Rregion))
                {
                    grid.RemoveAllTriangles(surface.Triangles);
                }
                else
                {
                    //BaseObjects.Console.WriteLine($"Remaining surface {surface.Id}");
                    remainingSurfaces.Add(surface);
                }
            }
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Test and remove surfaces: Remaining surfaces {remainingSurfaces.Count} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return remainingSurfaces;
        }

        private static List<GroupingCollection> UnionTestAndRemoveGroups(IWireFrameMesh grid, GroupingCollection[] groups, Space space)
        {
            var remainingGroups = new List<GroupingCollection>();
            foreach (var group in groups)
            {
                var testPoint = GetTestPoint(group.Triangles);
                if (testPoint is null)
                {
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
                if (interiorTestPoint is null || interiorTestPoint.IsNaN)
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
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Included group and invert: Groups inverted {invertedGroups} Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
        }

        private const double SpikeThresehold = 1e-5;

        private static Point3D GetTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var internalTriangle = triangles.Where(t => t.Triangle.MinHeight > SpikeThresehold).OrderByDescending(t => t.Triangle.Area).FirstOrDefault();
            return internalTriangle?.Triangle.Center;
        }

        private static Point3D GetInternalTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var triangle = triangles.Where(t => t.Triangle.MinHeight > SpikeThresehold).OrderByDescending(t => t.Triangle.Area).FirstOrDefault();
            if (triangle is null)
            {
                return null;
            }

            var direction = Vector3D.Average([triangle.A.Normal, triangle.B.Normal, triangle.C.Normal]);
            return triangle.Triangle.Center + -1e-6 * direction;
        }

        internal static void FoldPrimming(this IWireFrameMesh output)
        {
            var start = DateTime.Now;
            var foldedTriangles = output.Triangles.Where(t => t.IsFolded).ToArray();
            if (!foldedTriangles.Any()) { return; }

            var space = new Space(output.Triangles.Select(t => t.Triangle).ToArray());
            var straightenedNormals = new Dictionary<int, Vector3D>();
            foreach (var foldedTriangle in foldedTriangles)
            {
                var principleNormal = foldedTriangle.Triangle.Normal.Direction;
                var testPoint = foldedTriangle.Triangle.Center + 1e-6 * principleNormal;
                var spaceRegion = space.RegionOfPoint(testPoint);
                if (spaceRegion == Region.Interior) { principleNormal = -principleNormal; }

                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.A, principleNormal);
                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.B, principleNormal);
                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.C, principleNormal);
            }

            var replacements = new List<SurfaceTriangle>();

            foreach (var foldedTriangle in foldedTriangles)
            {
                Ray3D a = PositionNormal.GetRay(foldedTriangle.A);
                Ray3D b = PositionNormal.GetRay(foldedTriangle.B);
                Ray3D c = PositionNormal.GetRay(foldedTriangle.C);
                if (straightenedNormals.ContainsKey(foldedTriangle.A.Id)) { a = new Ray3D(foldedTriangle.A.Position, straightenedNormals[foldedTriangle.A.Id].Direction); }
                if (straightenedNormals.ContainsKey(foldedTriangle.B.Id)) { b = new Ray3D(foldedTriangle.B.Position, straightenedNormals[foldedTriangle.B.Id].Direction); }
                if (straightenedNormals.ContainsKey(foldedTriangle.C.Id)) { c = new Ray3D(foldedTriangle.C.Position, straightenedNormals[foldedTriangle.C.Id].Direction); }
                replacements.Add(new SurfaceTriangle(a, b, c));
            }

            output.RemoveAllTriangles(foldedTriangles);
            output.AddRangeTriangles(replacements, "", 0);

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Fold Priming Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
        }

        private static int RemoveTags(IWireFrameMesh output)
        {
            output.ShowVitals();

            var start = DateTime.Now;
            var tags = output.Triangles.Where(t => t.AdjacentAnyCount < 3).ToArray();
            //BaseObjects.Console.WriteLine($"Remove \n{string.Join("\n", tags.Select(t => $"{t.Id} {t.Key} {t.Triangle.MinimumHeight.Normal.Magnitude.ToString("E2")}"))}");
            var table = tags.ToDictionary(t => t.Id, t => t);
            while (tags.Any())
            {
                tags = tags.SelectMany(t => t.SingleAdjacents).Where(t => !table.ContainsKey(t.Id)).DistinctBy(t => t.Id).ToArray();
                //BaseObjects.Console.WriteLine($"Remove \n{string.Join("\n", tags.Select(t => $"{t.Id} {t.Key} {t.Triangle.MinimumHeight.Normal.Magnitude.ToString("E2")}"))}");
                foreach (var tag in tags) { table[tag.Id] = tag; }
            }
            output.RemoveAllTriangles(table.Values);
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Remove tags {table.Values.Count} {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            return table.Values.Count;
        }

        private static int RemoveSpikes(IWireFrameMesh output)
        {
            int count = 0;
            var clusters = ClusterExtraction(output);
            foreach (var cluster in clusters)
            {
                if (cluster.Triangles.All(t => t.Triangle.MinHeight < SpikeThresehold)) { output.RemoveAllTriangles(cluster.Triangles); count++; }
            }
            return count;
        }

        public static void NearCollinearTrianglePairs(this IWireFrameMesh mesh)
        {
            var nearCollinearTriangles = mesh.Triangles.Where(t => t.Triangle.MinHeight < 1e-7 );
            //Console.WriteLine($"Near Collinear Triangles: {nearCollinearTriangles.Count()} [{string.Join(",",nearCollinearTriangles.Select(t => t.Id))}]");
            Console.WriteLine($"Near Collinear Triangles: {nearCollinearTriangles.Count()}");
            Console.WriteLine($"Minimum heights: {string.Join(",", nearCollinearTriangles.Select(t => t.Triangle.MinHeight.ToString("e1")))}");
            Console.WriteLine($"Cardinalities  : {string.Join(",", nearCollinearTriangles.Select(t => t.PositionCardinalities))}");
            Console.WriteLine($"Maximum lengths: {string.Join(",", nearCollinearTriangles.Select(t => t.Triangle.MaxEdge.Length.ToString("e1")))}");
            Console.WriteLine($"Adjacent counts: {string.Join(",", nearCollinearTriangles.Select(t => $"({t.ABadjacents.Count}, {t.BCadjacents.Count}, {t.CAadjacents.Count})"))}");

            RunDividingTriangles(mesh);
            var continueRun = RunStartingTriangles(mesh);
            nearCollinearTriangles = mesh.Triangles.Where(t => t.Triangle.MinHeight < 1e-7);
            Console.WriteLine($"Near Collinear Triangles: {nearCollinearTriangles.Count()}");
            while (continueRun)
            {
                RunDividingTriangles(mesh);
                continueRun = RunStartingTriangles(mesh);

                nearCollinearTriangles = mesh.Triangles.Where(t => t.Triangle.MinHeight < 1e-7);
                Console.WriteLine($"Near Collinear Triangles: {nearCollinearTriangles.Count()}");
            }
            Console.WriteLine($"Minimum heights: {string.Join(",", nearCollinearTriangles.Select(t => t.Triangle.MinHeight.ToString("e1")))}");
            Console.WriteLine($"Cardinalities  : {string.Join(",", nearCollinearTriangles.Select(t => t.PositionCardinalities))}");
            Console.WriteLine($"Maximum lengths: {string.Join(",", nearCollinearTriangles.Select(t => t.Triangle.MaxEdge.Length.ToString("e1")))}");
            Console.WriteLine($"Adjacent counts: {string.Join(",", nearCollinearTriangles.Select(t => $"({t.ABadjacents.Count}, {t.BCadjacents.Count}, {t.CAadjacents.Count})"))}");
            mesh.ShowSmallDistances();
        }

        private static void RunDividingTriangles(IWireFrameMesh mesh)
        {
            var dividableTriangles = mesh.Triangles.Where(t => {
                if (t.Triangle.MinHeight < 2e-7) { return false; }
                int count = 0;
                if (ABpairQualifies(t)) { count++; }
                if (BCpairQualifies(t)) { count++; }
                if (CApairQualifies(t)) { count++; }
                return count > 1;
            });

            var removals = new List<PositionTriangle>();
            var additions = new List<SurfaceTriangle>();

            foreach (var triangle in dividableTriangles)
            {
                removals.Add(triangle);

                var centerRay = new Ray3D(triangle.Triangle.Center, triangle.Triangle.Normal);

                additions.Add(new SurfaceTriangle(PositionNormal.GetRay(triangle.A), PositionNormal.GetRay(triangle.B), centerRay));
                additions.Add(new SurfaceTriangle(PositionNormal.GetRay(triangle.B), PositionNormal.GetRay(triangle.C), centerRay));
                additions.Add(new SurfaceTriangle(PositionNormal.GetRay(triangle.C), PositionNormal.GetRay(triangle.A), centerRay));
            }

            //Console.WriteLine($"Dividable triangles {dividableTriangles.Count()} Removals {removals.Count} Additions {additions.Count}");

            mesh.RemoveAllTriangles(removals);
            mesh.AddRangeTriangles(additions, "", 0);
        }

        private static bool RunStartingTriangles(IWireFrameMesh mesh)
        {
            var startingTriangles = mesh.Triangles.Where(t => {
                if (t.Triangle.MinHeight < 1e-7) { return false; }
                int count = 0;
                if (ABpairQualifies(t)) { count++; }
                if (BCpairQualifies(t)) { count++; }
                if (CApairQualifies(t)) { count++; }
                return count == 1;
            });
            //Console.WriteLine($"Starting triangles {startingTriangles.Count()}");

            var removals = new List<PositionTriangle>();
            var additions = new List<SurfaceTriangle>();
            foreach (var triangle in startingTriangles)
            {
                var pair = GetPair(triangle);
                //Console.Write($"{triangle.Id} {pair.Id}");
                //Console.WriteLine(Rediagonalize(triangle, pair));
                Rediagonalize(triangle, pair, removals, additions);
            }

            //Console.WriteLine($"Triangles to remove {removals.Count} Triangles to add {additions.Count}");
            if (!removals.Any()) { return false; }
            mesh.RemoveAllTriangles(removals);
            mesh.AddRangeTriangles(additions, "", 0);
            return true;
        }

        private static bool ABpairQualifies(PositionTriangle t)
        {
            if (t.ABadjacents.Count() != 1) { return false; }
            var pair = t.ABadjacents.Single();
            if (pair.Triangle.MinHeight > 1e-7) { return false; }

            var side = pair.Triangle.GetEdgeMatch(t.Triangle.EdgeAB.Length);
            if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB; }
            if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC; }
            if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA; }

            //if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB && (pair.A.PositionObject.PositionNormals.Count() == 1 || pair.B.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC && (pair.B.PositionObject.PositionNormals.Count() == 1 || pair.C.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA && (pair.C.PositionObject.PositionNormals.Count() == 1 || pair.A.PositionObject.PositionNormals.Count() == 1); }

            return false;
        }

        private static bool BCpairQualifies(PositionTriangle t)
        {
            if (t.BCadjacents.Count() != 1) { return false; }
            var pair = t.BCadjacents.Single();
            if (pair.Triangle.MinHeight > 1e-7) { return false; }

            var side = pair.Triangle.GetEdgeMatch(t.Triangle.EdgeBC.Length);
            if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB; }
            if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC; }
            if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA; }

            //if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB && (pair.A.PositionObject.PositionNormals.Count() == 1 || pair.B.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC && (pair.B.PositionObject.PositionNormals.Count() == 1 || pair.C.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA && (pair.C.PositionObject.PositionNormals.Count() == 1 || pair.A.PositionObject.PositionNormals.Count() == 1); }

            return false;
        }

        private static bool CApairQualifies(PositionTriangle t)
        {
            if (t.CAadjacents.Count() != 1) { return false; }
            var pair = t.CAadjacents.Single();
            if (pair.Triangle.MinHeight > 1e-7) { return false; }

            var side = pair.Triangle.GetEdgeMatch(t.Triangle.EdgeCA.Length);
            if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB; }
            if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC; }
            if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA; }

            //if (side == Triangle3D.Matching.AB) { return !pair.Triangle.CoverhangsAB && (pair.A.PositionObject.PositionNormals.Count() == 1 || pair.B.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.BC) { return !pair.Triangle.AoverhangsBC && (pair.B.PositionObject.PositionNormals.Count() == 1 || pair.C.PositionObject.PositionNormals.Count() == 1); }
            //if (side == Triangle3D.Matching.CA) { return !pair.Triangle.BoverhangsCA && (pair.C.PositionObject.PositionNormals.Count() == 1 || pair.A.PositionObject.PositionNormals.Count() == 1); }

            return false;
        }
        private static PositionTriangle GetPair(PositionTriangle t)
        {
            if (ABpairQualifies(t)) { return t.ABadjacents.Single(); }
            if (BCpairQualifies(t)) { return t.BCadjacents.Single(); }
            if (CApairQualifies(t)) { return t.CAadjacents.Single(); }
            return null;
        }

        private static string Rediagonalize(PositionTriangle a, PositionTriangle b)
        {
            var positions = new []{ a.Positions,  b.Positions }.SelectMany(p => p);
            var groups = positions.GroupBy(p => p.Id);
            var edges = groups.Where(g => g.Count() == 2).Select(g => g.First()).ToArray();
            var corners = groups.Where(g => g.Count() == 1).Select(g => g.First()).ToArray();
            if (edges.Length != 2 || corners.Length != 2) { return $"Triangles {a.Id}, {b.Id} are separate."; }

            var edges2 = corners;
            var corners2 = edges;

            var a2 = new[] { edges2[0], edges2[1], corners2[0] };
            var b2 = new[] { edges2[0], edges2[1], corners2[1] };

            var triangleA = new Triangle3D(a2[0].Position, a2[1].Position, a2[2].Position);
            var triangleB = new Triangle3D(b2[0].Position, b2[1].Position, b2[2].Position);

            var minAB = Math.Min(a.Triangle.MinHeight, b.Triangle.MinHeight);
            var minAB2 = Math.Min(triangleA.MinHeight, triangleB.MinHeight);

            return $"[{a.Triangle.MinHeight.ToString("e1")}, {b.Triangle.MinHeight.ToString("e1")}] => [{triangleA.MinHeight.ToString("e1")}, {triangleB.MinHeight.ToString("e1")}] {(minAB2 * 0.95 > minAB ? "**": minAB2 > minAB ? "*" : "")}";
        }

        private static void Rediagonalize(PositionTriangle a, PositionTriangle b, List<PositionTriangle> removal, List<SurfaceTriangle> additions)
        {
            var positions = new[] { a.Positions, b.Positions }.SelectMany(p => p);
            var groups = positions.GroupBy(p => p.Id);
            var edges = groups.Where(g => g.Count() == 2).Select(g => g.First()).ToArray();
            var corners = groups.Where(g => g.Count() == 1).Select(g => g.First()).ToArray();
            if (edges.Length != 2 || corners.Length != 2) { return; }

            var edges2 = corners;
            var corners2 = edges;

            var a2 = new[] { edges2[0], edges2[1], corners2[0] };
            var b2 = new[] { edges2[0], edges2[1], corners2[1] };

            var triangleA = new Triangle3D(a2[0].Position, a2[1].Position, a2[2].Position);
            var triangleB = new Triangle3D(b2[0].Position, b2[1].Position, b2[2].Position);

            var minAB = Math.Min(a.Triangle.MinHeight, b.Triangle.MinHeight);
            var minAB2 = Math.Min(triangleA.MinHeight, triangleB.MinHeight);

            if (minAB2 * 0.95 > minAB)
            {
                removal.Add(a);
                removal.Add(b);

                additions.Add(new SurfaceTriangle(PositionNormal.GetRay(a2[0]), PositionNormal.GetRay(a2[1]), PositionNormal.GetRay(a2[2])));
                additions.Add(new SurfaceTriangle(PositionNormal.GetRay(b2[0]), PositionNormal.GetRay(b2[1]), PositionNormal.GetRay(b2[2])));
            }
        }

        private static void ApplyPrincipleNormal(Dictionary<int, Vector3D> straightenedNormals, PositionNormal position, Vector3D principleNormal)
        {
            if (Vector3D.Dot(position.Normal, principleNormal) < 0)
            {
                if (!straightenedNormals.ContainsKey(position.Id))
                {
                    straightenedNormals[position.Id] = principleNormal;
                }
                else
                {
                    straightenedNormals[position.Id] += principleNormal;
                }
            }
        }
    }
}
