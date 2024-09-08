using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Microsoft.VisualBasic;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshDivision : IBox
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

        internal LineSegment3D LinkedDivision
        {
            get { return new LineSegment3D(VertexA.Point, VertexB.Point); }
        }

        internal IEnumerable<DivisionVertexContainer> VerticiesAB
        {
            get
            {
                yield return VertexA;
                yield return VertexB;
            }
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(LinkedDivision).Margin(1e-3);
                }
                return _box;
            }
        }

        public double Length
        {
            get { return Point3D.Distance(VertexA.Point, VertexB.Point); }
        }

        private List<IntermeshDivision> _multiples;

        public void AddMultiple(IntermeshDivision multiple)
        {
            if (Id == multiple.Id) { return; }
            if (_multiples is null && multiple._multiples is null)
            {
                _multiples = new List<IntermeshDivision>();
                multiple._multiples = _multiples;
            }
            else if (_multiples is null)
            {
                _multiples = multiple._multiples;
            }
            else if (multiple._multiples is null)
            {
                multiple._multiples = _multiples;
            }

            if (_multiples.Any(m => m.Id == multiple.Id)) { return; }

            _multiples.Add(multiple);
        }

        public IEnumerable<IntermeshDivision> Multiples
        {
            get
            {
                return _multiples?.Where(m => m.Id != Id) ?? Enumerable.Empty<IntermeshDivision>();
            }
        }

        public void Disable()
        {
            Disabled = true;
        }
        public bool Disabled { get; private set; }
    }
}
