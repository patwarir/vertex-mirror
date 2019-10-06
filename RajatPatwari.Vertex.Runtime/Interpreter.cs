using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private readonly IList<FunctionImplementation> _functions;

        public Interpreter(IList<FunctionImplementation> functions) =>
            _functions = functions;

        private static object[] Flatten(Stack<(Datatype datatype, object value)> stack, int numParams)
        {
            var array = new object[numParams--];
            /*for (; stack.Count > 0 && numParams > 0; numParams--)
                array[numParams] = stack.Pop().value;*/
            while (stack.Count > 0 && numParams >= 0)
            {
                array[numParams] = stack.Pop().value;
                numParams--;
            }
            return array;
        }

        private void RunFunction(FunctionImplementation function)
        {
            int position = 0;
            while (position < function.Buffer.Length)
            {
                var operationCode = function.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.NoOperation)
                { }

                else if (operationCode == OperationCode.JumpAlways)
                    position = function.GetLabelPosition(function.Buffer.ReadIdentifier(position));
                else if (operationCode == OperationCode.JumpTrue)
                {
                    var name = function.Buffer.ReadIdentifier(position);
                    if ((bool)function.Stack.Pop().value)
                        position = function.GetLabelPosition(name);
                    else
                        position += 1 + name.Length;
                }

                else if (operationCode == OperationCode.Call)
                {
                    var declaration = function.Buffer.ReadDeclaration(position);
                    position += declaration.QualifiedName.Length + declaration.Parameters.Count() + 3;

                    if (declaration.QualifiedName.StartsWith("std.") && declaration.QualifiedName.Contains("::"))
                    {
                        var stdReturn = StandardLibrary.StandardLibraryCaller.CallFunction(
                            new FunctionDeclaration(declaration.QualifiedName, declaration.Return, declaration.Parameters),
                            Flatten(function.Stack, declaration.Parameters.Count()));

                        if (stdReturn.@return)
                            function.Stack.Push(stdReturn.value ?? throw new InvalidOperationException(nameof(stdReturn)));
                    }
                    else if (
                        declaration.QualifiedName.Remove(declaration.QualifiedName.LastIndexOf("::"))
                        != function.Declaration.QualifiedName.Remove(function.Declaration.QualifiedName.LastIndexOf("::")))
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var @new = _functions.Single(newFunction =>
                            newFunction.Declaration.QualifiedName == declaration.QualifiedName
                            && newFunction.Declaration.Return == declaration.Return
                            && newFunction.Declaration.Parameters.SequenceEqual(declaration.Parameters));

                        RunFunction(@new);

                        if (@new.Declaration.Return != Datatype.Void)
                            function.Stack.Push(@new.ReturnValue);

                        @new.Reset();
                    }
                }
                else if (operationCode == OperationCode.Return)
                {
                    if (function.Declaration.Return != Datatype.Void)
                        function.ReturnValue = function.Stack.Pop();
                    return;
                }

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
                    (Datatype datatype, object value) valueTop = function.Stack.Pop(),
                        valueBottom = function.Stack.Pop();
                    function.Stack.Push(valueTop); function.Stack.Push(valueBottom);
                }

                else if (operationCode == OperationCode.LoadLiteral)
                {
                    var type = function.Buffer.ReadDatatype(position++);

                    object value; int offset;
                    switch (type)
                    {
                        case Datatype.Boolean:
                            value = function.Buffer.ReadBoolean(position);
                            offset = 1;
                            break;
                        case Datatype.Integer:
                            value = function.Buffer.ReadInteger(position);
                            offset = 8;
                            break;
                        case Datatype.Float:
                            value = function.Buffer.ReadFloat(position);
                            offset = 8;
                            break;
                        case Datatype.String:
                            var @string = function.Buffer.ReadString(position);
                            value = @string;
                            offset = 1 + @string.Length * 2;
                            break;
                        default:
                            throw new InvalidOperationException(nameof(type));
                    }

                    function.Stack.Push((type, value));
                    position += offset;
                }

                else if (operationCode == OperationCode.LoadParameter || operationCode == OperationCode.LoadLocal)
                {
                    var index = function.Buffer.ReadIndex(position++);
                    function.Stack.Push(operationCode switch
                    {
                        OperationCode.LoadParameter => function.ParameterValues[index],
                        OperationCode.LoadLocal => function.Locals[index],
                        _ => throw new InvalidOperationException(nameof(operationCode))
                    });
                }
                else if (operationCode == OperationCode.SetParameter || operationCode == OperationCode.SetLocal)
                {
                    var index = function.Buffer.ReadIndex(position++);
                    var pop = function.Stack.Pop();
                    switch (operationCode)
                    {
                        case OperationCode.SetParameter:
                            function.ParameterValues[index] = pop;
                            break;
                        case OperationCode.SetLocal:
                            function.Locals[index] = pop;
                            break;
                        default:
                            throw new InvalidOperationException(nameof(operationCode));
                    }
                }
            }
        }

        public void Run() =>
            RunFunction(
                _functions.Single(function =>
                    function.Declaration.QualifiedName.EndsWith("::main")
                    && function.Declaration.Return == Datatype.Void
                    && !function.Declaration.Parameters.Any()));
    }
}