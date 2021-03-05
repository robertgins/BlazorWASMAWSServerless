using System;

namespace DebugConsoleCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press X to exit...");
            char exitChar = Console.ReadKey().KeyChar;
            while (exitChar != 'X' && exitChar != 'x')
            {
                exitChar = Console.ReadKey().KeyChar;
            }
        }
    }
}
