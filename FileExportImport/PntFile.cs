using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Console = BaseObjects.Console;
using BaseObjects;


namespace FileExportImport
{
    public static class PntFile
    {
        public static void Export(IWireFrameMesh mesh, string fileName)
        {
            DateTime start = DateTime.Now;
            fileName = $"{fileName}.pnt";
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            FileWrite(mesh, fileName);
            DateTime end = DateTime.Now;
            FileInfo info = new FileInfo(fileName);
            Console.WriteLine($"Exported .PNT Pnt File: {fileName}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Positions: {mesh.Positions.Count.ToString("#,##0")} PositionNormals: {mesh.Positions.Sum(p => p.PositionNormals.Count).ToString("#,##0")} Triangles: {mesh.Triangles.Count.ToString("#,##0")}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine($"Elapsed time: {(end - start).TotalMilliseconds.ToString("#,##0")} milliseconds. File size: {info.DisplayFileSize()}", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            Console.WriteLine();
        }

        public static IWireFrameMesh Import<T>(Func<T> createMesh, string fileName) where T : IWireFrameMesh
        {
            DateTime start = DateTime.Now;
            fileName = $"{fileName}.pnt";
            int index = 0;
            Point3D activePosition = null;

            Dictionary<int, Point3D> positions = new Dictionary<int, Point3D>();
            Dictionary<int, Vector3D> normals = new Dictionary<int, Vector3D>();
            T mesh = createMesh();

            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    ++index;
                    string[] parts = line.Split(' ').Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();

                    if (parts.Length != 4) { continue; }

                    if (parts[0] == "p")
                    {
                        activePosition = new Point3D(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
                    }
                    if (parts[0] == "n" && activePosition is not null)
                    {
                        positions[index] = activePosition;
                        normals[index] = new Vector3D(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
                    }
                    if (parts[0] == "t")
                    {
                        var x = int.Parse(parts[1]);
                        var y = int.Parse(parts[2]);
                        var z = int.Parse(parts[3]);

                        mesh.AddTriangle(positions[x], normals[x], positions[y], normals[y], positions[z], normals[z]);
                    }
                }
            }

            DateTime end = DateTime.Now;
            FileInfo info = new FileInfo(fileName);
            Console.WriteLine($"Imported .PNT Pnt File: {fileName}", ConsoleColor.Magenta, ConsoleColor.DarkBlue);
            Console.WriteLine($"Positions: {mesh.Positions.Count.ToString("#,##0")} PositionNormals: {mesh.Positions.Sum(p => p.PositionNormals.Count).ToString("#,##0")} Triangles: {mesh.Triangles.Count.ToString("#,##0")}", ConsoleColor.Magenta, ConsoleColor.DarkBlue);
            Console.WriteLine($"Elapsed time: {(end - start).TotalMilliseconds.ToString("#,##0")} milliseconds. File size: {info.DisplayFileSize()}", ConsoleColor.Magenta, ConsoleColor.DarkBlue);
            Console.WriteLine();
            return mesh;
        }

        private static void FileWrite(IWireFrameMesh mesh, string fileName)
        {
            var templateP = $"0.{"0".Repeat(12)}";
            var templateN = $"0.{"0".Repeat(9)}";

            using (StreamWriter file = new StreamWriter(fileName))
            {
                Dictionary<PositionNormal, int> indexTable = new Dictionary<PositionNormal, int>();
                int index = 0;

                foreach (var position in mesh.Positions)
                {
                    ++index;
                    file.WriteLine($"p {position.Point.X.ToString(templateP)} {position.Point.Y.ToString(templateP)} {position.Point.Z.ToString(templateP)}");
                    foreach (var normal in position.PositionNormals)
                    {
                        indexTable[normal] = ++index;
                        file.WriteLine($"n {normal.Normal.X.ToString(templateN)} {normal.Normal.Y.ToString(templateN)} {normal.Normal.Z.ToString(templateN)}");
                    }
                }
                foreach (var triangle in mesh.Triangles)
                {
                    int indexA = indexTable[triangle.A];
                    int indexB = indexTable[triangle.B];
                    int indexC = indexTable[triangle.C];
                    file.WriteLine($"t {indexA} {indexB} {indexC}");
                }
            }
        }
    }
}
