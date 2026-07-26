using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Operations.Intermesh.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshTriangle : IBox
    {
        private static int _id = 0;
        private PositionTriangle _triangle;
        private static object lockObject = new object();
        internal IntermeshTriangle(PositionTriangle triangle)
        {
            _triangle = triangle;
            lock (lockObject)
            {
                Id = _id++;
            }
        }

        public int Id { get; }
        public PositionTriangle PositionTriangle { get { return _triangle; } }

        public Rectangle3D Box
        {
            get
            {
                return _triangle.Box;
            }
        }

        public IntermeshEdgeSlot AB { get; set; }
        public IntermeshEdgeSlot BC { get; set; }
        public IntermeshEdgeSlot CA { get; set; }

        public Combination3 Key
        {
            get
            {
                var points = PerimeterSlots.SelectMany(p => p.EndPoints).DistinctBy(p => p.Id).ToArray();
                return new Combination3(points[0].Id, points[1].Id, points[2].Id);
            }
        }

        public IEnumerable<IntermeshSegment> PerimeterSegments
        {
            get
            {
                if (AB is not null) { foreach (var segment in AB.Segments) { yield return segment; } }
                if (BC is not null) { foreach (var segment in BC.Segments) { yield return segment; } }
                if (CA is not null) { foreach (var segment in CA.Segments) { yield return segment; } }
            }
        }

        public IEnumerable<IntermeshEdgeSlot> PerimeterSlots
        {
            get
            {
                if (AB is not null) { yield return AB; }
                if (BC is not null) { yield return BC; }
                if (CA is not null) { yield return CA; }
            }
        }

        private List<IntermeshEdgeSlot> _intersectionSlots = new List<IntermeshEdgeSlot>();

        public void RemoveIntersectionSlot(IntermeshEdgeSlot intersection)
        {
            _intersectionSlots.Remove(intersection);
        }

        public void RemoveIntersectionSlots(IEnumerable<IntermeshEdgeSlot> intersections)
        {
            foreach (var intersection in intersections)
            {
                RemoveIntersectionSlot(intersection);
            }
        }

        public bool AddIntersectionSlot(IntermeshEdgeSlot intersection)
        {
            if (_intersectionSlots.Any(t => t.Id == intersection.Id)) { return false; }
            _intersectionSlots.Add(intersection);
            return true;
        }

        public IEnumerable<IntermeshEdgeSlot> IntersectionSlots
        {
            get
            {
                return _intersectionSlots;
            }
        }

        public IEnumerable<IntermeshEdgeSlot> EdgeSlots
        {
            get
            {
                foreach (var slot in PerimeterSlots)
                {
                    yield return slot;
                }
                foreach (var slot in IntersectionSlots)
                {
                    yield return slot;
                }
            }
        }

        public IEnumerable<IntermeshSegment> IntersectionSegments
        {
            get
            {
                return _intersectionSlots.SelectMany(i => i.Segments);
            }
        }

        public IEnumerable<IntermeshSegment> Segments
        {
            get
            {
                foreach (var segment in PerimeterSegments)
                {
                    yield return segment;
                }
                foreach (var intersection in IntersectionSegments)
                {
                    yield return intersection;
                }
            }
        }

        public Triangle3D Triangle
        {
            get { return _triangle.Triangle; }
        }

        public List<IntermeshTriangle> Gathering { get; } = new List<IntermeshTriangle>();
        public List<IntermeshTriangle> IntersectingTriangles { get; } = new List<IntermeshTriangle>();
        public Dictionary<int, IntermeshIntersection> GatheringSets { get; } = new Dictionary<int, IntermeshIntersection>();

        public List<FillTriangle> Fillings { get; set; } = new List<FillTriangle>();
        public Vector3D NormalFromProjectedPoint(Point3D point)
        {
            var projection = Triangle.Plane.Projection(point);

            var c = Triangle.GetBarycentricCoordinate(projection);

            return (c.λ1 * PositionTriangle.A.Normal.Direction + c.λ2 * PositionTriangle.B.Normal.Direction + c.λ3 * PositionTriangle.C.Normal.Direction).Direction;
        }

        public Ray3D RayFromProjectedPoint(Point3D point)
        {
            var projection = Triangle.Plane.Projection(point);
            if (projection.IsNaN) { projection = point; }

            var c = Triangle.GetBarycentricCoordinate(projection);
            var normal = (c.λ1 * PositionTriangle.A.Normal.Direction + c.λ2 * PositionTriangle.B.Normal.Direction + c.λ3 * PositionTriangle.C.Normal.Direction).Direction;

            return new Ray3D(projection, normal);
        }

        public override string ToString()
        {
            return $"Intermesh Triangle Id: {Id}";
        }
    }
}
