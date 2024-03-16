using Console = BaseObjects.Console;

namespace BasicObjects
{
    public static class TableDisplays
    {
        public static void ShowCountSpread<T>(string title, IEnumerable<T> list, Func<T, int> GetCount)
        {
            Console.WriteLine(title, ConsoleColor.Yellow);
            var countTable = new Dictionary<int, int>();
            foreach (var element in list)
            {
                var count = GetCount(element);
                if (!countTable.ContainsKey(count)) { countTable[count] = 0; }
                countTable[count]++;
            }
            foreach (var pair in countTable.OrderBy(p => p.Key))
            {
                Console.WriteLine($"{pair.Key}:{pair.Value}");
            }
            Console.WriteLine();
        }
        public static void ShowCountSpreadWithSum<T>(string title, IEnumerable<T> list, Func<T, int> GetCount, int keyPadding = 3, int valuePadding = 8, int sumPadding = 8)
        {
            Console.WriteLine(title, ConsoleColor.Yellow);
            var countTable = new Dictionary<int, int>();
            foreach (var element in list)
            {
                var count = GetCount(element);
                if (!countTable.ContainsKey(count)) { countTable[count] = 0; }
                countTable[count]++;
            }
            int sum = 0;
            foreach (var pair in countTable.OrderBy(p => p.Key))
            {
                sum += pair.Key * pair.Value;
                Console.Write($"{pair.Key.ToString().PadLeft(keyPadding)}: {pair.Value.ToString().PadLeft(valuePadding)}");
                Console.WriteLine($" + {sum.ToString().PadLeft(sumPadding)}", ConsoleColor.Green);
            }
            Console.WriteLine();
        }
    }
}
