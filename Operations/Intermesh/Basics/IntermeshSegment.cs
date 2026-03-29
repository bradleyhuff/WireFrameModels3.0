using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
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

        public IntermeshSegment(IntermeshPoint a, IntermeshPoint b) : this(new IntermeshCapsule(a, b)) { }
        public IntermeshSegment(IntermeshCapsule capsule)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            A = capsule.A;
            B = capsule.B;
            Key = new Combination2(A.Id, B.Id);
            _capsules.Add(capsule);
        }

        public int Id { get; }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }

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
