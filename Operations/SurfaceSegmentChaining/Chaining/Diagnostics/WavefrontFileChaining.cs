using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;

namespace Operations.SurfaceSegmentChaining.Chaining.Diagnostics
{
    internal static class WavefrontFileChaining
    {
        public static void Export<T>(ChainingException<T> e, string fileName, double height = 0.01)
        {
            foreach(var record in e.Logs.Select((r, i) => new { Value = r, Index = i }))
            {
                {
                    var mesh = WireFrameMesh.CreateMesh();
                    var starter = new[] { e.ReferenceArray[record.Value.Start], e.ReferenceArray[record.Value.Chaining[0]] };
                    foreach (var element in starter.Select((r, i) => new { Record = r, Index = i}))
                    {
                        mesh.AddPoint(element.Record.Point + -height * 0.5 * (1 - element.Index) * element.Record.Normal, Vector3D.Zero);
                    }
                    mesh.EndRow();
                    foreach (var element in starter)
                    {
                        mesh.AddPoint(element.Point, Vector3D.Zero);
                    }
                    mesh.EndRow();
                    foreach (var element in starter.Select((r, i) => new { Record = r, Index = i }))
                    {
                        mesh.AddPoint(element.Record.Point + height * 0.5 * (1 - element.Index) * element.Record.Normal, Vector3D.Zero);
                    }
                    mesh.EndRow();
                    mesh.EndGrid();
                    WavefrontFile.ErrorExport(mesh, $"{fileName}/Chains-Start-{record.Index} ");
                }
                {
                    var mesh = WireFrameMesh.CreateMesh();
                    var chainLoop = record.Value.Chaining.Select(l => e.ReferenceArray[l]).ToArray();

                    if (chainLoop.Length > 1)
                    {
                        var firstPoint = e.ReferenceArray[record.Value.Chaining[0]];
                        mesh.AddPoint(firstPoint.Point + -height * 1.5 * firstPoint.Normal, Vector3D.Zero);
                        foreach (var ray in chainLoop.Skip(1))
                        {
                            mesh.AddPoint(ray.Point + -height * ray.Normal, Vector3D.Zero);
                        }
                        mesh.AddPoint(firstPoint.Point, Vector3D.Zero);
                        mesh.EndRow();
                        foreach (var ray in chainLoop)
                        {
                            mesh.AddPoint(ray.Point, Vector3D.Zero);
                        }
                        mesh.AddPoint(firstPoint.Point, Vector3D.Zero);
                        mesh.EndRow();
                        mesh.AddPoint(firstPoint.Point + height * 1.5 * firstPoint.Normal, Vector3D.Zero);
                        foreach (var ray in chainLoop.Skip(1))
                        {
                            mesh.AddPoint(ray.Point + height * ray.Normal, Vector3D.Zero);
                        }
                        mesh.AddPoint(firstPoint.Point, Vector3D.Zero);
                        mesh.EndRow();
                        mesh.EndGrid();
                    }

                    WavefrontFile.ErrorExport(mesh, $"{fileName}/Chains-Loop-{record.Index} ");
                }
            }
        }
    }
}
