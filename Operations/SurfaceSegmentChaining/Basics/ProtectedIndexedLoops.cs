using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class ProtectedIndexedLoops
    {
        public ProtectedIndexedLoops() { }
        public ProtectedIndexedLoops(
            IReadOnlyList<int[]> perimeterIndexLoops,
            IReadOnlyList<int[]> indexLoops,
            IReadOnlyList<int[]> indexSpurredLoops,
            IReadOnlyList<int[]> indexSpurs
            )
        {
            PerimeterIndexLoops = perimeterIndexLoops;
            IndexLoops = indexLoops;
            IndexSpurredLoops = indexSpurredLoops;
            IndexSpurs = indexSpurs;
        }
        protected IReadOnlyList<int[]> PerimeterIndexLoops { get; private set; }
        protected IReadOnlyList<int[]> IndexLoops { get; private set; }
        protected IReadOnlyList<int[]> IndexSpurredLoops { get; private set; }
        protected IReadOnlyList<int[]> IndexSpurs { get; private set; }

        public static T Create<T>(ProtectedIndexedLoops input) where T : ProtectedIndexedLoops, new()
        {
            T output = new T();
            output.PerimeterIndexLoops = input.PerimeterIndexLoops;
            output.IndexLoops = input.IndexLoops;
            output.IndexSpurredLoops = input.IndexSpurredLoops;
            output.IndexSpurs = input.IndexSpurs;
            return output;
        }

        internal static IEnumerable<T> SplitByPerimeterIndexLoops<T>(ProtectedIndexedLoops input) where T : ProtectedIndexedLoops, new()
        {
            for (int i = 0; i < input.PerimeterIndexLoops.Count; i++)
            {
                T output = new T();
                output.PerimeterIndexLoops = [input.PerimeterIndexLoops[i]];
                output.IndexLoops = input.IndexLoops;
                output.IndexSpurredLoops = input.IndexSpurredLoops;
                output.IndexSpurs = input.IndexSpurs;
                yield return output;
            }
        }
    }
}
