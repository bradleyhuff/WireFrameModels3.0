using BaseObjects;
using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Basics;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
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
            var positionClusters = edgePositions.Select(GetCluster).ToArray();
            var qualifiedClusters = positionClusters.Where(c => c.Cluster.Length == 2 && IsCollinear(c)).ToArray();
            Console.WriteLine($"Qualified clusters {qualifiedClusters.Length}");

            mesh.RemovePositions(qualifiedClusters.Select(c => c.Position));
            ConsoleLog.WriteLine($"Remove collinear edge points: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
        }

        private static EdgeCluster GetCluster(Position pp)
        {
            var cluster1 = pp.PositionNormals[0].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();
            var cluster2 = pp.PositionNormals[1].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();

            return new EdgeCluster() { Position = pp, Cluster = cluster1.IntersectBy(cluster2.Select(c => c.Id), c => c.Id).ToArray() };
        }

        private static bool IsCollinear(EdgeCluster cluster)
        {
            var line = new Line3D(cluster.Cluster[0].Point, cluster.Cluster[1].Point);
            return line.PointIsOnLine(cluster.Position.Point);
        }

        private class EdgeCluster
        {
            public Position Position { get; set; }
            public Position[] Cluster { get; set; }
        }

        public static void RemoveShortSegments(this IWireFrameMesh mesh, double minimumLength)
        {
            var start = DateTime.Now;
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();

            mesh.ShowSegmentLengths(ConsoleColor.Red);

            Console.WriteLine($"Short segments {shortSegments.Length}");

            int lastLength = -1;
            while (shortSegments.Length > 0 && lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetEdgeRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

                mesh.RemovePositions(markedPositions);

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Edge short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }

            lastLength = -1;
            while (shortSegments.Length > 0 && lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetSurfaceRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

                mesh.RemovePositions(markedPositions);

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Surface short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            }

            //segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            //shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            //var mixedPositions = GetMixedRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
            //Console.WriteLine($"Mixed short positions {mixedPositions.Length}");
            //Console.WriteLine($"Remaining short segments {shortSegments.Length}");

            lastLength = -1;
            while (shortSegments.Length > 0 && lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetMixedRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();

                GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

                mesh.RemovePositions(markedPositions, new EdgeExclusions()/*, fillingIsAllowed*/);

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Mixed short segments {shortSegments.Length}");
            }

            //lastLength = -1;
            //while (shortSegments.Length > 0 && lastLength != shortSegments.Length)
            //{
            //    lastLength = shortSegments.Length;
            //    var removalPositions = GetEdgeCornerRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();

            //    GetMarkedPositions(removalPositions, out List<Position> markedPositions, out List<Position> unmarkedPositions);

            //    mesh.RemovePositions(markedPositions, fillingIsAllowed2);

            //    segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            //    shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            //    Console.WriteLine($"Edge Corner short segments {shortSegments.Length} Positions removed {removalPositions.Length}");
            //}

            //segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            //shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            var mixedPositions = GetMixedRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
            Console.WriteLine($"Mixed short positions {mixedPositions.Length}");
            //TableDisplays.ShowCountSpread("Remaining short positions", shortSegments, s => );


            Console.WriteLine($"Remaining short segments {shortSegments.Length}");
            var groups = shortSegments.GroupBy(s => s.Cardinality, new Combination2Comparer()).ToArray();
            foreach (var group in groups.OrderBy(g => g.Key.A))
            {
                Console.WriteLine($"{group.Key} {group.Count()}");
            }

            mesh.ShowSegmentLengths(ConsoleColor.Green);

            ConsoleLog.WriteLine($"Remove short segments: Elapsed time {(DateTime.Now - start).TotalSeconds.ToString("#,##0.00")} seconds.\n");
        }

        //public static int RemoveTagTriangles(this IWireFrameMesh mesh)
        //{
        //    var tagTriangles = mesh.Triangles.Where(t => t.AdjacentAnyCount <= 1).ToArray();
        //    var removed = 0;
        //    foreach(var triangle in tagTriangles) { removed += mesh.RemoveTriangle(triangle) ? 1 : 0; }
        //    return removed;
        //}

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
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(2, 3)))
            {
                if (segment.B.PositionObject.Cardinality == 2) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetSurfaceRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(1, 1)))
            {
                if (segment.A.Triangles.Count() > segment.B.Triangles.Count()) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetMixedRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments.Where(s => s.Cardinality == new Combination2(1, 2)))
            {
                if (segment.A.PositionObject.Cardinality > segment.B.PositionObject.Cardinality) { yield return segment.B.PositionObject; continue; }

                if (segment.B.PositionObject.Cardinality > segment.A.PositionObject.Cardinality) { yield return segment.A.PositionObject; continue; }
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

        public static void RemovePosition(this IWireFrameMesh mesh, Position position)
        {
            mesh.RemovePosition(position, null);
        }
        public static void RemovePosition(this IWireFrameMesh mesh, Position position, IFillConditionals<PositionNormal> fillConditionals)
        {
            var trianglesToRemove = new List<PositionTriangle>();
            var fillingsToAdd = new List<PositionNormal[]>();

            foreach (var positionNormal in position.PositionNormals)
            {
                var triangles = positionNormal.Triangles.ToArray();
                var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);

                var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillConditionals, position.Id);

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

        public static void RemovePositions(this IWireFrameMesh mesh, IEnumerable<Position> positions, IFillConditionals<PositionNormal> fillConditionals)
        {
            GetMarkedPositions(positions.ToArray(), out List<Position> markedPositions, out List<Position> unmarkedPositions);

            while (markedPositions.Count > 0)
            {
                var trianglesToRemove = new List<PositionTriangle>();
                var fillingsToAdd = new List<PositionNormal[]>();

                foreach (var position in markedPositions)
                {
                    foreach (var positionNormal in position.PositionNormals)
                    {
                        var triangles = positionNormal.Triangles.ToArray();
                        var segmentSet = CreateSurfaceSegmentSet(positionNormal, triangles);
                        var collection = new SurfaceSegmentCollections<PlanarFillingGroup, PositionNormal>(segmentSet);
                        var chain = SurfaceSegmentChaining<PlanarFillingGroup, PositionNormal>.Create(collection);
 
                        try
                        {                            
                            var planarFilling = new PlanarFilling<PlanarFillingGroup, PositionNormal>(chain, fillConditionals, position.Id);
                            var fillings = planarFilling.Fillings.Select(f => new PositionNormal[] { f.A.Reference, f.B.Reference, f.C.Reference });

                            var fillingIndicies = fillings.Select(f => new int[] { f[0].PositionObject.Id, f[1].PositionObject.Id, f[2].PositionObject.Id });

                            fillingsToAdd.AddRange(fillings);
                            trianglesToRemove.AddRange(triangles);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e.Message} Position {position.Id}  Chain length {chain.PerimeterLoops[0].Length} Edges {chain.PerimeterLoops[0].Count(p => p.Reference.PositionObject.Cardinality > 1)}", ConsoleColor.Red);
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

        //private static bool fillingIsAllowed(PositionNormal a, PositionNormal b, PositionNormal c)
        //{
        //    //return a.PositionObject.Cardinality != 2 || b.PositionObject.Cardinality != 2 || c.PositionObject.Cardinality != 2;
        //    //return true;
        //    return a.PositionObject.Cardinality < 2 || b.PositionObject.Cardinality < 2 || c.PositionObject.Cardinality < 2;
        //}

        //private static bool fillingIsAllowed2(PositionNormal a, PositionNormal b, PositionNormal c)
        //{
        //    //return true;
        //    //return a.PositionObject.Cardinality != 2 || b.PositionObject.Cardinality != 2 || c.PositionObject.Cardinality != 2;
        //    return (a.PositionObject.Cardinality < 3 || b.PositionObject.Cardinality < 3 || c.PositionObject.Cardinality < 3) && (a.PositionObject.Cardinality != 2 || b.PositionObject.Cardinality != 2 || c.PositionObject.Cardinality != 2);
        //}

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
