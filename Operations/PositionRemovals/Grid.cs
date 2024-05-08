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

namespace Operations.PositionRemovals
{
    public static class Grid
    {
        public static void RemovePosition(this IWireFrameMesh mesh, Position position)
        {
            var trianglesToRemove = new List<PositionTriangle>();
            var fillingsToAdd = new List<PositionNormal[]>();

            foreach (var positionNormal in position.PositionNormals)
            {
                var triangles = positionNormal.Triangles.ToArray();
                var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);

                var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, position.Id);
                var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });
                fillingsToAdd.AddRange(fillings);
                trianglesToRemove.AddRange(triangles);
            }

            foreach(var filling in fillingsToAdd)
            {
                mesh.AddTriangle(filling[0], filling[1], filling[2]);
            }
            Console.WriteLine($"Triangle add {fillingsToAdd.Count}");

            int removed = mesh.RemoveAllTriangles(trianglesToRemove);
            Console.WriteLine($"Triangles removed {removed}");
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, PositionNormal> CreateSurfaceSegmentSet(PositionNormal positionNormal, IEnumerable<PositionTriangle> triangles)
        {
            var arc = triangles.SelectMany(t => t.Edges).Where(e => !e.ContainsPosition(positionNormal.PositionObject)).ToList();
            var plane = new Plane(positionNormal.Position, positionNormal.Normal);
            var box = Rectangle3D.Containing(triangles.Select(t => t.Box).ToArray());
            var endPoints = arc.SelectMany(s => s.Positions).GroupBy(g => g.PositionObject.Id).Where(g => g.Count() == 1).Select(g => g.First()).ToArray();
            if (endPoints.Any())
            {
                arc.Add(new PositionEdge(endPoints[0], endPoints[1]));//Connect gap left by removed position.
            }

            return new SurfaceSegmentSets<PlanarFillingGroup, PositionNormal>
            {
                NodeId = positionNormal.PositionObject.Id,
                GroupObject = new PlanarFillingGroup(plane, box.Diagonal),
                DividingSegments = Array.Empty<SurfaceSegmentContainer<PositionNormal>>(),
                PerimeterSegments = arc.Select(e => new SurfaceSegmentContainer<PositionNormal>(
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.A), e.A.Id, e.A),
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.B), e.B.Id, e.B))).ToArray()
            };
        }
    }
}
