using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private readonly Package _package;

        public Interpreter(Package package) =>
            _package = package;

        private void RunFunction(Function function)
        {
            int position = 0;
            while (position < function.Buffer.Length)
            {
                var operationCode = function.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.NoOperation)
                { }

                else if (operationCode == OperationCode.JumpAlways)
                    position = function.GetLabelPosition(function.Buffer.ReadString(position));
                else if (operationCode == OperationCode.JumpTrue || operationCode == OperationCode.JumpFalse)
                {
                    var name = function.Buffer.ReadString(position);
                    var pop = (bool)(function.Stack.Pop().Value ?? throw new InvalidOperationException(nameof(Interpreter)));
                    if (operationCode == OperationCode.JumpTrue && pop || operationCode == OperationCode.JumpFalse && !pop)
                        position = function.GetLabelPosition(name);
                    else
                        position += 1 + name.Length;
                }

                else if (operationCode == OperationCode.Call)
                {
                    var newFunction = function.Buffer.ReadFunction(position);
                    position += newFunction.Name.Length + newFunction.Parameters.Count + 3;

                    if (newFunction.Name.StartsWith("std.") && newFunction.Name.Contains(':'))
                    {
                        var @return = StandardLibrary.Execute(newFunction.Name, newFunction.Return.Datatype, newFunction.Parameters.GetDatatypes(),
                            Function.Flatten(function.Stack, newFunction.Parameters.Count));

                        if (@return.returns)
                            function.Stack.Push(@return.value switch
                            {
                                bool value => (Scalar)value,
                                long value => (Scalar)value,
                                double value => (Scalar)value,
                                string value => (Scalar)value,
                                _ => throw new InvalidOperationException(nameof(@return))
                            });
                    }
                    else if (newFunction.Name.Contains(':'))
                    {
                        // TODO: Implement this.
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var @new = _package.FindBySignature(newFunction.Name, newFunction.Return.Datatype, newFunction.Parameters.GetDatatypes())
                            ?? throw new InvalidOperationException(nameof(newFunction));

                        for (var index = 0; index < newFunction.Parameters.Count; index++)
                            @new.Parameters.Update(newFunction.Parameters.Count - index - 1, function.Stack.Pop());

                        RunFunction(@new);

                        if (@new.Return.Datatype != Datatype.Void)
                            function.Stack.Push(@new.Stack.Pop());

                        @new.Reset();
                    }
                }
                else if (operationCode == OperationCode.Return)
                    return;

                else if (operationCode == OperationCode.Pop)
                    function.Stack.Pop();
                else if (operationCode == OperationCode.Clear)
                    function.Stack.Clear();
                else if (operationCode == OperationCode.Duplicate)
                {
                    var value = function.Stack.Pop();
                    function.Stack.Push(value); function.Stack.Push(value);
                }
                else if (operationCode == OperationCode.Rotate)
                {
                    Scalar valueTop = function.Stack.Pop(), valueBottom = function.Stack.Pop();
                    function.Stack.Push(valueTop); function.Stack.Push(valueBottom);
                }

                else if (operationCode == OperationCode.LoadLiteral)
                {
                    var type = function.Buffer.ReadDatatype(position++);

                    (object value, int offset) wrapper = type switch
                    {
                        Datatype.Boolean => (function.Buffer.ReadBoolean(position), 1),
                        Datatype.Integer => (function.Buffer.ReadInteger(position), 8),
                        Datatype.Float => (function.Buffer.ReadInteger(position), 8),
                        Datatype.String => (function.Buffer.ReadString(position), 0),
                        _ => throw new InvalidOperationException()
                    };

                    if (type == Datatype.String)
                        wrapper.offset += 1 + ((string)wrapper.value).Length;

                    var scalar = (Scalar)type;
                    scalar.DefineValue(wrapper.value);
                    function.Stack.Push(scalar);
                    position += wrapper.offset;
                }

                else if (operationCode == OperationCode.LoadGlobal || operationCode == OperationCode.LoadParameter
                    || operationCode == OperationCode.LoadConstant || operationCode == OperationCode.LoadLocal)
                {
                    var index = function.Buffer.Read(position++);
                    function.Stack.Push(operationCode switch
                    {
                        OperationCode.LoadGlobal => _package.Globals.Get(index),
                        OperationCode.LoadParameter => function.Parameters.Get(index),
                        OperationCode.LoadConstant => function.Constants.Get(index),
                        OperationCode.LoadLocal => function.Locals.Get(index),
                        _ => throw new InvalidOperationException(nameof(operationCode))
                    });
                }

                else if (operationCode == OperationCode.SetParameter || operationCode == OperationCode.SetLocal)
                {
                    var index = function.Buffer.Read(position++);
                    var pop = function.Stack.Pop();
                    switch (operationCode)
                    {
                        case OperationCode.SetParameter:
                            function.Parameters.Update(index, pop);
                            break;
                        case OperationCode.SetLocal:
                            function.Locals.Update(index, pop);
                            break;
                        default:
                            throw new InvalidOperationException(nameof(operationCode));
                    }
                }
            }
        }

        public void Run() =>
            RunFunction(_package.FindBySignature("main", Datatype.Void, Function.NoParameters));
    }
}