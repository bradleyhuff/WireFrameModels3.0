using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshCapsule
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public enum CapsuleType { Perimeter, Internal}

        public IntermeshCapsule(IntermeshDivisionOLD division, CapsuleType type):this(division.A, division.B, type)
        {
            Parent = division;
        }
        public IntermeshCapsule(IntermeshPointOLD a, IntermeshPointOLD b, CapsuleType type)
        {
            A = a;
            B = b;
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(a.Id, b.Id);
            Type = type;
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public CapsuleType Type { get; }

        public IntermeshDivisionOLD Parent { get; }
        public int Id { get; }
        public Combination2 Key { get; }
        public IntermeshPointOLD A { get; }
        public IntermeshPointOLD B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPointOLD> Points
        {
            get { yield return A; yield return B; }
        }
        public override string ToString()
        {
            return $"Intermesh Rod {Key}";
        }
    }
}
