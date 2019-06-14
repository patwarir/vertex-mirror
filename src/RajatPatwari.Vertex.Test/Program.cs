using RajatPatwari.Vertex.Runtime;
using System;
using System.IO;

namespace RajatPatwari.Vertex.Test
{
    public static class Program
    {
        public static void Main()
        {
            /*Console.Write("Please enter a file path: ");
            var path = Console.ReadLine();
            Console.WriteLine();*/

            var code = File.ReadAllText(/*path*/@"C:\Users\patwa\Desktop\Projects\vertex\samples\Full Test\main.vir");

            var parser = new Parser(code);
            parser.Run();

            var interpreter = new Interpreter(parser.Functions);
            interpreter.Run();
        }
    }
}