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

            main.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
            main.Buffer.WriteDatatype(Datatype.String);
            main.Buffer.WriteString("Hello, World!");

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.io::writeln", new List<Datatype>() { Datatype.String }, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_0", new List<Datatype>(), Datatype.MediumSigned);

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_1", new List<Datatype>(), Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Return);

            program.Add(main);

            var anotherFunction = new Function("func_0", Datatype.MediumSigned);

            anotherFunction.Locals.Append(new Scalar(Datatype.MediumSigned, 23));

            anotherFunction.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            anotherFunction.Buffer.WriteIndex(0);

            anotherFunction.Buffer.WriteOperationCode(OperationCode.Return);

            program.Add(anotherFunction);

            var finalFunction = new Function("func_1", Datatype.Void);

            finalFunction.Constants.Append(new Scalar(Datatype.String, "This was your input: "));

            finalFunction.Buffer.WriteOperationCode(OperationCode.LoadConstant);
            finalFunction.Buffer.WriteIndex(0);

            finalFunction.Buffer.WriteOperationCode(OperationCode.Call);
            finalFunction.Buffer.WriteFunction("std.io::readln", new List<Datatype>(), Datatype.String);

            finalFunction.Buffer.WriteOperationCode(OperationCode.Call);
            finalFunction.Buffer.WriteFunction("std.sfn::concat", new List<Datatype>() { Datatype.String, Datatype.String }, Datatype.String);

            finalFunction.Buffer.WriteOperationCode(OperationCode.Call);
            finalFunction.Buffer.WriteFunction("std.io::writeln", new List<Datatype>() { Datatype.String }, Datatype.Void);

            program.Add(finalFunction);

            var interpreter = new Interpreter(program);
            interpreter.Run();
        }
    }
}