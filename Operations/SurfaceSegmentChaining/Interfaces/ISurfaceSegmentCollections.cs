using Operations.SurfaceSegmentChaining.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Interfaces
{
    internal interface ISurfaceSegmentCollections<G, T> where G : class
    {
        IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        IReadOnlyCollection<LinkedSurfaceSegment<G>> LinkedSegments { get; }
        ProtectedLinkedIndexSegments<G, T> ProtectedLinkedIndexSegments { get; }
    }
}
