using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Basics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.PositionRemovals.FillActions;
using Operations.PositionRemovals.Interfaces;
using Operations.PositionRemovals.Internals;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using System.Collections.Generic;
using Console = BaseObjects.Console;

namespace Operations.PositionRemovals
{
    public static class Grid
    {
        public static void RemoveCoplanarSurfacePoints(this IWireFrameMesh mesh)
        {
            var start = DateTime.Now;
            var positions = mesh.Positions;
            var surfacePositions = positions.Where(p => p.Cardinality == 1).ToArray();
            Console.WriteLine($"Candidate surface positions {surfacePositions.Length}");
            var qualifiedPositions = surfacePositions.Where(IsCoplanar).ToArray();
            Console.WriteLine($"Qualified positions {qualifiedPositions.Length}");
            mesh.RemovePositions(qualifiedPositions);

            ConsoleLog.WriteLine($"Remove coplanar surface points: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
        }

        private static bool IsCoplanar(Position pp)
        {
            var plane = new Plane(pp.PositionNormals[0].Position, pp.PositionNormals[0].Normal.Direction);
            var points = pp.PositionNormals[0].Triangles.SelectMany(p => p.Positions).DistinctBy(p => p.Id).Select(p => p.Position).ToArray();
            return plane.AllPointsOnPlane(points);
        }

        public static void RemoveCollinearEdgePoints(this IWireFrameMesh mesh)
        {
            var start = DateTime.Now;
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var edgeSegments = segments.Where(s => s.A.PositionObject.Cardinality > 1 && s.B.PositionObject.Cardinality > 1).ToArray();
            var edgePositions = edgeSegments.SelectMany(e => e.Positions).Select(p => p.PositionObject).Where(p => p.Cardinality == 2).DistinctBy(p => p.Id).ToArray();
            Console.WriteLine($"Candidate edge positions {edgePositions.Length}");
            var positionClusters = edgePositions.Select(p => new EdgeCluster(p)).ToArray();

            var qualifiedClusters = positionClusters.Where(c => c.Cluster.Length == 2 && IsCollinear(c)).ToArray();
            Console.WriteLine($"Qualified clusters {qualifiedClusters.Length}");

            mesh.RemovePositions(qualifiedClusters.Select(c => c.Position));
            ConsoleLog.WriteLine($"Remove collinear edge points: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
        }

        private static bool IsCollinear(EdgeCluster cluster)
        {
            var line = new Line3D(cluster.Cluster[0].Point, cluster.Cluster[1].Point);
            return line.PointIsOnLine(cluster.Position.Point);
        }

        public static void RemoveShortSegments(this IWireFrameMesh mesh, double minimumLength)
        {
            var start = DateTime.Now;
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            mesh.ShowSegmentLengths(ConsoleColor.Red);

            Console.WriteLine($"Short segments {shortSegments.Length}");

            ShortEdgeSegmentRemoval(mesh, minimumLength);
            ShortSurfaceSegmentRemoval(mesh, minimumLength);
            ShortEdgeSurfaceSegmentRemoval(mesh, minimumLength);
            ShortEdgeCornerSegmentRemoval(mesh, minimumLength);

            //RemoveTagTriangles(mesh);

            segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            Console.WriteLine($"Remaining short segments {shortSegments.Length}");
            var groups = shortSegments.GroupBy(s => s.Cardinality, new Combination2Comparer()).ToArray();
            foreach (var group in groups.OrderBy(g => g.Key.A))
            {
                Console.WriteLine($"{group.Key} {group.Count()}");
            }

            mesh.ShowSegmentLengths(ConsoleColor.Green);

            ConsoleLog.WriteLine($"Remove short segments: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
        }

        private static void ShortEdgeSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            int lastLength = -1;
            while (lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalSets = GetEdgeRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalSets.Length == 0) { break; }

                GetMarkedPositions(removalSets, out List<Position> markedPositions, out List<Position> unmarkedPositions);
                mesh.RemovePositions(markedPositions.ToArray(), new FirstValidFill<PositionNormal>());


                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Edge short segments {shortSegments.Length} Positions removed {removalSets.Length}");
            }
        }

        private static void ShortSurfaceSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            int lastLength = -1;
            while (lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetSurfaceRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

                mesh.RemovePositions(markedPositions.ToArray(), new FirstValidFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Surface short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }
        }

        private static void ShortEdgeSurfaceSegmentRemoval(IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            int lastLength = -1;
            while (lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetEdgeSurfaceRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                if (removalPositions.Length == 0) { break; }

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

                mesh.RemovePositions(markedPositions.ToArray(), new FirstValidFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Edge surface short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }
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
                mesh.RemovePositions(markedPositions.ToArray(), new FirstValidFill<PositionNormal>());

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Edge Corner short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            } while (shortSegments.Length != lastLength);
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

            return tagTriangles.Length;
        }

        private static IEnumerable<Position> GetEdgeRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(2, 2)))
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

            if (positions.Length == 0) { return; }
            positions[0].PositionNormals[0].Triangles[0].GridClearMarks();

            foreach (var position in positions)
            {
                if (position.Triangles.Any(t => t.IsMarked)) { unmarkedPositions.Add(position); continue; }
                foreach (var triangle in position.Triangles) { triangle.Mark(); }
                markedPositions.Add(position);
            }
        }


        internal static void RemovePosition(this IWireFrameMesh mesh, Position position)
        {
            mesh.RemovePosition(position, null);
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

        public static void RemovePositions(this IWireFrameMesh mesh, IEnumerable<Position> positions)
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

                foreach (var position in markedPositions)
                {
                    EdgeCluster adjacentPoints = null;
                    if (position.Cardinality == 2) { adjacentPoints = new EdgeCluster(position); }

                    foreach (var positionNormal in position.PositionNormals)
                    {
                        var triangles = positionNormal.Triangles.ToArray();
                        var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                        var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                        var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);

                        var perimeterPoints = chain.PerimeterLoops.FirstOrDefault();

                        //remove opposite points
                        if(fillAction is not null)
                        {
                            var matchingPoints = new List<Position>();
                            if(perimeterPoints is not null)
                            {
                                matchingPoints.AddRange(perimeterPoints.Select(p => p.Reference.PositionObject).Where(p => p.Cardinality < 2));
                            }
                            if(adjacentPoints is not null)
                            {
                                matchingPoints.AddRange(adjacentPoints.Cluster.Where(c => c.Id != position.Id && c.Cardinality < 3));

                                var cornerPoint = adjacentPoints.Cluster.FirstOrDefault(c => c.Id != position.Id && c.Cardinality > 2);
                                if (cornerPoint is not null && perimeterPoints is not null)
                                {
                                    var cornerPositions = cornerPoint.Triangles.SelectMany(t => t.Positions).
                                        Select(p => p.PositionObject).DistinctBy(p => p.Id).
                                        Where(p => p.Id != cornerPoint.Id && p.Id != position.Id);
                                    if (cornerPositions.Any())
                                    {
                                        var cornerOpposites = cornerPositions.IntersectBy(perimeterPoints.Select(p => p.Reference.PositionObject.Id), c => c.Id).ToArray();
                                        fillAction.FillConditions.SetSecondaryMatchingPoints(matchingPoints.Concat(cornerOpposites).ToArray());
                                    }
                                }
                            }

                            fillAction.FillConditions.SetPrimaryMatchingPoints(matchingPoints.ToArray());
                        }
                        
                        var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillAction, position.Id);
                        var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });

                        fillingsToAdd.AddRange(fillings);
                        trianglesToRemove.AddRange(triangles);
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
            var arc0 = triangles.SelectMany(t => t.Edges).DistinctBy(e => e.Key, new Combination2Comparer()).ToList();
            var arc = arc0.Where(e => !e.ContainsPosition(positionNormal.PositionObject)).ToList();
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
