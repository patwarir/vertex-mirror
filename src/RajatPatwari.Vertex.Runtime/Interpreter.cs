using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private readonly IEnumerable<Function> _functions;

        public Interpreter(IEnumerable<Function> functions) =>
            _functions = functions;

        private Label GetLabel(Function function, string name) =>
            function.Labels.First(label => label.Name == name);

        private Function GetFunction(string name, Datatype @return, IEnumerable<Datatype> parameters) =>
            _functions.First(function => function.Name == name && function.Return == @return && function.Parameters.GetDatatypes().SequenceEqual(parameters));

        private object Check(Datatype datatype, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (datatype)
            {
                case Datatype.Boolean:
                    return (bool)value;
                case Datatype.Integer:
                    return (long)value;
                case Datatype.Float:
                    return (double)value;
                case Datatype.String:
                    return (string)value;
            }

            throw new ArgumentException($"{nameof(datatype)} {datatype} is valid for {value}!");
        }

        private object Arithmetic(OperationCode operation, object valueOne, object valueTwo)
        {
            if (operation == OperationCode.Add)
            {

            }
            else if (operation == OperationCode.Subtract)
            {

            }
            else if (operation == OperationCode.Multiply)
            {

            }
            else if (operation == OperationCode.Divide)
            {

            }
            else if (operation == OperationCode.Modulate)
            {

            }

            throw new InvalidOperationException($"Invalid arithmetic {nameof(operation)} {operation}!");
        }

        private bool Conditional(OperationCode type, object valueOne, object valueTwo)
        {
            if (type == OperationCode.CheckEquals)
            {

            }
            else if (type == OperationCode.CheckGreater)
            {

            }
            else if (type == OperationCode.CheckLess)
            {

            }
            else if (type == OperationCode.CheckGreaterEquals)
            {

            }
            else if (type == OperationCode.CheckLessEquals)
            {

            }

            throw new InvalidOperationException($"Invalid conditional {nameof(type)} {type}!");
        }

        private void RunFunction(Function function)
        {
            ushort position = 0;
            while (position < function.Buffer.Length)
            {
                var operationCode = function.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.NoOperation)
                { }

                else if (operationCode == OperationCode.JumpAlways)
                    position = GetLabel(function, function.Buffer.ReadString(position)).Position;
                else if (operationCode == OperationCode.JumpTrue)
                {
                    var name = function.Buffer.ReadString(position);
                    if ((bool)function.Stack.Pop())
                        position = GetLabel(function, name).Position;
                    else
                        position += (ushort)(1 + name.Length);
                }
                else if (operationCode == OperationCode.JumpFalse)
                {
                    var name = function.Buffer.ReadString(position);
                    if (!(bool)function.Stack.Pop())
                        position = GetLabel(function, name).Position;
                    else
                        position += (ushort)(1 + name.Length);
                }

                else if (operationCode == OperationCode.Call)
                {
                    var @return = function.Buffer.ReadDatatype(position++);

                    var name = function.Buffer.ReadString(position);
                    position += (ushort)(1 + name.Length);

                    var parameters = function.Buffer.ReadDatatypes(position);
                    position += (ushort)(1 + parameters.Count());

                    if (name.StartsWith('$'))
                    {
                        if (name == "$System.Console.Write" && @return == Datatype.Void)
                            Console.Write(Check(parameters.ElementAt(0), function.Stack.Pop()));
                        else if (name == "$System.Console.WriteLine" && @return == Datatype.Void)
                            Console.WriteLine(Check(parameters.ElementAt(0), function.Stack.Pop()));
                        else if (name == "$System.Console.Read" && @return == Datatype.Void)
                            Console.Read();
                        else if (name == "$System.Console.ReadLine" && @return == Datatype.String)
                            function.Stack.Push(Console.ReadLine());
                    }
                    else
                    {
                        var @new = GetFunction(name, @return, parameters);

                        for (var index = 0; index < parameters.Count(); index++)
                            @new.Parameters.Change((byte)(parameters.Count() - index - 1), function.Stack.Pop());

                        RunFunction(@new);

                        if (@new.Return != Datatype.Void)
                            function.Stack.Push(Check(@new.Return, @new.Stack.Pop()));
                    }
                }
                else if (operationCode == OperationCode.Return)
                    return;

                else if (operationCode == OperationCode.Throw)
                {
                    var name = function.Buffer.ReadString(position);
                    position += (ushort)(1 + name.Length);

                    var parameters = function.Buffer.ReadDatatypes(position);
                    position += (ushort)(1 + parameters.Count());

                    if (name == "$System.Exception" && parameters.Count() == 0)
                        throw new Exception();
                    else if (name == "$System.Exception" && parameters.Count() == 1)
                        throw new Exception((string)function.Stack.Pop());

                    else if (name == "$System.InvalidOperationException" && parameters.Count() == 0)
                        throw new InvalidOperationException();
                    else if (name == "$System.InvalidOperationException" && parameters.Count() == 1)
                        throw new InvalidOperationException((string)function.Stack.Pop());

                    else if (name == "$System.ArgumentException" && parameters.Count() == 0)
                        throw new ArgumentException();
                    else if (name == "$System.ArgumentException" && parameters.Count() == 1)
                        throw new ArgumentException((string)function.Stack.Pop());
                    else if (name == "$System.ArgumentException" && parameters.Count() == 2)
                    {
                        string parameter = (string)function.Stack.Pop(), message = (string)function.Stack.Pop();
                        throw new ArgumentException(message, parameter);
                    }

                    else if (name == "$System.ArgumentNullException" && parameters.Count() == 0)
                        throw new ArgumentNullException();
                    else if (name == "$System.ArgumentNullException" && parameters.Count() == 1)
                        throw new ArgumentNullException((string)function.Stack.Pop());
                    else if (name == "$System.ArgumentNullException" && parameters.Count() == 2)
                    {
                        string message = (string)function.Stack.Pop(), parameter = (string)function.Stack.Pop();
                        throw new ArgumentNullException(parameter, message);
                    }
                }

                else if (operationCode == OperationCode.Pop)
                    function.Stack.Pop();
                else if (operationCode == OperationCode.Duplicate)
                {
                    var value = function.Stack.Pop();
                    function.Stack.Push(value); function.Stack.Push(value);
                }

                else if (operationCode == OperationCode.LoadBoolean)
                {
                    function.Stack.Push(function.Buffer.ReadBoolean(position));
                    position++;
                }
                else if (operationCode == OperationCode.LoadInteger)
                {
                    function.Stack.Push(function.Buffer.ReadInteger(position));
                    position += 8;
                }
                else if (operationCode == OperationCode.LoadFloat)
                {
                    function.Stack.Push(function.Buffer.ReadFloat(position));
                    position += 8;
                }
                else if (operationCode == OperationCode.LoadString)
                {
                    var value = function.Buffer.ReadString(position);
                    function.Stack.Push(value);
                    position += (ushort)(1 + value.Count());
                }

                else if (operationCode == OperationCode.LoadParameter)
                {
                    function.Stack.Push(function.Parameters.GetValue(function.Buffer.ReadByte(position)));
                    position++;
                }
                else if (operationCode == OperationCode.LoadConstant)
                {
                    function.Stack.Push(function.Constants.GetValue(function.Buffer.ReadByte(position)));
                    position++;
                }
                else if (operationCode == OperationCode.LoadLocal)
                {
                    function.Stack.Push(function.Locals.GetValue(function.Buffer.ReadByte(position)));
                    position++;
                }

                else if (operationCode == OperationCode.SetParameter)
                {
                    function.Parameters.Change(function.Buffer.ReadByte(position), function.Stack.Pop());
                    position++;
                }
                else if (operationCode == OperationCode.SetLocal)
                {
                    function.Locals.Change(function.Buffer.ReadByte(position), function.Stack.Pop());
                    position++;
                }

                else if (operationCode == OperationCode.IncrementParameter)
                {
                    var index = function.Buffer.ReadByte(position);
                    var old = function.Parameters.GetValue(index);

                    if (old is long @int)
                        function.Parameters.Change(index, @int + 1);
                    else if (old is double @float)
                        function.Parameters.Change(index, @float + 1);

                    position++;
                }
                else if (operationCode == OperationCode.IncrementLocal)
                {
                    var index = function.Buffer.ReadByte(position);
                    var old = function.Locals.GetValue(index);

                    if (old is long @int)
                        function.Locals.Change(index, @int + 1);
                    else if (old is double @float)
                        function.Locals.Change(index, @float + 1);

                    position++;
                }

                else if (operationCode == OperationCode.DecrementParameter)
                {
                    var index = function.Buffer.ReadByte(position);
                    var old = function.Parameters.GetValue(index);

                    if (old is long @int)
                        function.Parameters.Change(index, @int - 1);
                    else if (old is double @float)
                        function.Parameters.Change(index, @float - 1);

                    position++;
                }
                else if (operationCode == OperationCode.DecrementLocal)
                {
                    var index = function.Buffer.ReadByte(position);
                    var old = function.Locals.GetValue(index);

                    if (old is long @int)
                        function.Locals.Change(index, @int - 1);
                    else if (old is double @float)
                        function.Locals.Change(index, @float - 1);

                    position++;
                }

                else if (operationCode == OperationCode.Negate)
                {
                    var value = function.Stack.Pop();

                    if (value is long @int)
                        value = -@int;
                    else if (value is double @float)
                        value = -@float;

                    function.Stack.Push(value);
                }

                else if (operationCode == OperationCode.Add)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Arithmetic(OperationCode.Add, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.Subtract)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Arithmetic(OperationCode.Subtract, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.Multiply)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Arithmetic(OperationCode.Multiply, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.Divide)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Arithmetic(OperationCode.Divide, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.Modulate)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Arithmetic(OperationCode.Modulate, valueOne, valueTwo));
                }

                else if (operationCode == OperationCode.CheckEquals)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Conditional(OperationCode.CheckEquals, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.CheckGreater)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Conditional(OperationCode.CheckGreater, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.CheckLess)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Conditional(OperationCode.CheckLess, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.CheckGreaterEquals)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Conditional(OperationCode.CheckGreaterEquals, valueOne, valueTwo));
                }
                else if (operationCode == OperationCode.CheckLessEquals)
                {
                    object valueTwo = function.Stack.Pop(), valueOne = function.Stack.Pop();
                    function.Stack.Push(Conditional(OperationCode.CheckLessEquals, valueOne, valueTwo));
                }
            }
        }

        public void Run() =>
            RunFunction(GetFunction("main", Datatype.Void, new List<Datatype>()));
    }
}