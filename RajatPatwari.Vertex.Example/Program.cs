﻿using RajatPatwari.Vertex.Runtime;
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

            main.Buffer.WriteOperationCode(OperationCode.Call);
            main.Buffer.WriteFunction("func_1", Function.NoParameters, Datatype.Void);

            main.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(main);

            var func0 = new Function("func_0", (Scalar)Datatype.Integer);

            func0.Locals.Append((Scalar)23);

            func0.Buffer.WriteOperationCode(OperationCode.LoadLocal);
            func0.Buffer.Write(0);

            func0.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(func0);

            var func1 = new Function("func_1", (Scalar)Datatype.Void);

            func1.Constants.Append((Scalar)"This was your input: ");

            func1.Buffer.WriteOperationCode(OperationCode.LoadConstant);
            func1.Buffer.Write(0);

            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sio:readln", Function.NoParameters, Datatype.String);

            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sfn:cat", new List<Datatype>() { Datatype.String, Datatype.String }, Datatype.String);

            func1.Buffer.WriteOperationCode(OperationCode.Call);
            func1.Buffer.WriteFunction("std.sio:writeln", new List<Datatype> { Datatype.String }, Datatype.Void);

            func1.Buffer.WriteOperationCode(OperationCode.Return);

            package.Functions.Add(func1);

            var interpreter = new Interpreter(package);
            interpreter.Run();
        }
    }
}