using Operations.PlanarFilling.Basics;

namespace Operations.PlanarFilling.Filling
{
    internal partial class PlanarFilling<G, T> where G : PlanarFillingGroup
    {
        private class IndexSurfaceTriangle
        {
            public IndexSurfaceTriangle(int indexPointA, int indexPointB, int indexPointC)
            {
                IndexPointA = indexPointA;
                IndexPointB = indexPointB;
                IndexPointC = indexPointC;
            }
            public int IndexPointA { get; }
            public int IndexPointB { get; }
            public int IndexPointC { get; }

            public IEnumerable<int> Indicies
            {
                get
                {
                    yield return IndexPointA;
                    yield return IndexPointB;
                    yield return IndexPointC;
                }
            }
        }
    }
}
