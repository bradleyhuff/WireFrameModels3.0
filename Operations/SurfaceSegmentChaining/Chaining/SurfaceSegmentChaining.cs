using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Chaining.Diagnostics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining
{
    internal class SurfaceSegmentChaining<G, T> : ISurfaceSegmentChaining<G, T> where G : class
    {
        private enum Traversal
        {
            NoSet,
            RightSide,
            LeftSide
        }
        private class InternalProtectedLinkedIndexSegments : ProtectedLinkedIndexSegments<G, T>
        {
            public IReadOnlyCollection<LinkedIndexSegment<G>> GetLinkedIndexSegments()
            {
                return LinkedIndexSegments ?? new LinkedIndexSegment<G>[0];
            }
        }

        private IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        private List<LinkedIndexSurfaceSegment<G, T>> _linkedSegments;
        private List<LinkedIndexSurfaceSegment<G, T>> _virtualLinkedSegments = new List<LinkedIndexSurfaceSegment<G, T>>();

        internal static ISurfaceSegmentChaining<G, T> Create(ISurfaceSegmentCollections<G, T> collections)
        {
            var linkedSegments = ProtectedLinkedIndexSegments<G, T>.Create<InternalProtectedLinkedIndexSegments>(collections.ProtectedLinkedIndexSegments).
                GetLinkedIndexSegments().Where(l => l.IndexPointA != l.IndexPointB).
                Select(l => new LinkedIndexSurfaceSegment<G, T>(l.GroupKey, l.GroupObject, l.IndexPointA, l.IndexPointB, l.Rank)).ToList();

            var chaining = new SurfaceSegmentChaining<G, T>();
            chaining.Run(collections.ReferenceArray, linkedSegments);
            return chaining;
        }

        protected SurfaceSegmentChaining() { }

        protected virtual void Run(IReadOnlyList<SurfaceRayContainer<T>> referenceArray, List<LinkedIndexSurfaceSegment<G, T>> linkedSegments)
        {
            _referenceArray = referenceArray;
            _linkedSegments = linkedSegments;
            BuildAssociationTable(_linkedSegments);
            AddEndPointSegments();
            BuildAssociationTable(_virtualLinkedSegments);
            BuildLinks();
            _linkedSegments.AddRange(_virtualLinkedSegments);

            PullFromPerimeters();
            PullFromJunctions();
            PullIsolatedLoops();

            _protectedIndexedLoops = new ProtectedIndexedLoops(_perimeterIndexLoops, _indexLoops, _indexSpurredLoops, _indexSpurs);
        }

        private Dictionary<int, List<LinkedIndexSurfaceSegment<G, T>>> _indexAssociationTable = new Dictionary<int, List<LinkedIndexSurfaceSegment<G, T>>>();

        private void BuildAssociationTable(IEnumerable<LinkedIndexSurfaceSegment<G, T>> linkedSegments)
        {
            foreach (var linkedSegment in linkedSegments)
            {
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointA)) { _indexAssociationTable[linkedSegment.IndexPointA] = new List<LinkedIndexSurfaceSegment<G, T>>(); }
                if (!_indexAssociationTable.ContainsKey(linkedSegment.IndexPointB)) { _indexAssociationTable[linkedSegment.IndexPointB] = new List<LinkedIndexSurfaceSegment<G, T>>(); }

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
            LinkedIndexSurfaceSegment<G, T> segment;
            int index = 0;
            int count = 0;

            while ((segment = GetNextStart(perimeterSegments, ref index, (l) => l.Rank == Rank.Perimeter && l.Passes < 1)) is not null)
            {
                _loggingElement.Start = segment.IndexPointB;
                _loggingElement.Note = "Pull from perimeters";
                var indexChain = PullChainWithNoJunction(segment.IndexPointA, segment, (l) => l.Rank == Rank.Perimeter && l.Passes < 1 && l.GroupKey == segment.GroupKey).ToArray();
                _perimeterIndexLoops.Add(indexChain);
                _perimeterLoopGroupKeys.Add(segment.GroupKey);
                _perimeterLoopGroupObjects.Add(segment.GroupObject);
                count++;
                if (count > _linkedSegments.Count)
                {
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Non terminating  perimeter loop pull.", _loggingElements, _referenceArray);
                }
            }
        }

        private LinkedIndexSurfaceSegment<G, T> GetNextStart(LinkedIndexSurfaceSegment<G, T>[] array, ref int index, Func<LinkedIndexSurfaceSegment<G, T>, bool> conditional)
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
        private Dictionary<int, SurfaceRayContainer<T>> _virtualPoints = new Dictionary<int, SurfaceRayContainer<T>>();

        private void AddEndPointSegments()
        {
            var endPointSegments = _indexAssociationTable.Values.Where(e => e.Count == 1).Select(l => l.Single());

            foreach (var endPointSegment in endPointSegments)
            {
                var rayA = _referenceArray[endPointSegment.IndexPointA];
                var rayB = _referenceArray[endPointSegment.IndexPointB];

                var rayM = new Ray3D(0.5 * (rayA.Point + rayB.Point), (rayA.Normal + rayB.Normal).Direction);
                var vectorAB = rayA.Point - rayB.Point;
                var offsetDirection = Vector3D.Cross(vectorAB.Direction, rayM.Normal).Direction;
                var rayOffset = new SurfaceRayContainer<T>(new Ray3D(rayM.Point + vectorAB.Magnitude * 1e-3 * offsetDirection, rayM.Normal), rayA.TriangleNormal, 0, rayA.Reference);
                virtualIndex--;
                _virtualPoints[virtualIndex] = rayOffset;


                var segmentA = new LinkedIndexSurfaceSegment<G, T>(endPointSegment.GroupKey, endPointSegment.GroupObject, endPointSegment.IndexPointA, virtualIndex, endPointSegment.Rank);
                var segmentB = new LinkedIndexSurfaceSegment<G, T>(endPointSegment.GroupKey, endPointSegment.GroupObject, virtualIndex, endPointSegment.IndexPointB, endPointSegment.Rank);

                _virtualLinkedSegments.Add(segmentA);
                _virtualLinkedSegments.Add(segmentB);
                PrepassLoop(endPointSegment, segmentA, segmentB);
            }
        }

        private void PrepassLoop(params LinkedIndexSurfaceSegment<G, T>[] loopSegments)
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
            LinkedIndexSurfaceSegment<G, T> segment;

            int index = 0;
            int count = 0;
            while ((segment = GetNextStart(junctionSegments, ref index, (l) => (l.LinksA.Count > 1 || l.LinksB.Count > 1) && l.Passes < 2 && l.Rank == Rank.Dividing)) is not null)
            {
                _loggingElement.Start = segment.GetOppositeJunctionPoint();
                _loggingElement.Note = "Pull from junctions";
                var indexChain = PullChain(segment.GetAJunctionPoint(), segment, (l) => l.GroupKey == segment.GroupKey).ToArray();
                SetLoops(indexChain, segment);
                count++;
                if (count > _linkedSegments.Count)
                {
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Non terminating junction chain pull.", _loggingElements, _referenceArray);
                }
            }
        }

        private void SetLoops(int[] indexChain, LinkedIndexSurfaceSegment<G, T> segment)
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
            LinkedIndexSurfaceSegment<G, T> segment;
            var isolatedLoops = _linkedSegments.Where(j => j.Passes < 1).ToArray();
            int index = 0;
            int count = 0;
            while ((segment = GetNextStart(isolatedLoops, ref index, (l) => l.Passes < 1)) is not null)
            {
                _loggingElement.Start = segment.IndexPointB;
                _loggingElement.Note = "Pull isolated loops";
                var indexChain = PullChainWithNoJunction(segment.IndexPointA, segment, (l) => l.Passes < 1 && l.GroupKey == segment.GroupKey).ToArray();
                if (indexChain.Length == 1 && indexChain[0] == segment.IndexPointA) { return; }
                _indexLoops.Add(indexChain);
                _loopGroupKeys.Add(segment.GroupKey);
                _loopGroupObjects.Add(segment.GroupObject);
                count++;
                if (count > _linkedSegments.Count)
                {
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Non terminating isolated loop pull.", _loggingElements, _referenceArray);
                }
            }
        }

        private IEnumerable<int> PullChainWithNoJunction(int startPoint, LinkedIndexSurfaceSegment<G, T> startNode, Func<LinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            var firstPoint = startPoint;
            var firstSegment = startNode;
            var currentSegment = firstSegment;
            var currentPoint = firstPoint;

            _loggingElement.Chaining.Add(currentPoint);
            yield return currentPoint;

            LinkedIndexSurfaceSegment<G, T> nextSegment = null;
            do
            {
                var forwardLinks = currentSegment.GetLinksAtIndex(currentPoint).Where(l => linkConstraint(l));
                nextSegment = forwardLinks.SingleOrDefault();
                if (nextSegment is null) { yield break; }

                var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
                if (currentPoint == nextPoint)
                {
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Non terminating no junction chain pull.", _loggingElements, _referenceArray);
                }
                currentPoint = nextPoint;
                nextSegment.Passes++;
                if (firstPoint != currentPoint)
                {
                    _loggingElement.Chaining.Add(currentPoint);
                    yield return currentPoint;
                }
                currentSegment.AddTraversalPair(nextSegment);
                currentSegment = nextSegment;
            }
            while (firstPoint != currentPoint);

            _loggingElements.Add(_loggingElement);
            _loggingElement = new LoggingElement();
        }

        private IEnumerable<int> PullChain(int startPoint, LinkedIndexSurfaceSegment<G, T> startNode, Func<LinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            var firstPoint = startPoint;
            var firstSegment = startNode;
            var secondPoint = -1;
            var currentSegment = firstSegment;
            var currentPoint = firstPoint;

            _loggingElement.Chaining.Add(currentPoint);
            yield return currentPoint;

            LinkedIndexSurfaceSegment<G, T> nextSegment = null;
            Traversal traversal = Traversal.NoSet;
            bool continueChain;

            do
            {
                nextSegment = GetNextSegment(currentSegment, currentPoint, ref traversal, linkConstraint, false);

                var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
                // set second point here...
                if (secondPoint < 0) { secondPoint = nextPoint; }
                if (currentPoint == nextPoint)
                {
                    throw new ChainingException<T>($"Non terminating chain pull.", _loggingElements, _referenceArray);
                }

                currentPoint = nextPoint;
                nextSegment.Passes++;
                currentSegment.AddTraversalPair(nextSegment);
                currentSegment = nextSegment;
                continueChain = ContinueChain(firstPoint, secondPoint, currentPoint, currentSegment, traversal, linkConstraint);

                if (continueChain)
                {
                    _loggingElement.Chaining.Add(currentPoint);
                    yield return currentPoint;
                }
            }
            while (continueChain);

            _loggingElements.Add(_loggingElement);
            _loggingElement = new LoggingElement();
        }

        private LinkedIndexSurfaceSegment<G, T> GetNextSegment(
            LinkedIndexSurfaceSegment<G, T> currentSegment,
            int currentPoint, ref Traversal traversal,
            Func<LinkedIndexSurfaceSegment<G, T>, bool> linkConstraint,
            bool allowCompletedSegments,
            bool throwExceptions = true)
        {
            var forwardLinks = currentSegment.GetLinksAtIndex(currentPoint).Where(l => linkConstraint(l));//.Where(l => l.Passes < 2);
            //if (!allowCompletedSegments) { forwardLinks = forwardLinks.Where(l => l.Passes < 2); }
            //if (!forwardLinks.Any()) { return null; }
            var nextSegment = forwardLinks.First();
            if (forwardLinks.Count() > 1)
            {
                Traversal nextTraversal = traversal;

                GetDirectionalLinks(currentPoint, currentSegment, forwardLinks,
                    out LinkedIndexSurfaceSegment<G, T> leftMostLink, 
                    out LinkedIndexSurfaceSegment<G, T> rightMostLink);

                var leftLinkTraversed = currentSegment.WasTraversed(leftMostLink);
                var rightLinkTraversed = currentSegment.WasTraversed(rightMostLink);

                if (leftLinkTraversed && rightLinkTraversed)
                {
                    if (!throwExceptions) { return null; }
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Both left and right sides have already been traversed.", _loggingElements, _referenceArray);
                }

                if (rightLinkTraversed) { nextTraversal = Traversal.LeftSide; }
                if (leftLinkTraversed) { nextTraversal = Traversal.RightSide; }

                if (traversal != nextTraversal && nextTraversal != Traversal.NoSet && traversal != Traversal.NoSet)
                {
                    if (!throwExceptions) { return null; }
                    _loggingElements.Add(_loggingElement);
                    throw new ChainingException<T>($"Traversal {traversal} -> next traversal {nextTraversal}.  Both traversals must equal.", _loggingElements, _referenceArray);
                }
                if (nextTraversal == Traversal.NoSet && leftMostLink.Key != rightMostLink.Key) { nextTraversal = Traversal.RightSide; }
                traversal = nextTraversal;

                nextSegment = traversal == Traversal.LeftSide ? leftMostLink : rightMostLink;
            }
            return nextSegment;
        }

        private bool ContinueChain(int firstPoint, int secondPoint, int currentPoint,
            LinkedIndexSurfaceSegment<G, T> currentSegment, Traversal traversal,
            Func<LinkedIndexSurfaceSegment<G, T>, bool> linkConstraint)
        {
            if (firstPoint != currentPoint) { return true; }

            var nextSegment = GetNextSegment(currentSegment, currentPoint, ref traversal, linkConstraint, true, throwExceptions: false);
            if (nextSegment is null) { return false; }
            var nextPoint = GetOppositeIndex(nextSegment, currentPoint);
            return secondPoint != nextPoint;
        }

        private void GetDirectionalLinks(int currentPoint, LinkedIndexSurfaceSegment<G, T> current, IEnumerable<LinkedIndexSurfaceSegment<G, T>> forwardLinks,
            out LinkedIndexSurfaceSegment<G, T> leftMostLink, out LinkedIndexSurfaceSegment<G, T> rightMostLink)
        {
            LinkedIndexSurfaceSegment<G, T> minOption = null;
            LinkedIndexSurfaceSegment<G, T> maxOption = null;
            double minAngle = 2 * Math.PI;
            double maxAngle = 0;

            var head = _referenceArray.ElementAtOrDefault(currentPoint) ?? _virtualPoints[currentPoint];

            int tailIndex = current.Opposite(currentPoint);
            Ray3D tail = _referenceArray.ElementAtOrDefault(tailIndex) ?? _virtualPoints[tailIndex];
            Vector3D tail2 = tail.Point - head.Point;
            Vector3D tailDirection = (tail.Point - head.Point).Direction;

            foreach (var forwardLink in forwardLinks)
            {
                var forwardLinkIndex = forwardLink.Opposite(currentPoint);
                Ray3D link = _referenceArray.ElementAtOrDefault(forwardLinkIndex) ?? _virtualPoints[forwardLinkIndex];
                Vector3D linkDirection = (link.Point - head.Point).Direction;
                Vector3D link2 = (link.Point - head.Point);

                var angle = Vector3D.SignedAngle(head.TriangleNormal, tailDirection, linkDirection);
                //Console.WriteLine($"Angle 0 found. ");
                if (angle == 0)
                {
                    leftMostLink = forwardLink;
                    rightMostLink = forwardLink;
                    return;
                }
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

            //if (backLink is null && minOption.Key == maxOption.Key)
            //{
            //    _loggingElements.Add(_loggingElement);
            //    throw new ChainingException<T>($"Forward link options are matching. {minOption.Key}", _loggingElements, _referenceArray);
            //}

            leftMostLink = maxOption;
            rightMostLink = minOption;
        }

        private int GetOppositeIndex(LinkedIndexSurfaceSegment<G, T> link, int headIndex)
        {
            if (link.IndexPointA == headIndex)
            {
                return link.IndexPointB;
            }
            if (link.IndexPointB == headIndex)
            {
                return link.IndexPointA;
            }
            _loggingElements.Add(_loggingElement);
            throw new ChainingException<T>($"Opposite index of {headIndex} was not found.", _loggingElements, _referenceArray);
        }

        private List<int[]> _perimeterIndexLoops = new List<int[]>();
        private List<int[]> _indexLoops = new List<int[]>();
        private List<int[]> _indexSpurredLoops = new List<int[]>();
        private List<int[]> _indexSpurs = new List<int[]>();
        private List<LoggingElement> _loggingElements = new List<LoggingElement>();
        private LoggingElement _loggingElement = new LoggingElement();

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
