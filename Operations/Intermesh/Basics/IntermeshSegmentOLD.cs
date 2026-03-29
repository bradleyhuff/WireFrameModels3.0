using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshSegmentOLD : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshSegmentOLD(IntermeshPointOLD start, IntermeshPointOLD end)
        {
            Start = start;
            End = end;
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(start.Id, end.Id);
            _divisionPoints.Add(Start);
            _divisionPoints.Add(End);
            _orderedInternalPoints = null;
            Segment = new LineSegment3D(start.Point, end.Point);
            if (Id == 17009)
            {

            }
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public IntermeshPointOLD Start { get; }
        public IntermeshPointOLD End { get; }
        public LineSegment3D Segment { get; }

        private List<IntermeshPointOLD> _divisionPoints = new List<IntermeshPointOLD>();

        public IReadOnlyList<IntermeshPointOLD> DivisionPoints
        {
            get { return _divisionPoints; }
        }

        public IEnumerable<IntermeshPointOLD> Points
        {
            get { yield return Start; yield return End; }
        }

        public bool HasInternalDivisionPoints { get { return InternalDivisions > 0; } }
        public int InternalDivisions { get { return _divisionPoints.Count() - 2; } }

        private List<IntermeshPointOLD> _orderedInternalPoints = null;
        public IReadOnlyList<IntermeshPointOLD> InternalDivisionPoints
        {
            get
            {
                if (_orderedInternalPoints is null)
                {
                    _orderedInternalPoints = _divisionPoints.OrderBy(d => Point3D.Distance(Start.Point, d.Point)).Where(p => p.Id != Start.Id && p.Id != End.Id).ToList();
                }
                return _orderedInternalPoints;
            }
        }
        public bool Add(IntermeshPointOLD division)
        {
            if (Point3D.Distance(Start.Point, division.Point) > Segment.Length) { return false; }
            if (Point3D.Distance(End.Point, division.Point) > Segment.Length) { return false; }
            if (_divisionPoints.Any(t => t.Id == division.Id)) { return false; }
            _divisionPoints.Add(division);
            _orderedInternalPoints = null;
            return true;
        }

        private List<IntermeshSegmentOLD> _bases = new List<IntermeshSegmentOLD>();

        internal IReadOnlyList<IntermeshSegmentOLD> Bases
        {
            get { return _bases; }
        }

        internal bool Add(IntermeshSegmentOLD base_)
        {
            if (base_.Id == Id) { return false; }
            if (_bases.Any(b => b.Id == base_.Id)) { return false; }
            _bases.Add(base_);
            return true;
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && Start is not null && End is not null)
                {
                    _box = Rectangle3D.Containing(Start.Point, End.Point).Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public override string ToString()
        {
            return $"Intermesh Segment {Key}";
        }

        private List<IntermeshTriangleOLD> _triangles = new List<IntermeshTriangleOLD>();
        public IReadOnlyList<IntermeshTriangleOLD> Triangles
        {
            get { return _triangles; }
        }
        public bool Add(IntermeshTriangleOLD triangle)
        {
            if (_triangles.Any(t => t.Id == triangle.Id)) { return false; }
            _triangles.Add(triangle);
            return true;
        }

        public IEnumerable<IntermeshDivisionOLD> BuildDivisions(Combination2Dictionary<IntermeshDivisionOLD> table)
        {
            var orderedPoints = _divisionPoints.OrderBy(p => Point3D.Distance(p.Point, Start.Point)).ToArray();

            for (int i = 0; i < orderedPoints.Length - 1; i++)
            {
                var pointA = orderedPoints[i];
                var pointB = orderedPoints[i + 1];
                var key = new Combination2(pointA.Id, pointB.Id);
                if (!table.ContainsKey(key)) { table[key] = new IntermeshDivisionOLD(pointA, pointB); }
                var division = table[key];
                division.Add(this);

                orderedPoints[i].Add(division);
                orderedPoints[i + 1].Add(division);

                yield return division;
            }
        }
    }
}
