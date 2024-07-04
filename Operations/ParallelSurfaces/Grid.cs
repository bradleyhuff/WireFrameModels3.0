using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Operations.Groupings.Types;
using Operations.Intermesh;
using Operations.PositionRemovals;
using Operations.Regions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Operations.ParallelSurfaces
{
    public static class Grid
    {
        public static IWireFrameMesh ParallelSurfaces(this IWireFrameMesh mesh, double displacement)
        {
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();
            var surfaceTriangles = faces.SelectMany(f => CreateSet(f.ToArray(), displacement)).ToArray();
            var grid = mesh.CreateNewInstance();
            foreach (var t in surfaceTriangles)
            {
                grid.AddTriangle(t.A.Point, t.A.Normal, t.B.Point, t.B.Normal, t.C.Point, t.C.Normal);
            }

            grid.Intermesh();
            //Trim(grid);
            return grid;
        }

        private static IEnumerable<SurfaceTriangle> CreateSet(PositionTriangle[] triangles, double displacement)
        {
            foreach (var triangle in triangles)
            {
                var aa = new Ray3D(triangle.A.Position + displacement * triangle.A.Normal.Direction, triangle.A.Normal.Direction);
                var bb = new Ray3D(triangle.B.Position + displacement * triangle.B.Normal.Direction, triangle.B.Normal.Direction);
                var cc = new Ray3D(triangle.C.Position + displacement * triangle.C.Normal.Direction, triangle.C.Normal.Direction);

                yield return new SurfaceTriangle(aa, bb, cc);
            }
        }

        public static void Trim(this IWireFrameMesh mesh)
        {
            RemoveAllOpenFaces(mesh);
            RemoveInternalSurfaces(mesh);
            RemoveInvertedSurfaces(mesh);
            RemoveAllOpenFaces(mesh);
        }

        private static void RemoveInvertedSurfaces(IWireFrameMesh mesh)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).Select(f => f.Triangles).ToArray();
            Console.WriteLine($"Surfaces to check for inversion/trim {surfaces.Length}");
            List<PositionTriangle[]> surfacesToRemove = new List<PositionTriangle[]>();

            foreach (var surface in surfaces)
            {
                var faces = GroupingCollection.ExtractFaces(surface.ToArray()).ToArray();
                var space = new Space(surface.Select(t => t.Triangle).ToArray());
                foreach (var face in faces)
                {
                    var testPoint = GetTestPoint(face.Triangles);
                    var interiorTestPoint = testPoint.Point + -1e-9 * testPoint.Normal.Direction;

                    var region = space.RegionOfPoint(interiorTestPoint);
                    if (region == Region.Exterior)
                    {
                        surfacesToRemove.Add(surface.ToArray());
                        break;
                    }
                }
            }

            Console.WriteLine($"Surfaces to remove {surfacesToRemove.Count}");

            foreach (var surface in surfacesToRemove)
            {
                mesh.RemoveAllTriangles(surface);
            }
        }

        private static void RemoveInternalSurfaces(IWireFrameMesh mesh)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).Select(f => new GroupingCollection(f.Triangles)).ToArray();
            Console.WriteLine($"Surfaces to check for internals {surfaces.Length}");

            var associations = AssociatedGroupsByIntersection(surfaces).ToArray();
            List<PositionTriangle[]> surfacesToRemove = new List<PositionTriangle[]>();

            foreach (var association in associations)
            {
                var largestGroup = association.MaxBy(g => Rectangle3D.Containing(g.Triangles.Select(t => t.Triangle).ToArray()).Diagonal);
                var groupsToConsider = association.Where(a => a.Id != largestGroup.Id).OrderBy(a => a.InternalPoints.Any(p => p.Cardinality > 2)).ToArray();

                if (groupsToConsider.Any())
                {
                    surfacesToRemove.Add(groupsToConsider.First().Triangles.ToArray());
                }
            }

            Console.WriteLine($"Surfaces to remove {surfacesToRemove.Count}");

            foreach (var surfaceToRemove in surfacesToRemove)
            {
                mesh.RemoveAllTriangles(surfaceToRemove);
            }
        }

        private static IEnumerable<GroupingCollection[]> AssociatedGroupsByIntersection(GroupingCollection[] surfaces)
        {
            var lookup = new Dictionary<int, GroupingCollection>();
            foreach (var surface in surfaces)
            {
                foreach (var triangle in surface.Triangles)
                {
                    lookup[triangle.Id] = surface;
                }
            }

            var list = new List<GroupEdgeGroupCollection>();
            foreach (var surface in surfaces)
            {
                var perimeter = surface.PerimeterEdges;

                var edgeAssociatedGroups = surface.PerimeterEdges.
                    Select(e => new { Edge = e, Triangles = e.Triangles }).
                    Select(et => new GroupEdgeGroupCollection()
                    {
                        Edge = et.Edge,
                        Groups = et.Triangles.
                    Select(p => lookup[p.Id]).OrderBy(a => a.Id).ToArray()
                    });

                list.AddRange(edgeAssociatedGroups);
            }

            var regroup = Regroup(list);

            return regroup.Where(eg => IsLinked(eg.GroupEdges)).Select(r => r.Groups);
        }

        private class GroupEdgeGroupCollection
        {
            public GroupEdge Edge { get; set; }
            public GroupingCollection[] Groups { get; set; }
        }

        private class GroupCollectionGroupEdges
        {
            public GroupingCollection[] Groups { get; set; }
            public GroupEdge[] GroupEdges { get; set; }
        }

        private static List<GroupCollectionGroupEdges> Regroup(List<GroupEdgeGroupCollection> input)
        {
            var output = new List<GroupCollectionGroupEdges>();

            var groupings = new Dictionary<string, GroupingCollection[]>();
            var edgeSets = new Dictionary<string, List<GroupEdge>>();

            foreach (GroupEdgeGroupCollection edge in input)
            {
                var key = string.Join("|", edge.Groups.Select(g => g.Id));
                groupings[key] = edge.Groups;
            }

            foreach (GroupEdgeGroupCollection edge in input)
            {
                var key = string.Join("|", edge.Groups.Select(g => g.Id));
                if (!edgeSets.ContainsKey(key)) { edgeSets[key] = new List<GroupEdge>(); }
                edgeSets[key].Add(edge.Edge);
            }

            foreach (var keyPair in edgeSets)
            {
                output.Add(new GroupCollectionGroupEdges()
                {
                    Groups = groupings[keyPair.Key],
                    GroupEdges = keyPair.Value.DistinctBy(e => e.Key, new Combination2Comparer()).ToArray()
                });
            }

            return output;
        }

        private static bool IsLinked(GroupEdge[] groupEdges)
        {
            //return true;
            var table = new Dictionary<int, List<int>>();

            foreach (var groupEdge in groupEdges)
            {
                if (!table.ContainsKey(groupEdge.Key.A)) { table[groupEdge.Key.A] = new List<int>(); }
                if (!table.ContainsKey(groupEdge.Key.B)) { table[groupEdge.Key.B] = new List<int>(); }

                table[groupEdge.Key.A].Add(groupEdge.Key.B);
                table[groupEdge.Key.B].Add(groupEdge.Key.A);
            }
            if (table.Values.Any(v => v.Count != 2)) { return false; }

            int previousKey = -1;
            int currentKey = groupEdges.First().Key.A;
            int startKey = currentKey;

            var links = new List<int>() { startKey };

            do
            {
                if (!table.ContainsKey(currentKey)) { return false; }
                var keys = table[currentKey];
                int nextKey = keys[0];
                if (nextKey == previousKey) { nextKey = keys[1]; }
                if (nextKey == startKey) { break; }
                previousKey = currentKey;
                currentKey = nextKey;
                links.Add(currentKey);
            } 
            while (true);

            return links.Count == groupEdges.Length;
        }

        private static void RemoveAllOpenFaces(IWireFrameMesh mesh)
        {
            while (RemoveOpenFaces(mesh)) { }
        }

        private static bool RemoveOpenFaces(IWireFrameMesh mesh)
        {
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();
            Console.WriteLine($"Faces to check/trim {faces.Length}");

            List<PositionTriangle[]> facesToRemove = new List<PositionTriangle[]>();
            int i = -1;
            foreach (var face in faces)
            {
                i++;

                var collection = new GroupingCollection(face);
                var perimeter = collection.PerimeterEdges.ToArray();

                if (perimeter.Any(e => e.IsOpenEdge))
                {
                    facesToRemove.Add(face.ToArray());
                }
            }

            Console.WriteLine($"Faces to remove {facesToRemove.Count}");

            foreach (var face in facesToRemove)
            {
                mesh.RemoveAllTriangles(face);
            }

            return facesToRemove.Count > 0;
        }

        private static Ray3D GetTestPoint(IEnumerable<PositionTriangle> triangles)
        {
            var internalTriangle = triangles.Where(t => !t.Triangle.IsCollinear).OrderByDescending(t => t.Triangle.Area).FirstOrDefault() ?? triangles.First();
            var c = internalTriangle.Triangle.GetBarycentricCoordinate(internalTriangle.Triangle.Center);
            Vector3D normal = (c.λ1 * internalTriangle.A.Normal + c.λ2 * internalTriangle.B.Normal + c.λ3 * internalTriangle.C.Normal).Direction;
            return new Ray3D(internalTriangle.Triangle.Center, normal);
        }

    }
}
