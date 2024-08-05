using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using Operations.Intermesh;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.PlanarFilling.Basics;
using Plane = BasicObjects.GeometricObjects.Plane;
using Operations.PlanarFilling.Filling;
using Console = BaseObjects.Console;

namespace Operations.ParallelSurfaces
{
    public static class Grid
    {
        public static IWireFrameMesh SetFacePlates(this IWireFrameMesh mesh, double thickness)
        {
            var output = AddParallelSurfaces(mesh, thickness);

            output.Intermesh();

            RemoveFoldedSurfaces(output);
            FillPlateSides(output, thickness);
            ElbowFill(output);

            return output;
        }

        private static IWireFrameMesh AddParallelSurfaces(IWireFrameMesh mesh, double thickness)
        {
            var output = mesh.CreateNewInstance();
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();

            foreach (var face in faces.Select((s, i) => new { s, i }))
            {
                var parallelSurface = CreateParallelSurface(face.s, thickness);
                var addedTriangles = output.AddRangeTriangles(parallelSurface);
                foreach (var triangle in addedTriangles) { triangle.Trace = $"S{face.i}"; }
                addedTriangles = output.AddRangeTriangles(face.s.Select(PositionTriangle.GetSurfaceTriangle));
                foreach (var triangle in addedTriangles) { triangle.Trace = $"B{face.i}"; }
            }

            return output;
        }

        private static void RemoveFoldedSurfaces(IWireFrameMesh output)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(output.Triangles).ToArray();
            var groups = surfaces.GroupBy(s => s.Triangles.First().Trace);

            foreach (var group in groups.Where(g => g.Count() > 1))
            {
                foreach (var surface in group.Select((s, i) => new { s, i }))
                {
                    foreach (var triangle in surface.s.Triangles) { triangle.Trace = surface.i.ToString(); }
                }
                var foldedSurfaces = group.Where(s => IsFolded(s.Triangles)).ToArray();
                output.RemoveAllTriangles(foldedSurfaces.SelectMany(s => s.Triangles));

                foreach (var triangle in group.SelectMany(s => s.Triangles)) { triangle.Trace = group.Key; }
            }
        }

        private static void FillPlateSides(IWireFrameMesh output, double thickness)
        {
            var sides = output.CreateNewInstance();

            var groups = output.Triangles.GroupBy(t => t.Trace).ToArray();
            var pairs = groups.GroupBy(g => g.Key.Substring(1)).ToArray();

            var groupEdgePairs = GroupEdgePairs(pairs).ToArray();

            foreach (var pair in groupEdgePairs)
            {
                var baseEdges = pair[0];
                var parallelEdges = pair[1];

                var positions = parallelEdges.SelectMany(e => e.SelectMany(e => e.Positions)).Select(p => new PointNode(p.PositionObject.Point)).ToArray();
                var bucket = new BoxBucket<PointNode>(positions);

                var edgeTrace = parallelEdges[0][0].A.Triangles.First().Trace;
                edgeTrace = edgeTrace.Replace('S', 'B');

                var baseChain = baseEdges.Select(s => SurfaceSegmentChaining<Empty, Position>.Create(
                    new SurfaceSegmentCollections<Empty, Position>(CreateSurfaceSegmentSet(s)))).ToArray();

                var baseLoop = baseChain[0].PerimeterLoops[0].Select(p => p.Reference).ToArray();

                AddPlateSides(baseLoop, thickness, bucket, edgeTrace, sides);
            }

            output.AddGrid(sides);
        }

        private static void AddPlateSides(Position[] baseLoop, double thickness, BoxBucket<PointNode> bucket, string edgeTrace, IWireFrameMesh sides)
        {
            for (int i = 0; i < baseLoop.Length; i++)
            {
                var length = baseLoop.Length - 1;
                var position = baseLoop[i];
                var nextPosition = baseLoop[i < length ? i + 1 : 0];

                var commonTrace = position.Triangles.IntersectBy(nextPosition.Triangles.Select(t => t.Id), t => t.Id).Where(t => t.Trace != edgeTrace).Single().Trace;

                var normal = position.PositionNormals.Single(pn => pn.Triangles.First().Trace == edgeTrace).Normal;
                var nextNormal = nextPosition.PositionNormals.Single(pn => pn.Triangles.First().Trace == edgeTrace).Normal;

                var surfacePoint = position.Point + thickness * normal.Direction;
                var nextSurfacePoint = nextPosition.Point + thickness * nextNormal.Direction;
                var surfaceNormal = position.PositionNormals.Single(pn => pn.Triangles.First().Trace == commonTrace).Normal;

                var surfacePointIsFound = bucket.Fetch(new PointNode(surfacePoint)).Any(p => p.Point == surfacePoint);
                var nextSurfacePointIsFound = bucket.Fetch(new PointNode(nextSurfacePoint)).Any(p => p.Point == nextSurfacePoint);

                if (!surfacePointIsFound || !nextSurfacePointIsFound) { continue; }

                var nextSurfaceNormal = nextPosition.PositionNormals.Single(pn => pn.Triangles.First().Trace == commonTrace).Normal;

                var triangle1 = new SurfaceTriangle(new Ray3D(position.Point, surfaceNormal), new Ray3D(surfacePoint, surfaceNormal), new Ray3D(nextPosition.Point, nextSurfaceNormal));
                var triangle2 = new SurfaceTriangle(new Ray3D(surfacePoint, surfaceNormal), new Ray3D(nextSurfacePoint, nextSurfaceNormal), new Ray3D(nextPosition.Point, nextSurfaceNormal));
                var addedTriangles = sides.AddRangeTriangles([triangle1, triangle2]);
                foreach (var triangle in addedTriangles) { triangle.Trace = edgeTrace.Replace('B', 'E'); }
            }
        }

        private static void ElbowFill(IWireFrameMesh mesh)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();
            var groups = surfaces.GroupBy(s => s.Triangles.First().Trace.Substring(1));

            foreach (var group in groups)
            {
                var triangles = GroupTriangles(group);
                var openEdges = triangles.SelectMany(t => OpenEdges(t, group.Key)).ToArray();
                if (!openEdges.Any()) { continue; }

                var chain = SurfaceSegmentChaining<PlanarFillingGroup, Position>.Create(
                    new SurfaceSegmentCollections<PlanarFillingGroup, Position>(CreateSurfaceSegmentSet(openEdges)));               

                for (int i = 0; i < chain.PerimeterLoops.Count; i++)
                {
                    var points = chain.PerimeterLoops[i].Select(p => p.Point).ToArray();
                    var box = Rectangle3D.Containing(points);
                    var plane = new Plane(points[0], points[1], points[2]);
                    chain.PerimeterLoopGroupKeys[i] = i;
                    chain.PerimeterLoopGroupObjects[i] = new PlanarFillingGroup(plane, box.Diagonal);
                }

                var chains = Chaining.SplitByPerimeterLoops(chain);

                foreach(var element in chains.Select((s, i) => new { s, i }))
                {
                    var plane = element.s.PerimeterLoopGroupObjects.First().Plane;
                    var isOppositeDirection = element.s.PerimeterLoops.Single().Select(p => p.Reference.PositionNormals.Any(pn => Vector3D.Dot(pn.Normal, plane.Normal) < (-1 + 1e-6))).Any(r => r);
                    var normal = isOppositeDirection ? -plane.Normal : plane.Normal;
                    var planarFilling = new PlanarFilling<PlanarFillingGroup, Position>(element.s, element.i);
                    var fillings = planarFilling.Fillings.ToArray();

                    foreach (var filling in fillings)
                    {
                        mesh.AddTriangle(filling.Triangle.A.Point, normal, filling.Triangle.B.Point, normal, filling.Triangle.C.Point, normal);
                    }
                }
            }
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, Position> CreateSurfaceSegmentSet(IEnumerable<PositionEdge> perimeterEdges)
        {
            return new SurfaceSegmentSets<PlanarFillingGroup, Position>
            {
                DividingSegments = Array.Empty<SurfaceSegmentContainer<Position>>(),
                PerimeterSegments = perimeterEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.PositionObject.Id, e.B.PositionObject))).ToArray()
            };
        }

        private static IEnumerable<PositionTriangle> GroupTriangles(IGrouping<string, GroupingCollection> group)
        {
            foreach (var element in group)
            {
                foreach (var t in element.Triangles) { yield return t; }
            }
        }

        private static IEnumerable<PositionEdge> OpenEdges(PositionTriangle triangle, string group)
        {
            if (!triangle.ABadjacents.Any(t => t.Trace.Substring(1) == group)) yield return new PositionEdge(triangle.A, triangle.B);
            if (!triangle.BCadjacents.Any(t => t.Trace.Substring(1) == group)) yield return new PositionEdge(triangle.B, triangle.C);
            if (!triangle.CAadjacents.Any(t => t.Trace.Substring(1) == group)) yield return new PositionEdge(triangle.C, triangle.A);
        }

        private class PointNode : IBox
        {
            public PointNode(Point3D point)
            {
                Point = point;
            }
            public Point3D Point { get; }

            private Rectangle3D _box = null;
            public Rectangle3D Box
            {
                get
                {
                    if (_box is null)
                    {
                        _box = Point.Margin(1e-6);
                    }
                    return _box;
                }
            }
        }

        private static IEnumerable<GroupEdge[][][]> GroupEdgePairs(IGrouping<string, IGrouping<string, PositionTriangle>>[] pairs)
        {
            foreach (var pair in pairs)
            {
                var basePair = pair.Single(p => p.Key[0] == 'B');
                var surfacePair = pair.Single(p => p.Key[0] == 'S');
                var baseFaces = GroupingCollection.ExtractFaces(basePair).ToArray();
                var parallelSurfaces = GroupingCollection.ExtractSurfaces(surfacePair).ToArray();

                var baseEdges = baseFaces.Select(s => s.PerimeterEdges.ToArray()).ToArray();
                var parallelEdges = parallelSurfaces.Select(s => s.PerimeterEdges.ToArray()).ToArray();

                yield return new GroupEdge[][][] { baseEdges, parallelEdges };
            }
        }

        private static IEnumerable<SurfaceTriangle> CreateParallelSurface(IEnumerable<PositionTriangle> triangles, double displacement)
        {
            foreach (var triangle in triangles)
            {
                var aa = new Ray3D(triangle.A.Position + displacement * triangle.A.Normal.Direction, triangle.A.Normal.Direction);
                var bb = new Ray3D(triangle.B.Position + displacement * triangle.B.Normal.Direction, triangle.B.Normal.Direction);
                var cc = new Ray3D(triangle.C.Position + displacement * triangle.C.Normal.Direction, triangle.C.Normal.Direction);

                yield return new SurfaceTriangle(aa, bb, cc);
            }
        }

        private static bool IsFolded(IEnumerable<PositionTriangle> triangles)
        {
            if (!triangles.Any()) { return false; }

            var trace = triangles.First().Trace;

            foreach (var triangle in triangles)
            {
                if (triangle.ABadjacents.Count() > 1 && triangle.ABadjacents.Any(a => a.Trace == trace)) { return true; }
                if (triangle.BCadjacents.Count() > 1 && triangle.BCadjacents.Any(a => a.Trace == trace)) { return true; }
                if (triangle.CAadjacents.Count() > 1 && triangle.CAadjacents.Any(a => a.Trace == trace)) { return true; }
            }
            return false;
        }

        private static SurfaceSegmentSets<Empty, Position> CreateSurfaceSegmentSet(IEnumerable<GroupEdge> perimeterEdges)
        {
            return new SurfaceSegmentSets<Empty, Position>
            {
                DividingSegments = Array.Empty<SurfaceSegmentContainer<Position>>(),
                PerimeterSegments = perimeterEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.PositionObject.Id, e.B.PositionObject))).ToArray()
            };
        }
    }
}
