using BaseObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes;
using Console = BaseObjects.Console;

namespace Operations.Intermesh;

internal static class Grid
{
    public static void Intermesh(this IWireFrameMesh mesh)
    {
        DateTime start = DateTime.Now;
        ConsoleLog.Push("Intermesh");
        var collections = new IntermeshCollection(mesh);

        TriangleGathering.Action(collections.Triangles);
        CalculateIntersections.Action(collections.Triangles);
        SeparateProcesses(collections, out List<IntermeshTriangle> processTriangles);
        CalculateDivisions.Action(processTriangles);

        SetIntersectionLinks.Action(processTriangles);
        SetDivisionLinks.Action(processTriangles);
        var elasticLinks = BuildElasticLinks.Action(processTriangles);
        PullElasticLinks.Action(elasticLinks);
        var fillTriangles = ExtractFillTriangles.Action(elasticLinks);
        UpdateResultGrid(mesh, processTriangles, fillTriangles);

        ConsoleLog.Pop();
        ConsoleLog.WriteLine($"Intermesh: Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }

    public static void RemoveNearDegenerates(this IWireFrameMesh mesh)
    {
        mesh.RemoveAllTriangles(mesh.Triangles.Where(IsNearDegenerate).ToArray());
    }

    private static bool IsNearDegenerate(PositionTriangle triangle)
    {
        return triangle.Triangle.LengthAB < GapConstants.Proximity ||
            triangle.Triangle.LengthBC < GapConstants.Proximity ||
            triangle.Triangle.LengthCA < GapConstants.Proximity;
    }

    private static void SeparateProcesses(IntermeshCollection collections, out List<IntermeshTriangle> processTriangles)
    {
        var start = DateTime.Now;
        processTriangles = collections.Triangles.Where(t => t.Intersections.Any()).ToList();
    }

    private static void UpdateResultGrid(IWireFrameMesh mesh, IEnumerable<IntermeshTriangle> processTriangles, IEnumerable<FillTriangle> fillTriangles)
    {
        var start = DateTime.Now;

        var processPositionTriangles = processTriangles.Select(p => p.PositionTriangle).ToArray();
        var removalCount = mesh.RemoveAllTriangles(processPositionTriangles);
        foreach (var triangle in fillTriangles)
        {
            triangle.AddWireFrameTriangle(mesh);
        }
        ConsoleLog.WriteLine($"Update result grid: Triangle removals {removalCount} Fills {fillTriangles.Count()} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
    }
}
