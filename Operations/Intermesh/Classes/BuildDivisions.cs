using BaseObjects;
using BasicObjects.MathExtensions;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class BuildDivisions
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var table = new Combination2Dictionary<IntermeshDivision>();

            foreach (var element in intermeshTriangles)
            {
                foreach (var segment in element.Segments)
                {
                    var divisions = segment.BuildDivisions(table);
                    element.AddRange(divisions);
                }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Build divisions. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
