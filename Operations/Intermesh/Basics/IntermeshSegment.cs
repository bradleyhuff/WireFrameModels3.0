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
    internal class IntermeshSegment : IBox, IIntermeshEdge
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public enum ReplacementType { None, ShortSegment, NearParallelSegment }

        private List<IntermeshCapsule> _capsules = new List<IntermeshCapsule>();
        private IntermeshEdge _edge = new IntermeshEdge();
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
            Segments.Add(this);
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
        public IEnumerable<IntermeshPoint> OriginalPoints
        {
            get { yield return OriginalA; yield return OriginalB; }
        }

        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return A; yield return B; }
        }

        public IReadOnlyList<IntermeshCapsule> Capsules { get { return _capsules; } }

        public bool IsRemoved
        {
            get { return !Capsules.Any(); }
        }

        public void Remove()
        {
            _previous.Add(Capsules.ToArray());
            _capsules.Clear();
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
                //
                var firstPoint = _capsules[0].B;
                var lastPoint = _capsules[_capsules.Count - 1].A;
                //
                var firstElement = IntermeshCapsuleExtensions.Fetch(a, firstPoint);
                var lastElement = IntermeshCapsuleExtensions.Fetch(lastPoint, b);

                _capsules[0] = firstElement;
                _capsules[_capsules.Count - 1] = lastElement;
            }
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public bool SplitBy(IntermeshPoint p)
        {
            var splitBy = Capsules.SplitBy(p);
            var wasSplit = splitBy.Count() > Capsules.Count();
            if (wasSplit)
            {
                _previous.Add(splitBy.ToArray());
                _capsules = splitBy.ToList();
            }
            return wasSplit;
        }

        public bool AddExtension(IntermeshPoint a, IntermeshPoint b)
        {
            if (A.Id == a.Id)
            {
                _previous.Add(Capsules.ToArray());
                _capsules.Insert(0, IntermeshCapsuleExtensions.Fetch(b, a));
                return true;
            }
            if (A.Id == b.Id)
            {
                _previous.Add(Capsules.ToArray());
                _capsules.Insert(0, IntermeshCapsuleExtensions.Fetch(a, b));
                return true;
            }
            if (B.Id == a.Id)
            {
                _previous.Add(Capsules.ToArray());
                _capsules.Add(IntermeshCapsuleExtensions.Fetch(a, b));
                return true;
            }
            if (B.Id == b.Id)
            {
                _previous.Add(Capsules.ToArray());
                _capsules.Add(IntermeshCapsuleExtensions.Fetch(b, a));
                return true;
            }
            return false;
        }

        public ReplacementType ReplacementStatus { get; set; } = ReplacementType.None;

        public IntermeshSegment ReplacedBy { get; set; }
        public List<IntermeshSegment> Replaces { get; } = new List<IntermeshSegment>();

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

        public List<IntermeshSegment> Segments { get; set; } = new List<IntermeshSegment>();

        public IIntermeshEdge Switch()
        {
            _edge.Segments = new List<IntermeshSegment>() { this };
            return _edge;
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public override string ToString()
        {
            return $"Intermesh Segment Key: {Key} Original: {OriginalKey}  Id: {Id}";
        }
    }
}
