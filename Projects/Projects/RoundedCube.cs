using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.Basics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    internal class RoundedCube : ProjectBase
    {
        protected override void RunProject()
        {
            var cube = Cuboid.Create(1, 5, 1, 5, 1, 5);
            var radius = 0.1;
            var steps = 6;

            var faces = GroupingCollection.ExtractFaces(cube.Triangles).ToArray();
            var edgeGroups = faces.SelectMany(f => f.PerimeterEdges).GroupBy(s => s.Key, new Combination2Comparer()).ToArray();
            var corners = cube.Positions.Where(p => p.Cardinality > 2).ToArray();

            var roundedCube = cube.CreateNewInstance();

            foreach (var face in faces)
            {
                foreach (var triangle in face.Triangles)
                {
                    roundedCube.AddTriangle(
                        triangle.A.Position + radius * triangle.A.Normal, triangle.A.Normal,
                        triangle.B.Position + radius * triangle.B.Normal, triangle.B.Normal,
                        triangle.C.Position + radius * triangle.C.Normal, triangle.C.Normal);
                }
            }

            foreach (var edgeGroup in edgeGroups)
            {
                var groups = edgeGroup.ToArray();
                var pointA = groups[0].A.Position;
                var pointB = groups[1].B.Position;
                var arcA = VectorTransform3D.UnitCircularArc(groups[0].A.Normal, groups[1].A.Normal, steps).ToArray();
                var arcB = VectorTransform3D.UnitCircularArc(groups[0].B.Normal, groups[1].B.Normal, steps).ToArray();

                for (int i = 0; i < arcA.Length; i++)
                {
                    roundedCube.AddPoint(pointA + radius * arcA[i], arcA[i]);
                    roundedCube.AddPoint(pointB + radius * arcB[i], arcB[i]);
                    roundedCube.EndRow();
                }
                roundedCube.EndGrid();
            }

            foreach (var corner in corners)
            {
                var normals = corner.PositionNormals.Select(p => p.Normal).ToArray();

                BuildSection(roundedCube, radius, steps, corner.Point, normals[0], normals[1], normals[2]);
            }

            roundedCube.ShowVitals();

            WavefrontFile.Export(roundedCube, "Wavefront/roundedCube");
            PntFile.Export(roundedCube, "Pnt/RoundedCube");
        }

        private static void BuildSection(IWireFrameMesh mesh, double radius, int steps, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            var triangle = VectorTransform3D.UnitSphericalTriangle(n0, n1, n2, steps);

            for (int i = 0; i < triangle.Length - 1; i++)
            {
                var row = triangle[i];
                var nextRow = triangle[i + 1];

                for (int j = 0; j < row.Length - 1; j++)
                {
                    AddTriangle(mesh, radius, point, row[j], row[j + 1], nextRow[j]);
                    if (j < row.Length - 2) { AddTriangle(mesh, radius, point, row[j + 1], nextRow[j], nextRow[j + 1]); }
                }
            }
        }

        private static void AddTriangle(IWireFrameMesh mesh, double radius, Point3D point, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            mesh.AddTriangle(point + radius * n0.Direction, n0.Direction,
                point + radius * n1.Direction, n1.Direction,
                point + radius * n2.Direction, n2.Direction);
        }
    }
}
