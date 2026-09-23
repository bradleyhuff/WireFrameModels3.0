using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;

namespace Operations.Intermesh.Classes
{
    internal class ExtractFillTriangles
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var simpleFillCount = 0;
            var complexFillCount = 0;

            var fill = new ComplexFill();
            fill.GetFillings(intermeshTriangles);

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Extract fill triangles. Simple {simpleFillCount} Complex {complexFillCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
