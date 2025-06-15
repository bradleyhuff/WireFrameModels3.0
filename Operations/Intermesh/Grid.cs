using BaseObjects;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Classes;

namespace Operations.Intermesh;

internal static class Grid
{
    public static void Intermesh(this IWireFrameMesh mesh)
    {
        DateTime start = DateTime.Now;
        ConsoleLog.Push("Intermesh");
        var collection = mesh.Triangles.Select(t => new Basics.IntermeshTriangle(t)).ToArray();
        TriangleGathering.Action(collection);
        CalculateIntersections.Action(collection);
        LinkIntersections.Action(collection);
        BuildDivisions.Action(collection);
        collection = collection.Where(t => t.HasDivisions).ToArray();
        ExtractFillTriangles.Action(collection);
        //FillOverlapRemoval.Action(collection);
        UpdateResultsGrid.Action(mesh, collection);
        //OpenEdgesFill.Action(mesh);
        ConsoleLog.Pop();
        ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }
}
