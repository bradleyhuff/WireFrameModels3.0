using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Operations.Groupings.Types;
using Operations.Intermesh;
using Operations.PositionRemovals;
using Operations.Regions;

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
            RemoveInvertedSurfaces(mesh);
            RemoveAllOpenFaces(mesh);
            CornerCapping(mesh);
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

        private static void CornerCapping(IWireFrameMesh mesh)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).Select(f => f.Triangles).ToArray();
            Console.WriteLine($"Surfaces to check for corners/trim {surfaces.Length}");
            List<PositionTriangle[]> surfacesToRemove = new List<PositionTriangle[]>();

            foreach (var surface in surfaces)
            {
                var space = new Space(surface.Select(t => t.Triangle).ToArray());
                var testPoint = GetTestPoint(surface);
                var interiorTestPoint = testPoint.Point + -1e-9 * testPoint.Normal.Direction;

                var region = space.RegionOfPoint(interiorTestPoint);

                if (region == Region.Indeterminant && SurfaceHasCorner(surface))
                {
                    surfacesToRemove.Add(surface.ToArray());
                }
            }

            Console.WriteLine($"Surfaces to remove {surfacesToRemove.Count}");

            foreach (var surface in surfacesToRemove)
            {
                mesh.RemoveAllTriangles(surface);
            }
        }

        private static bool SurfaceHasCorner(IEnumerable<PositionTriangle> surface)
        {
            var collection = new GroupingCollection(surface);

            return collection.InternalPoints.Any(p => p.Cardinality > 2);
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
