using System;

namespace GenericUtil
{
    public static class ConsoleWrapper
    {
        public static void Write(object value, ConsoleColor foregroundColor)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(value);
            Console.ForegroundColor = temp;
        }

        public static void Write(object value)
        {
            Console.Write(value);
        }

        public static void WriteLine<T>(T[] value, ConsoleColor foregroundColor)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            foreach (var val in value)
            {
                Console.Write(val);
            }
            Console.WriteLine();
            Console.ForegroundColor = temp;
        }

        public static void WriteLine(object value, ConsoleColor foregroundColor)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(value);
            Console.ForegroundColor = temp;
        }

        public static void WriteLine<T>(T[] value)
        {
            foreach (var val in value)
            {
                Console.Write(val);
            }
        }

        public static void WriteLine(object value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
