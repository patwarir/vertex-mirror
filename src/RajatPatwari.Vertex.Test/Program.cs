using RajatPatwari.Vertex.Runtime;
using RajatPatwari.Vertex.Runtime.VirtualMachine;
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
            Console.WriteLine();

            var code = File.ReadAllText(path);*/

            var main = new Function("main", Datatype.Void);

            main.Locals.Add(0, Datatype.Integer, 4L);

            main.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            main.Buffer.WriteByte(0);

            main.Buffer.WriteOperationCode(OperationCode.Negate);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteDatatype(Datatype.Void);
            main.Buffer.WriteString("$System.Console.WriteLine");
            main.Buffer.WriteDatatypes(new[] { Datatype.Integer });

            var interpreter = new Interpreter(new[] { main });
            interpreter.Run();
        }
    }
}