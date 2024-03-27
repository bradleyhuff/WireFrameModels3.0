using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
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
            return Run(gridA, gridB, (a, b) => a == Region.OnBoundary && b == Region.Exterior || a == Region.Interior && b == Region.OnBoundary);
        }

        public static IWireFrameMesh Intersection(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run(gridA, gridB, (a, b) => (a == Region.OnBoundary && b == Region.Interior) || (a == Region.Interior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh Union(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run(gridA, gridB, (a, b) => (a == Region.OnBoundary && b == Region.Exterior) || (a == Region.Exterior && b == Region.OnBoundary));
        }

        public static IWireFrameMesh All(this IWireFrameMesh gridA, IWireFrameMesh gridB)
        {
            return Run(gridA, gridB, (a, b) => true);
        }
        private static IWireFrameMesh Run(IWireFrameMesh gridA, IWireFrameMesh gridB, Func<Region, Region, bool> includeGroup)
        {
            DateTime start = DateTime.Now;
            gridB = gridB.Clone();

            foreach(var triangle in gridA.Triangles){ triangle.Trace = "A"; }
            foreach (var triangle in gridB.Triangles) { triangle.Trace = "B"; }

            var spaceA = new Space(gridA.Triangles.Select(t => t.Triangle).ToArray());
            var spaceB = new Space(gridB.Triangles.Select(t => t.Triangle).ToArray());

            var result = gridA.CreateNewInstance();
            result.AddGrid(gridA);
            result.AddGrid(gridB);

            result = Intermesh.ElasticIntermeshOperations.Operations.Intermesh(result);

            var groups = GroupingCollection.ExtractSurfaces(result.Triangles).ToArray();

            var groupGrids = groups.Select(g => new { g.Id, Grid = g.CreateMesh() });
            var includedGroupGrids = new List<(int Id, IWireFrameMesh Grid)>();
            foreach (var groupGrid in groupGrids)
            {
                var testPoint = GetInternalTestPoint(groupGrid.Grid);
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

            var resultShell = new Space(includedGroupGrids.SelectMany(t => t.Grid.Triangles.Select(t => t.Triangle)).ToArray());
            foreach (var group in includedGroupGrids)
            {
                var testRay = GetInternalTestRay(group.Grid);
                var interiorTestPoint = testRay.Point + -1e-4 * testRay.Normal.Direction;
                var region = resultShell.RegionOfPoint(interiorTestPoint);
                if (region == Region.Exterior)
                {
                    foreach (var triangle in group.Grid.Triangles) { triangle.InvertNormals(); } //Console.WriteLine("Group invert.");
                }
            }
            result = result.CreateNewInstance();
            result.AddGrids(includedGroupGrids.Select(g => g.Grid));

            Console.WriteLine($"Set operation elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
            return result;
        }

        private static Point3D GetInternalTestPoint(IWireFrameMesh mesh)
        {
            var internalTriangle = mesh.Triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).FirstOrDefault() ?? mesh.Triangles.First();
            return internalTriangle.Triangle.Center;
        }

        private static Ray3D GetInternalTestRay(IWireFrameMesh mesh)
        {
            var internalTriangle = mesh.Triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).FirstOrDefault() ?? mesh.Triangles.First();
            var c = internalTriangle.Triangle.GetBarycentricCoordinate(internalTriangle.Triangle.Center);
            Vector3D normal = (c.λ1 * internalTriangle.A.Normal + c.λ2 * internalTriangle.B.Normal + c.λ3 * internalTriangle.C.Normal).Direction;
            return new Ray3D(internalTriangle.Triangle.Center, normal);
        }
    }
}
