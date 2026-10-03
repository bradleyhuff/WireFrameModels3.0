using BasicObjects.GeometricObjects;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarLoopSet<T>
    {
        internal PlanarLoopSet(Plane plane, double testSegmentLength, IReadOnlyList<SurfaceRayContainer<T>> referenceArray, IFillAction<T> fillAction, int[] perimeterIndexLoop, int triangleID)
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
        public bool PerimeterLoopDisabled { get; set; }
        public List<int[]> DividingIndexLoops { get; } = new List<int[]>();
        public List<bool> DividingLoopsDisabled { get; } = new List<bool>();
        public List<int[]> SpurredIndexLoops { get; } = new List<int[]>();
        public List<bool> SpurredLoopsDisabled { get; } = new List<bool>();
        public List<int[]> SpursIndex { get; } = new List<int[]>();
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
                    _perimeterLoop = new PlanarLoop<T>(Plane, _testSegmentLength, PerimeterLoopDisabled, _referenceArray, _fillAction, PerimeterIndexLoop, _triangleID);
                }
                return _perimeterLoop;
            }
        }
        public IReadOnlyList<PlanarLoop<T>> DividingLoops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = DividingIndexLoops.Select((l, i ) => new PlanarLoop<T>(Plane, _testSegmentLength, DividingLoopsDisabled[i], _referenceArray, _fillAction, l, _triangleID)).ToList();
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
                    _spurredLoops = SpurredIndexLoops.Select((l, i) => new PlanarLoop<T>(Plane, _testSegmentLength, SpurredLoopsDisabled[i], _referenceArray, _fillAction, l, _triangleID)).ToList();
                }
                return _spurredLoops;
            }
        }

        public IReadOnlyList<IndexSurfaceTriangle> GetFillTriangles()
        {
            if (_indexedFillTriangles is null)
            {
                LoopForFillings();
            }
            return _indexedFillTriangles;
        }

        private void LoopForFillings()
        {
            _indexedFillTriangles = [.. PerimeterLoop.GetFillTriangles()];
            if (!FillInteriorLoops) { return; }

            foreach (var loop in DividingLoops)
            {
                _indexedFillTriangles.AddRange(loop.GetFillTriangles());
            }
            foreach (var loop in SpurredLoops)
            {
                _indexedFillTriangles.AddRange(loop.GetFillTriangles());
            }
        }
    }
}
