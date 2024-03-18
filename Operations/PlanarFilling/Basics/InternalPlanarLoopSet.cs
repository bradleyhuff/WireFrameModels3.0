using BasicObjects.GeometricObjects;
using Operations.PlanarFilling.Filling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.PlanarFilling.Basics
{
    internal class InternalPlanarLoopSet
    {
        public InternalPlanarLoopSet(/*PlanarGroupNode planar,*/ Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            Plane = plane;
            PerimeterIndexLoop = perimeterIndexLoop;
            _referenceArray = referenceArray;
            _testSegmentLength = testSegmentLength;
            //_planar = planar;
            _triangleID = triangleID;
        }
        public Plane Plane { get; }
        //public PlanarGroupNode Planar
        //{
        //    get
        //    {
        //        return _planar;
        //    }
        //}
        public int[] PerimeterIndexLoop { get; }
        public List<int[]> IndexLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurredLoops { get; } = new List<int[]>();
        public List<int[]> IndexSpurs { get; } = new List<int[]>();
        public bool FillInteriorLoops { get; set; }

        private IReadOnlyList<Ray3D> _referenceArray;
        private double _testSegmentLength;
        private int _triangleID;
        private InternalPlanarLoop _perimeterLoop;
        private List<InternalPlanarLoop> _loops;
        private List<InternalPlanarLoop> _spurredLoops;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;
        //private PlanarGroupNode _planar;

        public double TestSegmentLength
        {
            get
            {
                return _testSegmentLength;
            }
        }

        public InternalPlanarLoop PerimeterLoop
        {
            get
            {
                if (_perimeterLoop is null)
                {
                    _perimeterLoop = new InternalPlanarLoop(/*_planar,*/ Plane, _testSegmentLength, _referenceArray, PerimeterIndexLoop, _triangleID);
                }
                return _perimeterLoop;
            }
        }
        public IReadOnlyList<InternalPlanarLoop> Loops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = IndexLoops.Select(l => new InternalPlanarLoop(Plane, _testSegmentLength, _referenceArray, l, _triangleID)).ToList();
                }
                return _loops;
            }
        }

        public IReadOnlyList<InternalPlanarLoop> SpurredLoops
        {
            get
            {
                if (_spurredLoops is null)
                {
                    _spurredLoops = IndexSpurredLoops.Select(l => new InternalPlanarLoop(Plane, _testSegmentLength, _referenceArray, l, _triangleID)).ToList();
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
