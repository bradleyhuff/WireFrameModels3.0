using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
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
