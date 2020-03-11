using RajatPatwari.Vertex.Runtime;
using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Example
{
    public static class Program
    {
        public static void Main()
        {
            var package = new Package("rajat_patwari.test.one");

            var main = new Function("main", Function.Void);

            main.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
            main.Buffer.WriteDatatype(Datatype.String);
            main.Buffer.WriteString("Hello, World!");

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_0", Function.NoParameters, Datatype.Integer);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.cst:to_str", new List<Datatype> { Datatype.Integer }, Datatype.String);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(main);

            var func0 = new Function("func_0", (Scalar)Datatype.Integer);

            func0.Locals.Append((Scalar)23);

            func0.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            func0.Buffer.Write(0);

            func0.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(func0);

            var interpreter = new Interpreter(package);
            interpreter.Run();
        }
    }
}