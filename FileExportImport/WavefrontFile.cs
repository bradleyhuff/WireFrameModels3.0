using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;
using BaseObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using BasicObjects.GeometricObjects;

namespace FileExportImport
{
    public static class WavefrontFile
    {
        public static void Export(IEnumerable<PositionTriangle> triangles, string fileName)
        {
            var grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(triangles.Select(t => t.Triangle), "", 0);
            Export(grid, fileName);
        }

        public static void Export(IEnumerable<LineSegment3D> segments, string fileName)
        {
            var grid = WireFrameMesh.Create();
            grid.AddRangeTriangles(segments.Select(s => new Triangle3D(s.Start, s.Center, s.End)), "", 0);
            Export(grid, fileName);
        }
        public static void Export(IEnumerable<IWireFrameMesh> meshes, string fileName)
        {
            int count = 0;
            foreach (var mesh in meshes)
            {
                Export(mesh, $"{fileName}-{count++}");
            }
        }
        public static void Export(IWireFrameMesh mesh, string fileName)
        {
            DateTime start = DateTime.Now;
            fileName = $"{fileName}.obj";
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            FileWrite(mesh, fileName);
            DateTime end = DateTime.Now;
            FileInfo info = new FileInfo(fileName);
            Console.WriteLine($"Exported .OBJ Wavefront File: {fileName}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Positions: {(mesh?.Positions?.Count ?? 0).ToString("#,##0")} PositionNormals: {(mesh?.Positions?.Sum(p => p.PositionNormals.Count) ?? 0).ToString("#,##0")} Triangles: {(mesh?.Triangles?.Count ?? 0).ToString("#,##0")}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Elapsed time: {(end - start).TotalMilliseconds.ToString("#,##0")} milliseconds. File size: {info.DisplayFileSize()}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
        }

        public static void ErrorExport(IWireFrameMesh mesh, string fileName)
        {
            DateTime start = DateTime.Now;
            fileName = $"{fileName}.obj";
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            FileWrite(mesh, fileName);
            DateTime end = DateTime.Now;
            FileInfo info = new FileInfo(fileName);
            Console.WriteLine($"Exported .OBJ Wavefront File: {fileName}", ConsoleColor.Magenta, ConsoleColor.DarkRed);
            Console.WriteLine($"Positions: {(mesh?.Positions?.Count ?? 0).ToString("#,##0")} PositionNormals: {(mesh?.Positions?.Sum(p => p.PositionNormals.Count) ?? 0).ToString("#,##0")} Triangles: {(mesh?.Triangles?.Count ?? 0).ToString("#,##0")}", ConsoleColor.Magenta, ConsoleColor.DarkRed);
            Console.WriteLine($"Elapsed time: {(end - start).TotalMilliseconds.ToString("#,##0")} milliseconds. File size: {info.DisplayFileSize()}", ConsoleColor.Magenta, ConsoleColor.DarkRed);
            Console.WriteLine();
        }

        private static void FileWrite(IWireFrameMesh mesh, string fileName)
        {
            var templateP = $"0.{"0".Repeat(9)}";
            var templateN = $"0.{"0".Repeat(6)}";

            using (StreamWriter file = new StreamWriter(fileName))
            {
                Dictionary<PositionNormal, int> indexTable = new Dictionary<PositionNormal, int>();
                List<PositionNormal> positionNormals = new List<PositionNormal>();
                int index = 0;
                foreach (var position in mesh?.Positions ?? Enumerable.Empty<Position>())
                {
                    foreach (var positionNormal in position.PositionNormals)
                    {
                        indexTable[positionNormal] = ++index;
                        positionNormals.Add(positionNormal);
                    }
                }

                foreach (var positionNormal in positionNormals)
                {
                    file.WriteLine($"v {positionNormal.Position.X.ToString(templateP)} {positionNormal.Position.Y.ToString(templateP)} {positionNormal.Position.Z.ToString(templateP)}");
                }
                foreach (var positionNormal in positionNormals)
                {
                    file.WriteLine($"vn {positionNormal.Normal.X.ToString(templateN)} {positionNormal.Normal.Y.ToString(templateN)} {positionNormal.Normal.Z.ToString(templateN)}");
                }
                foreach (var triangle in mesh?.Triangles ?? Enumerable.Empty<PositionTriangle>())
                {
                    int indexA = indexTable[triangle.A];
                    int indexB = indexTable[triangle.B];
                    int indexC = indexTable[triangle.C];
                    file.WriteLine($"f {indexA}//{indexA} {indexB}//{indexB} {indexC}//{indexC}");
                }
            }
        }
    }
}
