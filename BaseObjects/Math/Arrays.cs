
namespace BasicObjects.MathExtensions
{
    public static class Arrays
    {
        public static bool IsEqualTo(this int[] a, int[] b)
        {

            if (a.Length != b.Length) { return false; }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }
            return true;
        }

        public static int[] Difference(this int[] a, int[] b)
        {
            return DifferenceYield(a, b).ToArray();
        }
        private static IEnumerable<int> DifferenceYield(List<int> a, List<int> b)
        {
            var table = new Dictionary<int, bool>();
            foreach (var element in b)
            {
                table[element] = true;
            }
            foreach (var element in a)
            {
                if (!table.ContainsKey(element)) { yield return element; }
            }
        }

        public static List<int> Difference(this List<int> a, List<int> b)
        {
            return DifferenceYield(a, b).ToList();
        }
        private static IEnumerable<int> DifferenceYield(int[] a, int[] b)
        {
            var table = new Dictionary<int, bool>();
            foreach (var element in b)
            {
                table[element] = true;
            }
            foreach (var element in a)
            {
                if (!table.ContainsKey(element)) { yield return element; }
            }
        }

        public static IEnumerable<T> Unwrap<T>(this T[] points, int startIndex)
        {
            for (int i = 0; i <= points.Length; i++)
            {
                yield return points[(startIndex + i) % points.Length];
            }
        }

        public static IEnumerable<T> AlternatingUnwrap<T>(this T[] points, int startIndex)
        {
            int i = 1;
            int j = points.Length - 1;

            yield return points[startIndex];

            while (j >= i)
            {
                yield return points[(startIndex + i) % points.Length];
                i++;
                if (i > j) { break; }
                yield return points[(startIndex + j) % points.Length];
                j--;
            }
        }

        public static IEnumerable<T> Rotate<T>(this T[] points, int startIndex)
        {
            for (int i = 0; i < points.Length; i++)
            {
                yield return points[(startIndex + i) % points.Length];
            }
        }

        public static IEnumerable<T> UnwrapToFirst<T>(this T[] points, Func<T, int, bool> startCondition)
        {
            if (!points.Select((v, i) => new { v, i }).Any(p => startCondition(p.v, p.i))) { return points; }
            var start = points.Select((v, i) => new { v, i }).First(p => startCondition(p.v, p.i)).i;
            return Unwrap(points, start);
        }

        public static IEnumerable<T> UnwrapToBeginning<T>(this T[] points, Func<T, int, bool> startCondition)
        {
            if (!points.Select((v, i) => new { v, i }).Any(p => startCondition(p.v, p.i))) { return points; }
            var start = 0;
            for (int i = 0; i < points.Length; i++)
            {
                int p = (i - 1 + points.Length) % points.Length;
                if (!startCondition(points[p], p) && startCondition(points[i], i)) { start = i; break; }
            }

            return Rotate(points, start).Where((p, i) => startCondition(p, i));
        }

        public static IEnumerable<T> RotateToFirst<T>(this T[] points, Func<T, int, bool> startCondition)
        {
            if (!points.Select((v, i) => new { v, i }).Any(p => startCondition(p.v, p.i))) { return points; }
            var start = points.Select((v, i) => new { v, i }).First(p => startCondition(p.v, p.i)).i;
            return Rotate(points, start);
        }

        public static IEnumerable<T> RotateToBeginning<T>(this T[] points, Func<T, int, bool> startCondition)
        {
            if (!points.Select((v, i) => new { v, i }).Any(p => startCondition(p.v, p.i))) { return points; }
            var start = 0;
            for (int i = 0; i < points.Length; i++)
            {
                int p = (i - 1 + points.Length) % points.Length;
                if (!startCondition(points[p], p) && startCondition(points[i], i)) { start = i; break; }
            }

            return Rotate(points, start);
        }

        public static IEnumerable<T[]> SplitAt<T>(this T[] points, Func<T, bool> splitCondition, bool includeSplitElements = false)
        {
            var row = new List<T>();
            foreach (var point in points)
            {
                if (splitCondition(point))
                {
                    if (row.Count > 0)
                    {
                        yield return row.ToArray();
                    }
                    row = new List<T>();
                }
                else
                {
                    row.Add(point);
                }
            }
            if (row.Count > 0)
            {
                yield return row.ToArray();
            }
        }

        public static IEnumerable<T> TakeWhileIncluding<T>(this IEnumerable<T> list, Func<T, int, bool> takeCondition)
        {
            foreach (var item in list.Select((v, i) => new { v, i }))
            {
                yield return item.v;
                if (takeCondition(item.v, item.i)) { yield break; }
            }
        }
    }
}
