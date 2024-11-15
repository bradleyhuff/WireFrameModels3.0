using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;

namespace Operations.Regions
{
    internal class Space
    {
        private class Triangle3DNodeX : IBox
        {
            public Triangle3DNodeX(ITriangle triangle)
            {
                Triangle = triangle;
                Box = new Rectangle3D(0, 0,
                    triangle.Triangle.Box.MinPoint.Y, triangle.Triangle.Box.MaxPoint.Y,
                    triangle.Triangle.Box.MinPoint.Z, triangle.Triangle.Box.MaxPoint.Z).Margin(BoxBucket.MARGINS);
            }

            public ITriangle Triangle { get; }
            public Rectangle3D Box { get; }
        }

        private class Triangle3DNodeY : IBox
        {
            public Triangle3DNodeY(ITriangle triangle)
            {
                Triangle = triangle;
                Box = new Rectangle3D(triangle.Triangle.Box.MinPoint.X, triangle.Triangle.Box.MaxPoint.X,
                    0, 0,
                    triangle.Triangle.Box.MinPoint.Z, triangle.Triangle.Box.MaxPoint.Z).Margin(BoxBucket.MARGINS);
            }

            public ITriangle Triangle { get; }
            public Rectangle3D Box { get; }
        }

        private class Triangle3DNodeZ : IBox
        {
            public Triangle3DNodeZ(ITriangle triangle)
            {
                Triangle = triangle;
                Box = new Rectangle3D(triangle.Triangle.Box.MinPoint.X, triangle.Triangle.Box.MaxPoint.X,
                    triangle.Triangle.Box.MinPoint.Y, triangle.Triangle.Box.MaxPoint.Y,
                    0, 0).Margin(BoxBucket.MARGINS);
            }

            public ITriangle Triangle { get; }
            public Rectangle3D Box { get; }
        }

        private class Point3DNodeX : IBox
        {
            public Point3DNodeX(Point3D point)
            {
                _box = new Rectangle3D(0, 0, point.Y, point.Y, point.Z, point.Z);
            }

            private Rectangle3D _box;
            public Rectangle3D Box { get { return _box; } }
        }

        private class Point3DNodeY : IBox
        {
            public Point3DNodeY(Point3D point)
            {
                _box = new Rectangle3D(point.X, point.X, 0, 0, point.Z, point.Z);
            }

            private Rectangle3D _box;
            public Rectangle3D Box { get { return _box; } }
        }

        private class Point3DNodeZ : IBox
        {
            public Point3DNodeZ(Point3D point)
            {
                _box = new Rectangle3D(point.X, point.X, point.Y, point.Y, 0, 0);
            }

            private Rectangle3D _box;
            public Rectangle3D Box { get { return _box; } }
        }

        private class TriangleNode : ITriangle
        {
            public TriangleNode(Triangle3D triangle, int tag) { Triangle = triangle; Tag = tag; }
            public Triangle3D Triangle { get; }
            public int Tag { get; }
        }

        private class Triangle3DNodeBucketX : BoxBucket<Triangle3DNodeX>
        {
            public Triangle3DNodeBucketX(IEnumerable<ITriangle> triangles) : base(triangles.Select(t => new Triangle3DNodeX(t)).ToArray()) { }
            public IEnumerable<ITriangle> Fetch(Point3DNodeX node) { return Fetch(node.Box).Select(t => t.Triangle); }
        }

        private class Triangle3DNodeBucketY : BoxBucket<Triangle3DNodeY>
        {
            public Triangle3DNodeBucketY(IEnumerable<ITriangle> triangles) : base(triangles.Select(t => new Triangle3DNodeY(t)).ToArray()) { }
            public IEnumerable<ITriangle> Fetch(Point3DNodeY node) { return Fetch(node.Box).Select(t => t.Triangle); }
        }

        private class Triangle3DNodeBucketZ : BoxBucket<Triangle3DNodeZ>
        {
            public Triangle3DNodeBucketZ(IEnumerable<ITriangle> triangles) : base(triangles.Select(t => new Triangle3DNodeZ(t)).ToArray()) { }
            public IEnumerable<ITriangle> Fetch(Point3DNodeZ node) { return Fetch(node.Box).Select(t => t.Triangle); }
        }

        private Triangle3DNodeBucketX _bucketX;
        private Triangle3DNodeBucketY _bucketY;
        private Triangle3DNodeBucketZ _bucketZ;

        public Space(IEnumerable<Triangle3D> triangles)
        {
            _bucketX = new Triangle3DNodeBucketX(triangles.Select(t => new TriangleNode(t, 0)));
            _bucketY = new Triangle3DNodeBucketY(triangles.Select(t => new TriangleNode(t, 0)));
            _bucketZ = new Triangle3DNodeBucketZ(triangles.Select(t => new TriangleNode(t, 0)));
        }

        public Space(IEnumerable<ITriangle> triangles)
        {
            _bucketX = new Triangle3DNodeBucketX(triangles);
            _bucketY = new Triangle3DNodeBucketY(triangles);
            _bucketZ = new Triangle3DNodeBucketZ(triangles);
        }

        public Region RegionOfPoint(Point3D point)
        {
            return RegionOfPoint(point, t => t);
        }

        public Region RegionOfPoint(Point3D point, int tag)
        {
            return RegionOfPoint(point, t => t.Where(t => t.Tag == tag));
        }

        public Region RegionOfPoint(Point3D point, Func<IEnumerable<ITriangle>, IEnumerable<ITriangle>> filter)
        {
            var lineX = new Line3D(point, Vector3D.BasisX);
            var lineY = new Line3D(point, Vector3D.BasisY);
            var lineZ = new Line3D(point, Vector3D.BasisZ);

            int voteForInterior = 0;
            int voteForExterior = 0;

            {
                var matches = filter(_bucketX.Fetch(new Point3DNodeX(point)));

                var intersections = GetIntersections(lineX, matches.Select(m => m.Triangle)).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }
            {
                var matches = filter(_bucketY.Fetch(new Point3DNodeY(point)));

                var intersections = GetIntersections(lineY, matches.Select(m => m.Triangle)).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }
            if (voteForInterior >= 2) { return Region.Interior; }
            if (voteForExterior >= 2) { return Region.Exterior; }
            {
                var matches = filter(_bucketZ.Fetch(new Point3DNodeZ(point)));

                var intersections = GetIntersections(lineZ, matches.Select(m => m.Triangle)).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }

            if (voteForInterior >= 2) { return Region.Interior; }
            if (voteForExterior >= 2) { return Region.Exterior; }

            return Region.Indeterminant;
        }

        private IEnumerable<Point3D?> GetIntersections(Line3D line, IEnumerable<Triangle3D> matches)
        {
            return matches.Select(m => new { intersection = m.Plane.Intersection(line), lineIsOnPlane = m.Plane.LineIsOnPlane(line), triangle = m }).
                Where(p => p.lineIsOnPlane || p.triangle.PointIsOn(p.intersection)).Select(p => p.lineIsOnPlane ? null : p.intersection).DistinctBy(x => x);
        }
    }
}
