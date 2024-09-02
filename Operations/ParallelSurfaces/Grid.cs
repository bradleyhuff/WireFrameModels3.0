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
using BasicObjects.MathExtensions;
using Operations.Regions;
using Operations.PositionRemovals.FillActions;

namespace Operations.ParallelSurfaces
{
    public static class Grid
    {
        public static IWireFrameMesh SetFacePlates(this IWireFrameMesh mesh, double thickness)
        {
            FoldPrimming(mesh);

            var output = AddParallelSurfaces(mesh, thickness);
            output.Intermesh();

            RemoveFoldedSurfaces(output);
            RemoveClosedSurfaces(output);
            RemoveUnderLeavingFolds(output);


            FillPlateSides(output, thickness);
            ObliqueNormalAdjustments(output);
            ElbowFill(output, thickness);
            ElbowNormals(output);

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

        private static void RemoveClosedSurfaces(IWireFrameMesh output)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(output.Triangles).ToArray();

            var closedSurfaces = surfaces.Where(s => s.PerimeterEdges.Count == 0 && s.Triangles.First().Trace[0] == 'S').ToArray();
            output.RemoveAllTriangles(closedSurfaces.SelectMany(s => s.Triangles));
        }

        private static void RemoveUnderLeavingFolds(IWireFrameMesh output)
        {
            var faces = GroupingCollection.ExtractFaces(output.Triangles).ToArray();
            var foldGroups = faces.Select(f => GroupingCollection.ExtractFolds(f).ToArray()).ToArray();
            var underleavingGroups = foldGroups.Where(g => g.Length > 1);

            foreach (var group in underleavingGroups)
            {
                RemoveUnderLeavingFold(output, group);
            }
        }

        private static void RemoveUnderLeavingFold(IWireFrameMesh output, GroupingCollection[] group)
        {
            //Console.WriteLine($"Underleaving group {group.Length}");

            var first = group[0];
            var upperId = -1;
            for (int i = 1; i < group.Length; i++)
            {
                var order = Order(first, group[i]);
                if (order is null) { continue; }

                upperId = order[0].Id; break;
            }
            if (upperId == -1) { return; }

            foreach (var element in group.Where(g => g.Id != upperId))
            {
                output.RemoveAllTriangles(element.Triangles);
            }
        }

        private static GroupingCollection[] Order(GroupingCollection a, GroupingCollection b)
        {
            var perimeterA = a.PerimeterEdges;
            var perimeterB = b.PerimeterEdges;
            var oldTraceA = a.Triangles.First().Trace;
            var oldTraceB = b.Triangles.First().Trace;

            foreach (var t in a.Triangles) { t.Trace = "A"; }
            foreach (var t in b.Triangles) { t.Trace = "B"; }

            var common = perimeterA.IntersectBy(perimeterB.Select(p => p.Key), p => p.Key, new Combination2Comparer()).ToArray();

            if (common.Any())
            {
                var triangles = common.First().Triangles;
                // Determine order from here...

                var triangleA = triangles.Single(t => t.Trace == "A");
                var triangleB = triangles.Single(t => t.Trace == "B");
                //Console.WriteLine($"Order check. Triangle A {triangleA.Id} Triangle B {triangleB.Id}");

                var normal = (triangleB.A.Normal + triangleB.B.Normal + triangleB.C.Normal).Direction;
                var normalLine = new Line3D(triangleB.Triangle.Center, triangleB.Triangle.Center + normal);
                var intersectionA = triangleA.Triangle.Plane.Intersection(normalLine);
                var normalIntersection = (intersectionA - triangleB.Triangle.Center).Direction;
                var polarity = Math.Sign(Vector3D.Dot(normal, normalIntersection));
                //Console.WriteLine($"Center {triangleB.Triangle.Center} intersection {intersectionA} polarity {polarity}");

                foreach (var t in a.Triangles) { t.Trace = oldTraceA; }
                foreach (var t in b.Triangles) { t.Trace = oldTraceB; }

                if (polarity == 1)
                {
                    //A is upper
                    return [a, b];
                }
                if (polarity == -1)
                {
                    //B is upper
                    return [b, a];
                }
            }

            return null;
        }

        private static void FoldPrimming(IWireFrameMesh output)
        {
            var foldedTriangles = output.Triangles.Where(t => t.IsFolded).ToArray();
            Console.WriteLine($"Folded triangles {foldedTriangles.Length}");
            if (!foldedTriangles.Any()) { Console.WriteLine(); return; }

            var space = new Space(output.Triangles.Select(t => t.Triangle).ToArray());
            var straightenedNormals = new Dictionary<int, Vector3D>();
            foreach (var foldedTriangle in foldedTriangles)
            {
                var principleNormal = foldedTriangle.Triangle.Normal.Direction;
                var testPoint = foldedTriangle.Triangle.Center + 1e-6 * principleNormal;
                var spaceRegion = space.RegionOfPoint(testPoint);
                if (spaceRegion == Region.Interior) { principleNormal = -principleNormal; }

                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.A, principleNormal);
                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.B, principleNormal);
                ApplyPrincipleNormal(straightenedNormals, foldedTriangle.C, principleNormal);
            }

            var replacements = new List<SurfaceTriangle>();

            foreach (var foldedTriangle in foldedTriangles)
            {
                Ray3D a = PositionNormal.GetRay(foldedTriangle.A);
                Ray3D b = PositionNormal.GetRay(foldedTriangle.B);
                Ray3D c = PositionNormal.GetRay(foldedTriangle.C);
                if (straightenedNormals.ContainsKey(foldedTriangle.A.Id)) { a = new Ray3D(foldedTriangle.A.Position, straightenedNormals[foldedTriangle.A.Id].Direction); }
                if (straightenedNormals.ContainsKey(foldedTriangle.B.Id)) { b = new Ray3D(foldedTriangle.B.Position, straightenedNormals[foldedTriangle.B.Id].Direction); }
                if (straightenedNormals.ContainsKey(foldedTriangle.C.Id)) { c = new Ray3D(foldedTriangle.C.Position, straightenedNormals[foldedTriangle.C.Id].Direction); }
                replacements.Add(new SurfaceTriangle(a, b, c));
            }

            output.RemoveAllTriangles(foldedTriangles);
            output.AddRangeTriangles(replacements);

            Console.WriteLine();
        }

        private static void ApplyPrincipleNormal(Dictionary<int, Vector3D> straightenedNormals, PositionNormal position, Vector3D principleNormal)
        {
            if (Vector3D.Dot(position.Normal, principleNormal) < 0)
            {
                if (!straightenedNormals.ContainsKey(position.Id))
                {
                    straightenedNormals[position.Id] = principleNormal;
                }
                else
                {
                    straightenedNormals[position.Id] += principleNormal;
                }
            }
        }

        private static void ApplyPrincipleNormal(Dictionary<int, Vector3D> straightenedNormals, PositionNormal position, Vector3D principleNormal, double weight)
        {
            if (!straightenedNormals.ContainsKey(position.PositionObject.Id))
            {
                straightenedNormals[position.PositionObject.Id] = weight * principleNormal;
            }
            else
            {
                straightenedNormals[position.PositionObject.Id] += weight * principleNormal;
            }
        }

        private static void FillPlateSides(IWireFrameMesh output, double thickness)
        {
            var sides = output.CreateNewInstance();

            var groups = output.Triangles.GroupBy(t => t.Trace).ToArray();
            var pairs = groups.GroupBy(g => g.Key.Substring(1)).ToArray();

            var groupEdgePairs = GroupEdgePairs(pairs).ToArray();

            var plateTriangles = new List<PositionTriangle>();

            foreach (var pair in groupEdgePairs)
            {
                var baseEdges = pair[0];
                var parallelEdges = pair[1];

                var positions = parallelEdges.SelectMany(e => e.SelectMany(e => e.Positions)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();
                var bucket = new BoxBucket<Position>(positions);

                var edgeTrace = parallelEdges[0][0].A.Triangles.First().Trace;
                edgeTrace = edgeTrace.Replace('S', 'B');

                var baseChain = baseEdges.Select(s => SurfaceSegmentChaining<Empty, Position>.Create(
                    new SurfaceSegmentCollections<Empty, Position>(CreateSurfaceSegmentSet(s)))).ToArray();

                var baseLoop = baseChain[0].PerimeterLoops[0].Select(p => p.Reference).ToArray();

                AddPlateSides(baseLoop, thickness, bucket, edgeTrace, sides);
            }

            output.AddGrid(sides);
        }

        private static void ObliqueNormalAdjustments(IWireFrameMesh output)
        {
            var sideTriangles = output.Triangles.Where(t => t.Trace[0] == 'E').ToArray();
            var sideTriangleSets = sideTriangles.GroupBy(t => t.Trace);

            foreach (var sideTriangleSet in sideTriangleSets)
            {
                TriangleSetObliqueAdjustments(sideTriangleSet, output);
            }
        }

        private static void TriangleSetObliqueAdjustments(IGrouping<string, PositionTriangle> sideTriangleSet, IWireFrameMesh output)
        {
            var bases = new Dictionary<int, PositionNormal>();
            var surfaces = new Dictionary<int, PositionNormal>();
            var pairing = new Dictionary<int, int>();
            foreach (var triangle in sideTriangleSet)
            {
                var basePositions = GetBases(triangle);
                var surfacePositions = GetSurfaces(triangle);
                foreach (var basePosition in basePositions)
                {
                    bases[basePosition.Id] = basePosition;
                }
                foreach (var surfacePosition in surfacePositions)
                {
                    surfaces[surfacePosition.Id] = surfacePosition;
                }
            }
            foreach (var triangle in sideTriangleSet)
            {
                var basePositions = GetBases(triangle);
                var surfacePositions = GetSurfaces(triangle);
                foreach (var basePosition in basePositions)
                {
                    foreach (var surfacePosition in surfacePositions)
                    {
                        if (!pairing.ContainsKey(basePosition.Id))
                        {
                            pairing[basePosition.Id] = surfacePosition.Id;
                            continue;
                        }
                        var distance = Point3D.Distance(bases[basePosition.Id].Position, surfaces[pairing[basePosition.Id]].Position);
                        var newDistance = Point3D.Distance(bases[basePosition.Id].Position, surfaces[surfacePosition.Id].Position);
                        if (newDistance < distance) { pairing[basePosition.Id] = surfacePosition.Id; }
                    }
                }
            }

            var obliquePositions = new Dictionary<int, bool>();
            var adjustedNormals = new Dictionary<int, Vector3D>();

            foreach (var pair in pairing)
            {
                var point = bases[pair.Key].Position;
                var vectorBase = (surfaces[pair.Value].Position - bases[pair.Key].Position).Direction;
                var vectorNormal = bases[pair.Key].Normal.Direction;
                var vectorNormal2 = surfaces[pair.Value].Normal.Direction;
                var angle = Vector3D.Angle(vectorBase, vectorNormal);
                var angle2 = Vector3D.Angle(vectorBase, vectorNormal2);

                if (Math.Abs(angle - Math.PI / 2) > 1e-6)
                {
                    Console.WriteLine($"Pair [{pair.Key}: {bases[pair.Key].PositionObject.Id},{pair.Value}: {surfaces[pair.Value].PositionObject.Id}] {angle} {angle2}");
                    obliquePositions[pair.Key] = true;
                    obliquePositions[pair.Value] = true;

                    var orthogonalPoint = SolveForOrthogonalPoint(point + vectorNormal, point + vectorBase, point);
                    adjustedNormals[pair.Key] = (orthogonalPoint - point).Direction;
                    adjustedNormals[pair.Value] = adjustedNormals[pair.Key];
                }
            }

            var trianglesToReplace = sideTriangleSet.Where(t => obliquePositions.ContainsKey(t.A.Id) || obliquePositions.ContainsKey(t.B.Id) || obliquePositions.ContainsKey(t.C.Id)).ToArray();
            Console.WriteLine($"{sideTriangleSet.Key} Triangles to replace {trianglesToReplace.Length}");
            var replacements = trianglesToReplace.Select(t => new SurfaceTriangle(
                new Ray3D(t.A.Position, adjustedNormals.ContainsKey(t.A.Id) ? adjustedNormals[t.A.Id] : t.A.Normal),
                new Ray3D(t.B.Position, adjustedNormals.ContainsKey(t.B.Id) ? adjustedNormals[t.B.Id] : t.B.Normal),
                new Ray3D(t.C.Position, adjustedNormals.ContainsKey(t.C.Id) ? adjustedNormals[t.C.Id] : t.C.Normal)));

            output.RemoveAllTriangles(trianglesToReplace);
            output.AddRangeTriangles(replacements, sideTriangleSet.Key);
        }

        private static Point3D SolveForOrthogonalPoint(Point3D a, Point3D b, Point3D center)
        {
            var bc = b - center;
            var ba = b - a;
            var top = bc.X * bc.X + bc.Y * bc.Y + bc.Z * bc.Z;
            var bottom = ba.X * bc.X + ba.Y * bc.Y + ba.Z * bc.Z;
            double alpha = top / bottom;
            Console.WriteLine($"Alpha {alpha} {top} / {bottom}");

            return alpha * a + (1 - alpha) * b;
        }

        private static IEnumerable<PositionNormal> GetBases(PositionTriangle triangle)
        {
            if (triangle.A.PositionObject.Triangles.Any(t => t.Trace[0] == 'B')) yield return triangle.A;
            if (triangle.B.PositionObject.Triangles.Any(t => t.Trace[0] == 'B')) yield return triangle.B;
            if (triangle.C.PositionObject.Triangles.Any(t => t.Trace[0] == 'B')) yield return triangle.C;
        }

        private static IEnumerable<PositionNormal> GetSurfaces(PositionTriangle triangle)
        {
            if (triangle.A.PositionObject.Triangles.Any(t => t.Trace[0] == 'S')) yield return triangle.A;
            if (triangle.B.PositionObject.Triangles.Any(t => t.Trace[0] == 'S')) yield return triangle.B;
            if (triangle.C.PositionObject.Triangles.Any(t => t.Trace[0] == 'S')) yield return triangle.C;
        }

        private static void AddPlateSides(Position[] baseLoop, double thickness, BoxBucket<Position> bucket, string edgeTrace, IWireFrameMesh grid)
        {
            for (int i = 0; i < baseLoop.Length; i++)
            {
                var length = baseLoop.Length - 1;
                var position = baseLoop[i];
                var nextPosition = baseLoop[i < length ? i + 1 : 0];

                var commonTriangles = position.Triangles.IntersectBy(nextPosition.Triangles.Select(t => t.Id), t => t.Id).Where(t => t.Trace != edgeTrace);
                if (!commonTriangles.Any()) { continue; }
                var commonTrace = commonTriangles.Single().Trace;

                var normal = position.PositionNormals.Single(pn => pn.Triangles.First().Trace == edgeTrace).Normal;
                var nextNormal = nextPosition.PositionNormals.Single(pn => pn.Triangles.First().Trace == edgeTrace).Normal;

                var surfacePoint = position.Point + thickness * normal.Direction;
                var nextSurfacePoint = nextPosition.Point + thickness * nextNormal.Direction;
                //
                var surfaceNormal = position.PositionNormals.Single(pn => pn.Triangles.First().Trace == commonTrace).Normal;//

                var matchingSurfacePoint = bucket.Fetch(new PointNode(surfacePoint)).SingleOrDefault(p => p.Point == surfacePoint);
                var matchingNextSurfacePoint = bucket.Fetch(new PointNode(nextSurfacePoint)).SingleOrDefault(p => p.Point == nextSurfacePoint);

                var surfacePointIsFound = matchingSurfacePoint is not null;
                var nextSurfacePointIsFound = matchingNextSurfacePoint is not null;

                if (!surfacePointIsFound || !nextSurfacePointIsFound) { continue; }

                //
                var nextSurfaceNormal = nextPosition.PositionNormals.Single(pn => pn.Triangles.First().Trace == commonTrace).Normal;//

                var triangle1 = new SurfaceTriangle(new Ray3D(position.Point, surfaceNormal), new Ray3D(surfacePoint, surfaceNormal), new Ray3D(nextPosition.Point, nextSurfaceNormal));
                var triangle2 = new SurfaceTriangle(new Ray3D(surfacePoint, surfaceNormal), new Ray3D(nextSurfacePoint, nextSurfaceNormal), new Ray3D(nextPosition.Point, nextSurfaceNormal));
                var addedTriangles = grid.AddRangeTriangles([triangle1, triangle2]);
                foreach (var triangle in addedTriangles) { triangle.Trace = edgeTrace.Replace('B', 'E'); }
            }
        }
        private static void ElbowFill(IWireFrameMesh mesh, double thickness)
        {
            var surfaces = GroupingCollection.ExtractSurfaces(mesh.Triangles).ToArray();
            var groups = surfaces.GroupBy(s => s.Triangles.First().Trace.Substring(1));

            var elbowTriangles = new List<TriangleTrace>();

            foreach (var group in groups)
            {
                var triangles = GroupTriangles(group);
                var openEdges = triangles.SelectMany(t => OpenEdges(t, group.Key)).ToArray();
                if (!openEdges.Any()) { continue; }

                var baseGroup = group.Single(g => g.Triangles.Any(t => t.Trace[0] == 'B'));
                var surfaceGroup = group.Single(g => g.Triangles.Any(t => t.Trace[0] == 'S'));
                var basePositions = baseGroup.PerimeterEdges.SelectMany(e => e.Positions).DistinctBy(p => p.Id).ToArray();
                var surfacePositions = surfaceGroup.PerimeterEdges.SelectMany(e => e.Positions).DistinctBy(p => p.Id).ToArray();
                var bucket = new BoxBucket<PositionNormal>(surfacePositions);

                var dividerEdges = new List<PositionEdge>();
                foreach (var position in basePositions)
                {
                    var surfacePoint = position.Position + thickness * position.Normal.Direction;
                    var matchingSurfacePoint = bucket.Fetch(new PointNode(surfacePoint)).FirstOrDefault(p => p.Position == surfacePoint);
                    if (matchingSurfacePoint != null)
                    {
                        var positionEdge = new PositionEdge(position, matchingSurfacePoint);
                        Console.WriteLine($"Surface normal match [{position.PositionObject.Id}, {matchingSurfacePoint.Id}]");
                        dividerEdges.Add(positionEdge);
                    }
                }

                var baseEdgeSurfaceCollection = new SurfaceSegmentCollections<PlanarFillingGroup, Position>(CreateSurfaceSegmentSet(openEdges, dividerEdges));

                var chain = BaseDividerSurfaceChaining<PlanarFillingGroup, Position>.Create(baseEdgeSurfaceCollection);
                var chains = Chaining.SplitByPerimeterLoops(chain);

                foreach (var element in chains.Select((s, i) => new { s, i }))
                {
                    //var allPoints = element.s.PerimeterLoops[0].Select(p => p.Reference);
                    //Console.WriteLine($"{string.Join(",", allPoints.Select(p => $"{p.Id}{(IsBasePosition(p) ? "*" : "")} "))}");
                    var surfacePoints = element.s.PerimeterLoops[0].
                        Select(p => p.Reference).ToArray().
                        UnwrapToBeginning((p, i) => IsSurfacePosition(p)).
                        Reverse().
                        ToArray();
                    Console.WriteLine($"{string.Join(",", surfacePoints.Select(p => $"{p.Id} "))}");
                    var basePoints = element.s.PerimeterLoops[0].
                        Select(p => p.Reference).ToArray().
                        UnwrapToBeginning((p, i) => IsBasePosition(p)).
                        ToArray();
                    //Console.WriteLine($"{string.Join(",", basePoints.Select(p => $"{p.Id} "))}");
                    //Console.WriteLine($"Elbow: Surface points {surfacePoints.Count()} Base points {basePoints.Count()} All points {allPoints.Count()}");

                    int s = 0;
                    int r = 0;
                    var firstSurfacePoint = surfacePoints.First().Id;
                    var lastSurfacePoint = surfacePoints.Last().Id;
                    for (int b = 0; b < basePoints.Length - 1; b++)
                    {
                        if (s + 1 < surfacePoints.Length &&
                            Point3D.Distance(basePoints[b + 1].Point, surfacePoints[s].Point) > Point3D.Distance(basePoints[b + 1].Point, surfacePoints[s + 1].Point))
                        {
                            elbowTriangles.Add(new TriangleTrace(basePoints[b].Point, surfacePoints[s].Point, surfacePoints[s + 1].Point, $"F{element.i}R{r}"));
                            s++;
                        }
                        if (surfacePoints[s].Id == lastSurfacePoint && basePoints[b].Cardinality > 3) { r++; }
                        elbowTriangles.Add(new TriangleTrace(basePoints[b].Point, basePoints[b + 1].Point, surfacePoints[s].Point, $"F{element.i}R{r}"));
                        if (surfacePoints[s].Id == firstSurfacePoint && basePoints[b + 1].Cardinality > 3) { r++; }
                    }
                }
            }

            foreach (var triangle in elbowTriangles)
            {
                mesh.AddTriangle(triangle.Triangle, triangle.Trace);
            }
        }

        private class TriangleTrace
        {
            public TriangleTrace(Point3D a, Point3D b, Point3D c, string trace)
            {
                Triangle = new Triangle3D(a, b, c);
                Trace = trace;
            }
            public Triangle3D Triangle { get; }
            public string Trace { get; }
        }

        private class SurfaceTriangleTrace
        {
            public SurfaceTriangleTrace(Ray3D a, Ray3D b, Ray3D c, string trace)
            {
                Triangle = new SurfaceTriangle(a, b, c);
                Trace = trace;
            }
            public SurfaceTriangle Triangle { get; }
            public string Trace { get; }
        }

        private static void ElbowNormals(IWireFrameMesh mesh)
        {
            var elbowNormals = mesh.Triangles.Where(t => t.Trace[0] == 'F');
            if (!elbowNormals.Any()) { return; }

            var space = new Space(mesh.Triangles.Select(t => t.Triangle));
            var elbowGroups = elbowNormals.GroupBy(t => t.Trace).ToArray();
            Console.WriteLine($"Elbow groups {string.Join(", ", elbowGroups.Select(g => $"{g.Key}: {g.Count()}"))}");

            foreach (var elbowGroup in elbowGroups)
            {
                var triangles = elbowGroup.ToArray();
                var normals = new Dictionary<int, Vector3D>();

                foreach (var triangle in triangles)
                {
                    var principleNormal = triangle.Triangle.Normal.Direction;
                    var testPoint = triangle.Triangle.Center + 1e-6 * principleNormal;
                    var spaceRegion = space.RegionOfPoint(testPoint);
                    if (spaceRegion == Region.Interior) { principleNormal = -principleNormal; }

                    var area = triangle.Triangle.Area;
                    ApplyPrincipleNormal(normals, triangle.A, principleNormal, area);
                    ApplyPrincipleNormal(normals, triangle.B, principleNormal, area);
                    ApplyPrincipleNormal(normals, triangle.C, principleNormal, area);
                }

                var replacements = new List<SurfaceTriangleTrace>();

                foreach (var triangle in triangles)
                {
                    var a = NormalLookups(triangle.A, normals);
                    var b = NormalLookups(triangle.B, normals);
                    var c = NormalLookups(triangle.C, normals);

                    replacements.Add(new SurfaceTriangleTrace(a, b, c, triangle.Trace));
                }

                mesh.RemoveAllTriangles(triangles);
                foreach (var triangle in replacements)
                {
                    mesh.AddTriangle(triangle.Triangle, triangle.Trace);
                }
            }

            elbowNormals = mesh.Triangles.Where(t => t.Trace[0] == 'F');
            elbowGroups = elbowNormals.GroupBy(t => t.Trace).ToArray();

            foreach (var elbowGroup in elbowGroups)
            {
                var triangles = elbowGroup.ToArray();

                var normalOverrides = new Dictionary<int, Vector3D>();

                var cuspPoints = triangles.SelectMany(t => t.Positions).DistinctBy(p => p.Id).
                    Where(p =>
                        p.PositionObject.Triangles.Any(t => t.Trace[0] == 'S' &&
                        !p.PositionObject.Triangles.Any(t => t.Trace[0] == 'E') &&
                        p.PositionObject.Cardinality > 2)).ToArray();
                Console.WriteLine($"{elbowGroup.Key} Cusp points {string.Join(",", cuspPoints.Select(s => $"{s.Id}: {s.PositionObject.Id}: {s.PositionObject.Cardinality}"))}");


                var cuspSets = cuspPoints.Select(c =>
                    new { c.Id, Position = c.Position, PositionTriangles = c.Triangles.Where(t => t.Trace[0] == 'F').ToArray() }).
                        GroupBy(c => c.Id).ToArray();

                foreach (var cuspSet in cuspSets.Where(c => c.Any()))
                {
                    var cuspTriangles = cuspSet.SelectMany(c => c.PositionTriangles);

                    var triangleBasePointGroups = cuspTriangles.
                        Select(t => new { t.Id, Positions = t.Positions.Where(p => p.PositionObject.Triangles.Any(pt => pt.Trace[0] == 'B')) }).
                        GroupBy(t => t.Id).ToDictionary(pv => pv.Key, pv => pv.SelectMany(v => v.Positions).ToArray());

                    var positionTriangleRelations = new Dictionary<int, List<int>>();

                    foreach (var pair in triangleBasePointGroups)
                    {
                        foreach (var position in pair.Value)
                        {
                            if (!positionTriangleRelations.ContainsKey(position.Id)) { positionTriangleRelations[position.Id] = new List<int>(); }
                            positionTriangleRelations[position.Id].Add(pair.Key);
                        }
                    }

                    var orderedSurfacePoints = new List<PositionNormal>();
                    var positionNormal = triangleBasePointGroups.First(p => p.Value.Length == 1).Value.First();
                    var triangleId = triangleBasePointGroups.First(p => p.Value.Length == 1).Key;
                    orderedSurfacePoints.Add(positionNormal);

                    while (true)
                    {
                        triangleId = positionTriangleRelations[positionNormal.Id].Single(s => s != triangleId);
                        positionNormal = triangleBasePointGroups[triangleId].SingleOrDefault(pn => pn.Id != positionNormal.Id);
                        if (positionNormal is null) { break; }
                        orderedSurfacePoints.Add(positionNormal);
                    }

                    var normals = orderedSurfacePoints.Select((pn, i) => new { Order = i, Index = pn.Id, Normal = pn.Normal, Radius = Point3D.Distance(pn.Position, cuspSet.First().Position)}).ToList();

                    if (normals.Count > 3)
                    {
                        var normalsSubset = normals.Skip(1).Take(normals.Count - 2).ToArray();
                        var maxRadius = BasicObjects.Math.Math.Max(normalsSubset.Select(n => n.Radius).ToArray()) * 1.1;

                        if (normals.First().Radius > maxRadius) { normals.RemoveAt(0); }
                        if (normals.Last().Radius > maxRadius) { normals.RemoveAt(normals.Count - 1); }
                    }

                    var normalsVectors = normals.ToDictionary(n => n.Index, n => new { n.Order, n.Normal });

                    var replacements = new List<SurfaceTriangleTrace>();
                    foreach (var triangle in cuspTriangles)
                    {
                        var a = normalsVectors.ContainsKey(triangle.A.Id) ? normalsVectors[triangle.A.Id] : null;
                        var b = normalsVectors.ContainsKey(triangle.B.Id) ? normalsVectors[triangle.B.Id] : null;
                        var c = normalsVectors.ContainsKey(triangle.C.Id) ? normalsVectors[triangle.C.Id] : null;

                        var options = new[] { a, b, c}.Where(e => e is not null).OrderByDescending(e => e.Order).ToArray();
                        var apply = options.FirstOrDefault();

                        var aa = PositionNormal.GetRay(triangle.A);
                        var bb = PositionNormal.GetRay(triangle.B);
                        var cc = PositionNormal.GetRay(triangle.C);

                        if (triangle.A.Id == cuspSet.Key && apply is not null) { aa = new Ray3D(aa.Point, apply.Normal); }
                        if (triangle.B.Id == cuspSet.Key && apply is not null) { bb = new Ray3D(bb.Point, apply.Normal); }
                        if (triangle.C.Id == cuspSet.Key && apply is not null) { cc = new Ray3D(cc.Point, apply.Normal); }

                        replacements.Add(new SurfaceTriangleTrace(aa, bb, cc, triangle.Trace));
                    }

                    mesh.RemoveAllTriangles(cuspTriangles);//.SelectMany(p => p.PositionTriangles));
                    foreach (var triangle in replacements)
                    {
                        mesh.AddTriangle(triangle.Triangle, triangle.Trace);
                    }
                }
            }
        }

        private static Ray3D NormalLookups(PositionNormal p, Dictionary<int, Vector3D> normals)
        {
            if (normals.ContainsKey(p.PositionObject.Id)) { return new Ray3D(p.Position, normals[p.PositionObject.Id].Direction); }
            return PositionNormal.GetRay(p);
        }

        private static bool IsBasePosition(Position p)
        {
            return p.Triangles.Any(t => t.Trace[0] == 'B');
        }

        private static bool IsSurfacePosition(Position p)
        {
            return p.Triangles.Any(t => t.Trace[0] == 'S');
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, Position> CreateSurfaceSegmentSet(IEnumerable<PositionEdge> openEdges, IEnumerable<PositionEdge> dividerEdges)
        {
            return new SurfaceSegmentSets<PlanarFillingGroup, Position>
            {
                DividingSegments = dividerEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.PositionObject.Id, e.B.PositionObject))).ToArray(),
                PerimeterSegments = openEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.PositionObject.Id, e.B.PositionObject))).ToArray(),
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
