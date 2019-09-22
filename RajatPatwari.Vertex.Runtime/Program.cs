using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var program = new Package("rajatpatwari_test");

            var main = new Function("main", Datatype.Void);

            main.Constants.Append(new Scalar(Datatype.String, "Hello, World!"));

            main.Buffer.WriteOperationCode(OperationCode.LoadConstant);
            main.Buffer.WriteIndex(0);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteIdentifier("std.io::writeln");
            main.Buffer.WriteDatatypes(new List<Datatype>() { Datatype.String });
            main.Buffer.WriteDatatype(Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Return);

            program.Add(main);

            var interpreter = new Interpreter(program);
            interpreter.Run();
        }
    }
}