
namespace Operations.PlanarFilling.Filling.Internals
{
    internal class IndexSurfaceTriangle
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IndexSurfaceTriangle(int indexPointA, int indexPointB, int indexPointC, int[] indexLoop, int fillId)
        {
            IndexPointA = indexPointA;
            IndexPointB = indexPointB;
            IndexPointC = indexPointC;
            IndexLoop = indexLoop;

            lock (lockObject)
            {
                Id = _id++;
            }
            FillId = fillId;
        }
        public int Id { get; }
        public int FillId { get; }
        public int IndexPointA { get; }
        public int IndexPointB { get; }
        public int IndexPointC { get; }

        public int[] IndexLoop { get; }

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
