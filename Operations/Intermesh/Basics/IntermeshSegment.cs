using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshSegment : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshSegment(IntermeshPoint start, IntermeshPoint end)
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
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public IntermeshPoint Start { get; }
        public IntermeshPoint End { get; }
        public LineSegment3D Segment { get; }

        private List<IntermeshPoint> _divisionPoints = new List<IntermeshPoint>();

        public IReadOnlyList<IntermeshPoint> DivisionPoints
        {
            get { return _divisionPoints; }
        }

        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return Start; yield return End; }
        }

        public bool HasInternalDivisionPoints { get { return InternalDivisions > 0; } }
        public int InternalDivisions { get { return _divisionPoints.Count() - 2; } }

        private List<IntermeshPoint> _orderedInternalPoints = null;
        public IReadOnlyList<IntermeshPoint> InternalDivisionPoints
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
        public bool Add(IntermeshPoint division)
        {
            if (Point3D.Distance(Start.Point, division.Point) > Segment.Length) { return false; }
            if (Point3D.Distance(End.Point, division.Point) > Segment.Length) { return false; }
            if (_divisionPoints.Any(t => t.Id == division.Id)) { return false; }
            _divisionPoints.Add(division);
            _orderedInternalPoints = null;
            return true;
        }

        private List<IntermeshSegment> _bases = new List<IntermeshSegment>();

        internal IReadOnlyList<IntermeshSegment> Bases
        {
            get { return _bases; }
        }

        internal bool Add(IntermeshSegment base_)
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

        private List<IntermeshTriangle> _triangles = new List<IntermeshTriangle>();
        public IReadOnlyList<IntermeshTriangle> Triangles
        {
            get { return _triangles; }
        }
        public bool Add(IntermeshTriangle triangle)
        {
            if (_triangles.Any(t => t.Id == triangle.Id)) { return false; }
            _triangles.Add(triangle);
            return true;
        }

        public IEnumerable<IntermeshDivision> BuildDivisions(Combination2Dictionary<IntermeshDivision> table)
        {
            var orderedPoints = _divisionPoints.OrderBy(p => Point3D.Distance(p.Point, Start.Point)).ToArray();

            for (int i = 0; i < orderedPoints.Length - 1; i++)
            {
                var pointA = orderedPoints[i];
                var pointB = orderedPoints[i + 1];
                var key = new Combination2(pointA.Id, pointB.Id);
                if (!table.ContainsKey(key)) { table[key] = new IntermeshDivision(pointA, pointB); }
                var division = table[key];
                division.Add(this);

                orderedPoints[i].Add(division);
                orderedPoints[i + 1].Add(division);

                yield return division;
            }
        }
    }
}
