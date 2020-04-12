using RajatPatwari.Vertex.Runtime;
using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Example
{
    public static class Program
    {
        public static void Main()
        {
            // pkg rajat_patwari.test.one
            var package = new Package("rajat_patwari.test.one");

            // fn main() -> vd
            var main = new Function("main", Function.Void);

            // ld.lt str "Hello, World!"
            main.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
            main.Buffer.WriteDatatype(Datatype.String);
            main.Buffer.WriteString("Hello, World!");

            // call std.sio:writeln(str) -> vd
            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            // call func_0() -> int
            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_0", Function.NoParameters, Datatype.Integer);

            // call std.cst:to_str(int) -> str
            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.cst:to_str", new List<Datatype> { Datatype.Integer }, Datatype.String);

            // call std.sio:writeln(str) -> vd
            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            // call func_1() -> vd
            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_1", Function.NoParameters, Datatype.Void);

            // ret
            main.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(main);

            // fn func_0() -> int
            var func0 = new Function("func_0", (Scalar)Datatype.Integer);

            // lc {
            //     int
            // }
            //
            // ld.lt int 23
            // st.lc 0
            func0.Locals.Append((Scalar)23);

            // ld.lc 0
            func0.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            func0.Buffer.Write(0);

            // ret
            func0.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(func0);

            // fn func_1() -> vd
            var func1 = new Function("func_1", (Scalar)Datatype.Void);

            // cn {
            //     str "This was your input: "
            // }
            func1.Constants.Append((Scalar)"This was your input: ");

            // ld.cn 0
            func1.Buffer.WriteOperationCode(OperationCode.LoadConstant);
            func1.Buffer.Write(0);

            // call std.sio:readln() -> str
            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sio:readln", Function.NoParameters, Datatype.String);

            // call std.sfn:cat(str, str) -> str
            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sfn:cat", new List<Datatype>() { Datatype.String, Datatype.String }, Datatype.String);

            // call std.sio:writeln(str) -> vd
            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            // ret
            func1.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(func1);

            var interpreter = new Interpreter(package);
            interpreter.Run();
        }
    }
}