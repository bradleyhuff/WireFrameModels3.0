using BasicObjects.GeometricObjects;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;
using static Operations.SurfaceSegmentChaining.Chaining.Extensions.SpurChaining;
using Operations.Filling.Interfaces;

namespace Operations.Filling.Abstracts
{
    internal abstract class AbstractFilling<G, T>
    {
        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private IReadOnlyList<int[]> _perimeterIndexLoops;
        private IReadOnlyList<int[]> _indexLoops;
        private IReadOnlyList<int[]> _indexSpurredLoops;
        private IReadOnlyList<int[]> _indexSpurs;
        private List<IFillingLoopSet> _planarLoopSets = new List<IFillingLoopSet>();
        private ISurfaceSegmentChaining<G, T> _chaining;
        private List<IndexSurfaceTriangle> _indexedFillTriangles;

        public AbstractFilling(ISurfaceSegmentChaining<G, T> chaining, int triangleID)
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
            var table = new Dictionary<int, List<IFillingLoopSet>>();

            for (int i = 0; i < _chaining.PerimeterLoopGroupKeys.Count; i++)
            {
                var key = _chaining.PerimeterLoopGroupKeys[i];
                var groupObject = _chaining.PerimeterLoopGroupObjects[i];
                if (!table.ContainsKey(key))
                {
                    table[key] = new List<IFillingLoopSet>();
                }
                table[key].Add(
                    CreateFillingLoopSet(groupObject, _referenceArray, _perimeterIndexLoops[i], _triangleID)
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

        private void CombinePerimeterLoops(Dictionary<int, List<IFillingLoopSet>> table)
        {
            var newTable = new Dictionary<int, List<IFillingLoopSet>>();
            foreach (var multiplePerimeterLoops in table.Where(p => p.Value.Count > 1))
            {
                var loops = multiplePerimeterLoops.Value.Select(l => l.PerimeterLoop).ToArray();
                List<IFillingLoop> outerMostLoops;
                List<IFillingLoop> restOfLoops;
                AbstractLoop.ExtractOuterMostLoopsFromRest(loops, out outerMostLoops, out restOfLoops);

                var first = multiplePerimeterLoops.Value.First();
                var newList = new List<IFillingLoopSet>();
                newTable[multiplePerimeterLoops.Key] = newList;
                newList.Add(CreateFillingLoopSet(first, _referenceArray, outerMostLoops[0].IndexLoop.ToArray(), _triangleID));
                newList[0].IndexLoops.AddRange(restOfLoops.Select(l => l.IndexLoop.ToArray()));
                newList[0].FillInteriorLoops = false;
            }
            foreach (var pair in newTable)
            {
                table[pair.Key] = pair.Value;
            }
        }

        protected abstract IFillingLoopSet CreateFillingLoopSet(object input, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID);
        private void SetLoopNestings()
        {
            foreach (var planarLoopSet in _planarLoopSets)
            {
                List<IFillingLoop> outerMostLoops;
                List<IFillingLoop> restOfLoops;
                AbstractLoop.ExtractOuterMostLoopsFromRest(planarLoopSet.Loops, out outerMostLoops, out restOfLoops);

                planarLoopSet.PerimeterLoop.InternalLoops = outerMostLoops;
                foreach (var outerMostLoop in outerMostLoops)
                {
                    SetLoopNesting(outerMostLoop, restOfLoops);
                }
            }
        }

        private void SetLoopNesting(IFillingLoop exteriorLoop, List<IFillingLoop> restOfLoops)
        {
            var interiorLoops = restOfLoops.Where(exteriorLoop.OutLineContainsLoop).ToArray();
            if (!interiorLoops.Any()) { return; }

            List<IFillingLoop> outerMostLoops;
            List<IFillingLoop> restOfLoops2;
            AbstractLoop.ExtractOuterMostLoopsFromRest(interiorLoops, out outerMostLoops, out restOfLoops2);

            exteriorLoop.InternalLoops = outerMostLoops;
            foreach (var outerMostLoop in outerMostLoops)
            {
                SetLoopNesting(outerMostLoop, restOfLoops2);
            }
        }
    }
}
