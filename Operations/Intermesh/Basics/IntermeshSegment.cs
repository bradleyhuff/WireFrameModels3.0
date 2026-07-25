using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Operations.Intermesh.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshSegment : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();

        private List<IntermeshCapsule> _capsules = new List<IntermeshCapsule>();
        private List<IntermeshCapsule[]> _previous = new List<IntermeshCapsule[]>();

        public IntermeshSegment(IntermeshPoint a, IntermeshPoint b) : this(IntermeshCapsuleExtensions.Fetch(a, b)) { }
        public IntermeshSegment(IntermeshCapsule capsule)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            OriginalA = capsule.A;
            OriginalB = capsule.B;
            OriginalKey = new Combination2(OriginalA.Id, OriginalB.Id);
            Segment = new LineSegment3D(OriginalA.Point, OriginalB.Point);
            _capsules.Add(capsule);
        }

        public int Id { get; }

        public IntermeshPoint OriginalA { get; }
        public IntermeshPoint OriginalB { get; }

        public IntermeshPoint A
        {
            get
            {
                if (!_capsules.Any()) { return OriginalA; }
                return _capsules.First().A;//
            }
        }

        public IntermeshPoint B
        {
            get
            {
                if (!_capsules.Any()) { return OriginalB; }
                return _capsules.Last().B;//
            }
        }

        public LineSegment3D Segment { get; private set; }

        public double SegmentAngleDifference
        {
            get
            {
                var vector1 = B.Point - A.Point;
                return Vector3D.Angle(vector1.Direction, Segment.Vector.Direction);
            }
        }
        public double CollinearDeviation
        {
            get
            {
                var line = new Line3D(A.Point, B.Point);
                var distances = _capsules.Points().Select(p => line.Distance(p.Point)).ToArray();
                return BasicObjects.Math.Math.Max(distances);
            }
        }

        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return A; yield return B; }
        }

        public bool WasChanged
        {
            get { return _previous.Any(); }
        }

        public IReadOnlyList<IntermeshCapsule> Capsules { get { return _capsules; } }

        public IntermeshSegment Replacement { get; set; }

        public bool IsRemoved
        {
            get { return !_capsules.Any(); }
        }

        public void Remove()
        {
            _previous.Add(_capsules.ToArray());
            _capsules.Clear();
        }

        public void ClearHistory()
        {
            _previous.Clear();
        }

        public void ReplaceStartAndEndWith(IntermeshPoint a, IntermeshPoint b)
        {
            if (_capsules.Count == 0)
            {
                _capsules.Add(IntermeshCapsuleExtensions.Fetch(a, b));
            }
            else if (_capsules.Count == 1)
            {
                _previous.Add(Capsules.ToArray());
                _capsules[0] = IntermeshCapsuleExtensions.Fetch(a, b);
            }
            else
            {
                _previous.Add(Capsules.ToArray());

                var firstPoint = _capsules[0].B;
                var lastPoint = _capsules[_capsules.Count - 1].A;
                var firstElement = IntermeshCapsuleExtensions.Fetch(a, firstPoint);
                var lastElement = IntermeshCapsuleExtensions.Fetch(lastPoint, b);

                _capsules[0] = firstElement;
                _capsules[_capsules.Count - 1] = lastElement;
            }
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public bool CapsuleSplit(IntermeshPoint p)
        {
            var split = GetCapsuleToSplit(p);
            if (split is not null)
            {
                var split1 = IntermeshCapsuleExtensions.Fetch(split.A, p);
                var split2 = IntermeshCapsuleExtensions.Fetch(p, split.B);

                if (_capsules.Any(c => c.Id == split1.Id)) { return false; }
                if (_capsules.Any(c => c.Id == split2.Id)) { return false; }

                if (_capsules.Replace(split, [split1, split2]))
                {
                    _previous.Add(_capsules.ToArray());
                }
                else { return false; }


                return true;
            }

            return false;
        }

        private IntermeshCapsule GetCapsuleToSplit(IntermeshPoint p)
        {
            {
                var projection = Segment.Projection(p.Point, GapConstants.Resolver);
                if (projection is null) { return null; }
            }

            var eligibleCapsules = new List<IntermeshCapsule>();

            foreach (var capsule in _capsules.Where(c => c.A.Id != p.Id && c.B.Id != p.Id))
            {
                var projection = capsule.Segment.Projection(p.Point, GapConstants.Resolver);
                if (projection is null) { continue; }
                //var distance = Point3D.Distance(projection, p.Point);
                //if (distance < 1e-9)
                {
                    eligibleCapsules.Add(capsule);
                }
            }

            return eligibleCapsules.Nearest(p);
        }

        public bool ExtendWith(IntermeshPoint p)
        {
            if (A.Id == p.Id || B.Id == p.Id) { return false; }

            var distanceA = Point3D.Distance(p.Point, A.Point);
            var distanceB = Point3D.Distance(p.Point, B.Point);

            var projection = Segment.Projection(p.Point, 0);
            if (projection is not null && distanceA > GapConstants.Resolver && distanceB > GapConstants.Resolver) { return false; }

            _previous.Add(Capsules.ToArray());
            if (distanceA < distanceB)
            {
                _capsules.Insert(0, IntermeshCapsuleExtensions.Fetch(p, _capsules.First().A));
            }
            else
            {
                _capsules.Add(IntermeshCapsuleExtensions.Fetch(_capsules.Last().B, p));
            }

            return true;
        }

        public bool ReplaceWith(IntermeshPoint old_, IntermeshPoint new_)
        {
            if (old_.Id == new_.Id) { return false; }
            if (!_capsules.SelectMany(c => c.Points).Any(p => p.Id == old_.Id))
            {
                return false;
            }
            _previous.Add(Capsules.ToArray());
            for (int i = 0; i < _capsules.Count; i++)
            {
                if (_capsules[i].A.Id == old_.Id) { _capsules[i] = IntermeshCapsuleExtensions.Fetch(new_, _capsules[i].B); }
                if (_capsules[i].B.Id == old_.Id) { _capsules[i] = IntermeshCapsuleExtensions.Fetch(_capsules[i].A, new_); }
            }

            return true;
        }

        public Combination2 OriginalKey { get; }

        public Combination2 Key
        {
            get { return new Combination2(A.Id, B.Id); }
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && OriginalA is not null && OriginalB is not null)
                {
                    _box = Rectangle3D.Containing(OriginalA.Point, OriginalB.Point).Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        private List<IntermeshSegment> _contacts = new List<IntermeshSegment>();
        public IReadOnlyList<IntermeshSegment> Contacts { get { return _contacts; } }

        public bool AddContacts(IntermeshSegment segment)
        {
            if (Id == segment.Id) { return false; }
            if (_contacts.Any(c => c.Id == segment.Id)) { return false; }

            _contacts.Add(segment);
            return true;
        }

        public int AddRangeContacts(IEnumerable<IntermeshSegment> segments)
        {
            int count = 0;
            foreach (var segment in segments)
            {
                count += AddContacts(segment) ? 1 : 0;
            }
            return count;
        }

        public void ClearContacts()
        {
            _contacts.Clear();
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public override string ToString()
        {
            return $"Intermesh Segment Key: {Key} Id: {Id}";
        }
    }
}
