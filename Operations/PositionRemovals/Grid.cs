using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.PositionRemovals.FillActions;
using Operations.PositionRemovals.Interfaces;
using Operations.PositionRemovals.Internals;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Chaining.Diagnostics;
using Operations.SurfaceSegmentChaining.Collections;
using Console = BaseObjects.Console;
using Plane = BasicObjects.GeometricObjects.Plane;

namespace Operations.PositionRemovals
{
    public static class Grid
    {
        public static void RemoveShortSegments(this IWireFrameMesh mesh, double minimumLength)
        {
            var start = DateTime.Now;
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            mesh.ShowSegmentLengths(ConsoleColor.Red);
            ConsoleLog.Push($"Remove short segments");

            var test = mesh.Triangles.Where(t => t.Key == new Combination3(13924379, 13942790, 13942842));

            int lastShortSegments = 0;
            do
            {
                lastShortSegments = shortSegments.Length;
                ShortEdgeSegmentRemoval(mesh, minimumLength);
                ShortSurfaceSegmentRemoval(mesh, minimumLength);
                ShortEdgeSurfaceSegmentRemoval(mesh, minimumLength);
                ShortEdgeCornerSegmentRemoval(mesh, minimumLength);
                ShortCornerSegmentRemoval(mesh, minimumLength);
                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                ConsoleLog.WriteLine($"Remaining short segments {shortSegments.Length}");
                ShowRemainingGroups(shortSegments);
            }
            while (shortSegments.Length != lastShortSegments);

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Remove short segments: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
            mesh.ShowSegmentLengths(ConsoleColor.Green);
        }

        public static void RemoveCoplanarSurfacePoints(this IWireFrameMesh mesh)
        {
            var start = DateTime.Now;
            ConsoleLog.Push("Remove coplanar surface points");
            var positions = mesh.Positions;
            var surfacePositions = positions.Where(p => p.Cardinality == 1).ToArray();
            ConsoleLog.WriteLine($"Candidate surface positions {surfacePositions.Length}");
            var qualifiedPositions = surfacePositions.Where(IsCoplanar).ToArray();
            ConsoleLog.WriteLine($"Qualified positions {qualifiedPositions.Length}");
            mesh.RemovePositions(qualifiedPositions);
            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Remove coplanar surface points: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");
        }

        public static void RemoveCollinearEdgePoints(this IWireFrameMesh mesh)
        {
            var start = DateTime.Now;
            ConsoleLog.Push("Remove collinear edge points");
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var edgeSegments = segments.Where(s => s.A.PositionObject.Cardinality > 1 && s.B.PositionObject.Cardinality > 1).ToArray();
            var edgePositions = edgeSegments.SelectMany(e => e.Positions).Select(p => p.PositionObject).Where(p => p.Cardinality == 2).DistinctBy(p => p.Id).ToArray();
            ConsoleLog.WriteLine($"Candidate edge positions {edgePositions.Length}");
            var positionClusters = edgePositions.Select(p => new EdgeCluster(p)).ToArray();

            var qualifiedClusters = positionClusters.Where(c => c.Cluster.Length == 2 && IsCollinear(c)).ToArray();
            ConsoleLog.WriteLine($"Qualified clusters {qualifiedClusters.Length}");

            mesh.RemovePositions(qualifiedClusters.Select(c => c.Position));
            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Remove collinear edge points: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.");

        }

        private static void ShowRemainingGroups(PositionEdge[] shortSegments)
        {
            var groups = shortSegments.GroupBy(s => s.Cardinality, new Combination2Comparer()).ToArray();
            foreach (var group in groups.OrderBy(g => g.Key.A))
            {
                ConsoleLog.WriteLine($"{group.Key} {group.Count()}");
            }
        }

        private static bool IsCoplanar(Position pp)
        {
            var plane = new Plane(pp.PositionNormals[0].Position, pp.PositionNormals[0].Normal.Direction);
            var points = pp.PositionNormals[0].Triangles.SelectMany(p => p.Positions).DistinctBy(p => p.Id).Select(p => p.Position).ToArray();
            return plane.AllPointsOnPlane(points);
        }

        private static bool IsCollinear(EdgeCluster cluster)
        {
            var line = new Line3D(cluster.Cluster[0].Point, cluster.Cluster[1].Point);
            return line.PointIsOnLine(cluster.Position.Point);
        }

        private static void ShortEdgeSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            ConsoleLog.WriteLine($"Edge short segments {shortSegments.Length}");
            int lastLength = -1;
            do
            {
                lastLength = shortSegments.Length;
                var removalSets = GetEdgeRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalSets.Length == 0) { break; }

                GetMarkedPositions(removalSets, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new GeneralRemovalFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                ConsoleLog.WriteLine($"Edge short segments {shortSegments.Length} Positions removed {removalSets.Length}");
            }
            while (shortSegments.Length != lastLength);
        }

        private static void ShortSurfaceSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            int lastLength = -1;
            do
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetSurfaceRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new GeneralRemovalFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Surface short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }
            while (shortSegments.Length != lastLength);
        }

        private static void ShortEdgeSurfaceSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            int lastLength = -1;
            do
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetEdgeSurfaceRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new GeneralRemovalFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                ConsoleLog.WriteLine($"Edge surface short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }
            while (shortSegments.Length != lastLength);
        }

        private static void ShortEdgeCornerSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            int lastLength = -1;
            do
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetEdgeCornerRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new GeneralRemovalFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                ConsoleLog.WriteLine($"Edge corner short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }
            while (shortSegments.Length != lastLength);
        }

        private static void ShortCornerSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            int lastLength = -1;
            do
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetCornerRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new CornerRemovalFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                ConsoleLog.WriteLine($"Corner short segments {shortSegments.Length}  Positions removed {removalPositions.Length}");
            }
            while (shortSegments.Length != lastLength);
        }

        public static int RemoveTagTriangles(this IWireFrameMesh mesh)
        {
            var tagTriangles = mesh.Triangles.Where(t => t.AdjacentAnyCount <= 2).ToArray();
            Console.WriteLine($"Tag triangles {string.Join(",", tagTriangles.Select(t => t.Key))}");
            var removed = 0;
            foreach (var triangle in tagTriangles) { removed += mesh.RemoveTriangle(triangle) ? 1 : 0; }
            Console.WriteLine($"Removed {removed}");
            return removed;
        }

        public static int ShowTagTriangles(this IWireFrameMesh mesh)
        {
            var tagTriangles = mesh.Triangles.Where(t => t.AdjacentAnyCount <= 2).ToArray();
            Console.WriteLine($"Tag triangles {string.Join(",", tagTriangles.Select(t => t.Key))}");
            var test = WireFrameMesh.Create();
            test.AddRangeTriangles(tagTriangles.Select(t => t.Triangle));
            WavefrontFile.Export(test, "Wavefront/TagTriangles");
            return tagTriangles.Length;
        }

        public static void ShowOpenEdges(this IWireFrameMesh mesh)
        {
                var test = WireFrameMesh.Create();
                var openEdges = mesh.Triangles.SelectMany(t => t.OpenEdges);
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => o.Segment))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.PositionObject.Point}<{o.A.PositionObject.Id}>, {o.B.PositionObject.Point}<{o.B.PositionObject.Id}>]"))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.Normal}<{o.A.PositionObject.Id}>, {o.B.Normal}<{o.B.PositionObject.Id}>]"))}");
                test.AddRangeTriangles(openEdges.Select(e => e.Plot));
                WavefrontFile.Export(test, $"Wavefront/TagOpenEdges");
        }

        private static IEnumerable<Position> GetEdgeRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(2, 2)))
            {
                if (segment.A.Triangles.Count() > segment.B.Triangles.Count()) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetCornerRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(3, 3)))
            {
                if (segment.A.Triangles.Count() > segment.B.Triangles.Count()) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetEdgeCornerRemovalPositions(PositionEdge[] segments)
        {
            return segments.Where(s => s.Cardinality.A == 2 && s.Cardinality.B > 2).
                Select(e => e.A.PositionObject.Cardinality == 2 ? e.A.PositionObject : e.B.PositionObject);
        }

        private static IEnumerable<Position> GetSurfaceRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(1, 1)))
            {
                if (segment.A.Triangles.Count() > segment.B.Triangles.Count()) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetEdgeSurfaceRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(1, 2)))
            {
                if (segment.A.PositionObject.Cardinality == 1) { yield return segment.A.PositionObject; }
                yield return segment.B.PositionObject;
            }
        }

        private static void GetMarkedPositions(Position[] positions, out List<Position> markedPositions, out List<Position> unmarkedPositions)
        {
            markedPositions = new List<Position>();
            unmarkedPositions = new List<Position>();

            if (positions?.FirstOrDefault()?.
                PositionNormals?.FirstOrDefault()?.
                Triangles?.FirstOrDefault() is null) { return; }
            positions[0].PositionNormals[0].Triangles[0].GridClearMarks();

            foreach (var position in positions)
            {
                if (position.Triangles.Any(t => t.IsMarked)) { unmarkedPositions.Add(position); continue; }
                foreach (var triangle in position.Triangles) { triangle.Mark(); }
                markedPositions.Add(position);
            }
        }

        public static void FanRemovePosition(this IWireFrameMesh mesh, Position position, Position fanPosition)
        {
            var fill = new PointFanFill<PositionNormal>();
            fill.FanPositions = [fanPosition];
            RemovePosition(mesh, position, fill);
        }

        internal static void RemovePosition(this IWireFrameMesh mesh, Position position, IFillAction<PositionNormal> fillAction)
        {
            var trianglesToRemove = new List<PositionTriangle>();
            var fillingsToAdd = new List<PositionNormal[]>();

            foreach (var positionNormal in position.PositionNormals)
            {
                var triangles = positionNormal.Triangles.ToArray();
                var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);

                var perimeterPoints = chain.PerimeterLoops.FirstOrDefault();

                fillAction?.PresetMatching(position, perimeterPoints);

                var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillAction, position.Id);

                var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });
                fillingsToAdd.AddRange(fillings);
                trianglesToRemove.AddRange(triangles);
            }

            foreach (var filling in fillingsToAdd)
            {
                mesh.AddTriangle(filling[0], filling[1], filling[2]);
            }
            mesh.RemoveAllTriangles(trianglesToRemove);
        }

        internal static void RemovePositions(this IWireFrameMesh mesh, IEnumerable<Position> positions)
        {
            mesh.RemovePositions(positions, null);
        }

        internal static void RemovePositions(this IWireFrameMesh mesh, IEnumerable<Position> positions, IFillAction<PositionNormal> fillAction)
        {
            GetMarkedPositions(positions.ToArray(), out List<Position> markedPositions, out List<Position> unmarkedPositions);

            while (markedPositions.Count > 0)
            {
                var trianglesToRemove = new List<PositionTriangle>();
                var fillingsToAdd = new List<PositionNormal[]>();

                foreach (var position in markedPositions.Where(p => p.Cardinality < 3))
                {
                    foreach (var positionNormal in position.PositionNormals)
                    {
                        var triangles = positionNormal.Triangles.ToArray();
                        var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                        var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                        var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);

                        var perimeterPoints = chain.PerimeterLoops.FirstOrDefault();

                        fillAction?.PresetMatching(position, perimeterPoints);
                        try
                        {
                            var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillAction, position.Id);
                            var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });

                            fillingsToAdd.AddRange(fillings);
                            trianglesToRemove.AddRange(triangles);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                foreach (var position in markedPositions.Where(p => p.Cardinality > 2))
                {
                    var trianglePositions = position.Triangles.SelectMany(t => t.Positions.Select(p => p.PositionObject)).DistinctBy(p => p.Id).ToArray();
                    if (trianglePositions.Count(p => p.Cardinality > 2 && p.Id != position.Id) > 1) { Console.WriteLine($"Corner by-pass {position.Id}"); continue; }

                    var antipodePosition = trianglePositions.SingleOrDefault(p => p.Cardinality > 2 && p.Id != position.Id);
                    if (antipodePosition is null) { Console.WriteLine($"Corner by-pass no antipode {position.Id}"); continue; }

                    foreach (var positionNormal in position.PositionNormals.ToArray())
                    { 
                        try
                        {
                            trianglesToRemove.AddRange(positionNormal.Triangles);

                            var segmentSet = CreateSurfaceCornerSet(mesh, positionNormal, antipodePosition);
                            if (segmentSet is null)
                            {
                                continue;
                            }

                            var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                            var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);
                            var perimeterPoints = chain.PerimeterLoops.FirstOrDefault();

                            fillAction?.PresetMatching(position, perimeterPoints);
                            var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillAction, position.Id);
                            var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });
                            fillingsToAdd.AddRange(fillings);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                foreach (var filling in fillingsToAdd)
                {
                    mesh.AddTriangle(filling[0], filling[1], filling[2]);
                }
                mesh.RemoveAllTriangles(trianglesToRemove);

                GetMarkedPositions(unmarkedPositions.ToArray(), out markedPositions, out unmarkedPositions);
            }
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, PositionNormal> CreateSurfaceSegmentSet(PositionNormal positionNormal, IEnumerable<PositionTriangle> triangles)
        {
            return CreateSurfaceSegmentSet(positionNormal.PositionObject, positionNormal.Normal, triangles);
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, PositionNormal> CreateSurfaceSegmentSet(Position position, Vector3D normal, IEnumerable<PositionTriangle> triangles)
        {
            var arc0 = triangles.SelectMany(t => t.Edges).DistinctBy(e => e.Key, new Combination2Comparer()).ToList();
            var arc = arc0.Where(e => !e.ContainsPosition(position)).ToList();
            var plane = new Plane(position.Point, normal);
            var box = Rectangle3D.Containing(triangles.Select(t => t.Box).ToArray());
            var endPoints = arc.SelectMany(s => s.Positions).GroupBy(g => g.PositionObject.Id).Where(g => g.Count() == 1).Select(g => g.First()).ToArray();
            if (endPoints.Any())
            {
                arc.Add(new PositionEdge(endPoints[0], endPoints[1]));//Connect gap left by removed position.
            }

            return new SurfaceSegmentSets<PlanarFillingGroup, PositionNormal>
            {
                NodeId = position.Id,
                GroupObject = new PlanarFillingGroup(plane, box.Diagonal),
                DividingSegments = Array.Empty<SurfaceSegmentContainer<PositionNormal>>(),
                PerimeterSegments = arc.Select(e => new SurfaceSegmentContainer<PositionNormal>(
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.A), normal, e.A.Id, e.A),
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.B),normal, e.B.Id, e.B))).ToArray()
            };
        }

        private static SurfaceSegmentSets<PlanarFillingGroup, PositionNormal> CreateSurfaceCornerSet(IWireFrameMesh mesh, PositionNormal positionNormal, Position antipodePosition)
        {
            var position = positionNormal.PositionObject;
            var antipode = positionNormal.Triangles.SelectMany(t => t.Positions).SingleOrDefault(p => p.PositionObject.Cardinality > 2 && p.PositionObject.Id != position.Id);
            if (antipode is null)
            {
                antipode = mesh.AddPoint(antipodePosition.Point, positionNormal.Normal);
            }

            var triangles = positionNormal.Triangles.ToArray();

            var perimeterArc = triangles.SelectMany(t => t.Edges).Where(e => e.A.PositionObject.Id != position.Id && e.B.PositionObject.Id != position.Id).ToArray();
            var positionEdges = triangles.SelectMany(t => t.Edges).Where(e => e.A.PositionObject.Id == position.Id || e.B.PositionObject.Id == position.Id).ToArray();

            var endPointGroups = perimeterArc.SelectMany(p => p.Positions).GroupBy(p => p.PositionObject.Id);
            var endPoints = endPointGroups.Where(g => g.Count() == 1).Select(g => g.Single());
            var links = endPoints.Select(e => new PositionEdge(e, antipode)).Where(l => l.A.PositionObject.Id != l.B.PositionObject.Id).ToArray();

            var perimeter = perimeterArc.Concat(links).DistinctBy(p => p.Key, new Combination2Comparer()).ToArray();
            if (perimeter.Length < 3) { return null; }

            var plane = new Plane(position.Point, position.Normal);
            var box = Rectangle3D.Containing(triangles.Select(t => t.Box).ToArray());

            return new SurfaceSegmentSets<PlanarFillingGroup, PositionNormal>
            {
                NodeId = position.Id,
                GroupObject = new PlanarFillingGroup(plane, box.Diagonal),
                DividingSegments = Array.Empty<SurfaceSegmentContainer<PositionNormal>>(),
                PerimeterSegments = perimeter.Select(e => new SurfaceSegmentContainer<PositionNormal>(
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.A), positionNormal.Normal, e.A.Id, e.A),
                    new SurfaceRayContainer<PositionNormal>(PositionNormal.GetRay(e.B), positionNormal.Normal, e.B.Id, e.B))).ToArray()
            };
        }
    }
}
