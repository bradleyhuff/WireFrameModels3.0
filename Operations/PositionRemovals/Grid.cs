using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining;
using Operations.SurfaceSegmentChaining.Collections;
using Console = BaseObjects.Console;

namespace Operations.PositionRemovals
{
    public static class Grid
    {
        public static void RemoveCoplanarSurfacePoints(this IWireFrameMesh mesh)
        {
            var positions = mesh.Positions;
            var surfacePositions = positions.Where(p => p.Cardinality == 1).ToArray();
            Console.WriteLine($"Candidate surface positions {surfacePositions.Length}");
            var qualifiedPositions = surfacePositions.Where(IsCoplanar).ToArray();
            Console.WriteLine($"Qualified positions {qualifiedPositions.Length}");
            foreach (var position in qualifiedPositions)
            {
                mesh.RemovePosition(position);
            }

        }

        private static bool IsCoplanar(Position pp)
        {
            var plane = new Plane(pp.PositionNormals[0].Position, pp.PositionNormals[0].Normal.Direction);
            var points = pp.PositionNormals[0].Triangles.SelectMany(p => p.Positions).DistinctBy(p => p.Id).Select(p => p.Position).ToArray();
            return plane.AllPointsOnPlane(points);
        }

        public static void RemoveCollinearEdgePoints(this IWireFrameMesh mesh)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var edgeSegments = segments.Where(s => s.A.PositionObject.Cardinality > 1 && s.B.PositionObject.Cardinality > 1).ToArray();
            var edgePositions = edgeSegments.SelectMany(e => e.Positions).Select(p => p.PositionObject).Where(p => p.Cardinality == 2).DistinctBy(p => p.Id).ToArray();
            Console.WriteLine($"Candidate edge positions {edgePositions.Length}");
            var positionClusters = edgePositions.Select(GetCluster).ToArray();
            var qualifiedClusters = positionClusters.Where(c => c.Cluster.Length == 2 && IsCollinear(c)).ToArray();
            Console.WriteLine($"Qualified clusters {qualifiedClusters.Length}");
            foreach (var cluster in qualifiedClusters)
            {
                mesh.RemovePosition(cluster.Position);
            }
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
            return line.PointIsOnLine(cluster.Position.Point, 1e-6);
        }

        private class EdgeCluster
        {
            public Position Position { get; set; }
            public Position[] Cluster { get; set; }
        }



        public static void RemoveShortSegments(this IWireFrameMesh mesh, double minimumLength)
        {
            var segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            var shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
            
            {
                var groups = segments.GroupBy(p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p.A.Position, p.B.Position)))).OrderBy(g => g.Key).ToArray();
                Console.WriteLine();
                foreach (var group in groups)
                {
                    Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}", ConsoleColor.Red);
                }
                Console.WriteLine();
            }

            Console.WriteLine($"Short segments {shortSegments.Length}");

            int lastLength = -1;
            while (shortSegments.Length > 0 && lastLength != shortSegments.Length)
            {
                lastLength = shortSegments.Length;
                var removalPositions = GetRemovalPositions(shortSegments).DistinctBy(p => p.Id).ToArray();
                var markedPositions = GetMarkedPositions(removalPositions).ToArray();

                foreach (var position in markedPositions)
                {
                    mesh.RemovePosition(position);
                }

                segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
                shortSegments = segments.Where(s => s.Segment.Length < minimumLength).ToArray();
                Console.WriteLine($"Short segments {shortSegments.Length}");
            }

            segments = mesh.Triangles.SelectMany(t => t.Edges).DistinctBy(s => s.Key, new Combination2Comparer()).ToArray();
            {
                var groups = segments.GroupBy(p => (int)Math.Floor(3 * Math.Log10(Point3D.Distance(p.A.Position, p.B.Position)))).OrderBy(g => g.Key).ToArray();
                Console.WriteLine();
                foreach (var group in groups)
                {
                    Console.WriteLine($"{Math.Pow(10, group.Key / 3.0).ToString("E2")}  {group.Count()}", ConsoleColor.Green);
                }
                Console.WriteLine();
            }
        }

        private static IEnumerable<Position> GetRemovalPositions(PositionEdge[] segments)
        {
            foreach (var segment in segments)
            {
                var positions = segment.Positions.Select(p => p.PositionObject).Where(p => p.Cardinality < 3).ToArray();
                if (positions.Length == 0) { continue; }
                if (positions.Length == 1) { yield return positions[0]; continue; }

                if (segment.A.Triangles.Count() > segment.B.Triangles.Count()) { yield return segment.B.PositionObject; continue; }
                yield return segment.A.PositionObject;
            }
        }

        private static IEnumerable<Position> GetMarkedPositions(Position[] positions)
        {
            if (positions.Length == 0) { yield break; }
            positions[0].PositionNormals[0].Triangles[0].GridClearMarks();

            foreach (var position in positions)
            {
                if (position.Triangles.Any(t => t.IsMarked)) { continue; }
                foreach (var triangle in position.Triangles) { triangle.Mark(); }
                yield return position;
            }
        }


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

            foreach (var filling in fillingsToAdd)
            {
                mesh.AddTriangle(filling[0], filling[1], filling[2]);
            }
            mesh.RemoveAllTriangles(trianglesToRemove);
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
