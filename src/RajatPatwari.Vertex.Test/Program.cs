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
            Console.Write("Please enter a file path: ");
            var path = Console.ReadLine();
            Console.WriteLine();

            var code = File.ReadAllText(path);

            // Interpreter Test

            var main = new Function("main", Datatype.Void);

            main.Locals.Add(0, Datatype.Integer, 4L);
            main.Locals.Add(1, Datatype.Integer, 2L);

            main.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            main.Buffer.WriteByte(0);

            main.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            main.Buffer.WriteByte(1);

            main.Buffer.WriteOperationCode(OperationCode.Subtract);

            main.Buffer.WriteOperationCode(OperationCode.LoadInteger);
            main.Buffer.WriteInteger(1L);

            main.Buffer.WriteOperationCode(OperationCode.CheckGreater);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteDatatype(Datatype.Void);
            main.Buffer.WriteString("$System.Console.WriteLine");
            main.Buffer.WriteDatatypes(new[] { Datatype.Boolean });

            var interpreter = new Interpreter(new[] { main });
            interpreter.Run();
        }
    }
}