using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Threading;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal static class CalculateDivisions
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            var divisionState = new DivisionState();
            var divisionIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            divisionIterator.RunSingle<DivisionState, DivisionThread>(DivisionAction, divisionState);
            ConsoleLog.WriteLine($"Calculate divisions. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {divisionState.Threads}");
        }

        private static void DivisionAction(IntermeshTriangle node, DivisionThread threadState, DivisionState state)
        {
            var intersections = node.Intersections.ToArray();
            if (intersections.All(i => i.DivisionIsSet)) { return; }
            var bucket = new BoxBucket<IntermeshIntersection>(intersections);
            lock (node)
            {
                foreach (var intersectionNode in intersections.Where(i => !i.DivisionIsSet))
                {
                    if (!intersectionNode.DivisionIsSet)
                    {
                        intersectionNode.DivisionIsSet = true;

                        var matches = bucket.Fetch(intersectionNode).Where(i => i.Id != intersectionNode.Id).ToArray();

                        intersectionNode.AddDivisions(FullSegmentDivisions(intersectionNode, matches).ToArray());
                    }
                }
            }
        }

        private static IEnumerable<IntermeshDivision> PointIntersections(IntermeshIntersection segment, IEnumerable<IntermeshIntersection> matches)
        {
            foreach (var match in matches)
            {
                var point = LineSegment3D.PointIntersection(segment.Intersection, match.Intersection);
                if (point is not null) { yield return new IntermeshDivision() { RootIntersection = segment, Point = point, PointIntersection = match }; }
            }
        }

        private static IEnumerable<IntermeshDivision> FullSegmentDivisions(IntermeshIntersection segment, IEnumerable<IntermeshIntersection> matches)
        {
            var divisionNodes = PointIntersections(segment, matches).OrderBy(p => Point3D.Distance(segment.Intersection.Start, p.Point)).ToArray();
            if (!divisionNodes.Any())
            {
                var divisionNode = new IntermeshDivision();
                divisionNode.Division = segment.Intersection;
                divisionNode.RootIntersection = segment;
                yield return divisionNode;
                yield break;
            }

            {
                var divisionNode = new IntermeshDivision();
                divisionNode.RootIntersection = segment;
                divisionNode.Division = new LineSegment3D(segment.Intersection.Start, divisionNodes[0].Point);
                divisionNode.IntersectionB.Add(divisionNodes[0].PointIntersection);

                yield return divisionNode;
            }

            for (int i = 0; i < divisionNodes.Length - 1; i++)
            {
                var element = divisionNodes[i];
                var nextElement = divisionNodes[i + 1];

                var divisionNode = new IntermeshDivision();
                divisionNode.RootIntersection = segment;
                divisionNode.Division = new LineSegment3D(element.Point, nextElement.Point);
                divisionNode.IntersectionA.Add(element.PointIntersection);
                divisionNode.IntersectionB.Add(nextElement.PointIntersection);

                yield return divisionNode;
            }

            {
                var divisionNode = new IntermeshDivision();
                divisionNode.RootIntersection = segment;
                divisionNode.Division = new LineSegment3D(divisionNodes.Last().Point, segment.Intersection.End);
                divisionNode.IntersectionA.Add(divisionNodes.Last().PointIntersection);

                yield return divisionNode;
            }
        }

        private class DivisionThread : BaseThreadState { }
        private class DivisionState : BaseState<DivisionThread> { }
    }
}
