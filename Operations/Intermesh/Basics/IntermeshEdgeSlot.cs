using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    public class IntermeshEdgeSlot
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
        public LineSegment3D TotalSegment
        {
            get {
                var firstPoint = Segments.First(s => !s.IsRemoved).A.Point;
                var lastPoint = Segments.Last(s => !s.IsRemoved).B.Point;
                return new LineSegment3D(firstPoint, lastPoint);
            }
        }
        public IEnumerable<IntermeshPoint> EndPoints
        {
            get
            {
                yield return Segments.First(s => !s.IsRemoved).A;
                yield return Segments.Last(s => !s.IsRemoved).B;
            }
        }

        public IEnumerable<(IntermeshPoint Junction, IReadOnlyList<IntermeshSegment> Segments, IReadOnlyList<IntermeshSegment> Waywards)> JunctionPoints
        {
            get
            {
                var table = new GroupingDictionary<int, List<IntermeshSegment>>(() => new List<IntermeshSegment>());
                var listing = new Dictionary<int, IntermeshPoint>();

                foreach (var segment in Segments.Where(s => !s.IsRemoved))
                {
                    table[segment.A.Id].Add(segment);
                    table[segment.B.Id].Add(segment);
                    listing[segment.A.Id] = segment.A;
                    listing[segment.B.Id] = segment.B;
                }

                var junctionPoints = table.Where(p => p.Value.Count() > 2 || (Key.Indicies.Any(i => i == p.Key) && p.Value.Count() > 1)).ToArray();

                foreach (var junctionPoint in junctionPoints)
                {
                    yield return new (listing[junctionPoint.Key], junctionPoint.Value, junctionPoint.Value.Where(j => table[j.A.Id].Count == 1 || table[j.B.Id].Count == 1).ToList());
                }
            }
        }

        public override string ToString()
        {
            return $"Intermesh Slot Key: {Key} Id: {Id}";
        }

    }
}
