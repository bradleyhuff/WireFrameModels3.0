using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets.Interfaces;
using Collections.Buckets;
using Operations.Regions;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.PositionRemovals.Interfaces;
using Collections.WireFrameMesh.Basics;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarLoop<T> : IBox
    {
        private static int _id = 0;
        public PlanarLoop(Plane plane, double testSegmentLength, IReadOnlyList<SurfaceRayContainer<T>> referenceArray, ISharedFillConditionals fillConditionals, int[] indexLoop, int triangleID)
        {
            Id = _id++;
            _triangleID = triangleID;
            Plane = plane;
            IndexLoop = indexLoop.ToList();
            _referenceArray = referenceArray;
            _testSegmentLength = testSegmentLength;
            _fillConditionals = fillConditionals;
        }

        ISharedFillConditionals _fillConditionals;
        IReadOnlyList<SurfaceRayContainer<T>> _referenceArray;
        double _testSegmentLength;
        PlanarRegions<T> _shellOutLine;
        PlanarRegions<T> _shell;
        IReadOnlyList<Point3D> _projectedPoints;
        PlanarSegment<T>[] _loopSegments;
        List<PlanarLoop<T>> _internalLoops = new List<PlanarLoop<T>>();
        IndexTracker _tracker;
        Dictionary<int, bool> _passOver = new Dictionary<int, bool>();
        int _startCount = 0;
        int _passOverCount = 0;
        List<PlanarLoop<T>> _unmergedInternalLoops = new List<PlanarLoop<T>>();
        List<PlanarLoop<T>> _mergedInternalLoops = new List<PlanarLoop<T>>();
        Rectangle3D _box;
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
        public Plane Plane { get; }
        public List<int> IndexLoop { get; private set; }
        public IReadOnlyList<Point3D> ProjectedLoopPoints
        {
            get
            {
                if (_projectedPoints is null)
                {
                    _projectedPoints = IndexLoop.Select(i => Plane.Projection(_referenceArray[i].Point)).ToArray();
                }
                return _projectedPoints;
            }
        }

        public IReadOnlyCollection<PlanarSegment<T>> LoopSegments
        {
            get
            {
                if (_loopSegments is null)
                {
                    _loopSegments = PlanarSegment<T>.GetLoop(this, ProjectedLoopPoints.ToArray()).ToArray();
                }
                return _loopSegments;
            }
        }

        private PlanarRegions<T> ShellOutLines
        {
            get
            {
                if (_shellOutLine is null)
                {
                    _shellOutLine = new PlanarRegions<T>(Plane, _testSegmentLength);
                    _shellOutLine.Load(LoopSegments);
                }
                return _shellOutLine;
            }
        }

        private PlanarRegions<T> Shell
        {
            get
            {
                if (_shell is null)
                {
                    _shell = new PlanarRegions<T>(Plane, _testSegmentLength);
                    _shell.Load(LoopSegments.Concat(InternalLoops.SelectMany(l => l.LoopSegments)));
                }
                return _shell;
            }
        }

        public Region OutLineRegionOfPoint(Point3D point)
        {
            return ShellOutLines.RegionOfProjectedPoint(point);
        }

        public Region RegionOfPoint(Point3D point)
        {
            return Shell.RegionOfProjectedPoint(point);
        }

        public List<PlanarLoop<T>> InternalLoops
        {
            get { return _internalLoops; }
            set
            {
                _internalLoops = value ?? new List<PlanarLoop<T>>();
                _shell = null;
                _unmergedInternalLoops = _internalLoops.ToList();
            }
        }

        public bool OutLineContainsLoop(PlanarLoop<T> input)
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
                    if (_fillConditionals is null) { LoopForFillings(0, true); return _indexedFillTriangles; }
                    if (LoopForFillings(0, false)) return _indexedFillTriangles;
                    if (LoopForFillings(1, false)) return _indexedFillTriangles;
                    if (LoopForFillings(-1, true)) return _indexedFillTriangles;
                }
                return _indexedFillTriangles;
            }
        }

        private bool LoopForFillings(int displacement = 0, bool showError = false)
        {
            _tracker = new IndexTracker(IndexLoop.Select((p, i) => i));
            _passOver = _tracker.Tracking.Select(i => new KeyValuePair<int, bool>(i, false)).ToDictionary(p => p.Key, p => p.Value);
            _tracker.AdvanceStep(displacement);

            _startCount = _tracker.Count;
            _passOverCount = 0;
            _indexedFillTriangles = new List<IndexSurfaceTriangle>();
            if (InternalLoops.Any(l => l.IndexLoop.Intersect(IndexLoop).Any())) { return true; } // All internal loops must not touch the loop.

            while (true)
            {
                if (_tracker.Count < 3) return true;//Fully filled

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
                            if (TrackingError(showError)) { return false; }
                        }
                        continue;
                    }
                    AdvanceAndFill(leftIndex, index, rightIndex);
                    return true; //Fully filled
                }

                if (_passOver[index])
                {
                    AdvanceAndPassOver(index);
                    if (TrackingError(showError)) { return false; }
                    continue;
                }

                if (HasIntersections(leftIndex, rightIndex))
                {
                    if (!MergeWithIntersectingInternalLoop(leftIndex, index, rightIndex))
                    {
                        AdvanceAndPassOver(index);
                        if (TrackingError(showError)) { return false; }
                    }
                    continue;
                }

                if (EnclosesInternalLoops(leftIndex, index, rightIndex))
                {
                    if (!MergeWithAnEnclosedInternalLoopAndAdvance(leftIndex, index, rightIndex))
                    {
                        AdvanceAndPassOver(index);
                        if (TrackingError(showError)) { return false; }
                    }
                    continue;
                }

                if (FillIsAllowed(leftIndex, index, rightIndex) && CrossesInterior(leftIndex, rightIndex))
                {
                    AdvanceAndFill(leftIndex, index, rightIndex);
                }

                AdvanceAndPassOver(index);
                if (TrackingError(showError)) { return false; }
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
            _passOverCount = 0;
            _tracker.RemoveIndex(index);
            _tracker.AdvanceStep(2);
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
                {
                    InternalLoop.FillLoopError++;
                    throw new Exception("Could not be filled.");
                    //Console.WriteLine($"Triangle node {_triangleID} Loop {Id} could not be filled {_passOverCount} > {_startCount}: [[{_tracker.Count}]]\n {String.Join(", ", _tracker.Tracking.Select((t, i) => $"{t}:{ProjectedLoopPoints[t]}"))}");
                }

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
            var testSegment = new PlanarSegment<T>(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            PlanarSegment<T> nearestIntersection = Shell.GetNearestIntersectingSegment(testSegment);
            PlanarLoop<T> intersectingLoop = nearestIntersection.ParentLoop;
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

        private bool MergeInternalLoop(int leftIndex, int index, int rightIndex, PlanarLoop<T> internalLoop)
        {
            int startIndex = GetNearestInternalLoopPoint(ProjectedLoopPoints[index], internalLoop);

            var testSegment = new PlanarSegment<T>(internalLoop.ProjectedLoopPoints[startIndex], ProjectedLoopPoints[index]);
            if (Shell.HasIntersection(testSegment)) { return false; }

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

        private bool EnclosedByTriangle(Triangle3D triangle)
        {
            if (!ProjectedLoopPoints.Any()) { return false; }
            return triangle.PointIsIn(ProjectedLoopPoints.First());
        }

        private bool HasIntersections(int leftIndex, int rightIndex)
        {
            var testSegment = new PlanarSegment<T>(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return Shell.HasIntersection(testSegment);
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

        private Point3D GetNextPoint(Point3D leftPoint, Point3D currentPoint, Point3D startLoopPoint, Point3D leftLoopPoint, Point3D rightLoopPoint)
        {
            var direction = Plane.Normal.Direction;

            var angle = Vector3D.SignedAngle(direction, (leftPoint - currentPoint).Direction, (startLoopPoint - currentPoint).Direction);
            var rightOptionAngle = Vector3D.SignedAngle(direction, (currentPoint - startLoopPoint).Direction, (rightLoopPoint - startLoopPoint).Direction);

            if (System.Math.Sign(angle) == System.Math.Sign(rightOptionAngle)) { return rightLoopPoint; }
            return leftLoopPoint;
        }

        private int GetNearestInternalLoopPoint(Point3D basePoint, PlanarLoop<T> interiorLoop)
        {
            var distances = interiorLoop.ProjectedLoopPoints.Select((p, i) => new { Index = i, Distance = Point3D.Distance(basePoint, p) });
            var minDistance = distances.Min(d => d.Distance);
            return distances.First(d => d.Distance == minDistance).Index;
        }

        private bool FillIsAllowed(int leftIndex, int index, int rightIndex)
        {
            return _fillConditionals?.AllowFill(
                _referenceArray[IndexLoop[leftIndex]].Reference as PositionNormal, 
                _referenceArray[IndexLoop[index]].Reference as PositionNormal, 
                _referenceArray[IndexLoop[rightIndex]].Reference as PositionNormal) ?? true;
        }

        private bool CrossesInterior(int leftIndex, int rightIndex)
        {
            var testSegment = new PlanarSegment<T>(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return Shell.CrossesInterior(testSegment);
        }

        private bool IsAtBoundary(int leftIndex, int rightIndex)
        {
            var testSegment = new PlanarSegment<T>(ProjectedLoopPoints[leftIndex], ProjectedLoopPoints[rightIndex]);
            return Shell.IsAtBoundary(testSegment);
        }

        public static void ExtractOuterMostLoopsFromRest(IEnumerable<PlanarLoop<T>> loops,
            out List<PlanarLoop<T>> outerMostLoops, out List<PlanarLoop<T>> restOfLoops)
        {
            var table = loops.Select(i => new KeyValuePair<PlanarLoop<T>, bool>(i, true)).ToDictionary(p => p.Key, p => p.Value);
            var bucket = new BoxBucket<PlanarLoop<T>>(loops.ToArray());

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

        public static class InternalLoop
        {
            public static int FillLoopError = 0;
        }
    }
}
