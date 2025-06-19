using System.Xml.Linq;
using Console = BaseObjects.Console;

namespace BaseObjects
{
    public static class ConsoleLog
    {
        private static Stack<string> stack = new Stack<string>();
        private static ConsoleColor[] colors = [ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Gray];
        public static int MaximumLevels = 16;
        private static object lockObject = new Object();

        public static void Push(string parent)
        {
            stack.Push(parent);
        }

        public static void Pop()
        {
            stack.Pop();
        }
        public static void WriteLine(string message)
        {
            if (stack.Count >= MaximumLevels) { return; }
            lock (lockObject)
            {
                foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
                {
                    Console.Write($"{element.Message} > ", colors[element.Index % colors.Length]);
                }
                Console.WriteLine(message, colors[stack.Count % colors.Length]);
            }
        }
        public static void WriteNextLine(string message)
        {
            if (stack.Count >= MaximumLevels) { return; }
            lock (lockObject)
            {
                foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
                {
                    Console.Write($"{" ".Repeat(element.Message.Length)}   ");
                }
                Console.WriteLine(message, colors[stack.Count % colors.Length]);
            }
        }
        public static void WriteNextLine(string message, ConsoleColor color)
        {
            if (stack.Count >= MaximumLevels) { return; }
            lock (lockObject)
            {
                foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
                {
                    Console.Write($"{" ".Repeat(element.Message.Length)}   ");
                }
                Console.WriteLine(message, color);
            }
        }
    }
}
