
namespace Operations.PlanarFilling.Filling.Interfaces
{
    internal interface IFillingLoopSet
    {
        public int[] PerimeterIndexLoop { get; }
        public List<int[]> IndexLoops { get; }
        public List<int[]> IndexSpurredLoops { get; }
        public List<int[]> IndexSpurs { get; }
        public bool FillInteriorLoops { get; set; }
        public IFillingLoop PerimeterLoop { get; }
        public IReadOnlyList<IFillingLoop> Loops { get; }
        public IReadOnlyList<IFillingLoop> SpurredLoops { get; }
        public IReadOnlyList<IndexSurfaceTriangle> FillTriangles { get; }
    }
}
