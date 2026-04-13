using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects
{
    public static class Extensions
    {
        public static string Repeat(this string s, int times, string separator = "")
        {
            return string.Join(separator, Enumerable.Repeat(s, times));
        }

        public static IEnumerable<object[]> GroupCounts<T>(this IEnumerable<T> list, Func<T, int> groupCount)
        {
            return list.GroupCounts(groupCount, (key, count) => { return [key, count.ToString("#,##0")]; });
        }
        public static IEnumerable<object[]> GroupCounts<T>(this IEnumerable<T> list, Func<T, int> groupCount, Func<int, int, object[]> lineOutput)
        {
            var groups = list.GroupBy(g => groupCount(g)).OrderBy(g => g.Key);
            foreach (var group in groups)
            {
                yield return lineOutput(group.Key, group.Count());
            }            
        }

        public static IEnumerable<object[]> GroupCountAccumulates<T>(this IEnumerable<T> list, Func<T, int> groupCount)
        {
            return list.GroupCountAccumulates(groupCount, (key, count, accumulate) => { return [key, count.ToString("#,##0"), accumulate.ToString("#,##0")]; });
        } 
        public static IEnumerable<object[]> GroupCountAccumulates<T>(this IEnumerable<T> list, Func<T, int> groupCount, Func<int, int, int, object[]> lineOutput)
        {
            int accumulate = 0;
            var groups = list.GroupBy(g => groupCount(g)).OrderBy(g => g.Key);
            foreach(var group in groups)
            {
                accumulate += group.Count();
                yield return lineOutput( group.Key, group.Count(), accumulate);
            }
        }

        public static string DisplayRow(this object[] row)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{row.First()}");

            foreach (var element in row.Skip(1))
            {
                sb.Append($": {element}");
            }

            return sb.ToString();
        }

        public static string DisplayByLine(this IEnumerable<object[]> line)
        {
            return string.Join("\n", line.Select(l => l.DisplayRow()));
        }

        public static string DisplayByLine(this IEnumerable<string> line)
        {
            return string.Join("\n", line);
        }
    }
}
