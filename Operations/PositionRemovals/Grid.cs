using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Elastics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.PositionRemovals
{
    public static class Grid
    {
        public static void RemovePosition(this IWireFrameMesh mesh, Position position)
        {
            var trianglesToRemove = new List<PositionTriangle>();
            var table = new Dictionary<int, PositionNormal>();
            var fillingsToAdd = new List<int[]>();

            foreach (var positionNormal in position.PositionNormals)
            {
                var triangles = positionNormal.Triangles.ToArray();
                foreach(var triangle in triangles)
                {
                    table[triangle.A.Id] = triangle.A;
                    table[triangle.B.Id] = triangle.B;
                    table[triangle.C.Id] = triangle.C;
                }
                var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                var collection = new SurfaceSegmentCollections<PlanarFillingGroup>(segmentSet);
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, int>.Create(collection);

                var planarFilling = new PlanarFilling<PlanarFillingGroup, int>(chain, position.Id);
                var fillings = planarFilling.Fillings.Select(f => new int[] { f.A.Reference, f.B.Reference, f.C.Reference });
                fillingsToAdd.AddRange(fillings);
                trianglesToRemove.AddRange(triangles);
            }

            foreach(var filling in fillingsToAdd)
            {
                mesh.AddTriangle(table[filling[0]], table[filling[1]], table[filling[2]]);
            }
            Console.WriteLine($"Triangle add {fillingsToAdd.Count}");

            int removed = mesh.RemoveAllTriangles(trianglesToRemove);
            Console.WriteLine($"Triangles removed {removed}");
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, int> CreateSurfaceSegmentSet(PositionNormal positionNormal, IEnumerable<PositionTriangle> triangles)
        {
            var arc = triangles.SelectMany(t => t.Edges).Where(e => !e.ContainsPosition(positionNormal.PositionObject)).ToList();
            var plane = new Plane(positionNormal.Position, positionNormal.Normal);
            var box = Rectangle3D.Containing(triangles.Select(t => t.Box).ToArray());
            var endPoints = arc.SelectMany(s => s.Positions).GroupBy(g => g.PositionObject.Id).Where(g => g.Count() == 1).Select(g => g.First()).ToArray();
            arc.Add(new PositionEdge(endPoints[0], endPoints[1]));

            return new SurfaceSegmentSets<PlanarFillingGroup, int>
            {
                NodeId = positionNormal.PositionObject.Id,
                GroupObject = new PlanarFillingGroup(plane, box.Diagonal),
                DividingSegments = new SurfaceSegmentContainer<int>[0],
                PerimeterSegments = arc.Select(e => new SurfaceSegmentContainer<int>(
                    new SurfaceRayContainer<int>(PositionNormal.GetRay(e.A), e.A.Id, e.A.Id),
                    new SurfaceRayContainer<int>(PositionNormal.GetRay(e.B), e.B.Id, e.B.Id))).ToArray()
            };
        }
    }
}
