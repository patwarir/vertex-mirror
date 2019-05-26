using RajatPatwari.Vertex.Runtime;
using RajatPatwari.Vertex.Runtime.AbstractSyntaxTree;
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

            /*Console.WriteLine("CODE:");
            for (var index = 0; index < code.Length; index++)
                Console.WriteLine($"{index}\t{code[index]}");

            Console.Write("\n\n\n");*/

            var lexer = new Lexer(code);
            lexer.Run();

            /*Console.WriteLine("TOKENS:");
            for (var index = 0; index < lexer.Tokens.Count; index++)
                Console.WriteLine($"{index}\t{lexer.Tokens[index].GetType().Name.AsSpan().Slice(0, 6).ToString()}\t|{lexer.Tokens[index]}|");

            Console.Write("\n\n\n");*/

            var parser = new Parser(lexer.Tokens);
            parser.Run();

            /*Console.WriteLine("SYNTAX TREE:");
            foreach (var treeObject in parser.TreeObjects)
            {
                Console.WriteLine(treeObject);

                var function = (Function)treeObject;
                foreach (var treeObj in function.TreeObjects)
                {
                    if (treeObj is Conditional conditional)
                    {
                        Console.WriteLine($"\t{conditional.GetType().Name.ToString().AsSpan().Slice(0, 6).ToString()}\t{conditional}");

                        foreach (var expression in conditional.Expressions)
                            Console.WriteLine($"\t\t{expression.GetType().Name.ToString().AsSpan().Slice(0, 6).ToString()}\t{expression}");
                    }
                    else
                        Console.WriteLine($"\t{treeObj.GetType().Name.ToString().AsSpan().Slice(0, 6).ToString()}\t{treeObj}");
                }
            }

            Console.Write("\n\n\n");*/

            var converter = new RepresentationConverter(parser.TreeObjects);
            converter.Run();

            /*Console.WriteLine("BUFFER:");
            foreach (var function in converter.FunctionRepresentations)
            {
                Console.WriteLine(function);

                for (var index = 0; index < function.Buffer.Stream.Count; index++)
                    Console.WriteLine($"\t{index}\t{function.Buffer.Stream[index]}");
            }

            Console.Write("\n\n\n");

            Console.WriteLine("RUN:\n");*/
            var interpreter = new Interpreter(converter.FunctionRepresentations);
            interpreter.Run();
        }
    }
}