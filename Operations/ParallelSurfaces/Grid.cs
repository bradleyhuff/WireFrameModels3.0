using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using Operations.Intermesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            PositionTriangle[] triangles;


            //var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();
            //Console.WriteLine($"Faces to check/trim {faces.Length}");

            do
            {
                triangles = mesh.Triangles.Where(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()).ToArray();
                Console.WriteLine($"Triangles to trim {triangles.Length}");
                mesh.RemoveAllTriangles(triangles);
            } while (mesh.Triangles.Any(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()));


            //triangles = mesh.Triangles.Where(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()).ToArray();
            //Console.WriteLine($"Triangles to trim {triangles.Length}");
            //mesh.RemoveAllTriangles(triangles);
            //triangles = mesh.Triangles.Where(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()).ToArray();
            //Console.WriteLine($"Triangles to trim {triangles.Length}");
            //mesh.RemoveAllTriangles(triangles);
            //triangles = mesh.Triangles.Where(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()).ToArray();
            //Console.WriteLine($"Triangles to trim {triangles.Length}");
            //mesh.RemoveAllTriangles(triangles);
            //triangles = mesh.Triangles.Where(t => !t.ABadjacents.Any() || !t.BCadjacents.Any() || !t.CAadjacents.Any()).ToArray();
            //Console.WriteLine($"Triangles to trim {triangles.Length}");
            //mesh.RemoveAllTriangles(triangles);
        }

    }
}
