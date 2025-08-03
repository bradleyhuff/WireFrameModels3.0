using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using System;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class NearDegenerateFillStrategy : IFillStrategy
    {
        private static int count = 0;
        public static int Count
        {
            get { return count; }
        }
        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            if (!ShouldUseStrategy(triangle)) { throw new InvalidOperationException($"Incorrect triangle fill strategy used."); }
            count++;
            var divisions = triangle.NonSpurDivisions.Select(d => (d.A, d.B));
            var perimeterPoints = triangle.PerimeterPoints;
            var vertices = triangle.Verticies;


            //if (triangle.Id == 9979)
            //{
            //    var p81 = triangle.Points.Single(p => p.Id == 5842);
            //    var p82 = triangle.Points.Single(p => p.Id == 5838);
            //    var p83 = triangle.Points.Single(p => p.Id == 5845);
            //    var p84 = triangle.Points.Single(p => p.Id == 5841);

            //    var grid = WireFrameMesh.Create();
            //    var point81 = triangle.Triangle.MinimumHeightScale(p81.Point, 0.25 / triangle.Triangle.AspectRatio);
            //    var point82 = triangle.Triangle.MinimumHeightScale(p82.Point, 0.25 / triangle.Triangle.AspectRatio);
            //    var point83 = triangle.Triangle.MinimumHeightScale(p83.Point, 0.25 / triangle.Triangle.AspectRatio);
            //    var point84 = triangle.Triangle.MinimumHeightScale(p84.Point, 0.25 / triangle.Triangle.AspectRatio);

            //    //grid.AddTriangle(point81, point82, point84, "", 0);
            //    //grid.AddTriangle(point81, point83, point84, "", 0);
            //    //grid.AddTriangle(point82, point83, point84, "", 0);

            //    //grid.AddTriangle(point81, point83, point82, "", 0);

            //    //grid.Apply(Transform.Translation(-new Vector3D(point81.X, point81.Y, point81.Z)));
            //    //grid.Apply(Transform.Scale(1000));

            //    //WavefrontFile.Export(grid, "Wavefront/ProblemTriangles");
            //    Console.WriteLine($"{triangle.Triangle}");
            //    Console.WriteLine($"Point84: {p84.Point}");
            //    Console.WriteLine($"Point84 is inside of triangle: {triangle.Triangle.PointIsIn(p84.Point)}");
            //    Console.WriteLine($"Point84 is on triangle: {triangle.Triangle.PointIsOn(p84.Point)}");
            //    Console.WriteLine($"Point84 is on segment: {new LineSegment3D(p81.Point, p83.Point).PointIsOnSegment(p84.Point)}");
            //    Console.WriteLine($"Distance point81 and point84 {Point3D.Distance(p81.Point, p84.Point)}");
            //    Console.WriteLine($"Distance point82 and point84 {Point3D.Distance(p82.Point, p84.Point)}");
            //    Console.WriteLine($"Distance point83 and point84 {Point3D.Distance(p83.Point, p84.Point)}");
            //    Console.WriteLine($"point84 is coplanar {triangle.Triangle.Plane.PointIsOnPlane(p84.Point)}");

            //    foreach (var t in p84.Triangles)
            //    {
            //        Console.WriteLine($"Triangle Id {t.Id} P84 is in {t.Triangle.PointIsIn(p84.Point)} P84 is on {t.Triangle.PointIsOn(p84.Point)} P84 is vertex {t.Verticies.Any(v => v.Point == p84.Point)}");
            //    }
            //}


            var strategy = new NearDegenerateFill<IntermeshPoint>(divisions, p => p.Point, p => p.Id, p => vertices.Any(pp => pp.Id == p.Id), p => perimeterPoints.Any(pp => pp.Id == p.Id));

            foreach (var filling in strategy.GetFill())
            {
                triangle.Fillings.Add(new FillTriangle(triangle, filling.Item1, filling.Item2, filling.Item3));
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return triangle.IsNearDegenerate;
        }
    }
}
