using RajatPatwari.Vertex.Runtime;
using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Example
{
    public static class Program
    {
        public static void Main()
        {
            var package = new Package("rajat_patwari.test");
            package.Globals.Append((Scalar)56);

            var main = new Function("main", Function.Void);

            main.Buffer.WriteOperationCode(OperationCode.NoOperation);

            main.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
            main.Buffer.WriteDatatype(Datatype.String);
            main.Buffer.WriteString("Hello, World!");

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("globals", Function.NoParameters, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(main);

            var globals = new Function("globals", Function.Void);

            globals.Buffer.WriteOperationCode(OperationCode.LoadGlobal);
            globals.Buffer.Write(0);

            globals.Buffer.WriteOperationCode(OperationCode.Call);
            globals.Buffer.WriteFunction("std.cst:to_str", new List<Datatype> { Datatype.Integer }, Datatype.String);

            globals.Buffer.WriteOperationCode(OperationCode.Call);
            globals.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            globals.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(globals);

            var interpreter = new Interpreter(package);
            interpreter.Run();
        }
    }
}