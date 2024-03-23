using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;

namespace Operations.Groupings.FileExportImport
{
    public static class WavefrontFileGroups
    {
        public static void ExportByFace(IWireFrameMesh mesh, string fileName)
        {
            var clusters = GroupingCollection.ExtractFaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Faces {clusters.Count()} [{string.Join(",", clusters.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = clusters.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes, fileName);
        }
        public static void ExportBySurface(IWireFrameMesh mesh, string fileName)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Surfaces {surfaces.Count()} [{string.Join(",", surfaces.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = surfaces.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes, fileName);
        }

        public static void ExportByCluster(IWireFrameMesh mesh, string fileName)
        {
            var clusters = GroupingCollection.ExtractClusters(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Clusters {clusters.Count()} [{string.Join(",", clusters.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = clusters.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes, fileName);
        }
    }
}
