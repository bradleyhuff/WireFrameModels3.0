using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;

namespace Operations.Intermesh.Classes
{
    internal class ExtractFillTriangles
    {
        static ComplexFillStrategy _fillStrategy = new ComplexFillStrategy();
        static IFillStrategy[] _fillStrategies = { /*new SimpleFillStrategy(),*/ _fillStrategy };
        
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

                //Logging.ShowLog = false;
                fillStrategy.GetFillTriangles(triangle);
            }

            //var usedLoops = _fillStrategy.UsedLoops.Where(kp => kp.Value.Count > 1);
            //if (usedLoops.Any()) {

            //    var usedLoops2 = usedLoops.Where(kp => kp.Key.Array.Length > 3);

            //}

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Extract fill triangles. Simple {simpleFillCount} Complex {complexFillCount} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
