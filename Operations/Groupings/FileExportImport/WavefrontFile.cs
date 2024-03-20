using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;

namespace Operations.Groupings.FileExportImport
{
    public static class WavefrontFileGroups
    {
        public static void ExportBySurface(IWireFrameMesh mesh, string fileName)
        {
            var grouping = new GroupingCollection(mesh.Triangles);
            var surfaces = grouping.ExtractSurfaces();
            Console.WriteLine();
            Console.WriteLine($"Surfaces {surfaces.Count()} [{string.Join(",", surfaces.Select(s => s.Triangles.Count))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = GetGroupMeshes(surfaces);
            WavefrontFile.Export(meshes, fileName);
        }

        private static IEnumerable<IWireFrameMesh> GetGroupMeshes(IEnumerable<GroupingCollection> groups)
        {
            foreach(var group in groups)
            {
                var groupMesh = WireFrameMesh.CreateMesh();
                foreach (var triangle in group.Triangles)
                {
                    triangle.AddWireFrameTriangle(groupMesh);
                }
                yield return groupMesh;
            }

        }
    }
}
