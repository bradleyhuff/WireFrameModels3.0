using BasicObjects.MathExtensions;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining.Diagnostics;
using Operations.SurfaceSegmentChaining.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal class BaseDividerSurfaceChaining<G, T> : ISurfaceSegmentChaining<G, T> where G : class
    {
        private class InternalProtectedLinkedIndexSegments : ProtectedLinkedIndexSegments<G, T>
        {
            public IReadOnlyCollection<LinkedIndexSegment<G>> GetLinkedIndexSegments()
            {
                return LinkedIndexSegments ?? new LinkedIndexSegment<G>[0];
            }
        }
        internal static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentCollections<G, T> collections)
        {
            var linkedSegments = ProtectedLinkedIndexSegments<G, T>.Create<InternalProtectedLinkedIndexSegments>(collections.ProtectedLinkedIndexSegments).
                GetLinkedIndexSegments().Where(l => l.IndexPointA != l.IndexPointB).
                Select(l => new LinkedIndexSegment<G, T>(l.GroupKey, l.GroupObject, l.IndexPointA, l.IndexPointB, l.Rank)).ToList();

            var chaining = new BaseDividerSurfaceChaining<G, T>();
            chaining.Run(collections.ReferenceArray, linkedSegments);
            return chaining;
        }

        protected BaseDividerSurfaceChaining() { }

        protected virtual void Run(IReadOnlyList<SurfaceRayContainer<T>> referenceArray, List<LinkedIndexSegment<G, T>> linkedSegments)
        {
            _referenceArray = referenceArray;
            _linkedSegments = linkedSegments;
            BuildAssociationTable(_linkedSegments);
            BuildLinks();
            var keys = GetDividingKeys(_linkedSegments);
            var chainSegments = PullSegmentsFromPerimeters().ToList();

            var assemblyTable = GetAssemblyTable(chainSegments, keys);
            BuildPerimeterLoops(assemblyTable, keys);

            var chainLoops = PullLoopsFromPerimeters().ToList();
            foreach ( var loop in chainLoops ) { AddPerimeterLoop(loop); }

            _protectedIndexedLoops = new ProtectedIndexedLoops(_perimeterIndexLoops, _indexLoops, _indexSpurredLoops, _indexSpurs);
        }

        private Dictionary<int, List<LinkedIndexSegment<G, T>>> _indexAssociationTable = new Dictionary<int, List<LinkedIndexSegment<G, T>>>();

        private void BuildAssociationTable(IEnumerable<LinkedIndexSegment<G, T>> linkedSegments)
        {
            foreach (var linkedSegment in linkedSegments)
            {
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointA)) { _indexAssociationTable[linkedSegment.IndexPointA] = new List<LinkedIndexSegment<G, T>>(); }
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointB)) { _indexAssociationTable[linkedSegment.IndexPointB] = new List<LinkedIndexSegment<G, T>>(); }

                _indexAssociationTable[linkedSegment.IndexPointA].Add(linkedSegment);
                _indexAssociationTable[linkedSegment.IndexPointB].Add(linkedSegment);
            }
        }

        private void BuildPerimeterLoops(Combination4Dictionary<List<int[]>> assemblyTable, Dictionary<int, Combination2> dividingKeys)
        {
            foreach(var assembly in assemblyTable.Values.Where(v => v.Count == 2))
            {
                var firstIndex = assembly[0][0];
                var lastIndex = assembly[0][assembly[0].Length - 1];
                var firstKey = dividingKeys[firstIndex];
                var lastKey = dividingKeys[lastIndex];

                var firstDivisionIndex = OppositeInKey(firstKey, firstIndex);
                var lastDivisionIndex = OppositeInKey(lastKey, lastIndex);

                var perimeterLoop = assembly[0].ToList();

                var firstIndex2 = assembly[1][0];
                var lastIndex2 = assembly[1][assembly[1].Length - 1];

                if (firstDivisionIndex == firstIndex2 && lastDivisionIndex == lastIndex2)
                {
                    perimeterLoop.AddRange(assembly[1].Reverse());
                    AddPerimeterLoop(perimeterLoop);
                    continue;
                }
                if (firstDivisionIndex == lastIndex2 && lastDivisionIndex == firstIndex2)
                {
                    perimeterLoop.AddRange(assembly[1]);
                    AddPerimeterLoop(perimeterLoop);
                    continue;
                }
                throw new InvalidOperationException($"Perimeter loop could not be assembled.");
            }

        }

        private void AddPerimeterLoop(IEnumerable<int> perimeterLoop)
        {
            _perimeterIndexLoops.Add(perimeterLoop.ToArray());
            _perimeterLoopGroupKeys.Add(0);
            _perimeterLoopGroupObjects.Add(null);
        }

        private int OppositeInKey(Combination2 key, int index)
        {
            if (key.A == index) { return key.B; }
            if (key.B == index) { return key.A; }
            throw new InvalidOperationException($"Opposite {index} not found in {key}");
        }

        private void BuildLinks()
        {
            foreach (var pair in _indexAssociationTable)
            {
                foreach (var element in pair.Value.Where(e => e.IndexPointA == pair.Key))
                {
                    element.LinksA.AddRange(pair.Value.Where(e => e != element));
                }
                foreach (var element in pair.Value.Where(e => e.IndexPointB == pair.Key))
                {
                    element.LinksB.AddRange(pair.Value.Where(e => e != element));
                }
            }
        }

        private Dictionary<int,Combination2> GetDividingKeys(IEnumerable<LinkedIndexSegment<G, T>> linkedSegments)
        {
            var divisions = linkedSegments.Where(l => l.Rank == Rank.Dividing);

            Dictionary<int, Combination2> table = new Dictionary<int, Combination2>();
            foreach(var division in divisions)
            {
                table[division.IndexPointA] = division.Key;
                table[division.IndexPointB] = division.Key;
            }
            return table;
        }

        private Combination4Dictionary<List<int[]>> GetAssemblyTable(List<int[]> segments, Dictionary<int, Combination2> dividingKeys)
        {
            var table = new Combination4Dictionary<List<int[]>>();

            foreach(var segment in segments)
            {
                var firstKey = dividingKeys[segment[0]];
                var lastKey = dividingKeys[segment[segment.Length - 1]];
                var assemblyKey = new Combination4(firstKey.A, firstKey.B, lastKey.A, lastKey.B);
                if (!table.ContainsKey(assemblyKey)) { table[assemblyKey] = new List<int[]>(); }
                table[assemblyKey].Add(segment);
            }

            return table;
        }

        private IEnumerable<int[]> PullSegmentsFromPerimeters()
        {
            var perimeterSegments = _linkedSegments.Where(l => l.Rank == Rank.Perimeter).ToArray();
            LinkedIndexSegment<G, T> segment;
            int index = 0;
            int count = 0;

            while ((segment = GetNextStart(perimeterSegments, ref index, 
                (l) => l.Rank == Rank.Perimeter && l.Passes < 1 && (l.LinksA.Any(l => l.Rank == Rank.Dividing) || l.LinksB.Any(l => l.Rank == Rank.Dividing)))) is not null)
            {
                int startLink = segment.LinksA.Any(l => l.Rank == Rank.Dividing) ? segment.IndexPointA : segment.IndexPointB;
                var indexChain = PullChainWithNoJunction(startLink, segment).ToArray(); 

                yield return indexChain;
                count++;
                if (count > _linkedSegments.Count)
                {
                    throw new InvalidOperationException($"Non terminating  perimeter loop pull.");
                }
            }
        }

        private IEnumerable<int[]> PullLoopsFromPerimeters()
        {
            var perimeterSegments = _linkedSegments.Where(l => l.Rank == Rank.Perimeter).ToArray();
            LinkedIndexSegment<G, T> segment;
            int index = 0;
            int count = 0;

            while ((segment = GetNextStart(perimeterSegments, ref index,
                (l) => l.Rank == Rank.Perimeter && l.Passes < 1)) is not null)
            {
                var indexChain = PullChainWithNoJunction(segment.IndexPointA, segment).ToArray();

                yield return indexChain;
                count++;
                if (count > _linkedSegments.Count)
                {
                    throw new InvalidOperationException($"Non terminating  perimeter loop pull.");
                }
            }
        }

        private LinkedIndexSegment<G, T> GetNextStart(LinkedIndexSegment<G, T>[] array, ref int index, Func<LinkedIndexSegment<G, T>, bool> conditional)
        {
            if (array.Length == 0) { return null; }
            int count = 0;
            while (count <= array.Length)
            {
                count++;
                index++;
                index = index % array.Length;
                var nextElement = array[index];
                if (conditional(nextElement)) { return nextElement; }
            }
            return null;
        }

        private IEnumerable<int> PullChainWithNoJunction(int startPoint, LinkedIndexSegment<G, T> startNode)
        {
            var firstPoint = startPoint;
            var firstSegment = startNode;
            var currentSegment = firstSegment;
            var currentPoint = firstPoint;

            yield return currentPoint;

            do
            {
                var nextPoint = GetOppositeIndex(currentSegment, currentPoint);
                currentSegment.Passes++;

                var forwardLinks = currentSegment.GetLinksAtOppositeIndex(currentPoint);
                if (forwardLinks.Count() != 1) {
                    yield return nextPoint;
                    yield break; 
                }

                if (currentPoint == nextPoint)
                {
                    throw new InvalidOperationException($"Non terminating no junction chain pull.");
                }
                currentPoint = nextPoint;
                if (firstPoint != currentPoint)
                {
                    yield return currentPoint;
                }
                currentSegment = forwardLinks.Single();
            }
            while (firstPoint != currentPoint);
        }

        private int GetOppositeIndex(LinkedIndexSegment<G, T> link, int headIndex)
        {
            if (link.IndexPointA == headIndex)
            {
                return link.IndexPointB;
            }
            if (link.IndexPointB == headIndex)
            {
                return link.IndexPointA;
            }
            throw new InvalidOperationException($"Opposite index of {headIndex} was not found.");
        }


        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private List<LinkedIndexSegment<G, T>> _linkedSegments;
        private List<int[]> _perimeterIndexLoops = new List<int[]>();
        private List<int[]> _indexLoops = new List<int[]>();
        private List<int[]> _indexSpurredLoops = new List<int[]>();
        private List<int[]> _indexSpurs = new List<int[]>();

        private List<SurfaceRayContainer<T>[]> _perimeterLoops;
        private List<SurfaceRayContainer<T>[]> _loops;
        private List<SurfaceRayContainer<T>[]> _spurredLoops;
        private List<SurfaceRayContainer<T>[]> _spurs;
        private List<int> _perimeterLoopGroupKeys = new List<int>();
        private List<int> _loopGroupKeys = new List<int>();
        private List<int> _spurredLoopGroupKeys = new List<int>();
        private List<int> _spurGroupKeys = new List<int>();
        private List<G> _perimeterLoopGroupObjects = new List<G>();
        private List<G> _loopGroupObjects = new List<G>();
        private List<G> _spurredLoopGroupObjects = new List<G>();
        private List<G> _spurGroupObjects = new List<G>();

        private ProtectedIndexedLoops _protectedIndexedLoops = new ProtectedIndexedLoops();
        public ProtectedIndexedLoops ProtectedIndexedLoops
        {
            get { return _protectedIndexedLoops; }
        }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get { return _referenceArray; } }
        public IReadOnlyList<SurfaceRayContainer<T>[]> PerimeterLoops
        {
            get
            {
                if (_perimeterLoops is null)
                {
                    _perimeterLoops = _perimeterIndexLoops.Select(l => l.Select(i => _referenceArray[i]).ToArray()).ToList();
                }
                return _perimeterLoops;
            }
        }
        public IReadOnlyList<SurfaceRayContainer<T>[]> Loops
        {
            get
            {
                if (_loops is null)
                {
                    _loops = _indexLoops.Select(l => l.Select(i => _referenceArray[i]).ToArray()).ToList();
                }
                return _loops;
            }
        }
        public IReadOnlyList<SurfaceRayContainer<T>[]> SpurredLoops
        {
            get
            {
                if (_spurredLoops is null)
                {
                    _spurredLoops = _indexSpurredLoops.Select(l => l.Select(i => _referenceArray[i]).ToArray()).ToList();
                }
                return _spurredLoops;
            }
        }
        public IReadOnlyList<SurfaceRayContainer<T>?[]> Spurs
        {
            get
            {
                if (_spurs is null)
                {
                    _spurs = _indexSpurs.Select(l => l.Select(i => _referenceArray[i]).ToArray()).ToList();
                }
                return _spurs;
            }
        }

        public List<int> PerimeterLoopGroupKeys { get { return _perimeterLoopGroupKeys; } }
        public IReadOnlyList<int> LoopGroupKeys { get { return _loopGroupKeys; } }
        public IReadOnlyList<int> SpurredLoopGroupKeys { get { return _spurredLoopGroupKeys; } }
        public IReadOnlyList<int> SpurGroupKeys { get { return _spurGroupKeys; } }

        public List<G> PerimeterLoopGroupObjects { get { return _perimeterLoopGroupObjects; } }
        public IReadOnlyList<G> LoopGroupObjects { get { return _loopGroupObjects; } }
        public IReadOnlyList<G> SpurredLoopGroupObjects { get { return _spurredLoopGroupObjects; } }
        public IReadOnlyList<G> SpurGroupObjects { get { return _spurGroupObjects; } }
    }
}
