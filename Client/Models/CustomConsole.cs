using System;

namespace SharpC2.Models
{
    public static class CustomConsole
    {
        public static void WriteMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] {message}");
            Console.ResetColor();
        }
        
        public static void WriteWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] {warning}");
            Console.ResetColor();
        }

        public static void WriteError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[x] {error}");
            Console.ResetColor();
        }
    }
}