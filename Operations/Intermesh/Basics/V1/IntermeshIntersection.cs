using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Basics.V1
{
    internal class IntermeshIntersection : IBox
    {
        private static int _id = 0;

        public IntermeshIntersection()
        {
            Id = _id++;
        }
        public int Id { get; }

        public void Disable()
        {
            Disabled = true;
        }
        public bool Disabled { get; private set; }

        private LineSegment3D _intersection;
        public LineSegment3D Intersection
        {
            get { return _intersection; }
            set { _intersection = value; }
        }

        public IntermeshTriangle IntersectorA { get; set; }
        public IntermeshTriangle IntersectorB { get; set; }

        public IEnumerable<IntermeshTriangle> Intersectors
        {
            get
            {
                if (IntersectorA is not null) yield return IntersectorA;
                if (IntersectorB is not null) yield return IntersectorB;
            }
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(Intersection).Margin(1e-3);
                }
                return _box;
            }
        }

        private List<IntermeshDivision> _divisions = new List<IntermeshDivision>();
        public IReadOnlyList<IntermeshDivision> Divisions
        {
            get { return _divisions; }
        }

        public bool DivisionIsSet { get; set; }

        public void AddDivisions(IEnumerable<IntermeshDivision> newDivisions)
        {
            _divisions.AddRange(newDivisions);
            _divisions = _divisions.DistinctBy(d => d.Id).ToList();
            if (_divisions.Count > 1)
            {
                var firstPoint = _divisions[0].VertexA.Point;
                var parityDirection = Intersection.Vector;
                _divisions = _divisions.
                    Select(d => new
                    {
                        Parity = Math.Sign(Vector3D.Dot(d.VertexA.Point - firstPoint, parityDirection)),
                        Distance = Point3D.Distance(firstPoint, d.VertexA.Point),
                        Node = d
                    }).
                    OrderBy(d => d.Parity * d.Distance).Select(d => d.Node).ToList();
            }
        }

        private IntersectionVertexContainer _vertexA;
        private IntersectionVertexContainer _vertexB;

        internal IntersectionVertexContainer VertexA
        {
            get
            {
                if (_vertexA is null) { _vertexA = new IntersectionVertexContainer(this, 'a'); }
                return _vertexA;
            }
        }
        internal IntersectionVertexContainer VertexB
        {
            get
            {
                if (_vertexB is null) { _vertexB = new IntersectionVertexContainer(this, 'b'); }
                return _vertexB;
            }
        }
        internal IEnumerable<IntersectionVertexContainer> VerticiesAB
        {
            get
            {
                yield return VertexA;
                yield return VertexB;
            }
        }

        internal LineSegment3D LinkedIntersection
        {
            get { return new LineSegment3D(VertexA.Point, VertexB.Point); }
        }

        private List<IntermeshIntersection> _multiples;

        public void AddMultiple(IntermeshIntersection multiple)
        {
            if (Id == multiple.Id) { return; }
            if (_multiples is null && multiple._multiples is null)
            {
                _multiples = new List<IntermeshIntersection>();
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

        public IEnumerable<IntermeshIntersection> Multiples
        {
            get
            {
                return _multiples?.Where(m => m.Id != Id) ?? Enumerable.Empty<IntermeshIntersection>();
            }
        }

        public void ClearDisabledDivisions()
        {
            _divisions = _divisions.Where(x => !x.Disabled).ToList();
        }
    }
}
