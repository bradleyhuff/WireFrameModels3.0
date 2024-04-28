using BasicObjects.GeometricObjects;
using Operations.PlanarFilling.Filling.Interfaces;
using Operations.PlanarFilling.Filling;

namespace Operations.PlanarFilling.Abstracts
{
    internal abstract class AbstractLoopSet : IFillingLoopSet
    {
        internal AbstractLoopSet(IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            PerimeterIndexLoop = perimeterIndexLoop;
            _referenceArray = referenceArray;
            _triangleID = triangleID;
        }
        public int[] PerimeterIndexLoop { get; }
        public List<int[]> IndexLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurredLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurs { get; } = new List<int[]>();
        public bool FillInteriorLoops { get; set; }

        private IReadOnlyList<Ray3D> _referenceArray;
        private int _triangleID;
        private IFillingLoop _perimeterLoop;
        private List<IFillingLoop> _loops;
        private List<IFillingLoop> _spurredLoops;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;

        protected abstract IFillingLoop CreateFillingLoop(IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID);

        public IFillingLoop PerimeterLoop
        {
            get
            {
                if (_perimeterLoop is null)
                {
                    _perimeterLoop = CreateFillingLoop(_referenceArray, PerimeterIndexLoop, _triangleID);
                }
                return _perimeterLoop;
            }
        }
        public IReadOnlyList<IFillingLoop> Loops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = IndexLoops.Select(l => CreateFillingLoop(_referenceArray, l, _triangleID)).ToList();
                }
                return _loops;
            }
        }

        public IReadOnlyList<IFillingLoop> SpurredLoops
        {
            get
            {
                if (_spurredLoops is null)
                {
                    _spurredLoops = IndexSpurredLoops.Select(l => CreateFillingLoop(_referenceArray, l, _triangleID)).ToList();
                }
                return _spurredLoops;
            }
        }

        public IReadOnlyList<IndexSurfaceTriangle> FillTriangles
        {
            get
            {
                if (_indexedFillTriangles is null)
                {
                    LoopForFillings();
                }
                return _indexedFillTriangles;
            }
        }

        private void LoopForFillings()
        {
            _indexedFillTriangles = [.. PerimeterLoop.FillTriangles];
            if (!FillInteriorLoops) { return; }

            foreach (var loop in Loops)
            {
                _indexedFillTriangles.AddRange(loop.FillTriangles);
            }
            foreach (var loop in SpurredLoops)
            {
                _indexedFillTriangles.AddRange(loop.FillTriangles);
            }
        }
    }
}
