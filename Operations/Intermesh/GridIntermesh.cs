using BaseObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes;

namespace Operations.Intermesh;

internal static class GridIntermesh
{
    public static void Intermesh(this IWireFrameMesh mesh)
    {
        DateTime start = DateTime.Now;
        if (!Mode.ThreadedRun) ConsoleLog.Push("Intermesh");
        var collection = mesh.Triangles.Select(t => new Basics.IntermeshTriangle(t)).ToArray();
        TriangleGathering.Action(collection);
        CalculateIntersections.Action(collection);
        LinkIntersections.Action(collection, out BoxBucket<IntermeshPoint>  pointsBucket, out Combination2Dictionary<IntermeshSegment> segmentTable);
        SegmentBridging.Action(collection, pointsBucket, segmentTable);
        BuildDivisions.Action(collection);
        collection = collection.Where(t => t.HasInternalDivisions).ToArray();
        ExtractFillTriangles.Action(collection);
        //FillOverlapRemoval.Action(collection);
        //FillIntermesh.Action(collection);
        UpdateResultsGrid.Action(mesh, collection);
        //OpenEdgesFill.Action(mesh);
        if (!Mode.ThreadedRun) ConsoleLog.Pop();
        if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }

    public static void IntermeshSingle(this IWireFrameMesh mesh, Func<PositionTriangle, bool> include)
    {
        DateTime start = DateTime.Now;
        if (!Mode.ThreadedRun) ConsoleLog.Push("Intermesh");
        var collection = mesh.Triangles.Where(t => include(t)).Select(t => new Basics.IntermeshTriangle(t)).ToArray();
        TriangleGathering.ActionSingle(collection);
        CalculateIntersections.ActionSingle(collection);
        LinkIntersections.Action(collection, out BoxBucket<IntermeshPoint> pointsBucket, out Combination2Dictionary<IntermeshSegment> segmentTable);
        SegmentBridging.Action(collection, pointsBucket, segmentTable);
        BuildDivisions.Action(collection);
        collection = collection.Where(t => t.HasInternalDivisions).ToArray();
        ExtractFillTriangles.Action(collection);
        //FillOverlapRemoval.Action(collection);
        //FillIntermesh.Action(collection);
        UpdateResultsGrid.Action(mesh, collection);
        //OpenEdgesFill.Action(mesh);
        if (!Mode.ThreadedRun) ConsoleLog.Pop();
        if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }

    public static int ClusterId { get; set; }
}
