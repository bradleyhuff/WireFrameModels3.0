using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.PlanarFilling.Basics
{
    internal class ProtectedIndexedSurfaceTriangles
    {
        public ProtectedIndexedSurfaceTriangles() { }
        public ProtectedIndexedSurfaceTriangles(
            IReadOnlyList<IndexSurfaceTriangle> indexedSurfaceTriangles
            )
        {
            IndexedSurfaceTriangles = indexedSurfaceTriangles;
        }
        protected IReadOnlyList<IndexSurfaceTriangle> IndexedSurfaceTriangles { get; private set; }

        public static T Create<T>(ProtectedIndexedSurfaceTriangles input) where T : ProtectedIndexedSurfaceTriangles, new()
        {
            T output = new T();
            output.IndexedSurfaceTriangles = input.IndexedSurfaceTriangles;
            return output;
        }
    }
}
