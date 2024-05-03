using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling.Internals;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;
using static Operations.SurfaceSegmentChaining.Chaining.Extensions.SpurChaining;

namespace Operations.PlanarFilling.Filling
{
    internal partial class PlanarFilling<G, T> where G : PlanarFillingGroup
    {
        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private IReadOnlyList<int[]> _perimeterIndexLoops;
        private IReadOnlyList<int[]> _indexLoops;
        private IReadOnlyList<int[]> _indexSpurredLoops;
        private IReadOnlyList<int[]> _indexSpurs;
        private List<PlanarLoopSet> _planarLoopSets = new List<PlanarLoopSet>();
        private ISurfaceSegmentChaining<G, T> _chaining;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;

        public PlanarFilling(ISurfaceSegmentChaining<G, T> chaining, int triangleID)
        {
            _triangleID = triangleID;
            _chaining = chaining;
            _referenceArray = chaining.ReferenceArray;
            GetProtectedLoops();
            BuildPlanarLoopSets();
            SetLoopNestings();
            GetFillings();
        }

        private int _triangleID;
        private IEnumerable<SurfaceTriangleContainer<T>> _fillings;

        public IEnumerable<SurfaceTriangleContainer<T>> Fillings
        {
            get
            {
                if (_fillings is null)
                {
                    _fillings = _indexedFillTriangles.Select(f =>
                        new SurfaceTriangleContainer<T>(_referenceArray[f.IndexPointA], _referenceArray[f.IndexPointB], _referenceArray[f.IndexPointC])).ToArray();
                }
                return _fillings;
            }
        }

        private void GetFillings()
        {
            _indexedFillTriangles = new List<IndexSurfaceTriangle>();
            foreach (var planarLoopSet in _planarLoopSets)
            {
                _indexedFillTriangles.AddRange(planarLoopSet.FillTriangles);
            }
        }

        private void GetProtectedLoops()
        {
            var protectedIndexLoops = ProtectedIndexedLoops.Create<InternalProtectedIndexedLoops>(_chaining.ProtectedIndexedLoops);
            _perimeterIndexLoops = protectedIndexLoops.GetPerimeterIndexLoops();
            _indexLoops = protectedIndexLoops.GetIndexLoops();
            _indexSpurredLoops = protectedIndexLoops.GetIndexSpurredLoops();
            _indexSpurs = protectedIndexLoops.GetIndexSpurs();
        }

        private void BuildPlanarLoopSets()
        {
            var table = new Dictionary<int, List<PlanarLoopSet>>();

            for (int i = 0; i < _chaining.PerimeterLoopGroupKeys.Count; i++)
            {
                var key = _chaining.PerimeterLoopGroupKeys[i];
                var groupObject = _chaining.PerimeterLoopGroupObjects[i];
                if (!table.ContainsKey(key))
                {
                    table[key] = new List<PlanarLoopSet>();
                }
                table[key].Add(
                        new PlanarLoopSet(groupObject.Plane, groupObject.TestSegmentLength,
                            _referenceArray, _perimeterIndexLoops[i], _triangleID)
                    );
                table[key].Last().FillInteriorLoops = true;
            }

            for (int i = 0; i < _chaining.LoopGroupKeys.Count; i++)
            {
                var key = _chaining.LoopGroupKeys[i];
                table[key].Last().IndexLoops.Add(_indexLoops[i]);
            }

            for (int i = 0; i < _chaining.SpurredLoopGroupKeys.Count; i++)
            {
                var key = _chaining.SpurredLoopGroupKeys[i];
                table[key].Last().IndexSpurredLoops.Add(_indexSpurredLoops[i]);
            }

            for (int i = 0; i < _chaining.SpurGroupKeys.Count; i++)
            {
                var key = _chaining.SpurGroupKeys[i];
                table[key].Last().IndexSpurs.Add(_indexSpurs[i]);
            }

            CombinePerimeterLoops(table);

            _planarLoopSets = table.Values.SelectMany(k => k).ToList();
        }

        private void CombinePerimeterLoops(Dictionary<int, List<PlanarLoopSet>> table)
        {
            var newTable = new Dictionary<int, List<PlanarLoopSet>>();
            foreach (var multiplePerimeterLoops in table.Where(p => p.Value.Count > 1))
            {
                //Console.WriteLine($"Combine {multiplePerimeterLoops.Value.Count} perimeter loops");
                var loops = multiplePerimeterLoops.Value.Select(l => l.PerimeterLoop).ToArray();
                List<PlanarLoop> outerMostLoops;
                List<PlanarLoop> restOfLoops;
                PlanarLoop.ExtractOuterMostLoopsFromRest(loops, out outerMostLoops, out restOfLoops);

                var first = multiplePerimeterLoops.Value.First();
                var newList = new List<PlanarLoopSet>();
                newTable[multiplePerimeterLoops.Key] = newList;
                newList.Add(
                    new PlanarLoopSet(first.Plane, first.TestSegmentLength,
                    _referenceArray, outerMostLoops[0].IndexLoop.ToArray(), _triangleID));
                newList[0].IndexLoops.AddRange(restOfLoops.Select(l => l.IndexLoop.ToArray()));
                newList[0].FillInteriorLoops = false;
            }
            foreach (var pair in newTable)
            {
                table[pair.Key] = pair.Value;
            }
        }

        private void SetLoopNestings()
        {
            foreach (var planarLoopSet in _planarLoopSets)
            {
                List<PlanarLoop> outerMostLoops;
                List<PlanarLoop> restOfLoops;
                PlanarLoop.ExtractOuterMostLoopsFromRest(planarLoopSet.Loops, out outerMostLoops, out restOfLoops);

                planarLoopSet.PerimeterLoop.InternalLoops = outerMostLoops;
                foreach (var outerMostLoop in outerMostLoops)
                {
                    SetLoopNesting(outerMostLoop, restOfLoops);
                }
            }
            //Console.WriteLine($"Loop compares {InternalPlanarLoop.Compares}");
        }

        private void SetLoopNesting(PlanarLoop exteriorLoop, List<PlanarLoop> restOfLoops)
        {
            var interiorLoops = restOfLoops.Where(r => exteriorLoop.OutLineContainsLoop(r)).ToArray();
            if (!interiorLoops.Any()) { return; }

            List<PlanarLoop> outerMostLoops;
            List<PlanarLoop> restOfLoops2;
            PlanarLoop.ExtractOuterMostLoopsFromRest(interiorLoops, out outerMostLoops, out restOfLoops2);

            exteriorLoop.InternalLoops = outerMostLoops;
            foreach (var outerMostLoop in outerMostLoops)
            {
                SetLoopNesting(outerMostLoop, restOfLoops2);
            }
        }
    }
}
