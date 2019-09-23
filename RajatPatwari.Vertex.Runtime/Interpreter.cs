using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private readonly Package _package;

        public Interpreter(in Package package) =>
            _package = package;

        private void RunFunction(in Function function)
        {
            ushort position = 0;
            while (position < function.Buffer.Length)
            {
                var operationCode = function.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.NoOperation)
                { }

                else if (operationCode == OperationCode.JumpAlways)
                    position = function.GetLabel(function.Buffer.ReadIdentifier(position)).Position;
                else if (operationCode == OperationCode.JumpTrue || operationCode == OperationCode.JumpFalse)
                {
                    var name = function.Buffer.ReadIdentifier(position);
                    var pop = (bool)function.Stack.Pop().Value;
                    if (operationCode == OperationCode.JumpTrue && pop || operationCode == OperationCode.JumpFalse && !pop)
                        position = function.GetLabel(name).Position;
                    else
                        position += (ushort)(1 + name.Length);
                }

                else if (operationCode == OperationCode.Call)
                {
                    var (name, parameters, @return) = function.Buffer.ReadFunction(position);
                    position += (ushort)(name.Length + parameters.Count() + 3);

                    if (name.StartsWith("std.") && name.Contains("::"))
                    {
                        var stdReturn = StandardLibrary.FindAndCall(name, @return, parameters,
                            StandardLibrary.Flatten(function.Stack, (byte)parameters.Count()));

                        if (stdReturn.@return)
                            function.Stack.Push(stdReturn.value ?? throw new InvalidOperationException(nameof(stdReturn)));
                    }
                    else if (name.Contains("::"))
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var @new = _package.FindBySignature(name, @return, parameters);

                        for (var index = 0; index < parameters.Count(); index++)
                            @new.Parameters.Update((byte)(parameters.Count() - index - 1), function.Stack.Pop());

                        RunFunction(@new);

                        if (@new.Return != Datatype.Void)
                            function.Stack.Push(@new.Stack.Pop());
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

                    (object value, int offset) wrapper;
                    switch (type)
                    {
                        case Datatype.Boolean:
                            wrapper = (function.Buffer.ReadBoolean(position), 1);
                            break;

                        case Datatype.TinySigned:
                            wrapper = (function.Buffer.ReadTinySigned(position), 1);
                            break;
                        case Datatype.TinyUnsigned:
                            wrapper = (function.Buffer.ReadTinyUnsigned(position), 1);
                            break;

                        case Datatype.ShortSigned:
                            wrapper = (function.Buffer.ReadShortSigned(position), 2);
                            break;
                        case Datatype.ShortUnsigned:
                            wrapper = (function.Buffer.ReadShortUnsigned(position), 2);
                            break;

                        case Datatype.MediumSigned:
                            wrapper = (function.Buffer.ReadMediumSigned(position), 4);
                            break;
                        case Datatype.MediumUnsigned:
                            wrapper = (function.Buffer.ReadMediumUnsigned(position), 4);
                            break;

                        case Datatype.LongSigned:
                            wrapper = (function.Buffer.ReadLongSigned(position), 8);
                            break;
                        case Datatype.LongUnsigned:
                            wrapper = (function.Buffer.ReadLongUnsigned(position), 8);
                            break;

                        case Datatype.FloatSingle:
                            wrapper = (function.Buffer.ReadFloatSingle(position), 4);
                            break;
                        case Datatype.FloatDouble:
                            wrapper = (function.Buffer.ReadFloatDouble(position), 8);
                            break;

                        case Datatype.Character:
                            wrapper = (function.Buffer.ReadCharacter(position), 2);
                            break;
                        case Datatype.String:
                            var @string = function.Buffer.ReadString(position);
                            wrapper = (@string, 1 + @string.Length * 2);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }

                    function.Stack.Push(new Scalar(type, wrapper.value));
                    position += (ushort)wrapper.offset;
                }
                else if (operationCode == OperationCode.LoadParameter || operationCode == OperationCode.LoadConstant || operationCode == OperationCode.LoadLocal)
                {
                    var index = function.Buffer.ReadIndex(position++);
                    function.Stack.Push(operationCode switch
                    {
                        OperationCode.LoadParameter => function.Parameters.Get(index),
                        OperationCode.LoadConstant => function.Constants.Get(index),
                        OperationCode.LoadLocal => function.Locals.Get(index),
                        _ => throw new InvalidOperationException()
                    });
                }

                else if (operationCode == OperationCode.SetParameter || operationCode == OperationCode.SetLocal)
                {
                    var index = function.Buffer.ReadIndex(position++);
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
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        public void Run() =>
            RunFunction(_package.FindBySignature("main", Datatype.Void, new List<Datatype>()));
    }
}