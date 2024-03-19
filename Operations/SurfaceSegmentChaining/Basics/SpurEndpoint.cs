using Collections.Buckets.Interfaces;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SpurEndpoint<G, I, S, T> where G : TriangleFillingGroup where I : IBox where S : new()
    {
        public SpurEndpoint(int[] spurredLoop, int index, int groupKey, G group, BoxBucket<I> bucket, IReadOnlyList<SurfaceRayContainer<T>> referenceArray)
        {
            Index = index;
            SpurredLoop = spurredLoop;
            GroupKey = groupKey;
            Group = group;
            Bucket = bucket;
            ReferenceArray = referenceArray;
        }
        public int Index { get; }
        public int[] SpurredLoop { get; }
        public int GroupKey { get; }
        public G Group { get; }
        public BoxBucket<I> Bucket { get; }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        public S Status { get; } = new S();

        public override string ToString()
        {
            return $"Index: {SpurredLoop[Index]} Loop: {SpurredLoop.Length}";
        }
    }
}
