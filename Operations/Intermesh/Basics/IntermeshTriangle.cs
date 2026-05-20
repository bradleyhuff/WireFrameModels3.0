using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Operations.Intermesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshTriangle : IIntermeshTriangle
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

        private List<IntermeshEdgeSlot> _intersectionSegments = new List<IntermeshEdgeSlot>();

        public bool AddIntersection(IntermeshEdgeSlot intersection)
        {
            if (_intersectionSegments.Any(t => t.Id == intersection.Id)) { return false; }
            _intersectionSegments.Add(intersection);
            return true;
        }

        public IEnumerable<IntermeshEdgeSlot> IntersectionSlots
        {
            get
            {
                return _intersectionSegments;
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
                return _intersectionSegments.SelectMany(i => i.Segments);
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

        public List<IIntermeshTriangle> Gathering { get; } = new List<IIntermeshTriangle>();
        public List<IIntermeshTriangle> IntersectingTriangles { get; } = new List<IIntermeshTriangle>();
        public Dictionary<int, IntermeshIntersection> GatheringSets { get; } = new Dictionary<int, IntermeshIntersection>();
    }
}
