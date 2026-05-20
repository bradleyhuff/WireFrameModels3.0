using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshEdgeSlot
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public IntermeshEdgeSlot(IntermeshSegment segment)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(segment.A.Id, segment.B.Id);
            Segments.Add(segment);
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public List<IntermeshSegment> Segments { get; set; } = new List<IntermeshSegment>();
    }
}
