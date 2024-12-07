using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics.V2
{
    internal class IntermeshDivision
    {
        private static int _id = 0;
        public IntermeshDivision(IntermeshPoint a, IntermeshPoint b)
        {
            A = a;
            B = b;
            Id = _id++;
            Key = new Combination2(a.Id, b.Id);
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPoint> Points 
        {
            get { yield return A; yield return B; }
        }
    }
}
