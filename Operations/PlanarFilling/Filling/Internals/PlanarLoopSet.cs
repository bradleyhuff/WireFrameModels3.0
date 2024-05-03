using BasicObjects.GeometricObjects;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarLoopSet
    {
        internal PlanarLoopSet(Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            Plane = plane;
            PerimeterIndexLoop = perimeterIndexLoop;
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

        private IReadOnlyList<Ray3D> _referenceArray;
        private double _testSegmentLength;
        private int _triangleID;
        private PlanarLoop _perimeterLoop;
        private List<PlanarLoop> _loops;
        private List<PlanarLoop> _spurredLoops;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;

        public double TestSegmentLength
        {
            get
            {
                return _testSegmentLength;
            }
        }

        public PlanarLoop PerimeterLoop
        {
            get
            {
                if (_perimeterLoop is null)
                {
                    _perimeterLoop = new PlanarLoop(/*_planar,*/ Plane, _testSegmentLength, _referenceArray, PerimeterIndexLoop, _triangleID);
                }
                return _perimeterLoop;
            }
        }
        public IReadOnlyList<PlanarLoop> Loops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = IndexLoops.Select(l => new PlanarLoop(Plane, _testSegmentLength, _referenceArray, l, _triangleID)).ToList();
                }
                return _loops;
            }
        }

        public IReadOnlyList<PlanarLoop> SpurredLoops
        {
            get
            {
                if (_spurredLoops is null)
                {
                    _spurredLoops = IndexSpurredLoops.Select(l => new PlanarLoop(Plane, _testSegmentLength, _referenceArray, l, _triangleID)).ToList();
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
