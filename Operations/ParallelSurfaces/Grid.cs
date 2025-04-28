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
using BasicObjects.MathExtensions;
using Operations.Regions;
using Operations.Intermesh.Basics;
using Operations.SetOperators;

namespace Operations.ParallelSurfaces
{
    public static class Grid
    {
        public static IEnumerable<IWireFrameMesh> BuildFacePlates(this IWireFrameMesh mesh, double thickness)
        {
            Sets.FoldPrimming(mesh);
            var output = BuildParallelSurfaces(mesh, thickness).ToArray();
            foreach (var facePlate in output)
            {
                facePlate.Mesh.Intermesh();
                RemoveInternalFolds(facePlate.Mesh, thickness);
                BuildSurfaceLoops(facePlate);
                BuildQuadrangles(facePlate);
                AssignSurfacePoints(facePlate, thickness);
                AddSideTriangles(facePlate);
                SideTriangleAdjustments(facePlate);
                RemoveTags(facePlate.Mesh);
                FixZeroNormals(facePlate);
            }
            return output.Select(s => s.Mesh);
        }

        public static void BaseStrip(this IWireFrameMesh mesh)
        {
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).ToArray();
            var stripping = faces.Where(f => f.Triangles.Any(t => t.Trace[0] == 'B'));

            foreach (var remove in stripping)
            {
                mesh.RemoveAllTriangles(remove.Triangles);
            }

            faces = GroupingCollection.ExtractFaces(mesh.Triangles).ToArray();
            stripping = faces.Where(f => f.Triangles.Any(t => t.OpenEdges.Any()));

            foreach (var remove in stripping)
            {
                mesh.RemoveAllTriangles(remove.Triangles);
            }
        }

        public static IWireFrameMesh CombineFacePlates(IEnumerable<IWireFrameMesh> facePlates)
        {
            var output = facePlates.First().CreateNewInstance();
            foreach (var facePlate in facePlates)
            {
                output.AddGrid(facePlate);
            }

            output.Intermesh();

            return output;
        }

        public static void GiveOnlySurfaces(IWireFrameMesh input)
        {
            var nonSurfaces = input.Triangles.Where(t => t.Trace[0] == 'B' || t.Trace[0] == 'F');
            input.RemoveAllTriangles(nonSurfaces);
            input.Intermesh();
        }

        private class ParallelSurfaceSet
        {
            public IWireFrameMesh Mesh { get; set; }
            public int Index { get; set; }
            public List<BasePoint[]> BaseLoops { get; set; }
            public List<Point3D[]> SurfaceLoops { get; set; }
            public List<List<Quadrangle>> QuadrangleSets { get; set; }
        }

        private static IEnumerable<ParallelSurfaceSet> BuildParallelSurfaces(IWireFrameMesh mesh, double thickness)
        {
            var faceGroups = GroupingCollection.ExtractFaces(mesh.Triangles);

            foreach (var face in faceGroups.Select((s, i) => new { s, i }))
            {
                var output = new ParallelSurfaceSet();
                output.Index = face.i;
                output.Mesh = mesh.CreateNewInstance();

                CreateParallelSurface(output.Mesh, out List<BasePoint[]> baseLoops, face.s, face.i, thickness);
                output.BaseLoops = baseLoops;
                yield return output;
            }
        }

        private static void BuildSurfaceLoops(ParallelSurfaceSet facePlate)
        {
            var surfaceTriangles = facePlate.Mesh.Triangles.Where(t => t.Trace[0] == 'S');
            var surfacePerimeter = GroupingCollection.GetPerimeterEdges(surfaceTriangles);

            var surfaceEdgeSurfaceCollection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(CreateSurfaceSegmentSet(surfacePerimeter, Enumerable.Empty<GroupEdge>()));

            var surfaceChain = BaseDividerSurfaceChaining<PlanarFillingGroup, PositionNormal>.Create(surfaceEdgeSurfaceCollection);

            facePlate.SurfaceLoops = surfaceChain.PerimeterLoops.Select(p => p.Select(q => q.Point).ToArray()).ToList();
        }

        private static void BuildQuadrangles(ParallelSurfaceSet facePlate)
        {
            var quadrangleSets = new List<List<Quadrangle>>();
            Quadrangle last = null;
            foreach (var baseLoop in facePlate.BaseLoops)
            {
                var list = new List<Quadrangle>();
                for (int i = 0; i < baseLoop.Length - 1; i++)
                {
                    var current = new Quadrangle(baseLoop[i], baseLoop[i + 1]);
                    if (last != null) { last.Next = current; current.Last = last; }
                    list.Add(current);
                    last = current;
                }
                {
                    var current = new Quadrangle(baseLoop[baseLoop.Length - 1], baseLoop[0]);
                    if (last != null) { last.Next = current; current.Last = last; }
                    list.Add(current);
                    last = current;
                    list[0].Last = current;
                    current.Next = list[0];
                }

                quadrangleSets.Add(list);
            }
            facePlate.QuadrangleSets = quadrangleSets;
        }

        private static void AssignSurfacePoints(ParallelSurfaceSet facePlate, double thickness)
        {
            var allSurfacePoints = facePlate.SurfaceLoops.SelectMany(l => l).Select(p => new PointNode(p)).ToArray();
            if (!allSurfacePoints.Any()) { throw new InvalidDataException("No surface points found."); }
            var bucket = new BoxBucket<PointNode>(allSurfacePoints);

            foreach (var loop in facePlate.QuadrangleSets)
            {
                foreach (var quadrangle in loop)
                {
                    quadrangle.SurfaceA = GetNearestPoint(quadrangle.BaseA, bucket, thickness);
                    quadrangle.SurfaceB = GetNearestPoint(quadrangle.BaseB, bucket, thickness);
                }
            }
        }

        private static void AddSideTriangles(ParallelSurfaceSet facePlate)
        {
            foreach (var loop in facePlate.QuadrangleSets)
            {
                foreach (var quadrangle in loop)
                {
                    if (quadrangle.SurfaceA == quadrangle.SurfaceB)
                    {
                        facePlate.Mesh.AddTriangle(new SurfaceTriangle(
                            new Ray3D(quadrangle.BaseA, quadrangle.NormalA),
                            new Ray3D(quadrangle.BaseB, quadrangle.NormalB),
                            new Ray3D(quadrangle.SurfaceA, (quadrangle.NormalA + quadrangle.NormalB).Direction)), $"F{facePlate.Index}", 0);
                        continue;
                    }
                    facePlate.Mesh.AddTriangle(new SurfaceTriangle(
                        new Ray3D(quadrangle.BaseA, quadrangle.NormalA),
                        new Ray3D(quadrangle.BaseB, quadrangle.NormalB),
                        new Ray3D(quadrangle.SurfaceB, quadrangle.NormalB)), $"F{facePlate.Index}", 0);
                    facePlate.Mesh.AddTriangle(new SurfaceTriangle(
                        new Ray3D(quadrangle.SurfaceA, quadrangle.NormalA),
                        new Ray3D(quadrangle.SurfaceB, quadrangle.NormalB),
                        new Ray3D(quadrangle.BaseA, quadrangle.NormalA)), $"F{facePlate.Index}", 0);
                }
            }
        }

        private static void SideTriangleAdjustments(ParallelSurfaceSet facePlate)
        {
            var edgeTriangles = facePlate.Mesh.Triangles.Where(t => t.Trace[0] == 'F').ToArray();
            var looseTriangles = edgeTriangles.Where(t => t.AdjacentAnyCount < 3).ToArray();
            //Console.WriteLine($"Loose side triangles {looseTriangles.Count()}");
            if (!looseTriangles.Any()) { return; }

            var surfaceLoops = facePlate.SurfaceLoops.Select((l, i) => l.Select((p, j) => new SurfacePointNode(p, i, j))).ToArray();
            var bucket = new BoxBucket<SurfacePointNode>(surfaceLoops.SelectMany(p => p));

            foreach (var triangle in looseTriangles)
            {
                var closedPoint = PositionNormal.GetRay(triangle.ClosedPoints.Single());
                var openEdge = triangle.OpenEdges.Single();
                var openPointA = bucket.Fetch(new Rectangle3D(openEdge.A.Position, BoxBucket.MARGINS)).Single(p => p.Point == openEdge.A.Position);
                var openPointB = bucket.Fetch(new Rectangle3D(openEdge.B.Position, BoxBucket.MARGINS)).Single(p => p.Point == openEdge.B.Position);
                if (openPointA.Circuit != openPointB.Circuit) { throw new InvalidOperationException($"Circuits don't match A: {openPointA.Circuit} B: {openPointB.Circuit}"); }
                var circuit = surfaceLoops[openPointA.Circuit].ToArray();
                circuit = circuit.Rotate(openPointA.Index).ToArray();
                var segment = circuit.SegmentForward(e => e.Index == openPointB.Index).ToArray();
                if (segment.Length > circuit.Length / 2)
                {
                    segment = circuit.SegmentBackward(e => e.Index == openPointB.Index).ToArray();
                }

                //Console.WriteLine($"Loose edge [{openPointA.Circuit}, {openPointA.Index}] => [{openPointB.Circuit}, {openPointB.Index}]");
                //Console.WriteLine($"Circuit {circuit.Length} Segment [{string.Join(",", segment.Select(s => s.Index))}]");

                facePlate.Mesh.RemoveTriangle(triangle);

                var rayA = PositionNormal.GetRay(openEdge.A);
                var rayB = PositionNormal.GetRay(openEdge.B);
                var distance = Point3D.Distance(openEdge.A.Position, openEdge.B.Position);

                for (int i = 0; i < segment.Length - 1; i++)
                {
                    var pointA = segment[i];
                    var pointB = segment[i + 1];
                    var distanceA = Point3D.Distance(openEdge.A.Position, pointA.Point);
                    var distanceB = Point3D.Distance(openEdge.A.Position, pointB.Point);
                    var vectorA = Vector3D.Interpolation(openEdge.A.Normal, openEdge.B.Normal, distanceA / distance);
                    var vectorB = Vector3D.Interpolation(openEdge.A.Normal, openEdge.B.Normal, distanceB / distance);
                    facePlate.Mesh.AddTriangle(new SurfaceTriangle(closedPoint,
                        new Ray3D(pointA.Point, vectorA),
                        new Ray3D(pointB.Point, vectorB)), triangle.Trace, triangle.Tag);
                }
            }

            facePlate.Mesh.RemoveAllTriangles(facePlate.Mesh.Triangles.Where(t => t.AdjacentAnyCount < 3));

            //var grid = WireFrameMesh.Create();
            //grid.AddRangeTriangles(looseTriangles, "", 0);
            //WavefrontFile.Export(grid, $"Wavefront/LooseSideTriangles-{facePlate.Index}");
        }

        private static void RemoveTags(IWireFrameMesh output)
        {
            var tags = output.Triangles.Where(t => t.AdjacentAnyCount < 3);
            while (tags.Any())
            {
                output.RemoveAllTriangles(tags);
                tags = output.Triangles.Where(t => t.AdjacentAnyCount < 3);
            }
        }

        private static void FixZeroNormals(ParallelSurfaceSet facePlate)
        {
            var edgeTriangles = facePlate.Mesh.Triangles.Where(t => t.Trace[0] == 'F').ToArray();
            var zeroEdgeTriangles = edgeTriangles.Where(t => t.A.Normal == Vector3D.Zero || t.B.Normal == Vector3D.Zero || t.C.Normal == Vector3D.Zero);
            if (!zeroEdgeTriangles.Any()) { return; }

            var removeTriangles = new List<PositionTriangle>();
            var addTriangles = new List<SurfaceTriangle>();

            var space = new Space(facePlate.Mesh.Triangles.ToArray());
            foreach (var triangle in zeroEdgeTriangles)
            {
                var defaultNormal = triangle.Triangle.Normal.Direction;
                var testPoint = triangle.Triangle.Center + 1e-6 * defaultNormal;
                var spaceRegion = space.RegionOfPoint(testPoint);
                if (spaceRegion == Region.Interior) { defaultNormal = -defaultNormal; }

                var replacement = new SurfaceTriangle(
                    new Ray3D(triangle.A.Position, defaultNormal),
                    new Ray3D(triangle.B.Position, defaultNormal),
                    new Ray3D(triangle.C.Position, defaultNormal));

                removeTriangles.Add(triangle);
                addTriangles.Add(replacement);
            }

            facePlate.Mesh.RemoveAllTriangles(removeTriangles);
            facePlate.Mesh.AddRangeTriangles(addTriangles, $"F{facePlate.Index}", 0);
        }

        private static Point3D GetNearestPoint(Point3D point, BoxBucket<PointNode> bucket, double thickness)
        {
            thickness = Math.Abs(thickness);

            var multiple = 1.41;

            while (true)
            {
                var node = new Rectangle3D(point, multiple * thickness);

                var matches = bucket.Fetch(node);
                if (matches.Any())
                {
                    return point.GetNearestPoint(matches.Select(m => m.Point).ToArray());
                }
                multiple *= 1.41;
            }
        }

        private class SurfacePointNode : IBox
        {
            public SurfacePointNode(Point3D point, int circuit, int index)
            {
                Point = point;
                Circuit = circuit;
                Index = index;
            }

            public Point3D Point { get; }
            public int Circuit { get; }
            public int Index { get; }

            private Rectangle3D _box;
            public Rectangle3D Box
            {
                get
                {
                    if (_box is null && Point is not null)
                    {
                        _box = new Rectangle3D(Point, BoxBucket.MARGINS);
                    }
                    return _box;
                }
            }
        }

        private class Quadrangle
        {
            private static int _id = 0;

            private BasePoint _a, _b;
            private Vector3D _normalA;
            private Vector3D _normalB;

            private void GetNormals()
            {
                double maxDotProduct = -1;
                foreach (var normalA in _a.EdgeNormals)
                {
                    foreach (var normalB in _b.EdgeNormals)
                    {
                        var dotProduct = Vector3D.Dot(normalA, normalB);
                        if (dotProduct > maxDotProduct)
                        {
                            maxDotProduct = dotProduct;
                            _normalA = normalA;
                            _normalB = normalB;
                        }
                    }
                }
            }
            public Quadrangle(BasePoint a, BasePoint b)
            {
                Id = _id++;
                _a = a;
                _b = b;
            }
            public int Id { get; }
            public Quadrangle Last { get; set; }
            public Quadrangle Next { get; set; }

            public Point3D BaseA { get { return _a.Position; } }
            public Point3D BaseB { get { return _b.Position; } }
            public Point3D SurfaceA { get; set; }
            public Point3D SurfaceB { get; set; }

            public Vector3D NormalA
            {
                get
                {
                    if (_normalA is null)
                    {
                        GetNormals();
                    }
                    return _normalA;
                }
            }
            public Vector3D NormalB
            {
                get
                {
                    if (_normalB is null)
                    {
                        GetNormals();
                    }
                    return _normalB;
                }
            }

        }

        private static bool IsNearDegenerate(Triangle3D triangle)
        {
            return triangle.LengthAB < GapConstants.Proximity || triangle.LengthBC < GapConstants.Proximity || triangle.LengthCA < GapConstants.Proximity;
        }

        private static void RemoveInternalFolds(IWireFrameMesh output, double thickness)
        {
            var faceGroups = output.Triangles.GroupBy(t => t.Trace.Substring(1));
            foreach (var faceGroup in faceGroups)
            {
                var baseGroups = faceGroup.GroupBy(t => t.Trace[0]);

                var baseTriangles = baseGroups.Single(g => g.Key == 'B').ToArray();
                var surfaceTriangles = baseGroups.Single(g => g.Key == 'S').ToArray();

                RemoveInternalFolds(baseTriangles, surfaceTriangles, output, thickness);
            }
        }

        private static void RemoveInternalFolds(IEnumerable<PositionTriangle> baseTriangles, IEnumerable<PositionTriangle> surfaceTriangles, IWireFrameMesh output, double thickness)
        {
            var faces = GroupingCollection.ExtractFaces(surfaceTriangles).ToArray();
            var folds = faces.SelectMany(GroupingCollection.ExtractFolds).ToArray();
            if (folds.Length == 1) { return; }

            var bucket = new BoxBucket<PositionTriangle>(baseTriangles);

            foreach (var fold in folds)
            {
                var testPoint = fold.Triangles.First().Triangle.Center;

                var matches = bucket.Fetch(new Rectangle3D(testPoint, thickness + 1e-4));

                bool removeFold = false;
                int exteriorCount = 0;
                int count = 0;
                foreach (var match in matches)
                {
                    var projection = match.Triangle.Plane.Projection(testPoint);
                    if (!match.Triangle.PointIsOn(projection)) { continue; }
                    count++;
                    var distance = Point3D.Distance(testPoint, projection);
                    if (distance < thickness - GapConstants.Proximity) { removeFold = true; break; }
                    if (distance > thickness + GapConstants.Proximity) { exteriorCount++; }
                }

                if (removeFold || count == exteriorCount) { output.RemoveAllTriangles(fold.Triangles); }
            }
        }


        private static SurfaceSegmentSets<PlanarFillingGroup, PositionNormal> CreateSurfaceSegmentSet(IEnumerable<GroupEdge> openEdges, IEnumerable<GroupEdge> dividerEdges)
        {
            return new SurfaceSegmentSets<PlanarFillingGroup, PositionNormal>
            {
                DividingSegments = dividerEdges.Select(e => new SurfaceSegmentContainer<PositionNormal>(
                    new SurfaceRayContainer<PositionNormal>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.Normal, e.A.PositionObject.Id, e.A),
                    new SurfaceRayContainer<PositionNormal>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.Normal, e.B.PositionObject.Id, e.B))).ToArray(),
                PerimeterSegments = openEdges.Select(e => new SurfaceSegmentContainer<PositionNormal>(
                    new SurfaceRayContainer<PositionNormal>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.Normal, e.A.PositionObject.Id, e.A),
                    new SurfaceRayContainer<PositionNormal>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.Normal, e.B.PositionObject.Id, e.B))).ToArray(),
            };
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

        private static void CreateParallelSurface(IWireFrameMesh newMesh, out List<BasePoint[]> baseLoops, GroupingCollection face, int faceIndex, double displacement)
        {
            foreach (var triangle in face.GroupingTriangles)
            {
                var baseTriangle = newMesh.AddTriangle(CreateParallelSurface(triangle.PositionTriangle, 0e-4), $"B{faceIndex}", 0);
                var surfaceTriangle = CreateParallelSurface(triangle.PositionTriangle, displacement - 0e-4);
                if (IsNearDegenerate(surfaceTriangle.Triangle)) { continue; }
                newMesh.AddTriangle(surfaceTriangle, $"S{faceIndex}", 0);
            }

            var baseEdgeSurfaceCollection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(CreateSurfaceSegmentSet(face.PerimeterEdges, Enumerable.Empty<GroupEdge>()));

            var baseChain = BaseDividerSurfaceChaining<PlanarFillingGroup, PositionNormal>.Create(baseEdgeSurfaceCollection);

            baseLoops = baseChain.PerimeterLoops.Select(p => p.Select(q => new BasePoint(q.Reference)).ToArray()).ToList();
        }

        private class BasePoint
        {
            private static int _id = 0;
            public BasePoint(PositionNormal pn)
            {
                Id = _id++;
                SurfaceNormal = pn.Normal;
                Position = pn.Position;
                var surfacePlane = new Plane(Point3D.Zero, pn.Normal);
                EdgeNormals = pn.PositionObject.PositionNormals.Where(pn2 => pn2.Id != pn.Id).Select(pn3 => pn3.Normal).Select(pn4 => surfacePlane.Projection(pn4)).ToList();
            }
            public int Id { get; }
            public Vector3D SurfaceNormal { get; }
            public Point3D Position { get; }
            public IReadOnlyList<Vector3D> EdgeNormals { get; }
        }

        //private static SurfaceTriangle CreateBaseSurface(PositionTriangle triangle)
        //{
        //    var aa = new Ray3D(triangle.A.Position, triangle.A.Normal.Direction);
        //    var bb = new Ray3D(triangle.B.Position, triangle.B.Normal.Direction);
        //    var cc = new Ray3D(triangle.C.Position, triangle.C.Normal.Direction);

        //    return new SurfaceTriangle(aa, bb, cc);
        //}

        private static SurfaceTriangle CreateParallelSurface(PositionTriangle triangle, double displacement)
        {
            var aa = new Ray3D(triangle.A.Position + displacement * triangle.A.Normal.Direction, triangle.A.Normal.Direction);
            var bb = new Ray3D(triangle.B.Position + displacement * triangle.B.Normal.Direction, triangle.B.Normal.Direction);
            var cc = new Ray3D(triangle.C.Position + displacement * triangle.C.Normal.Direction, triangle.C.Normal.Direction);

            return new SurfaceTriangle(aa, bb, cc);
        }
    }
}
