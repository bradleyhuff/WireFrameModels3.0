using Collections.Buckets.Interfaces;
using Collections.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class OpenSpurEndpoint<G, I, S, T> where I : IBox where S : new()
    {
        public OpenSpurEndpoint(int[] openSpur, int groupKey, G group, BoxBucket<I> bucket, IReadOnlyList<SurfaceRayContainer<T>> referenceArray)
        {
            OpenSpur = openSpur;
            GroupKey = groupKey;
            Group = group;
            Bucket = bucket;
            ReferenceArray = referenceArray;
        }
        public int[] OpenSpur { get; }
        public int GroupKey { get; }
        public G Group { get; }
        public BoxBucket<I> Bucket { get; }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        public S Status { get; } = new S();
    }
}
