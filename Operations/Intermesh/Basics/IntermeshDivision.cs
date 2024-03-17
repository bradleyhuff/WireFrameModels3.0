using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshDivision
    {
        private static int _id = 0;
        public IntermeshDivision()
        {
            Id = _id++;
        }

        public int Id { get; }
        public LineSegment3D? Division { get; set; }

        public IntermeshIntersection RootIntersection { get; set; }
        public List<IntermeshIntersection> IntersectionA { get; set; } = new List<IntermeshIntersection>();
        public List<IntermeshIntersection> IntersectionB { get; set; } = new List<IntermeshIntersection>();
        public Point3D Point { get; set; }
        public IntermeshIntersection PointIntersection { get; set; }

        private DivisionVertexContainer _vertexA;
        private DivisionVertexContainer _vertexB;

        internal DivisionVertexContainer VertexA
        {
            get
            {
                if (_vertexA is null) { _vertexA = new DivisionVertexContainer(this, 'a'); }
                return _vertexA;
            }
        }
        internal DivisionVertexContainer VertexB
        {
            get
            {
                if (_vertexB is null) { _vertexB = new DivisionVertexContainer(this, 'b'); }
                return _vertexB;
            }
        }

        internal IEnumerable<DivisionVertexContainer> VerticiesAB
        {
            get
            {
                yield return VertexA;
                yield return VertexB;
            }
        }

        public double Length
        {
            get { return Point3D.Distance(VertexA.Point, VertexB.Point); }
        }

        public bool Disabled { get; set; }
    }
}
