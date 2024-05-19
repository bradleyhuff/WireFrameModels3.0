using BasicObjects.GeometricObjects;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class CollinearGroup<T>
    {
        private List<PlanarSegment<T>> _collinears = new List<PlanarSegment<T>>();
        private List<Point3D> _links = new List<Point3D>();
        private Point3D _start;
        private Point3D _end;
        private Plane _plane;

        public CollinearGroup(Plane plane)
        {
            _plane = plane;
        }

        public bool HasElements
        {
            get { return _collinears.Any(); }
        }

        public LineSegment3D Segment
        {
            get
            {
                if (_collinears.Any())
                {
                    return new LineSegment3D(_start, _end);
                }
                return null;
            }
        }

        public bool AddCollinear(PlanarSegment<T> collinear)
        {
            if (!_collinears.Any()) { _collinears.Add(collinear); _start = collinear.Segment.Start; _end = collinear.Segment.End; return true; }

            if (_start == collinear.Segment.Start)
            {
                _collinears.Add(collinear);
                _start = collinear.Segment.End;
                return true;
            }

            if (_start == collinear.Segment.End)
            {
                _collinears.Add(collinear);
                _start = collinear.Segment.Start;
                return true;
            }

            if (_end == collinear.Segment.Start)
            {
                _collinears.Add(collinear);
                _end = collinear.Segment.End;
                return true;
            }

            if (_end == collinear.Segment.End)
            {
                _collinears.Add(collinear);
                _end = collinear.Segment.Start;
                return true;
            }

            return false;
        }

        public bool AddLink(PlanarSegment<T> link)
        {
            if (_start == link.Segment.Start) { _links.Add(link.Segment.End); return true; }
            if (_start == link.Segment.End) { _links.Add(link.Segment.Start); return true; }
            if (_end == link.Segment.Start) { _links.Add(link.Segment.End); return true; }
            if (_end == link.Segment.End) { _links.Add(link.Segment.Start); return true; }
            return false;
        }

        public bool IsApplicable()
        {
            if (!_collinears.Any()) { return false; }
            var collinear = _collinears.First();
            var ray = new Ray3D(collinear.Segment.Start, collinear.Segment.Vector);
            var projectionLine = _plane.GetPerpendicular(ray);
            if (projectionLine is null) { return false; }

            var linkDirection1 = (projectionLine.Projection(_links[0]) - collinear.Segment.Start).Direction;
            var linkDirection2 = (projectionLine.Projection(_links[1]) - collinear.Segment.Start).Direction;
            return Vector3D.Dot(linkDirection1, linkDirection2) < 0;
        }

        public Point3D GetApplicablePoint()
        {
            return IsApplicable() ? _collinears.First().Segment.Start : null;
        }
    }
}
