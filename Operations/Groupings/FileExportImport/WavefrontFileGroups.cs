using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;

namespace Operations.Groupings.FileExportImport
{
    public static class WavefrontFileGroups
    {
        public static void ExportByFaces(IWireFrameMesh mesh, string fileName)
        {
            ExportByFaces(mesh, o => o, fileName);
        }

        public static void ExportByFaces(IWireFrameMesh mesh, Func<IWireFrameMesh, IWireFrameMesh> overlay, string fileName)
        {
            var clusters = GroupingCollection.ExtractFaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Faces {clusters.Count()} [{string.Join(",", clusters.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = clusters.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }

        public static void ExportBySurfaces(IWireFrameMesh mesh, string fileName)
        {
            ExportBySurfaces(mesh, o => o, fileName);
        }

        public static void ExportBySurfaces(IWireFrameMesh mesh, Func<IWireFrameMesh, IWireFrameMesh> overlay, string fileName)
        {
            var clusters = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Surfaces {clusters.Count()} [{string.Join(",", clusters.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = clusters.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }

        public static void ExportByClusters(IWireFrameMesh mesh, string fileName)
        {
            ExportByClusters(mesh, o => o, fileName);
        }

        public static void ExportByClusters(IWireFrameMesh mesh, Func<IWireFrameMesh, IWireFrameMesh> overlay, string fileName)
        {
            var clusters = GroupingCollection.ExtractClusters(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Clusters {clusters.Count()} [{string.Join(",", clusters.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = clusters.Select(s => s.CreateMesh());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }
    }
}
