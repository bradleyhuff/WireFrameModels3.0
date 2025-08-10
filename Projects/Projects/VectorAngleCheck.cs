using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class VectorAngleCheck : ProjectBase
    {
        protected override void RunProject()
        {
            var vectorA = new Vector3D(0,0,1);
            var vectorB = vectorA.Transform(Transform.Rotation(Vector3D.BasisX, 1e-15));

            Console.WriteLine($"Angle {Vector3D.Angle(vectorA, vectorB)}");


            var pivot = new Point3D(0.290035769184088, 0.390484310804401, 0.568865390874780);

            var point22 = new Point3D(0.290125035952756, 0.390605178612415, 0.569033597760443);
            var point18 = new Point3D(0.290018261253753, 0.390459997341340, 0.568838499540996);
            var point20 = new Point3D(0.290021574086046, 0.390465664028062, 0.568832886198591);

            var point17 = new Point3D(0.290606546744000, 0.391254583991000, 0.569966648972000);
            var point19 = new Point3D(0.290019965528076, 0.390462912547371, 0.568835611777751);
            var point21 = new Point3D(0.290035769184088, 0.390484310804401, 0.568865390874780);
            var point23 = new Point3D(0.290329236552740, 0.390880580473266, 0.569429289032155);

            //20 17
            //17 18
            //17 23
            //23 22
            //22 21
            //21 19
            //20 19
            //18 19

            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point18, point19);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point18-19");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point19, point20);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point19-20");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point19, point21);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point19-21");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point21, point22);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point21-22");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point22, point23);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point22-23");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point17, point23);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point17-23");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point17, point18);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point17-18");
            }
            {
                var grid = WireFrameMesh.Create();
                var segment = new LineSegment3D(point17, point20);
                grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
                grid.Apply(Transform.Translation(Point3D.Zero - point22));
                grid.Apply(Transform.Scale(1000));
                WavefrontFile.Export(grid, $"Wavefront/Point17-20");
            }



            //Console.WriteLine($"Pivot-22 {Vector3D.Angle(point22 - pivot, point18 - pivot)}");
            //Console.WriteLine($"Pivot-20 {Vector3D.Angle(point20 - pivot, point18 - pivot)}");


            //{
            //    var grid = WireFrameMesh.Create();
            //    var segment = new LineSegment3D(point18, pivot);
            //    grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
            //    grid.Apply(Transform.Translation(Point3D.Zero - pivot));
            //    grid.Apply(Transform.Scale(1000));
            //    WavefrontFile.Export(grid, $"Wavefront/Point18Pivot");
            //}
            //{
            //    var grid = WireFrameMesh.Create();
            //    var segment = new LineSegment3D(point22, pivot);
            //    grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
            //    grid.Apply(Transform.Translation(Point3D.Zero - pivot));
            //    grid.Apply(Transform.Scale(1000));
            //    WavefrontFile.Export(grid, $"Wavefront/Point22Pivot");
            //}
            //{
            //    var grid = WireFrameMesh.Create();
            //    var segment = new LineSegment3D(point20, pivot);
            //    grid.AddTriangle(segment.Start, segment.Center, segment.End, "", 0);
            //    grid.Apply(Transform.Translation(Point3D.Zero - pivot));
            //    grid.Apply(Transform.Scale(1000));
            //    WavefrontFile.Export(grid, $"Wavefront/Point20Pivot");
            //}
        }
    }
}
