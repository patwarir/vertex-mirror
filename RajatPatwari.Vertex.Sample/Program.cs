using RajatPatwari.Vertex.Runtime;
using System;
using System.IO;

namespace RajatPatwari.Vertex.Sample
{
    public static class Program
    {
        public static void Main()
        {
            var directory = Environment.CurrentDirectory;
            directory = directory.AsSpan().Slice(0, directory.LastIndexOf("RajatPatwari.Vertex")).ToString();
            directory = Path.Combine(directory, "vertex");

            var file = Path.Combine(directory, "main.vir");
            var code = File.ReadAllText(file);

            Console.WriteLine("CODE:");
            for (var index = 0; index < code.Length; index++)
                Console.WriteLine($"{index}|{code[index]}");

            Console.Write("\n\n\n");

            var lexer = new Lexer(code);
            lexer.Run();

            Console.WriteLine("TOKENS:");
            for (var index = 0; index < lexer.Tokens.Count; index++)
                Console.WriteLine($"{index}\t{lexer.Tokens[index].GetType().Name.AsSpan().Slice(0, 6).ToString()}\t|{lexer.Tokens[index]}|");

            Console.Write("\n\n\n");

            var parser = new Parser(lexer.Tokens);
            parser.Run();

            Console.WriteLine("SYNTAX TREE:");
            foreach (var treeObject in parser.TreeObjects)
                Console.WriteLine(treeObject);
        }
    }
}