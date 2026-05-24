using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;

namespace Operations.Intermesh.Classes
{
    internal class ExtractFillTriangles
    {
        static IFillStrategy[] _fillStrategies = { /*new SimpleFillStrategy(),*/ new ComplexFillStrategy() };
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var simpleFillCount = 0;
            var complexFillCount = 0;

            foreach (var triangle in intermeshTriangles)
            {
                var fillStrategy = _fillStrategies.First(s => s.ShouldUseStrategy(triangle));
                if (fillStrategy is SimpleFillStrategy) { simpleFillCount++; }
                if (fillStrategy is ComplexFillStrategy) { complexFillCount++; }

                Logging.ShowLog = false;
                fillStrategy.GetFillTriangles(triangle);
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Extract fill triangles. Simple {simpleFillCount} Complex {complexFillCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
