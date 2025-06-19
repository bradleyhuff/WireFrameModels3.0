using BaseObjects;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal class ExtractFillTriangles
    {
        static IFillStrategy[] _fillStrategies = { new SimpleFillStrategy(), new NearDegenerateFillStrategy(), new ComplexFillStrategy() };

        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;

            var simpleFillCount = 0;
            var nearDegenerateFillCount = 0;
            var complexFillCount = 0;

            foreach (var triangle in triangles)
            {
                var fillStrategy = _fillStrategies.First(s => s.ShouldUseStrategy(triangle));
                if (fillStrategy is SimpleFillStrategy) { simpleFillCount++; }
                if (fillStrategy is NearDegenerateFillStrategy) { nearDegenerateFillCount++; }
                if (fillStrategy is ComplexFillStrategy) { complexFillCount++; }

                fillStrategy.GetFillTriangles(triangle);
            }

            if (GridIntermesh.ShowLog) ConsoleLog.WriteLine($"Extract fill triangles. Simple {simpleFillCount} NearDegenerate {nearDegenerateFillCount} Complex {complexFillCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
