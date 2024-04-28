using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.Filling.Interfaces;
using Operations.Regions;

namespace Operations.Filling.Abstracts
{
    internal abstract class AbstractLoop : IFillingLoop, IBox
    {
        private static int _id = 0;
        public AbstractLoop(IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID)
        {
            Id = _id++;
            _triangleID = triangleID;
            IndexLoop = indexLoop.ToList();
            _referenceArray = referenceArray;

            _tracker = new IndexTracker(indexLoop.Select((p, i) => i));
            _passOver = _tracker.Tracking.Select(i => new KeyValuePair<int, bool>(i, false)).ToDictionary(p => p.Key, p => p.Value);
        }
        protected IReadOnlyList<Ray3D> _referenceArray;
        FillingSegment[] _loopSegments;
        List<IFillingLoop> _internalLoops = new List<IFillingLoop>();
        IndexTracker _tracker;
        Dictionary<int, bool> _passOver = new Dictionary<int, bool>();
        int _startCount = 0;
        int _passOverCount = 0;
        List<IFillingLoop> _unmergedInternalLoops = new List<IFillingLoop>();
        List<IFillingLoop> _mergedInternalLoops = new List<IFillingLoop>();
        Rectangle3D _box;
        IReadOnlyList<Point3D> _projectedPoints;
        List<IndexSurfaceTriangle> _indexedFillTriangles;
        int _triangleID;

        public int Id { get; }

        public Rectangle3D Box
        {
            get
            {
                _box = Rectangle3D.Containing(ProjectedLoopPoints.ToArray());
                return _box;
            }
        }

        public List<int> IndexLoop { get; private set; }

        protected abstract IReadOnlyList<Point3D> GetProjectedLoopPoints();

        public IReadOnlyList<Point3D> ProjectedLoopPoints
        {
            get
            {
                if (_projectedPoints is null)
                {
                    _projectedPoints = GetProjectedLoopPoints();
                }
                return _projectedPoints;
            }
        }

        public IReadOnlyCollection<FillingSegment> LoopSegments
        {
            get
            {
                if (_loopSegments is null)
                {
                    _loopSegments = FillingSegment.GetLoop(this, ProjectedLoopPoints.ToArray()).ToArray();
                }
                return _loopSegments;
            }
        }

        protected abstract IFillingRegions RegionOutLines { get; }
        protected abstract IFillingRegions RegionInternal { get; }

        private Region OutLineRegionOfPoint(Point3D point)
        {
            return RegionOutLines.RegionOfProjectedPoint(point);
        }

        public List<IFillingLoop> InternalLoops
        {
            get { return _internalLoops; }
            set
            {
                _internalLoops = value ?? new List<IFillingLoop>();
                _unmergedInternalLoops = _internalLoops.ToList();
                ResetWithInternalLoops();
            }
        }
        protected abstract void ResetWithInternalLoops();

        public bool OutLineContainsLoop(IFillingLoop input)
        {
            var difference = input.IndexLoop.Difference(IndexLoop);
            if (!difference.Any()) { return false; }
            var testPoint = _referenceArray[difference.First()].Point;

            return OutLineRegionOfPoint(testPoint) == Region.Interior;
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
            _startCount = _tracker.Count;
            _passOverCount = 0;
            _indexedFillTriangles = new List<IndexSurfaceTriangle>();
            if (InternalLoops.Any(l => l.IndexLoop.Intersect(IndexLoop).Any())) { return; } // All internal loops must not touch the loop.

            while (true)
            {
                if (_tracker.Count < 3) return;//Fully filled

                int leftIndex = _tracker.LookAheadStep(-1);
                int index = _tracker.LookAheadStep(0);
                int rightIndex = _tracker.LookAheadStep(1);

                if (_tracker.Count == 3)
                {
                    if (EnclosesInternalLoops(leftIndex, index, rightIndex))
                    {
                        if (!MergeWithAnEnclosedInternalLoopAndAdvance(leftIndex, index, rightIndex))
                        {
                            AdvanceAndPassOver(index);
                            if (TrackingError()) break;
                        }
                        continue;
                    }

                    AdvanceAndFill(leftIndex, index, rightIndex);
                    return; //Fully filled
                }

                if (_passOver[index])
                {
                    AdvanceAndPassOver(index);
                    if (TrackingError()) break;
                    continue;
                }

                if (HasIntersections(leftIndex, rightIndex))
                {
                    if (!MergeWithIntersectingInternalLoop(leftIndex, index, rightIndex))
                    {
                        AdvanceAndPassOver(index);
                        if (TrackingError()) break;
                    }
                    continue;
                }

                if (EnclosesInternalLoops(leftIndex, index, rightIndex))
                {
                    if (!MergeWithAnEnclosedInternalLoopAndAdvance(leftIndex, index, rightIndex))
                    {
                        AdvanceAndPassOver(index);
                        if (TrackingError()) break;
                    }
                    continue;
                }

                if (CrossesInterior(leftIndex, rightIndex))
                {
                    AdvanceAndFill(leftIndex, index, rightIndex);
                }

                AdvanceAndPassOver(index);
                if (TrackingError()) break;
            }

            _passOverCount = 0;
            while (true)
            {
                if (_tracker.Count < 3) return;//Fully filled


                int leftIndex = _tracker.LookAheadStep(-1);
                int index = _tracker.LookAheadStep(0);
                int rightIndex = _tracker.LookAheadStep(1);

                if (_tracker.Count == 3)
                {
                    AdvanceAndFill(leftIndex, index, rightIndex);
                    return; //Fully filled
                }

                if (IsAtBoundary(leftIndex, rightIndex))
                {
                    AdvanceAndFill(leftIndex, index, rightIndex);
                }
                AdvanceAndPassOver(index);
                if (TrackingError(true)) break;
            }
        }

        private void AdvanceAndPassOver(int index)
        {
            _passOver[index] = true;
            _tracker.AdvanceStep(1);
            _passOverCount++;
        }

        private void AdvanceAndFill(int leftIndex, int index, int rightIndex)
        {
            _passOver[leftIndex] = false;
            _passOver[index] = false;
            _passOver[rightIndex] = false;
            _tracker.RemoveIndex(index);
            _tracker.AdvanceStep(2);
            _passOverCount = 0;
            _indexedFillTriangles.Add(new IndexSurfaceTriangle(IndexLoop[leftIndex], IndexLoop[index], IndexLoop[rightIndex]));
        }

        private void Advance(int leftIndex, int index, int rightIndex)
        {
            _passOver[leftIndex] = false;
            _passOver[index] = false;
            _passOver[rightIndex] = false;
            _tracker.AdvanceStep(1);
            _passOverCount = 0;
        }

        private bool TrackingError(bool showMessage = false)
        {
            if (_passOverCount > _startCount)
            {
                if (showMessage)
                { InternalLoop.FillLoopError++; Console.WriteLine($"Triangle node {_triangleID} Loop {Id} could not be filled {_passOverCount} > {_startCount}: [[{_tracker.Count}]]\n {string.Join(", ", _tracker.Tracking.Select((t, i) => $"{t}:{ProjectedLoopPoints[t]}"))}"); }

                return true;
            }
            return false;
        }

        private bool MergeWithAnEnclosedInternalLoopAndAdvance(int leftIndex, int index, int rightIndex)
        {
            bool wasMerged = MergeWithAnEnclosedInternalLoop(leftIndex, index, rightIndex);
            if (!wasMerged) return false;
            Advance(leftIndex, index, rightIndex);
            return true;
        }

        private bool MergeWithIntersectingInternalLoop(int leftIndex, int index, int rightIndex)
        {
            var testSegment = new FillingSegment(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            FillingSegment nearestIntersection = RegionInternal.GetNearestIntersectingSegment(testSegment);
            IFillingLoop intersectingLoop = nearestIntersection.ParentLoop;
            if (intersectingLoop is null || !_unmergedInternalLoops.Contains(intersectingLoop)) return false;

            var triangle = new Triangle3D(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[index], ProjectedLoopPoints[rightIndex]);
            var unmergedEnclosedInternalLoops = _unmergedInternalLoops.Where(p => p.EnclosedByTriangle(triangle));
            var checkingLoops = unmergedEnclosedInternalLoops.Concat(new[] { intersectingLoop });

            foreach (var internalLoop in checkingLoops)
            {
                if (MergeInternalLoop(leftIndex, index, rightIndex, internalLoop)) { return true; }
            }
            return false;
        }

        private bool EnclosesInternalLoops(int leftIndex, int index, int rightIndex)
        {
            var triangle = new Triangle3D(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[index], ProjectedLoopPoints[rightIndex]);
            return InternalLoops.Any(p => p.EnclosedByTriangle(triangle));
        }

        private bool MergeWithAnEnclosedInternalLoop(int leftIndex, int index, int rightIndex)
        {
            var triangle = new Triangle3D(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[index], ProjectedLoopPoints[rightIndex]);
            var unmergedEnclosedInternalLoops = _unmergedInternalLoops.Where(p => p.EnclosedByTriangle(triangle));
            foreach (var internalLoops in unmergedEnclosedInternalLoops)
            {
                if (MergeInternalLoop(leftIndex, index, rightIndex, internalLoops)) { return true; }
            }
            return false;
        }

        private bool MergeInternalLoop(int leftIndex, int index, int rightIndex, IFillingLoop internalLoop)
        {
            int startIndex = GetNearestInternalLoopPoint(ProjectedLoopPoints[index], internalLoop);

            var testSegment = new FillingSegment(internalLoop.ProjectedLoopPoints[startIndex], ProjectedLoopPoints[index]);
            if (RegionInternal.HasIntersection(testSegment)) { return false; }

            Point3D currentPoint = ProjectedLoopPoints[index];
            Point3D startingPoint = _referenceArray[internalLoop.IndexLoop[startIndex]].Point;
            if (currentPoint == startingPoint) { return false; }

            int[] internalLoopIndicies = UnwrapLoopPoints(
                ProjectedLoopPoints[leftIndex], currentPoint,
                startIndex, internalLoop.IndexLoop.ToArray());

            int[] mergingIndicies = internalLoopIndicies.Concat(new[] { IndexLoop[index] }).ToArray();

            var insertionIndicies = mergingIndicies.Select((p, i) => i + ProjectedLoopPoints.Count).ToList();
            _tracker.InsertAt(_tracker.Tracking.IndexOf(rightIndex), insertionIndicies);
            _startCount += mergingIndicies.Length;
            foreach (var i in insertionIndicies)
            {
                _passOver.Add(i, false);
            }
            IndexLoop.AddRange(mergingIndicies);
            _projectedPoints = null;
            _unmergedInternalLoops.Remove(internalLoop);
            _mergedInternalLoops.Add(internalLoop);
            return true;
        }

        public bool EnclosedByTriangle(Triangle3D triangle)
        {
            if (!ProjectedLoopPoints.Any()) { return false; }
            return triangle.PointIsIn(ProjectedLoopPoints.First());
        }

        private bool HasIntersections(int leftIndex, int rightIndex)
        {
            var testSegment = new FillingSegment(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return RegionInternal.HasIntersection(testSegment);
        }

        private int[] UnwrapLoopPoints(Point3D leftPoint, Point3D currentPoint, int startIndex, int[] loopIndicies)
        {
            int leftIndex = (startIndex - 1 + loopIndicies.Length) % loopIndicies.Length;
            int rightIndex = (startIndex + 1 + loopIndicies.Length) % loopIndicies.Length;

            Point3D startingPoint = _referenceArray[loopIndicies[startIndex]].Point;
            if (startingPoint == currentPoint) { Console.WriteLine("Starting point and current points are equal."); }

            Point3D nextPoint = GetNextPoint(leftPoint, currentPoint,
                startingPoint, _referenceArray[loopIndicies[leftIndex]].Point, _referenceArray[loopIndicies[rightIndex]].Point);
            int[] unwrapped = loopIndicies.Unwrap(startIndex).ToArray();
            if (nextPoint != _referenceArray[unwrapped[1]].Point) { unwrapped = unwrapped.Reverse().ToArray(); }
            return unwrapped;
        }

        protected abstract Point3D GetNextPoint(Point3D leftPoint, Point3D currentPoint, Point3D startLoopPoint, Point3D leftLoopPoint, Point3D rightLoopPoint);

        private int GetNearestInternalLoopPoint(Point3D basePoint, IFillingLoop interiorLoop)
        {
            var distances = interiorLoop.ProjectedLoopPoints.Select((p, i) => new { Index = i, Distance = Point3D.Distance(basePoint, p) });
            var minDistance = distances.Min(d => d.Distance);
            return distances.First(d => d.Distance == minDistance).Index;
        }
        private bool CrossesInterior(int leftIndex, int rightIndex)
        {
            var testSegment = new FillingSegment(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return RegionInternal.CrossesInterior(testSegment);
        }

        private bool IsAtBoundary(int leftIndex, int rightIndex)
        {
            var testSegment = new FillingSegment(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return RegionInternal.IsAtBoundary(testSegment);
        }

        public static void ExtractOuterMostLoopsFromRest(IEnumerable<IFillingLoop> loops,
            out List<IFillingLoop> outerMostLoops, out List<IFillingLoop> restOfLoops)
        {
            var table = loops.Select(i => new KeyValuePair<IFillingLoop, bool>(i, true)).ToDictionary(p => p.Key, p => p.Value);
            var bucket = new BoxBucket<IFillingLoop>(loops.ToArray());

            foreach (var loop in loops)
            {
                foreach (var compare in bucket.Fetch(loop))
                {
                    if (loop == compare) { continue; }
                    if (loop.OutLineContainsLoop(compare)) { table[compare] = false; }
                }
            }

            outerMostLoops = table.Where(p => p.Value).Select(p => p.Key).ToList();
            restOfLoops = table.Where(p => !p.Value).Select(p => p.Key).ToList();
        }
    }

    public static class InternalLoop
    {
        public static int FillLoopError = 0;
    }
}
