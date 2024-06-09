using BasicObjects.GeometricObjects;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarLoopSet<T>
    {
        internal PlanarLoopSet(Plane plane, double testSegmentLength, IReadOnlyList<SurfaceRayContainer<T>> referenceArray, IFillAction<T> fillAction,  int[] perimeterIndexLoop, int triangleID)
        {
            Plane = plane;
            PerimeterIndexLoop = perimeterIndexLoop;
            _fillAction = fillAction;
            _referenceArray = referenceArray;
            _testSegmentLength = testSegmentLength;
            _triangleID = triangleID;
        }
        public Plane Plane { get; }
        public int[] PerimeterIndexLoop { get; }
        public List<int[]> IndexLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurredLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurs { get; } = new List<int[]>();
        public bool FillInteriorLoops { get; set; }

        private IFillAction<T> _fillAction;
        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private double _testSegmentLength;
        private int _triangleID;
        private PlanarLoop<T> _perimeterLoop;
        private List<PlanarLoop<T>> _loops;
        private List<PlanarLoop<T>> _spurredLoops;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;

        public double TestSegmentLength
        {
            get
            {
                return _testSegmentLength;
            }
        }

        public PlanarLoop<T> PerimeterLoop
        {
            get
            {
                if (_perimeterLoop is null)
                {
                    _perimeterLoop = new PlanarLoop<T>(Plane, _testSegmentLength, _referenceArray, _fillAction, PerimeterIndexLoop, _triangleID);
                }
                return _perimeterLoop;
            }
        }
        public IReadOnlyList<PlanarLoop<T>> Loops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = IndexLoops.Select(l => new PlanarLoop<T>(Plane, _testSegmentLength, _referenceArray, _fillAction, l, _triangleID)).ToList();
                }
                return _loops;
            }
        }

        public IReadOnlyList<PlanarLoop<T>> SpurredLoops
        {
            get
            {
                if (_spurredLoops is null)
                {
                    _spurredLoops = IndexSpurredLoops.Select(l => new PlanarLoop<T>(Plane, _testSegmentLength, _referenceArray, _fillAction, l, _triangleID)).ToList();
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
