using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class LinkedIndexSurfaceSegment<G> where G : class
    {
        public LinkedIndexSurfaceSegment(int indexPointA, int indexPointB, Rank rank, int groupKey = 0, G groupObject = null)
        {
            IndexPointA = indexPointA;
            IndexPointB = indexPointB;
            GroupObject = groupObject;
            Rank = rank;
            GroupKey = groupKey;
        }
        public int GroupKey { get; }
        public G GroupObject { get; }
        public int IndexPointA { get; }
        public int IndexPointB { get; }
        public Rank Rank { get; }

        private Combination2 _key;
        private bool _keyIsAssigned = false;
        public Combination2 Key
        {
            get
            {
                if (!_keyIsAssigned)
                {
                    _key = new Combination2(IndexPointA, IndexPointB);
                    _keyIsAssigned = true;
                }
                return _key;
            }
        }
    }
}
