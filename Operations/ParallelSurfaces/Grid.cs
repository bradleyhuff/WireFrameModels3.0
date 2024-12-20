﻿using BaseObjects;
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
using Console = BaseObjects.Console;
using BasicObjects.MathExtensions;
using Operations.Regions;
using Operations.Intermesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.SetOperators;

namespace Operations.ParallelSurfaces
{
    public static class Grid
    {
        public static IWireFrameMesh SetFacePlates(this IWireFrameMesh mesh, double thickness)
        {
            Sets.FoldPrimming(mesh);
            var output = AddParallelSurfaces(mesh, thickness);
            output.Intermesh();

            RemoveInternalFolds(output, thickness);
            FillPlateSides(output, thickness);
            ObliqueNormalAdjustments(output);
            ElbowFill(output, thickness);
            PlateSidesNormalReplacement(output);
            ElbowNormalBlending(output);

            return output;
        }

        public static IEnumerable<IWireFrameMesh> BuildFacePlates(this IWireFrameMesh mesh, double thickness)
        {
            Sets.FoldPrimming(mesh);
            var output = BuildParallelSurfaces(mesh, thickness).ToArray();
            foreach (var facePlate in output)
            {
                facePlate.Intermesh();
                RemoveInternalFolds(facePlate, thickness);
                FillPlateSides(facePlate, thickness);
                ObliqueNormalAdjustments(facePlate);
                ElbowFill(facePlate, thickness);
                PlateSidesNormalReplacement(facePlate);
                ElbowNormalBlending(facePlate);
            }
            return output;
        }

        public static IWireFrameMesh CombineFacePlates(IEnumerable<IWireFrameMesh> facePlates)
        {
            var output = facePlates.First().CreateNewInstance();
            foreach(var facePlate in facePlates)
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

        private static IWireFrameMesh AddParallelSurfaces(IWireFrameMesh mesh, double thickness)
        {
            var output = mesh.CreateNewInstance();
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();

            foreach (var face in faces.Select((s, i) => new { s, i }))
            {
                var parallelSurface = CreateParallelSurface(face.s, thickness).Where(t => !IsNearDegenerate(t.Triangle));
                {
                    var addedTriangles = output.AddRangeTriangles(parallelSurface, "", 0);
                    foreach (var triangle in addedTriangles) { triangle.Trace = $"S{face.i}"; }
                }
                {
                    var addedTriangles = output.AddRangeTriangles(face.s.Select(PositionTriangle.GetSurfaceTriangle), "", 0);
                    foreach (var triangle in addedTriangles) { triangle.Trace = $"B{face.i}"; }
                }
            }
            return output;
        }

        private static IEnumerable<IWireFrameMesh> BuildParallelSurfaces(IWireFrameMesh mesh, double thickness)
        {           
            var faces = GroupingCollection.ExtractFaces(mesh.Triangles).Select(f => f.Triangles).ToArray();

            foreach (var face in faces.Select((s, i) => new { s, i }))
            {
                var output = mesh.CreateNewInstance();
                var parallelSurface = CreateParallelSurface(face.s, thickness).Where(t => !IsNearDegenerate(t.Triangle));
                {
                    var addedTriangles = output.AddRangeTriangles(parallelSurface, "", 0);
                    foreach (var triangle in addedTriangles) { triangle.Trace = $"S{face.i}"; }
                }
                {
                    var addedTriangles = output.AddRangeTriangles(face.s.Select(PositionTriangle.GetSurfaceTriangle), "", 0);
                    foreach (var triangle in addedTriangles) { triangle.Trace = $"B{face.i}"; }
                }
                yield return output;
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

                if (Math.Abs(angle - Math.PI) < 1e-6)
                {
                    obliquePositions[pair.Key] = true;
                    obliquePositions[pair.Value] = true;
                    adjustedNormals[pair.Key] = Vector3D.Zero;
                    adjustedNormals[pair.Value] = adjustedNormals[pair.Key];
                }
                else if (Math.Abs(angle - Math.PI / 2) > 1e-6)
                {
                    obliquePositions[pair.Key] = true;
                    obliquePositions[pair.Value] = true;

                    var orthogonalPoint = SolveForOrthogonalPoint(point + vectorNormal, point + vectorBase, point);
                    adjustedNormals[pair.Key] = (orthogonalPoint - point).Direction;
                    adjustedNormals[pair.Value] = adjustedNormals[pair.Key];
                }
            }

            var trianglesToReplace = sideTriangleSet.Where(t => obliquePositions.ContainsKey(t.A.Id) || obliquePositions.ContainsKey(t.B.Id) || obliquePositions.ContainsKey(t.C.Id)).ToArray();
            var replacements = trianglesToReplace.Select(t => new SurfaceTriangle(
                new Ray3D(t.A.Position, adjustedNormals.ContainsKey(t.A.Id) ? adjustedNormals[t.A.Id] : t.A.Normal),
                new Ray3D(t.B.Position, adjustedNormals.ContainsKey(t.B.Id) ? adjustedNormals[t.B.Id] : t.B.Normal),
                new Ray3D(t.C.Position, adjustedNormals.ContainsKey(t.C.Id) ? adjustedNormals[t.C.Id] : t.C.Normal)));

            output.RemoveAllTriangles(trianglesToReplace);
            output.AddRangeTriangles(replacements, sideTriangleSet.Key, 0);
        }

        private static Point3D SolveForOrthogonalPoint(Point3D a, Point3D b, Point3D center)
        {
            var bc = b - center;
            var ba = b - a;
            var top = bc.X * bc.X + bc.Y * bc.Y + bc.Z * bc.Z;
            var bottom = ba.X * bc.X + ba.Y * bc.Y + ba.Z * bc.Z;
            double alpha = top / bottom;

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
                var addedTriangles = grid.AddRangeTriangles([triangle1, triangle2], "", 0);
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

                var baseGroup = group.FirstOrDefault(g => g.Triangles.Any(t => t.Trace[0] == 'B'));
                if (baseGroup is null) { continue; }
                var surfaceGroup = group.FirstOrDefault(g => g.Triangles.Any(t => t.Trace[0] == 'S'));
                if (surfaceGroup is null) { continue; }
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
                        dividerEdges.Add(positionEdge);
                    }
                }

                var baseEdgeSurfaceCollection = new SurfaceSegmentCollections<PlanarFillingGroup, Position>(CreateSurfaceSegmentSet(openEdges, dividerEdges));

                var chain = BaseDividerSurfaceChaining<PlanarFillingGroup, Position>.Create(baseEdgeSurfaceCollection);
                var chains = Chaining.SplitByPerimeterLoops(chain);

                foreach (var element in chains.Select((s, i) => new { s, i }))
                {
                    var surfacePoints = element.s.PerimeterLoops[0].
                        Select(p => p.Reference).ToArray().
                        UnwrapToBeginning((p, i) => IsSurfacePosition(p)).
                        Reverse().
                        ToArray();

                    var basePoints = element.s.PerimeterLoops[0].
                        Select(p => p.Reference).ToArray().
                        UnwrapToBeginning((p, i) => IsBasePosition(p)).
                        ToArray();

                    int s = 0;
                    var firstSurfacePoint = surfacePoints.First().Id;
                    var lastSurfacePoint = surfacePoints.Last().Id;
                    for (int b = 0; b < basePoints.Length - 1; b++)
                    {
                        while (s + 1 < surfacePoints.Length &&
                            Point3D.Distance(basePoints[b + 1].Point, surfacePoints[s].Point) > Point3D.Distance(basePoints[b + 1].Point, surfacePoints[s + 1].Point))
                        {
                            elbowTriangles.Add(new TriangleTrace(basePoints[b].Point, surfacePoints[s].Point, surfacePoints[s + 1].Point, $"F{element.i}", 0));
                            s++;
                        }
                        elbowTriangles.Add(new TriangleTrace(basePoints[b].Point, basePoints[b + 1].Point, surfacePoints[s].Point, $"F{element.i}", 0));
                    }

                    while (s + 1 < surfacePoints.Length)
                    {
                        elbowTriangles.Add(new TriangleTrace(basePoints[basePoints.Length - 1].Point, surfacePoints[s].Point, surfacePoints[s + 1].Point, $"F{element.i}", 0));
                        s++;
                    }
                }
            }

            foreach (var triangle in elbowTriangles)
            {
                mesh.AddTriangle(triangle.Triangle, triangle.Trace, triangle.Tag);
            }
        }

        private class TriangleTrace
        {
            public TriangleTrace(Point3D a, Point3D b, Point3D c, string trace, int tag)
            {
                Triangle = new Triangle3D(a, b, c);
                Trace = trace;
                Tag = tag;
            }
            public Triangle3D Triangle { get; }
            public string Trace { get; }
            public int Tag { get; }
        }

        private class SurfaceTriangleTrace
        {
            public SurfaceTriangleTrace(Ray3D a, Ray3D b, Ray3D c, string trace, int tag)
            {
                Triangle = new SurfaceTriangle(a, b, c);
                Trace = trace;
                Tag = tag;
            }
            public SurfaceTriangle Triangle { get; }
            public string Trace { get; }
            public int Tag { get; }
        }


        private static void ElbowNormalBlending(IWireFrameMesh mesh)
        {
            var elbowNormals = mesh.Triangles.Where(t => t.Trace[0] == 'F');
            if (!elbowNormals.Any()) { return; }

            var elbowGroups = elbowNormals.GroupBy(t => t.Trace).ToArray();
            foreach (var elbowGroup in elbowGroups)
            {
                var triangles = elbowGroup.ToArray();
                var replacementPositions = new Dictionary<int, Vector3D>();
                var referencePositions = new Dictionary<int, Vector3D>();
                var positionHolders = new Dictionary<int, List<PositionNormal>>();
                var cuspPositions = new Dictionary<int, List<PositionNormal>>();

                var surfaces = GroupingCollection.ExtractSurfaces(triangles);
                var perimeter = surfaces.Select(f => f.PerimeterEdges).First().SelectMany(e => e.Positions).Select(p => p.PositionObject).DistinctBy(p => p.Id);
                var surfacePositions = perimeter.Where(IsSurfacePosition);
                var basePositions = perimeter.Where(IsBasePosition);

                foreach (var position in basePositions.
                    Where(p =>
                        p.PositionNormals.Count(pn => pn.Triangles.Any(t => t.Trace[0] == 'E')) == 1))
                {
                    positionHolders[position.Id] = position.PositionNormals.ToList();
                    //Console.WriteLine($"ePosition {position.Id} {positionHolders.Count}");
                }

                foreach (var position in surfacePositions.
                    Where(p =>
                        p.PositionNormals.Count(pn => pn.Triangles.Any(t => t.Trace[0] == 'S')) > 1))
                {
                    cuspPositions[position.Id] = position.PositionNormals.ToList();
                    //Console.WriteLine($"sPosition {position.Id} {positionHolders.Count}");
                }
                //Console.WriteLine("Done");

                foreach (var triangle in triangles)
                {
                    if (positionHolders.ContainsKey(triangle.A.PositionObject.Id))
                    {
                        positionHolders[triangle.A.PositionObject.Id].RemoveAll(r => r.Id == triangle.A.Id);
                        referencePositions[triangle.A.PositionObject.Id] = triangle.A.Normal.Direction;
                    }
                    if (positionHolders.ContainsKey(triangle.B.PositionObject.Id))
                    {
                        positionHolders[triangle.B.PositionObject.Id].RemoveAll(r => r.Id == triangle.B.Id);
                        referencePositions[triangle.B.PositionObject.Id] = triangle.B.Normal.Direction;
                    }
                    if (positionHolders.ContainsKey(triangle.C.PositionObject.Id))
                    {
                        positionHolders[triangle.C.PositionObject.Id].RemoveAll(r => r.Id == triangle.C.Id);
                        referencePositions[triangle.C.PositionObject.Id] = triangle.C.Normal.Direction;
                    }
                }

                foreach (var kv in positionHolders.Where(kv => kv.Value.Count == 3))
                {
                    var list = kv.Value;
                    if (Vector3D.ArePerpendicular(list[0].Normal, list[1].Normal))
                    {
                        var cross = Vector3D.Cross(list[0].Normal, list[1].Normal).Direction;
                        var newVector = Vector3D.Cross(cross, list[2].Normal).Direction;
                        if (Vector3D.AreOpposite(newVector, referencePositions[kv.Key])) { newVector = -newVector; }
                        //Console.WriteLine($"1 New vector {newVector} Given vectors {string.Join(",", list.Select(pn => pn.Normal))} Cross {cross}");
                        replacementPositions[kv.Key] = newVector;
                        continue;
                    }

                    if (Vector3D.ArePerpendicular(list[1].Normal, list[2].Normal))
                    {
                        var cross = Vector3D.Cross(list[1].Normal, list[2].Normal).Direction;
                        var newVector = Vector3D.Cross(cross, list[0].Normal).Direction;
                        if (Vector3D.AreOpposite(newVector, referencePositions[kv.Key])) { newVector = -newVector; }
                        //Console.WriteLine($"3 New vector {newVector} Given vectors {string.Join(",", list.Select(pn => pn.Normal))} Cross {cross}");
                        replacementPositions[kv.Key] = newVector;
                        continue;
                    }

                    if (Vector3D.ArePerpendicular(list[0].Normal, list[2].Normal))
                    {
                        var cross = Vector3D.Cross(list[0].Normal, list[2].Normal).Direction;
                        var newVector = Vector3D.Cross(cross, list[1].Normal).Direction;
                        if (Vector3D.AreOpposite(newVector, referencePositions[kv.Key])) { newVector = -newVector; }
                        //Console.WriteLine($"2 New vector {newVector} Given vectors {string.Join(",", list.Select(pn => pn.Normal))} Cross {cross}");
                        replacementPositions[kv.Key] = newVector;
                        continue;
                    }
                }

                var replacements = new List<SurfaceTriangleTrace>();
                var removals = new List<PositionTriangle>();

                foreach (var triangle in triangles)
                {
                    Ray3D a = null;
                    Ray3D b = null;
                    Ray3D c = null;

                    if (replacementPositions.ContainsKey(triangle.A.PositionObject.Id))
                    {
                        a = new Ray3D(triangle.A.Position, replacementPositions[triangle.A.PositionObject.Id]);
                    }
                    if (replacementPositions.ContainsKey(triangle.B.PositionObject.Id))
                    {
                        b = new Ray3D(triangle.B.Position, replacementPositions[triangle.B.PositionObject.Id]);
                    }
                    if (replacementPositions.ContainsKey(triangle.C.PositionObject.Id))
                    {
                        c = new Ray3D(triangle.C.Position, replacementPositions[triangle.C.PositionObject.Id]);
                    }

                    if (a is null && b is null && c is null) { continue; }

                    if (a is null) { a = PositionNormal.GetRay(triangle.A); }
                    if (b is null) { b = PositionNormal.GetRay(triangle.B); }
                    if (c is null) { c = PositionNormal.GetRay(triangle.C); }

                    replacements.Add(new SurfaceTriangleTrace(a, b, c, triangle.Trace, triangle.Tag));
                    removals.Add(triangle);
                }

                mesh.RemoveAllTriangles(removals);
                foreach (var triangle in replacements)
                {
                    mesh.AddTriangle(triangle.Triangle, triangle.Trace, triangle.Tag);
                }
            }
        }

        private static void PlateSidesNormalReplacement(IWireFrameMesh mesh)
        {
            var zeroTriangles = mesh.Triangles.Where(t => t.A.Normal == Vector3D.Zero || t.B.Normal == Vector3D.Zero || t.C.Normal == Vector3D.Zero).ToArray();
            if (!zeroTriangles.Any()) { return; }

            var space = new Space(mesh.Triangles.Select(t => t.Triangle));
            var replacements = new List<SurfaceTriangleTrace>();
            foreach (var triangle in zeroTriangles)
            {
                var principleNormal = triangle.Triangle.Normal.Direction;
                var testPoint = triangle.Triangle.Center + 1e-6 * principleNormal;
                var spaceRegion = space.RegionOfPoint(testPoint);
                if (spaceRegion == Region.Interior) { principleNormal = -principleNormal; }

                var a = PositionNormal.GetRay(triangle.A);
                var b = PositionNormal.GetRay(triangle.B);
                var c = PositionNormal.GetRay(triangle.C);

                if (triangle.A.Normal == Vector3D.Zero) { a = new Ray3D(a.Point, principleNormal); }
                if (triangle.B.Normal == Vector3D.Zero) { b = new Ray3D(b.Point, principleNormal); }
                if (triangle.C.Normal == Vector3D.Zero) { c = new Ray3D(c.Point, principleNormal); }

                replacements.Add(new SurfaceTriangleTrace(a, b, c, triangle.Trace, triangle.Tag));
            }

            mesh.RemoveAllTriangles(zeroTriangles);
            foreach (var triangle in replacements)
            {
                mesh.AddTriangle(triangle.Triangle, triangle.Trace, triangle.Tag);
            }
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
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.Normal, e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.Normal, e.B.PositionObject.Id, e.B.PositionObject))).ToArray(),
                PerimeterSegments = openEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.Normal, e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.Normal, e.B.PositionObject.Id, e.B.PositionObject))).ToArray(),
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
                var surfacePair = pair.SingleOrDefault(p => p.Key[0] == 'S');
                if (surfacePair is null) { continue; }
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

        private static SurfaceSegmentSets<Empty, Position> CreateSurfaceSegmentSet(IEnumerable<GroupEdge> perimeterEdges)
        {
            return new SurfaceSegmentSets<Empty, Position>
            {
                DividingSegments = Array.Empty<SurfaceSegmentContainer<Position>>(),
                PerimeterSegments = perimeterEdges.Select(e => new SurfaceSegmentContainer<Position>(
                    new SurfaceRayContainer<Position>(new Ray3D(e.A.Position, Vector3D.Zero), e.A.Normal, e.A.PositionObject.Id, e.A.PositionObject),
                    new SurfaceRayContainer<Position>(new Ray3D(e.B.Position, Vector3D.Zero), e.B.Normal, e.B.PositionObject.Id, e.B.PositionObject))).ToArray()
            };
        }
    }
}
