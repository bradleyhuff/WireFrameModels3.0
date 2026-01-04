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
            if (Logging.ShowLog) { Console.WriteLine($"Triangle\n{string.Join("\n", vertices.Select(v => v.Point))}"); }

            var strategy = new NearDegenerateFill<IntermeshPoint>(divisions, p => p.Point, p => p.Id, p => vertices.Any(pp => pp.Id == p.Id), p => perimeterPoints.Any(pp => pp.Id == p.Id));

            foreach (var filling in strategy.GetFill())
            {
                triangle.Fillings.Add(new FillTriangle(triangle, filling.Item1, filling.Item2, filling.Item3));
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return triangle.Triangle.Normal.Magnitude == 0;
        }
    }
}
