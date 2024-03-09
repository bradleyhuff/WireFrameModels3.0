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
        public static void Write(string input, ConsoleColor color, ConsoleColor backGround)
        {
            var currentForegroundColor = System.Console.ForegroundColor;
            var currentBackgroundColor = System.Console.BackgroundColor;
            System.Console.ForegroundColor = color;
            System.Console.BackgroundColor = backGround;
            System.Console.Write(input);
            System.Console.ForegroundColor = currentForegroundColor;
            System.Console.BackgroundColor = currentBackgroundColor;
        }
        public static void WriteLine(string input, ConsoleColor color, ConsoleColor backGround)
        {
            var currentForegroundColor = System.Console.ForegroundColor;
            var currentBackgroundColor = System.Console.BackgroundColor;
            System.Console.ForegroundColor = color;
            System.Console.BackgroundColor = backGround;
            System.Console.WriteLine(input);
            System.Console.ForegroundColor = currentForegroundColor;
            System.Console.BackgroundColor = currentBackgroundColor;
        }
    }
}
