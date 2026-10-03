using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    public class ProtectedIndexedLoops
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
            DividingIndexLoops = indexLoops;
            IndexSpurredLoops = indexSpurredLoops;
            IndexSpurs = indexSpurs;
        }
        protected IReadOnlyList<int[]> PerimeterIndexLoops { get; private set; }
        protected IReadOnlyList<int[]> DividingIndexLoops { get; private set; }
        protected IReadOnlyList<int[]> IndexSpurredLoops { get; private set; }
        protected IReadOnlyList<int[]> IndexSpurs { get; private set; }

        public static T Create<T>(ProtectedIndexedLoops input) where T : ProtectedIndexedLoops, new()
        {
            T output = new T();
            output.PerimeterIndexLoops = input.PerimeterIndexLoops;
            output.DividingIndexLoops = input.DividingIndexLoops;
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
                output.DividingIndexLoops = input.DividingIndexLoops;
                output.IndexSpurredLoops = input.IndexSpurredLoops;
                output.IndexSpurs = input.IndexSpurs;
                yield return output;
            }
        }

        internal static T IncludeLoopsByIndex<T>(ProtectedIndexedLoops input, List<int> includedIndex) where T : ProtectedIndexedLoops, new()
        {
            T output = new T();
            output.PerimeterIndexLoops = input.PerimeterIndexLoops;

            var includedIndexedLoops = new List<int[]>();
            for (int i = 0; i < includedIndex.Count; i++)
            {
                includedIndexedLoops.Add(input.DividingIndexLoops[includedIndex[i]]);
            }

            output.DividingIndexLoops = includedIndexedLoops;
            output.IndexSpurredLoops = input.IndexSpurredLoops;
            output.IndexSpurs = input.IndexSpurs;
            return output;
        }

        internal static T DividingLoopSplitBy<T>(ProtectedIndexedLoops input, IEnumerable<int[]> splits, 
            Action<int, int[]> addLoopAction, 
            Action<int, int[],int, int[]> addSplitAction) where T : ProtectedIndexedLoops, new()
        {
            T output = new T();
            output.PerimeterIndexLoops = input.PerimeterIndexLoops;

            var inputDividingLoops = input.DividingIndexLoops.Select((l, i) => (GroupIndex: i, DividingLoop: (Loop: l, WasSplit: false))).ToList();
            var outputDividingLoops = new List<(int GroupIndex, (int[] Loop, bool WasSplit) DividingLoop)>();

            for (int i = 0; i < inputDividingLoops.Count; i++)
            {
                var dividingLoop = inputDividingLoops[i];

                var splitBy = SplitBy(dividingLoop.DividingLoop.Loop, splits).Select(s => (dividingLoop.GroupIndex, DividingLoop: s));
                outputDividingLoops.AddRange(splitBy);
                foreach (var split in splitBy.Where(s => s.DividingLoop.WasSplit))
                {
                    addSplitAction(dividingLoop.GroupIndex, dividingLoop.DividingLoop.Loop, split.GroupIndex, split.DividingLoop.Loop);
                }
            }

            output.DividingIndexLoops = outputDividingLoops.Select(s => s.DividingLoop.Loop).ToList();

            foreach (var loop in outputDividingLoops)
            {
                addLoopAction(loop.GroupIndex, loop.DividingLoop.Loop);
            }

            output.IndexSpurredLoops = input.IndexSpurredLoops;
            output.IndexSpurs = input.IndexSpurs;
            return output;
        }

        internal static T RemoveDividingLoops<T>(ProtectedIndexedLoops input, IEnumerable<int[]> toRemove, Action<int, IEnumerable<int[]>> addLoopAction) where T : ProtectedIndexedLoops, new()
        {
            T output = new T();
            output.PerimeterIndexLoops = input.PerimeterIndexLoops;

            var dividingLoops = new List<int[]>();
            for (int i = 0; i < input.DividingIndexLoops.Count; i++)
            {
                var dividingLoop = input.DividingIndexLoops[i];
                //if () { }

                //var dividingLoop = input.DividingIndexLoops[i];
                //if (!CanSplit(dividingLoop, split)) { dividingLoops.Add(dividingLoop); noSplitAction(i); continue; }
                //var splits = SplitBy(dividingLoop, split);
                //dividingLoops.AddRange(splits);
                //splitAction(i, splits);
            }

            output.DividingIndexLoops = dividingLoops;
            output.IndexSpurredLoops = input.IndexSpurredLoops;
            output.IndexSpurs = input.IndexSpurs;
            return output;
        }

        private static bool CanSplit(int[] input, IEnumerable<int> split)
        {
            var table = input.ToDictionary(kp => kp);
            return !split.Any(s => !table.ContainsKey(s)) && input.Length >= split.Count();
        }

        private static bool IsEqual(int[] input, IEnumerable<int> split)
        {
            var table = input.ToDictionary(kp => kp);
            return !split.Any(s => !table.ContainsKey(s)) && input.Length == split.Count();
        }

        private static IEnumerable<(int[] Loop, bool WasSplit)> SplitBy(int[] input, IEnumerable<int[]> splits)
        {
            var output = new List<(int[] Loop, bool WasSplit)>() { (input, false) };
            foreach (var split in splits)
            {
                var outputSplit = SplitBy(output[0].Loop, split).ToArray();
                output.RemoveAt(0);
                output.InsertRange(0, outputSplit);
            }
            return output;
        }

        private static IEnumerable<(int[] Split, bool WasSplit)> SplitBy(int[] input, int[] split)
        {
            if (!CanSplit(input, split)) { yield return (input, false); yield break; }

            yield return (split, true); // Return split

            var table = split.ToDictionary(kp => kp);

            var output = new List<int>();

            for (int i = 0; i < input.Length; i++)
            {
                var l = (i + input.Length - 1) % input.Length;
                var n = (i + 1) % input.Length;

                if (!table.ContainsKey(input[i])) { output.Add(input[i]); continue; }
                if (table.ContainsKey(input[l]) && !table.ContainsKey(input[n])) { output.Add(input[i]); continue; }
                if (!table.ContainsKey(input[l]) && table.ContainsKey(input[n])) { output.Add(input[i]); }
            }

            if (output.Any()) { yield return (output.ToArray(), true); }
        }

        private static IEnumerable<int[]> Subtract(int[] input, IEnumerable<int> toRemove)
        {
            var table = input.ToDictionary(kp => kp);
            var tableRemove = toRemove.ToDictionary(kp => kp);

            var loopStarted = false;
            for (int i = 0; i <= input.Length * 2; i++)
            {
                int ii = i % input.Length;
                bool containsKey = tableRemove.ContainsKey(input[ii]);
                if (containsKey) { loopStarted = true; }
                if (!loopStarted) { continue; }

            }

            yield break;
        }
    }
}
