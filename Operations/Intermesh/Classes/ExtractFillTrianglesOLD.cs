using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal class ExtractFillTrianglesOLD
    {
        static IFillStrategyOLD[] _fillStrategies = { new SimpleFillStrategyOLD(), /*new NearDegenerateFillStrategy(),*/ new ComplexFillStrategyOLD() };

        internal static void Action(IEnumerable<IntermeshTriangleOLD> triangles)
        {
            var start = DateTime.Now;

            var simpleFillCount = 0;
            var nearDegenerateFillCount = 0;
            var complexFillCount = 0;

            foreach (var triangle in triangles)
            {
                var fillStrategy = _fillStrategies.First(s => s.ShouldUseStrategy(triangle));
                if (fillStrategy is SimpleFillStrategyOLD) { simpleFillCount++; }
                if (fillStrategy is NearDegenerateFillStrategyOLD) { nearDegenerateFillCount++; }
                if (fillStrategy is ComplexFillStrategyOLD) { complexFillCount++; }

                Logging.ShowLog = false;
                fillStrategy.GetFillTriangles(triangle);
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Extract fill triangles. Simple {simpleFillCount} NearDegenerate {nearDegenerateFillCount} Complex {complexFillCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
