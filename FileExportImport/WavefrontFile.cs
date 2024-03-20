using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;

namespace FileExportImport
{
    public static class WavefrontFile
    {
        public static void Export(IEnumerable<IWireFrameMesh> meshes, string fileName)
        {
            int count = 0;
            foreach(var mesh in meshes)
            {
                Export(mesh, $"{fileName}-{count++}");
            }
        }
        public static void Export(IWireFrameMesh mesh, string fileName)
        {
            DateTime start = DateTime.Now;
            fileName = $"{fileName}.obj";
            FileWrite(mesh, fileName);
            DateTime end = DateTime.Now;
            FileInfo info = new FileInfo(fileName);
            Console.WriteLine($"Exported .OBJ Wavefront File: {fileName}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Positions: {mesh.Positions.Count.ToString("#,##0")} PositionNormals: {mesh.Positions.Sum(p => p.PositionNormals.Count)} Triangles: {mesh.Triangles.Count.ToString("#,##0")}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Elapsed time: {(end - start).TotalMilliseconds.ToString("#,##0")} milliseconds. File size: {info.DisplayFileSize()}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
        }
        private static void FileWrite(IWireFrameMesh mesh, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                Dictionary<PositionNormal, int> indexTable = new Dictionary<PositionNormal, int>();
                List<PositionNormal> positionNormals = new List<PositionNormal>();
                int index = 0;
                foreach (var position in mesh.Positions)
                {
                    foreach (var positionNormal in position.PositionNormals)
                    {
                        indexTable[positionNormal] = ++index;
                        positionNormals.Add(positionNormal);
                    }
                }

                foreach (var positionNormal in positionNormals)
                {
                    file.WriteLine($"v {positionNormal.Position.X.ToString("0.000000000")} {positionNormal.Position.Y.ToString("0.000000000")} {positionNormal.Position.Z.ToString("0.000000000")}");
                }
                foreach (var positionNormal in positionNormals)
                {
                    file.WriteLine($"vn {positionNormal.Normal.X.ToString("0.000000")} {positionNormal.Normal.Y.ToString("0.000000")} {positionNormal.Normal.Z.ToString("0.000000")}");
                }
                foreach (var triangle in mesh.Triangles)
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
