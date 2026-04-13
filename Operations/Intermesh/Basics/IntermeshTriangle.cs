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
        public Combination3 Key
        {
            get
            {
                return new Combination3(A.Id, B.Id, C.Id);
            }
        }
        public PositionTriangle PositionTriangle { get { return _triangle; } }

        public IntermeshPoint A { get; set; }
        public IntermeshPoint B { get; set; }
        public IntermeshPoint C { get; set; }

        public Rectangle3D Box
        {
            get
            {
                return _triangle.Box;
            }
        }

        public IIntermeshEdge AB { get; set; }
        public IIntermeshEdge BC { get; set; }
        public IIntermeshEdge CA { get; set; }

        public void SwitchEdges()
        {
            AB = AB.Switch();
            BC = BC.Switch();
            CA = CA.Switch();
            for (int i = 0; i < _intersectionSegments.Count; i++)
            {
                _intersectionSegments[i] = _intersectionSegments[i].Switch();
            }

            var group = AB.Key.Indicies.Concat(BC.Key.Indicies).Concat(CA.Key.Indicies).GroupBy(i => i);
            if (group.Count() != 3)
            {
                throw new InvalidDataException($"Vertex assigning conflict in triangle id {Id}");
            }
        }

        public IEnumerable<IntermeshPoint> Verticies
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
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

        public IEnumerable<IIntermeshEdge> PerimeterEdges
        {
            get
            {
                if (AB is not null) { yield return AB; }
                if (BC is not null) { yield return BC; }
                if (CA is not null) { yield return CA; }
            }
        }

        private List<IIntermeshEdge> _intersectionSegments = new List<IIntermeshEdge>();

        public bool AddIntersection(IntermeshSegment intersection)
        {
            if (_intersectionSegments.Any(t => t.Key == intersection.Key)) { return false; }
            _intersectionSegments.Add(intersection);
            return true;
        }

        public IEnumerable<IIntermeshEdge> IntersectionSegments
        {
            get
            {
                return _intersectionSegments;
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
