using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal partial class SurfaceSegmentChaining<G, T> : ISurfaceSegmentChaining<G, T> where G : class
    {
        private enum Traversal
        {
            NoSet,
            RightSide,
            LeftSide
        }
        private class InternalProtectedLinkedIndexSegments : ProtectedLinkedIndexSegments<G, T>
        {
            public IReadOnlyCollection<LinkedIndexSurfaceSegment<G>> GetLinkedIndexSegments()
            {
                return LinkedIndexSegments ?? new LinkedIndexSurfaceSegment<G>[0];
            }
        }

        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private List<InternalLinkedIndexSurfaceSegment<G, T>> _linkedSegments;
        private List<InternalLinkedIndexSurfaceSegment<G, T>> _virtualLinkedSegments = new List<InternalLinkedIndexSurfaceSegment<G, T>>();

        internal static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentCollections<G, T> collections)
        {
            return new SurfaceSegmentChaining<G, T>(collections);
        }

        private SurfaceSegmentChaining(ISurfaceSegmentCollections<G, T> collections) : this(collections.ReferenceArray,
            ProtectedLinkedIndexSegments<G, T>.Create<InternalProtectedLinkedIndexSegments>(collections.ProtectedLinkedIndexSegments).
            GetLinkedIndexSegments().Where(l => l.IndexPointA != l.IndexPointB).
            Select(l => new InternalLinkedIndexSurfaceSegment<G, T>(l.GroupKey, l.GroupObject, l.IndexPointA, l.IndexPointB, l.Rank)).ToList())
        { }

        internal SurfaceSegmentChaining(IReadOnlyList<SurfaceRayContainer<T>> referenceArray, List<InternalLinkedIndexSurfaceSegment<G, T>> linkedSegments)
        {
            _referenceArray = referenceArray;
            _linkedSegments = linkedSegments;
            BuildAssociationTable(_linkedSegments);
            AddEndPointSegments();
            BuildAssociationTable(_virtualLinkedSegments);
            BuildLinks();
            _linkedSegments.AddRange(_virtualLinkedSegments);
            SetJunctionAngles();

            PullFromPerimeters();
            PullFromJunctions();
            PullIsolatedLoops();

            _protectedIndexedLoops = new ProtectedIndexedLoops(_perimeterIndexLoops, _indexLoops, _indexSpurredLoops, _indexSpurs);
        }

        private Dictionary<int, List<InternalLinkedIndexSurfaceSegment<G, T>>> _indexAssociationTable = new Dictionary<int, List<InternalLinkedIndexSurfaceSegment<G, T>>>();

        private void BuildAssociationTable(IEnumerable<InternalLinkedIndexSurfaceSegment<G, T>> linkedSegments)
        {
            foreach (var linkedSegment in linkedSegments)
            {
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointA)) { _indexAssociationTable[linkedSegment.IndexPointA] = new List<InternalLinkedIndexSurfaceSegment<G, T>>(); }
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointB)) { _indexAssociationTable[linkedSegment.IndexPointB] = new List<InternalLinkedIndexSurfaceSegment<G, T>>(); }

                _indexAssociationTable[linkedSegment.IndexPointA].Add(linkedSegment);
                _indexAssociationTable[linkedSegment.IndexPointB].Add(linkedSegment);
            }
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
        private void PullFromPerimeters()
        {
            var perimeterSegments = _linkedSegments.Where(l => l.Rank == Rank.Perimeter).ToArray();
            InternalLinkedIndexSurfaceSegment<G, T> segment;
            int index = 0;
            int count = 0;

            while ((segment = GetNextStart(perimeterSegments, ref index, (l) => l.Rank == Rank.Perimeter && l.Passes < 1)) is not null)
            {
                var indexChain = PullChainWithNoJunction(segment.IndexPointA, segment, (l) => l.Rank == Rank.Perimeter && l.Passes < 1 && l.GroupKey == segment.GroupKey).ToArray();
                _perimeterIndexLoops.Add(indexChain);
                _perimeterLoopGroupKeys.Add(segment.GroupKey);
                _perimeterLoopGroupObjects.Add(segment.GroupObject);
                count++;
                if (count > _linkedSegments.Count)
                {
                    throw new InvalidOperationException($"Non terminating  perimeter loop pull.");
                }
            }
        }

        private InternalLinkedIndexSurfaceSegment<G, T> GetNextStart(InternalLinkedIndexSurfaceSegment<G, T>[] array, ref int index, Func<InternalLinkedIndexSurfaceSegment<G, T>, bool> conditional)
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

        private int virtualIndex = 0;
        private Dictionary<int, Ray3D> _virtualPoints = new Dictionary<int, Ray3D>();

        private void AddEndPointSegments()
        {
            var endPointSegments = _indexAssociationTable.Values.Where(e => e.Count == 1).Select(l => l.Single());

            foreach (var endPointSegment in endPointSegments)
            {
                var rayA = _referenceArray[endPointSegment.IndexPointA];
                var rayB = _referenceArray[endPointSegment.IndexPointB];
                //calculate midpoint between rayA and rayB with small height offset
                var rayM = new Ray3D(0.5 * (rayA.Point + rayB.Point), (rayA.Normal + rayB.Normal).Direction);
                var vectorAB = rayA.Point - rayB.Point;
                var offsetDirection = Vector3D.Cross(vectorAB.Direction, rayM.Normal).Direction;
                var rayOffset = new Ray3D(rayM.Point + vectorAB.Magnitude * 1e-3 * offsetDirection, rayM.Normal);
                virtualIndex--;
                _virtualPoints[virtualIndex] = rayOffset;


                var segmentA = new InternalLinkedIndexSurfaceSegment<G, T>(endPointSegment.GroupKey, endPointSegment.GroupObject, endPointSegment.IndexPointA, virtualIndex, endPointSegment.Rank);
                var segmentB = new InternalLinkedIndexSurfaceSegment<G, T>(endPointSegment.GroupKey, endPointSegment.GroupObject, virtualIndex, endPointSegment.IndexPointB, endPointSegment.Rank);

                _virtualLinkedSegments.Add(segmentA);
                _virtualLinkedSegments.Add(segmentB);
                PrepassLoop(endPointSegment, segmentA, segmentB);
            }
        }

        private void PrepassLoop(params InternalLinkedIndexSurfaceSegment<G, T>[] loopSegments)
        {
            for (int i = 0; i < loopSegments.Length; i++)
            {
                loopSegments[i].Passes++;
                loopSegments[i].AddTraversalPair(loopSegments[(i + 1) % loopSegments.Length]);
            }
        }

        private void PullFromJunctions()
        {
            var junctionSegments = _linkedSegments.Where(j => (j.LinksA.Count > 1 || j.LinksB.Count > 1) && j.Passes < 2 && j.Rank == Rank.Dividing).ToArray();
            InternalLinkedIndexSurfaceSegment<G, T> segment;

            int index = 0;
            int count = 0;
            while ((segment = GetNextStart(junctionSegments, ref index, (l) => (l.LinksA.Count > 1 || l.LinksB.Count > 1) && l.Passes < 2 && l.Rank == Rank.Dividing)) is not null)
            {
                var indexChain = PullChain(segment.GetAJunctionPoint(), segment, (l) => l.GroupKey == segment.GroupKey).ToArray();
                SetLoops(indexChain, segment);
                count++;
                if (count > _linkedSegments.Count)
                {
                    throw new InvalidOperationException($"Non terminating junction chain pull.");
                }
            }
        }

        private void SetLoops(int[] indexChain, InternalLinkedIndexSurfaceSegment<G, T> segment)
        {
            if (indexChain.Any(i => i < 0))
            {
                _indexSpurredLoops.Add(indexChain.Where(i => i >= 0).ToArray());
                _spurredLoopGroupKeys.Add(segment.GroupKey);
                _spurredLoopGroupObjects.Add(segment.GroupObject);

                var rotate = indexChain.RotateToFirst((v, i) => v < 0).ToArray();
                var groups = rotate.SplitAt(i => i < 0);
                foreach (var group in groups)
                {
                    _indexSpurs.Add(group);
                    _spurGroupKeys.Add(segment.GroupKey);
                    _spurGroupObjects.Add(segment.GroupObject);
                }
            }
            else
            {
                _indexLoops.Add(indexChain);
                _loopGroupKeys.Add(segment.GroupKey);
                _loopGroupObjects.Add(segment.GroupObject);
            }

        }

        private void PullIsolatedLoops()
        {
            InternalLinkedIndexSurfaceSegment<G, T> segment;
            var isolatedLoops = _linkedSegments.Where(j => j.Passes < 1).ToArray();
            int index = 0;
            int count = 0;
            while ((segment = GetNextStart(isolatedLoops, ref index, (l) => l.Passes < 1)) is not null)
            {
                var indexChain = PullChainWithNoJunction(segment.IndexPointA, segment, (l) => l.Passes < 1 && l.GroupKey == segment.GroupKey).ToArray();
                if (indexChain.Length == 1 && indexChain[0] == segment.IndexPointA) { return; }
                _indexLoops.Add(indexChain);
                _loopGroupKeys.Add(segment.GroupKey);
                _loopGroupObjects.Add(segment.GroupObject);
                count++;
                if (count > _linkedSegments.Count)
                {
                    throw new InvalidOperationException($"Non terminating isolated loop pull.");
                }
            }
        }

        private IEnumerable<int> PullChainWithNoJunction(int startPoint, InternalLinkedIndexSurfaceSegment<G, T> startNode, Func<InternalLinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            var firstPoint = startPoint;
            var firstSegment = startNode;
            var currentSegment = firstSegment;
            var currentPoint = firstPoint;

            yield return currentPoint;

            InternalLinkedIndexSurfaceSegment<G, T> nextSegment = null;
            do
            {
                var forwardLinks = currentSegment.GetLinksAtIndex(currentPoint).Where(l => linkConstraint(l));
                nextSegment = forwardLinks.SingleOrDefault();
                if (nextSegment is null) { yield break; }

                var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
                if (currentPoint == nextPoint)
                {
                    throw new InvalidOperationException($"Non-terminating no junction chain pull.");
                }
                currentPoint = nextPoint;
                nextSegment.Passes++;
                if (firstPoint != currentPoint)
                {
                    yield return currentPoint;
                }
                currentSegment.AddTraversalPair(nextSegment);
                currentSegment = nextSegment;
            }
            while (firstPoint != currentPoint);
        }

        private IEnumerable<int> PullChain(int startPoint, InternalLinkedIndexSurfaceSegment<G, T> startNode, Func<InternalLinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            var firstPoint = startPoint;
            var firstSegment = startNode;
            var secondPoint = -1;
            var currentSegment = firstSegment;
            var currentPoint = firstPoint;

            yield return currentPoint;

            InternalLinkedIndexSurfaceSegment<G, T> nextSegment = null;
            Traversal traversal = Traversal.NoSet;
            bool continueChain;

            do
            {
                nextSegment = GetNextSegment(currentSegment, currentPoint, ref traversal, linkConstraint);

                var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
                // set second point here...
                if (secondPoint < 0) { secondPoint = nextPoint; }
                if (currentPoint == nextPoint)
                {
                    throw new InvalidOperationException($"Non-terminating chain pull.");
                }
                currentPoint = nextPoint;
                nextSegment.Passes++;
                currentSegment.AddTraversalPair(nextSegment);
                currentSegment = nextSegment;
                continueChain = ContinueChain(firstPoint, secondPoint, currentPoint, currentSegment, traversal, linkConstraint);
                if (continueChain)
                {
                    yield return currentPoint;
                }
            }
            while (continueChain);
        }

        private InternalLinkedIndexSurfaceSegment<G, T> GetNextSegment(
            InternalLinkedIndexSurfaceSegment<G, T> currentSegment,
            int currentPoint, ref Traversal traversal,
            Func<InternalLinkedIndexSurfaceSegment<G, T>, bool> linkConstraint, bool throwExceptions = true)
        {
            var forwardLinks = currentSegment.GetLinksAtIndex(currentPoint).Where(l => linkConstraint(l));
            var nextSegment = forwardLinks.First();
            if (forwardLinks.Count() > 1)
            {
                Traversal nextTraversal = traversal;
                InternalLinkedIndexSurfaceSegment<G, T> leftMostLink = null;
                InternalLinkedIndexSurfaceSegment<G, T> rightMostLink = null;

                GetDirectionalLinks(currentPoint, currentSegment, forwardLinks, out leftMostLink, out rightMostLink);
                var leftLinkTraversed = currentSegment.WasTraversed(leftMostLink);
                var rightLinkTraversed = currentSegment.WasTraversed(rightMostLink);

                if (leftLinkTraversed && rightLinkTraversed)
                {
                    if (!throwExceptions) { return null; }
                    throw new InvalidOperationException("Both left and right sides have already been traversed.");
                }

                if (rightLinkTraversed) { nextTraversal = Traversal.LeftSide; }
                if (leftLinkTraversed) { nextTraversal = Traversal.RightSide; }

                if (traversal != nextTraversal && nextTraversal != Traversal.NoSet && traversal != Traversal.NoSet)
                {
                    if (!throwExceptions) { return null; }
                    throw new InvalidOperationException($"Traversal {traversal} -> next traversal {nextTraversal}.  Both traversals must equal.");
                }
                if (nextTraversal == Traversal.NoSet) { nextTraversal = Traversal.RightSide; }
                traversal = nextTraversal;

                nextSegment = traversal == Traversal.LeftSide ? leftMostLink : rightMostLink;
            }
            return nextSegment;
        }

        private bool ContinueChain(int firstPoint, int secondPoint, int currentPoint,
            InternalLinkedIndexSurfaceSegment<G, T> currentSegment, Traversal traversal,
            Func<InternalLinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            if (firstPoint != currentPoint) { return true; }

            var nextSegment = GetNextSegment(currentSegment, currentPoint, ref traversal, linkConstraint, throwExceptions: false);
            if (nextSegment is null) { return false; }
            var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
            return secondPoint != nextPoint;
        }

        private void GetDirectionalLinks(int currentPoint, InternalLinkedIndexSurfaceSegment<G, T> current, IEnumerable<InternalLinkedIndexSurfaceSegment<G, T>> forwardLinks,
            out InternalLinkedIndexSurfaceSegment<G, T> leftMostLink, out InternalLinkedIndexSurfaceSegment<G, T> rightMostLink)
        {
            InternalLinkedIndexSurfaceSegment<G, T> minOption = null;
            InternalLinkedIndexSurfaceSegment<G, T> maxOption = null;
            double minAngle = 2 * Math.PI;
            double maxAngle = 0;

            double controlAngle = current.GetJunctionAngleAtIndex(currentPoint) ?? 0;

            foreach (var forwardLink in forwardLinks)
            {
                var angle = (forwardLink.GetJunctionAngleAtIndex(currentPoint) ?? 0) - controlAngle;
                if (angle < 0) { angle += 2 * Math.PI; }

                if (angle < minAngle)
                {
                    minAngle = angle;
                    minOption = forwardLink;
                }
                if (angle > maxAngle)
                {
                    maxAngle = angle;
                    maxOption = forwardLink;
                }
            }

            leftMostLink = maxOption;
            rightMostLink = minOption;
        }

        private void SetJunctionAngles()
        {
            foreach (var segment in _linkedSegments)
            {
                if (segment.LinksA.Count > 1 && segment.JunctionAngleA is null)
                {
                    var trailingLinkIndicies = segment.LinksA.Select(l => GetOppositeIndex(l, segment.IndexPointA)).ToArray();
                    var angles = CalculateLinkAngles(segment.IndexPointA, segment.IndexPointB, trailingLinkIndicies).ToArray();

                    for (int i = 0; i < segment.LinksA.Count; i++)
                    {
                        SetJunctionAngle(segment.LinksA[i], angles[i], segment.IndexPointA);
                    }
                    segment.JunctionAngleA = 0;
                }
                if (segment.LinksB.Count > 1 && segment.JunctionAngleB is null)
                {
                    var trailingLinkIndicies = segment.LinksB.Select(l => GetOppositeIndex(l, segment.IndexPointB)).ToArray();
                    var angles = CalculateLinkAngles(segment.IndexPointB, segment.IndexPointA, trailingLinkIndicies).ToArray();

                    for (int i = 0; i < segment.LinksB.Count; i++)
                    {
                        SetJunctionAngle(segment.LinksB[i], angles[i], segment.IndexPointB);
                    }
                    segment.JunctionAngleB = 0;
                }
            }
        }

        private int GetOppositeIndex(InternalLinkedIndexSurfaceSegment<G, T> link, int headIndex)
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

        private void SetJunctionAngle(InternalLinkedIndexSurfaceSegment<G, T> link, double angle, int headIndex)
        {
            if (link.IndexPointA == headIndex)
            {
                if (link.JunctionAngleA != null) { Console.WriteLine($"Angle A was already set. Before {link.JunctionAngleA} After {angle}"); }
                link.JunctionAngleA = angle;
            }
            if (link.IndexPointB == headIndex)
            {
                if (link.JunctionAngleB != null) { Console.WriteLine($"Angle B was already set. Before {link.JunctionAngleB} After {angle}"); }
                link.JunctionAngleB = angle;
            }
        }

        private IEnumerable<double> CalculateLinkAngles(int headLinkIndex, int tailLinkIndex, int[] trailingLinkIndicies)
        {
            Ray3D head = _referenceArray.ElementAtOrDefault(headLinkIndex) ?? _virtualPoints[headLinkIndex];
            Plane plane = new Plane(head);
            Point3D headPoint = head.Point;
            Vector3D headDirection = head.Normal;

            Ray3D tail = _referenceArray.ElementAtOrDefault(tailLinkIndex) ?? _virtualPoints[tailLinkIndex];
            Point3D tailPoint = plane.Projection(tail.Point);
            Vector3D tailDirection = (tailPoint - headPoint).Direction;
            for (int i = 0; i < trailingLinkIndicies.Length; i++)
            {
                Ray3D link = _referenceArray.ElementAtOrDefault(trailingLinkIndicies[i]) ?? _virtualPoints[trailingLinkIndicies[i]];
                Point3D linkPoint = plane.Projection(link.Point);
                Vector3D linkDirection = (linkPoint - headPoint).Direction;
                double angle = Vector3D.SignedAngle(headDirection, tailDirection, linkDirection);
                if (angle < 0) { angle += 2 * Math.PI; }
                yield return angle;
            }
        }

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

        protected List<int[]> PerimeterIndexLoops
        {
            get { return _perimeterIndexLoops; }
        }

        protected List<int[]> IndexLoops
        {
            get { return _indexLoops; }
        }
        protected List<int[]> IndexSpurredLoops
        {
            get { return _indexSpurredLoops; }
        }

        protected List<int[]> IndexSpurs
        {
            get { return _indexSpurs; }
        }

        public IReadOnlyList<int> PerimeterLoopGroupKeys { get { return _perimeterLoopGroupKeys; } }
        public IReadOnlyList<int> LoopGroupKeys { get { return _loopGroupKeys; } }
        public IReadOnlyList<int> SpurredLoopGroupKeys { get { return _spurredLoopGroupKeys; } }
        public IReadOnlyList<int> SpurGroupKeys { get { return _spurGroupKeys; } }

        public IReadOnlyList<G> PerimeterLoopGroupObjects { get { return _perimeterLoopGroupObjects; } }
        public IReadOnlyList<G> LoopGroupObjects { get { return _loopGroupObjects; } }
        public IReadOnlyList<G> SpurredLoopGroupObjects { get { return _spurredLoopGroupObjects; } }
        public IReadOnlyList<G> SpurGroupObjects { get { return _spurGroupObjects; } }
    }
}
