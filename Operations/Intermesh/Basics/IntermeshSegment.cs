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

        private List<IntermeshCapsule> _capsules = new List<IntermeshCapsule>();
        private IntermeshEdge _edge = new IntermeshEdge();

        public IntermeshSegment(IntermeshPoint a, IntermeshPoint b) : this(IntermeshCapsule.Fetch(a, b)) { }
        public IntermeshSegment(IntermeshCapsule capsule)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            A = capsule.A;
            B = capsule.B;
            Key = new Combination2(A.Id, B.Id);
            Segment = new LineSegment3D(A.Point, B.Point);
            _capsules.Add(capsule);
            Segments.Add(this);
        }

        public int Id { get; }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return A; yield return B; }
        }

        public List<IntermeshCapsule> Capsules { get { return _capsules; } }

        public bool IsRemoved
        {
            get { return !Capsules.Any(); }
        }

        public void Remove()
        {
            Capsules.Clear();
        }

        public Combination2 Key { get; }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && A is not null && B is not null)
                {
                    _box = Rectangle3D.Containing(A.Point, B.Point).Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        public List<IntermeshSegment> VertexAContacts { get; } = new List<IntermeshSegment>();

        public List<IntermeshSegment> VertexBContacts { get; } = new List<IntermeshSegment>();

        public List<IntermeshSegment> LocalContacts { get; } = new List<IntermeshSegment>();

        public IEnumerable<IntermeshSegment> ContactsWithRemovedRecursion(Func<IntermeshSegment, IEnumerable<IntermeshSegment>> query)
        {
            return contactsWithRemovedRecursion(query).DistinctBy(c => c.Id).ToArray();
        }

        private class ContactStack
        {
            public ContactStack(int index, IntermeshSegment[] contacts)
            {
                Index = index;
                Contacts = contacts;
            }
            public int Index { get; set; }
            public IntermeshSegment[] Contacts { get; }
        }

        private IEnumerable<IntermeshSegment> contactsWithRemovedRecursion(Func<IntermeshSegment, IEnumerable<IntermeshSegment>> query)
        {
            var stack = new Stack<ContactStack>();
            stack.Push(new ContactStack(0, query(this).ToArray())); MaxStack = Math.Max(MaxStack, stack.Count);

            while (stack.Any())
            {
                var peek = stack.Peek();
                for (int i = peek.Index; i < peek.Contacts.Length; i++)
                {
                    var contact = peek.Contacts[i];
                    peek.Index = i + 1;
                    if (!contact.IsRemoved)
                    {
                        yield return contact;
                    }
                    else
                    {
                        stack.Push(new ContactStack(0, query(contact).ToArray())); MaxStack = Math.Max(MaxStack, stack.Count);
                        break;
                    }
                }
                if (peek.Index >= peek.Contacts.Length) { stack.Pop(); }
            }
        }

        public static int MaxStack { get; set; } = 0;
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
            return $"Intermesh Segment {Key}";
        }


    }
}
