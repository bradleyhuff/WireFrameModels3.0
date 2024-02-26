namespace BaseObjects
{
    public static class Console
    {
        public static void WriteLine()
        {
            System.Console.WriteLine();
        }
        public static void WriteLine(string input)
        {
            System.Console.WriteLine(input);
        }
        public static void Write(string input)
        {
            System.Console.Write(input);
        }
        public static void Write(string input, ConsoleColor color)
        {
            var currentForegroundColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.Write(input);
            System.Console.ForegroundColor = currentForegroundColor;
        }
        public static void WriteLine(string input, ConsoleColor color)
        {
            var currentForegroundColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(input);
            System.Console.ForegroundColor = currentForegroundColor;
        }
    }
}
