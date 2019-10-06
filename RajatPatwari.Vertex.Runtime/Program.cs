using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var function = new FunctionImplementation("rajatpatwari_test::main", Datatype.Void);
            function.ParameterValues.Add((Datatype.String, "Hello, World!"));
            function.Locals.Add((Datatype.String, string.Empty));

            function.Buffer.WriteOperationCode(OperationCode.LoadParameter);
            function.Buffer.WriteIndex(0);

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sio::ln_str", Datatype.String, new Datatype[0]));

            function.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
            function.Buffer.WriteDatatype(Datatype.String);
            function.Buffer.WriteString("It Works!");

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sfn::concat", Datatype.String, new Datatype[] { Datatype.String, Datatype.String }));

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sfn::concat", Datatype.String, new Datatype[] { Datatype.String, Datatype.String }));

            function.Buffer.WriteOperationCode(OperationCode.SetLocal);
            function.Buffer.WriteIndex(0);

            function.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            function.Buffer.WriteIndex(0);

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sio::writeln", Datatype.Void, new Datatype[] { Datatype.String }));

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sio::read", Datatype.Void, new Datatype[0]));

            function.Buffer.WriteOperationCode(OperationCode.Call);
            function.Buffer.WriteDeclaration(new FunctionDeclaration(
                "std.sio::clear", Datatype.Void, new Datatype[0]));

            var interpreter = new Interpreter(new List<FunctionImplementation>() { function });
            interpreter.Run();
        }
    }
}