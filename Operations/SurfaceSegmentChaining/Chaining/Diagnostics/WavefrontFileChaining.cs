using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Classes.V1.Elastics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining.Diagnostics
{
    internal static class WavefrontFileChaining
    {
        public static void Export<G, T>(ISurfaceSegmentChaining<G, T> chain, string fileName)
        {
            int i = 0;
            foreach (var perimeterLoop in chain.PerimeterLoops)
            {
                var mesh = WireFrameMesh.Create();

                for (int j = 0; j < perimeterLoop.Length - 1; j++)
                {
                    var segment = new LineSegment3D(perimeterLoop[j].Point, perimeterLoop[j + 1].Point);
                    mesh.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                }
                {
                    var segment = new LineSegment3D(perimeterLoop.Last().Point, perimeterLoop.First().Point);
                    mesh.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                }

                WavefrontFile.Export(mesh, $"{fileName}/PerimeterLoop-{i}");
                i++;
            }
            i = 0;
            foreach (var loop in chain.Loops)
            {
                var mesh = WireFrameMesh.Create();

                for (int j = 0; j < loop.Length - 1; j++)
                {
                    var segment = new LineSegment3D(loop[j].Point, loop[j + 1].Point);
                    mesh.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                }
                {
                    var segment = new LineSegment3D(loop.Last().Point, loop.First().Point);
                    mesh.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                }

                WavefrontFile.Export(mesh, $"{fileName}/Loop-{i}");
                i++;
            }

            i = 0;
            foreach (var loop in chain.SpurredLoops)
            {
                var mesh = WireFrameMesh.Create();

                for (int j = 0; j < loop.Length - 1; j++)
                {
                    var segment = new LineSegment3D(loop[j].Point, loop[j + 1].Point);
                    mesh.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                }

                WavefrontFile.Export(mesh, $"{fileName}/SpurredLoop-{i}");
                i++;
            }

        }

        public static void Export<T>(ElasticTriangle triangle, ChainingException<T> e, string fileName, double height = 0.01)
        {
            {
                var mesh = WireFrameMesh.Create();
                triangle.ExportWithPerimeters(mesh);
                WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Triangle-{triangle.Id}");
            }

            foreach (var record in e.Logs.Select((r, i) => new { Value = r, Index = i }))
            {
                {
                    var mesh = WireFrameMesh.Create();
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
                    var mesh = WireFrameMesh.Create();
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

        public static void Export(ElasticTriangle triangle, SpurLoopChainingException<PlanarFillingGroup, ElasticVertexCore> e, string fileName, double height = 0.01)
        {
            {
                var mesh = WireFrameMesh.Create();
                triangle.ExportWithPerimeters(mesh);
                WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/Triangle-{triangle.Id}");
            }
            foreach (var spurredLoop in e.Chain.SpurredLoops)
            {
                var spurPoints = spurredLoop.GroupBy(g => g.Index).Where(g => g.Count() > 1).Select(g => g.First()).ToArray();
                var perimeterPoints = triangle.PerimeterEdges.SelectMany(e => e.PerimeterPoints).ToArray();
                var freeSpurs = spurPoints.Where(s => !perimeterPoints.Any(p => p.Id == s.Index as int?));
                foreach (var freeSpur in freeSpurs)
                {
                    var mesh = WireFrameMesh.Create();
                    mesh.AddTriangle(freeSpur.Point + -2 * height * triangle.Triangle.Triangle.Normal, freeSpur.Point, freeSpur.Point + 2 * height * triangle.Triangle.Triangle.Normal, "", 0);

                    WavefrontFile.ErrorExport(mesh, $"{fileName}-{triangle.Id}/FreeSpurs-{triangle.Id}-{freeSpur.Index as int?}");
                }

            }

            foreach (var segment in e.SpurConnectingSegments.Select((r, i) => new { Value = r, Index = i }))
            {
                var mesh = WireFrameMesh.Create();
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

        public static void Export<T>(ISurfaceSegmentChaining<PlanarFillingGroup, T> chain, string fileName, double height = 0.01)
        {
            var mesh = WireFrameMesh.Create();
            var chainLoop = chain.PerimeterLoops.First();

            if (chainLoop.Length > 1)
            {
                for (int i = 0; i < chainLoop.Length - 1; i++)
                {
                    var segment = new LineSegment3D(chainLoop[i].Point, chainLoop[i + 1].Point);
                    mesh.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End), "", 0);
                }
                {
                    var segment = new LineSegment3D(chainLoop[0].Point, chainLoop[chainLoop.Length - 1].Point);
                    mesh.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End), "", 0);
                }
            }
            WavefrontFile.ErrorExport(mesh, $"{fileName}/Chains-Loop");
        }
    }
}
