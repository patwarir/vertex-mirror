using RajatPatwari.Vertex.Runtime;
using System;
using System.IO;

namespace RajatPatwari.Vertex.Example
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("What file do you want to run?");
            var file = Console.ReadLine();

            var parser = new Parser(File.ReadAllText(file));
            parser.Run();

            var interpreter = new Interpreter(parser.Package);
            interpreter.Run();
        }
    }
}