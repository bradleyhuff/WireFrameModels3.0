﻿using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;

namespace Operations.SurfaceSegmentChaining.Chaining.Diagnostics
{
    internal static class WavefrontFileChaining
    {
        public static void Export<T>(ElasticTriangle triangle, ChainingException<T> e, string fileName, double height = 0.01)
        {
            {
                var mesh = WireFrameMesh.CreateMesh();
                triangle.ExportWithSegments(mesh);
                WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Triangle-{triangle.Id}");
            }

            foreach (var record in e.Logs.Select((r, i) => new { Value = r, Index = i }))
            {
                {
                    var mesh = WireFrameMesh.CreateMesh();
                    var starter = new[] { e.ReferenceArray[record.Value.Start], e.ReferenceArray[record.Value.Chaining[0]] };
                    foreach (var element in starter.Select((r, i) => new { Record = r, Index = i }))
                    {
                        mesh.AddPoint(element.Record.Point + -height * 0.5 * (1 - element.Index) * element.Record.Normal);
                    }
                    mesh.EndRow();
                    foreach (var element in starter)
                    {
                        mesh.AddPoint(element.Point);
                    }
                    mesh.EndRow();
                    foreach (var element in starter.Select((r, i) => new { Record = r, Index = i }))
                    {
                        mesh.AddPoint(element.Record.Point + height * 0.5 * (1 - element.Index) * element.Record.Normal);
                    }
                    mesh.EndRow();
                    mesh.EndGrid();
                    WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Chains-Start-{triangle.Id}-{record.Index} ");
                }
                {
                    var mesh = WireFrameMesh.CreateMesh();
                    var chainLoop = record.Value.Chaining.Select(l => e.ReferenceArray[l]).ToArray();

                    if (chainLoop.Length > 1)
                    {
                        var firstPoint = e.ReferenceArray[record.Value.Chaining[0]];
                        mesh.AddPoint(firstPoint.Point + -height * 1.5 * firstPoint.Normal);
                        foreach (var ray in chainLoop.Skip(1))
                        {
                            mesh.AddPoint(ray.Point + -height * ray.Normal);
                        }
                        mesh.AddPoint(firstPoint.Point);
                        mesh.EndRow();
                        foreach (var ray in chainLoop)
                        {
                            mesh.AddPoint(ray.Point);
                        }
                        mesh.AddPoint(firstPoint.Point);
                        mesh.EndRow();
                        mesh.AddPoint(firstPoint.Point + height * 1.5 * firstPoint.Normal);
                        foreach (var ray in chainLoop.Skip(1))
                        {
                            mesh.AddPoint(ray.Point + height * ray.Normal);
                        }
                        mesh.AddPoint(firstPoint.Point);
                        mesh.EndRow();
                        mesh.EndGrid();
                    }

                    WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Chains-Loop-{triangle.Id}-{record.Index} ");
                }
            }
        }

        public static void Export(ElasticTriangle triangle, SpurLoopChainingException<TriangleFillingGroup, int> e, string fileName, double height = 0.01)
        {
            {
                var mesh = WireFrameMesh.CreateMesh();
                triangle.ExportWithSegments(mesh);
                WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Triangle-{triangle.Id}");
            }
            foreach (var spurredLoop in e.Chain.SpurredLoops)
            {
                var spurPoints = spurredLoop.GroupBy(g => g.Reference).Where(g => g.Count() > 1).Select(g => g.First()).ToArray();
                var perimeterPoints = triangle.PerimeterEdges.SelectMany(e => e.PerimeterPoints).ToArray();
                var freeSpurs = spurPoints.Where(s => !perimeterPoints.Any(p => p.Id == s.Reference as int?));
                foreach (var freeSpur in freeSpurs)
                {
                    var mesh = WireFrameMesh.CreateMesh();
                    mesh.AddTriangle(freeSpur.Point + -2 * height * triangle.Triangle.Triangle.Normal, freeSpur.Point, freeSpur.Point + 2 * height * triangle.Triangle.Triangle.Normal);

                    WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/FreeSpurs-{triangle.Id}-{freeSpur.Reference as int?}");
                }

            }

            foreach (var segment in e.SpurConnectingSegments.Select((r, i) => new { Value = r, Index = i }))
            {
                var mesh = WireFrameMesh.CreateMesh();
                var a = e.Chain.ReferenceArray[segment.Value.IndexPointA];
                var b = e.Chain.ReferenceArray[segment.Value.IndexPointB];

                mesh.AddPoint(a.Point + -height * triangle.Triangle.Triangle.Normal);
                mesh.AddPoint(b.Point + -height * triangle.Triangle.Triangle.Normal);
                mesh.EndRow();

                mesh.AddPoint(a.Point);
                mesh.AddPoint(b.Point);
                mesh.EndRow();

                mesh.AddPoint(a.Point + height * triangle.Triangle.Triangle.Normal);
                mesh.AddPoint(b.Point + height * triangle.Triangle.Triangle.Normal);
                mesh.EndRow();
                mesh.EndGrid();
                WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/SpurConnectingSegment-{triangle.Id}-{segment.Index}");
            }
        }
    }
}