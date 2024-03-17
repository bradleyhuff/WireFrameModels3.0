using BasicObjects.MathExtensions;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.Intermesh.Elastics
{
    internal class SurfaceElasticSegmentCollections<G> : ISurfaceSegmentCollections<G, int> where G : class
    {
        public SurfaceElasticSegmentCollections(IEnumerable<SurfaceSegmentSets<G, int>> segmentSets)
        {
            foreach (var segmentSet in segmentSets)
            {
                BuildCollections(segmentSet);
            }
        }

        public SurfaceElasticSegmentCollections(SurfaceSegmentSets<G, int> segmentSet)
        {
            BuildCollections(segmentSet);
        }

        private void BuildCollections(SurfaceSegmentSets<G, int> segmentSet)
        {
            var table = new Dictionary<int, SurfaceRayContainer<int>>();
            foreach (var segment in segmentSet.PerimeterSegments)
            {
                table[segment.A.Reference] = segment.A;
                table[segment.B.Reference] = segment.B;
            }
            foreach (var segment in segmentSet.DividingSegments)
            {
                table[segment.A.Reference] = segment.A;
                table[segment.B.Reference] = segment.B;
            }

            _referenceArray = table.Values.ToArray();
            var backTable = new Dictionary<int, int>();
            foreach (var element in _referenceArray.Select((e, i) => new { Element = e, Index = i })) { backTable[element.Element.Reference] = element.Index; }

            var keyTable = new Combination2Dictionary<bool>();
            foreach (var segment in segmentSet.PerimeterSegments)
            {
                var surfaceSegment = new LinkedIndexSurfaceSegment<G>(
                    backTable[segment.A.Reference], backTable[segment.B.Reference], Rank.Perimeter, segmentSet.GroupKey, segmentSet.GroupObject);
                AddToLinkSegments(keyTable, surfaceSegment);
            }
            foreach (var segment in segmentSet.DividingSegments)
            {
                var surfaceSegment = new LinkedIndexSurfaceSegment<G>(
                    backTable[segment.A.Reference], backTable[segment.B.Reference], Rank.Dividing, segmentSet.GroupKey, segmentSet.GroupObject);
                AddToLinkSegments(keyTable, surfaceSegment);
            }

            _protectedLinkedIndexSegments = new ProtectedLinkedIndexSegments<G, int>(_linkedIndexSegments);
        }

        private void AddToLinkSegments(Combination2Dictionary<bool> keyTable, LinkedIndexSurfaceSegment<G> surfaceSegment)
        {
            if (!keyTable.ContainsKey(surfaceSegment.Key))
            {
                _linkedIndexSegments.Add(surfaceSegment);
                keyTable[surfaceSegment.Key] = true;
            }
        }

        private ProtectedLinkedIndexSegments<G, int> _protectedLinkedIndexSegments;
        private SurfaceRayContainer<int>[] _referenceArray;
        private List<LinkedIndexSurfaceSegment<G>> _linkedIndexSegments = new List<LinkedIndexSurfaceSegment<G>>();
        private List<LinkedSurfaceSegment<G>> _linkedSegments = null;

        public IReadOnlyList<SurfaceRayContainer<int>> ReferenceArray
        {
            get { return _referenceArray; }
        }

        public IReadOnlyCollection<LinkedSurfaceSegment<G>> LinkedSegments
        {
            get
            {
                if (_linkedSegments is null)
                {
                    _linkedSegments = _linkedIndexSegments.Select(s =>
                        new LinkedSurfaceSegment<G>(_referenceArray[s.IndexPointA], _referenceArray[s.IndexPointB], s.Rank, s.GroupKey, s.GroupObject)).ToList();
                }
                return _linkedSegments;
            }
        }

        public ProtectedLinkedIndexSegments<G, int> ProtectedLinkedIndexSegments
        {
            get { return _protectedLinkedIndexSegments; }
        }
    }
}
