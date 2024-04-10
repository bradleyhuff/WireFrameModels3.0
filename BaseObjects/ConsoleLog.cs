using System.Xml.Linq;
using Console = BaseObjects.Console;

namespace BaseObjects
{
    public static class ConsoleLog
    {
        private static Stack<string> stack = new Stack<string>();
        private static ConsoleColor[] colors = [ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Gray];

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
            foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
            {
                Console.Write($"{element.Message} > ", colors[Math.Min(element.Index, colors.Length - 1)]);
            }
            Console.WriteLine(message, colors[Math.Min(stack.Count, colors.Length - 1)]);
        }
        public static void WriteNextLine(string message)
        {
            foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
            {
                Console.Write($"{" ".Repeat(element.Message.Length)}   ");
            }
            Console.WriteLine(message, colors[Math.Min(stack.Count, colors.Length - 1)]);
        }
        public static void WriteNextLine(string message, ConsoleColor color)
        {
            foreach (var element in stack.Reverse().Select((s, i) => new { Message = s, Index = i }))
            {
                Console.Write($"{" ".Repeat(element.Message.Length)}   ");
            }
            Console.WriteLine(message, color);
        }
    }
}
