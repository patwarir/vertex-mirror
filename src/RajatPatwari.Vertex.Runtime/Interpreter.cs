using System.Collections.Generic;
using System.Linq;
using RajatPatwari.Vertex.Runtime.VirtualMachine;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private readonly IEnumerable<Function> _functions;

        public Interpreter(IEnumerable<Function> functions) =>
            _functions = functions;

        private static Label GetLabel(Function function, string name) =>
            function.Labels.First(label => label.Name == name);

        private static Function GetFunction(IEnumerable<Function> functions, string name, string package, Datatype @return, IEnumerable<Datatype> parameters) =>
            functions.First(function => function.Name == name && function.Package == package && function.Return == @return && function.Parameters.GetDatatypes().SequenceEqual(parameters));

        private static Function GetFunctionIgnorePackage(IEnumerable<Function> functions, string name, Datatype @return, IEnumerable<Datatype> parameters) =>
            functions.First(function => function.Name == name && function.Return == @return && function.Parameters.GetDatatypes().SequenceEqual(parameters));

        private static void RunFunction(Function function)
        {
            ushort position = 0;
            while (position < function.Buffer.Length)
            {
                var operationCode = function.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.NoOperation)
                { }

                else if (operationCode == OperationCode.JumpAlways)
                    position = GetLabel(function, function.Buffer.ReadString(position)).Position;
                else if (operationCode == OperationCode.JumpTrue || operationCode == OperationCode.JumpFalse)
                {
                    var identifier = function.Buffer.ReadString(position);
                    var pop = (bool)function.Stack.Pop();
                    if (operationCode == OperationCode.JumpTrue && pop || operationCode == OperationCode.JumpFalse && !pop)
                        position = GetLabel(function, identifier).Position;
                    else
                        position += (ushort)(1 + identifier.Length);
                }

                else if (operationCode == OperationCode.Call)
                { }
                else if (operationCode == OperationCode.Throw)
                { }
                else if (operationCode == OperationCode.Return)
                    return;

                else if (operationCode == OperationCode.Cast)
                { }
                else if (operationCode == OperationCode.CheckType)
                { }

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
                    object valueTop = function.Stack.Pop(), valueBottom = function.Stack.Pop();
                    function.Stack.Push(valueTop); function.Stack.Push(valueBottom);
                }

                else if (operationCode == OperationCode.LoadLiteral)
                { }
                else if (operationCode == OperationCode.LoadParameter || operationCode == OperationCode.LoadConstant || operationCode == OperationCode.LoadLiteral)
                {
                    var index = function.Buffer.ReadIndex(position++);
                    switch (operationCode)
                    {
                        case OperationCode.LoadParameter:
                            function.Stack.Push(function.Parameters.GetValue(index));
                            break;
                        case OperationCode.LoadConstant:
                            function.Stack.Push(function.Constants.GetValue(index));
                            break;
                        case OperationCode.LoadLocal:
                            function.Stack.Push(function.Locals.GetValue(index));
                            break;
                    }
                }

                else if (operationCode == OperationCode.StoreParameter || operationCode == OperationCode.StoreLocal)
                {
                    var index = function.Buffer.ReadIndex(position++);
                    var pop = function.Stack.Pop();
                    switch (operationCode)
                    {
                        case OperationCode.StoreParameter:
                            function.Parameters.Update(index, pop);
                            break;
                        case OperationCode.StoreLocal:
                            function.Locals.Update(index, pop);
                            break;
                    }
                }
            }
        }

        public void Run() =>
            RunFunction(GetFunctionIgnorePackage(_functions, "main", Datatype.Void, new List<Datatype>()));
    }
}