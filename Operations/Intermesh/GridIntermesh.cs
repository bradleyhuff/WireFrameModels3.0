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
        
        var collection = mesh.Triangles.Select(t => new IntermeshTriangle(t)).ToArray();

        //var collectionOLD = mesh.Triangles.Select(t => new Basics.IntermeshTriangleOLD(t)).ToArray();
        TriangleGathering.Action(collection);
        CalculateIntersections.Action(collection);
        TriangleSegmentAssignments.Action(collection);

        //var collectionOLD = collection.Select(c => IntermeshTriangleOLD.ConvertFrom(c)).ToArray();
        var collectionOLD = mesh.Triangles.Select(t => new Basics.IntermeshTriangleOLD(t)).ToArray();

        TriangleGathering.Action(collectionOLD);
        CalculateIntersections.Action(collectionOLD);

        LinkIntersectionsOLD.Action(collectionOLD, out BoxBucket<IntermeshPointOLD>  pointsBucket, out Combination2Dictionary<IntermeshSegmentOLD> segmentTable);
        SegmentBridgingOLD.Action(collectionOLD, pointsBucket, segmentTable);
        BuildDivisionsOLD.Action(collectionOLD);
        //ResolveCapsules.Action(collection);
        collectionOLD = collectionOLD.Where(t => t.HasInternalDivisions).ToArray();
        ExtractFillTriangles.Action(collectionOLD);
        //FillOverlapRemoval.Action(collection);
        //FillIntermesh.Action(collection);
        UpdateResultsGrid.Action(mesh, collectionOLD);
        //OpenEdgesFill.Action(mesh);
        if (!Mode.ThreadedRun) ConsoleLog.Pop();
        if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }

    public static void IntermeshSingle(this IWireFrameMesh mesh, Func<PositionTriangle, bool> include)
    {
        DateTime start = DateTime.Now;
        if (!Mode.ThreadedRun) ConsoleLog.Push("Intermesh Single");
        //var collectionOLD = mesh.Triangles.Where(t => include(t)).Select(t => new Basics.IntermeshTriangleOLD(t)).ToArray();
        var collection = mesh.Triangles.Select(t => new Basics.IntermeshTriangle(t)).ToArray();

        TriangleGathering.ActionSingle(collection);
        CalculateIntersections.ActionSingle(collection);

        //var collectionOLD = collection.Select(c => IntermeshTriangleOLD.ConvertFrom(c)).ToArray();
        var collectionOLD = mesh.Triangles.Where(t => include(t)).Select(t => new Basics.IntermeshTriangleOLD(t)).ToArray();
        LinkIntersectionsOLD.Action(collectionOLD, out BoxBucket<IntermeshPointOLD> pointsBucket, out Combination2Dictionary<IntermeshSegmentOLD> segmentTable);
        SegmentBridgingOLD.Action(collectionOLD, pointsBucket, segmentTable);

        BuildDivisionsOLD.Action(collectionOLD);
        ResolveCapsules.Action(collectionOLD);
        collectionOLD = collectionOLD.Where(t => t.HasInternalDivisions).ToArray();
        ExtractFillTriangles.Action(collectionOLD);
        //FillOverlapRemoval.Action(collection);
        //FillIntermesh.Action(collection);
        UpdateResultsGrid.Action(mesh, collectionOLD);
        //OpenEdgesFill.Action(mesh);
        if (!Mode.ThreadedRun) ConsoleLog.Pop();
        if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }

    public static int ClusterId { get; set; }
}
