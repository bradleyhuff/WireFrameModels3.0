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
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Faces {faces.Count()} [{string.Join(",", faces.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = faces.Select(s => s.Create());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }

        public static void ExportBySurfaces(IWireFrameMesh mesh, string fileName)
        {
            ExportBySurfaces(mesh, o => o, fileName);
        }

        public static void ExportBySurfaces(IWireFrameMesh mesh, Func<IWireFrameMesh, IWireFrameMesh> overlay, string fileName)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Surfaces {surfaces.Count()} [{string.Join(",", surfaces.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = surfaces.Select(s => s.Create());
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
            var meshes = clusters.Select(s => s.Create());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }

        public static void ExportByFolds(IWireFrameMesh mesh, string fileName)
        {
            ExportByFolds(mesh, o => o, fileName);
        }

        public static void ExportByFolds(IWireFrameMesh mesh, Func<IWireFrameMesh, IWireFrameMesh> overlay, string fileName)
        {
            var folds = GroupingCollection.ExtractFolds(mesh.Triangles).ToArray();

            Console.WriteLine();
            Console.WriteLine($"Folds {folds.Count()} [{string.Join(",", folds.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
            var meshes = folds.Select(s => s.Create());
            WavefrontFile.Export(meshes.Select(m => overlay(m)), fileName);
        }
    }
}
