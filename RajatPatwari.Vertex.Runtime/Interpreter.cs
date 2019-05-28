using RajatPatwari.Vertex.Runtime.Representation;
using RajatPatwari.Vertex.Runtime.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Interpreter
    {
        private bool _ran = false;

        private readonly IList<FunctionRepresentation> _functionRepresentations;

        public Interpreter(IList<FunctionRepresentation> functionRepresentations) =>
            _functionRepresentations = functionRepresentations;

        private bool CheckTypes(IList<Datatype> types1, IList<Datatype> types2)
        {
            if (types1.Count != types2.Count)
                return false;

            for (var index = 0; index < types1.Count; index++)
            {
                if (types1[index] != types2[index])
                    return false;
            }

            return true;
        }

        private FunctionRepresentation GetFunction(string name, Datatype datatype, IList<Datatype> argumentTypes)
        {
            foreach (var function in _functionRepresentations)
                if (function.Function.Name.Value == name && function.Function.ReturnType == datatype && CheckTypes(function.Function.ArgumentTypes, argumentTypes))
                    return function;
            throw new InvalidOperationException($"No {nameof(FunctionRepresentation)} with {nameof(name)} {name}!");
        }

        private Type GetType(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (type.FullName == name)
                        return type;
            return null;
        }

        private Datatype GetDatatypeOfValue(object value)
        {
            if (value is bool)
                return Datatype.Boolean;
            else if (value is long)
                return Datatype.Integer;
            else if (value is double)
                return Datatype.Float;
            else if (value is string)
                return Datatype.String;

            throw new InvalidOperationException($"{nameof(value)} is invalid!");
        }

        private void CheckLocals(IList<Datatype> expectedLocalTypes, VariableList currentLocals)
        {
            foreach (var local in currentLocals.Variables)
            {
                if (local.Datatype != expectedLocalTypes[local.Index])
                    throw new InvalidOperationException($"Invalid {local} type!");
            }
        }

        private object Add(object value1, object value2)
        {
            if (value1 is long l1 && value2 is long l2)
                return l1 + l2;
            else if (value1 is double d1 && value2 is double d2)
                return d1 + d2;
            else if (value1 is string s1 && value2 is string s2)
                return s1 + s2;

            else if (value1 is long ld1 && value2 is double ld2)
                return ld1 + ld2;
            else if (value1 is double dl1 && value2 is long dl2)
                return dl1 + dl2;

            else if (value1 is string sb1 && value2 is bool sb2)
                return sb1 + sb2;
            else if (value1 is bool bs1 && value2 is string bs2)
                return bs1 + bs2;

            else if (value1 is string sl1 && value2 is long sl2)
                return sl1 + sl2;
            else if (value1 is long ls1 && value2 is string ls2)
                return ls1 + ls2;

            else if (value1 is string sd1 && value2 is double sd2)
                return sd1 + sd2;
            else if (value1 is double ds1 && value2 is string ds2)
                return ds1 + ds2;

            throw new InvalidOperationException($"Cannot {nameof(Add)}!");
        }

        private object Subtract(object value1, object value2)
        {
            if (value1 is long l1 && value2 is long l2)
                return l1 - l2;
            else if (value1 is double d1 && value2 is double d2)
                return d1 - d2;

            else if (value1 is long ld1 && value2 is double ld2)
                return ld1 - ld2;
            else if (value1 is double dl1 && value2 is long dl2)
                return dl1 - dl2;

            throw new InvalidOperationException($"Cannot {nameof(Subtract)}!");
        }

        private object Multiply(object value1, object value2)
        {
            if (value1 is long l1 && value2 is long l2)
                return l1 * l2;
            else if (value1 is double d1 && value2 is double d2)
                return d1 * d2;

            else if (value1 is long ld1 && value2 is double ld2)
                return ld1 * ld2;
            else if (value1 is double dl1 && value2 is long dl2)
                return dl1 * dl2;

            else if (value1 is string sl1 && value2 is long sl2)
            {
                var returnString = new StringBuilder();
                for (var index = 0; index < sl2; index++)
                    returnString.Append(sl1);
                return returnString.ToString();
            }
            else if (value1 is long ls1 && value2 is string ls2)
            {
                var returnString = new StringBuilder();
                for (var index = 0; index < ls1; index++)
                    returnString.Append(ls2);
                return returnString.ToString();
            }

            throw new InvalidOperationException($"Cannot {nameof(Multiply)}!");
        }

        private object Divide(object value1, object value2)
        {
            if (value1 is long l1 && value2 is long l2)
                return l1 / l2;
            else if (value1 is double d1 && value2 is double d2)
                return d1 / d2;

            else if (value1 is long ld1 && value2 is double ld2)
                return ld1 / ld2;
            else if (value1 is double dl1 && value2 is long dl2)
                return dl1 / dl2;

            throw new InvalidOperationException($"Cannot {nameof(Divide)}!");
        }

        private object Modulus(object value1, object value2)
        {
            if (value1 is long l1 && value2 is long l2)
                return l1 % l2;
            else if (value1 is double d1 && value2 is double d2)
                return d1 % d2;

            else if (value1 is long ld1 && value2 is double ld2)
                return ld1 % ld2;
            else if (value1 is double dl1 && value2 is long dl2)
                return dl1 % dl2;

            throw new InvalidOperationException($"Cannot {nameof(Modulus)}!");
        }

        private (int position, KeywordType type) GetPositionOfNextConditional(IList<ConditionalRepresentation> conditionals, int position)
        {
            int nextPosition = 0, range = int.MaxValue;
            var type = KeywordType.Undefined;
            foreach (var conditional in conditionals)
            {
                if (conditional.BufferPosition - position > 0 && conditional.BufferPosition - position < range)
                {
                    nextPosition = conditional.BufferPosition;
                    range = conditional.BufferPosition - position;
                    type = conditional.Conditional.Type;
                }
            }

            if (type == KeywordType.Undefined)
                throw new InvalidOperationException($"{nameof(conditionals)} is invalid!");

            return (nextPosition, type);
        }

        private int GetPositionOfEndConditional(IList<ConditionalRepresentation> conditionals, int position)
        {
            int nextPosition = 0, range = int.MaxValue;
            foreach (var conditional in conditionals)
            {
                if (conditional.BufferPosition - position > 0 && conditional.BufferPosition - position < range && conditional.Conditional.Type == KeywordType.EndIf)
                {
                    nextPosition = conditional.BufferPosition;
                    range = conditional.BufferPosition - position;
                }
            }
            return nextPosition;
        }

        private bool ObjectsEqual(object value1, object value2) =>
            EqualityComparer<object>.Default.Equals(value1, value2);

        /*private IList<FunctionRepresentation> GetNonMainFunctions()
        {
            var nonMainFunctions = new List<FunctionRepresentation>();
            foreach (var function in _functionRepresentations)
                if (function.Function.Name.Value.ToUpper() != "MAIN")
                    nonMainFunctions.Add(function);
            return nonMainFunctions;
        }*/

        private void RunConditional(int position, int end, FunctionRepresentation representation)
        {
            while (position < end)
            {
                var ifOpCode = representation.Buffer.ReadOperationCode(position++);

                if (ifOpCode == OperationCode.Call)
                {
                    var returnType = representation.Buffer.ReadOperationType(position++);

                    var name = representation.Buffer.ReadString(position);
                    position += 1 + name.Length;

                    var argumentTypeList = representation.Buffer.ReadOperationTypes(position);
                    position += 1 + argumentTypeList.Count;

                    if (name.StartsWith('$'))
                    {
                        if (name == "$System.Console.Write" && returnType == Datatype.Void)
                        {
                            switch (argumentTypeList[0])
                            {
                                case Datatype.Boolean:
                                    Console.Write((bool)representation.Stack.Pop());
                                    break;
                                case Datatype.Integer:
                                    Console.Write((long)representation.Stack.Pop());
                                    break;
                                case Datatype.Float:
                                    Console.Write((double)representation.Stack.Pop());
                                    break;
                                case Datatype.String:
                                    Console.Write((string)representation.Stack.Pop());
                                    break;
                            }
                        }
                        else if (name == "$System.Console.WriteLine" && returnType == Datatype.Void)
                        {
                            switch (argumentTypeList[0])
                            {
                                case Datatype.Boolean:
                                    Console.WriteLine((bool)representation.Stack.Pop());
                                    break;
                                case Datatype.Integer:
                                    Console.WriteLine((long)representation.Stack.Pop());
                                    break;
                                case Datatype.Float:
                                    Console.WriteLine((double)representation.Stack.Pop());
                                    break;
                                case Datatype.String:
                                    Console.WriteLine((string)representation.Stack.Pop());
                                    break;
                            }
                        }
                        else if (name == "$System.Console.Read" && returnType == Datatype.Void)
                            Console.Read();
                        else if (name == "$System.Console.ReadLine" && returnType == Datatype.String)
                            representation.Stack.Push(Console.ReadLine());

                        else
                            throw new InvalidOperationException($"Function with name of {name} was not found!");
                    }
                    else
                    {
                        var function = GetFunction(name, returnType, argumentTypeList);

                        var valueList = new List<object>();
                        for (var index = 0; index < argumentTypeList.Count; index++)
                            valueList.Add(representation.Stack.Pop());

                        for (var index = 0; index < argumentTypeList.Count; index++)
                        {
                            var argumentType = argumentTypeList[index];
                            object value = null;

                            switch (argumentType)
                            {
                                case Datatype.Boolean:
                                    value = (bool)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.Integer:
                                    value = (long)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.Float:
                                    value = (double)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.String:
                                    value = (string)valueList[valueList.Count - 1 - index];
                                    break;
                            }

                            function.ArgumentList.AddVariable(index, argumentType, value);
                        }

                        RunFunction(function);

                        if (function.Function.ReturnType != Datatype.Void)
                        {
                            object returnVal = null;
                            try
                            {
                                returnVal = function.Stack.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                switch (function.Function.ReturnType)
                                {
                                    case Datatype.Boolean:
                                        representation.Stack.Push((bool)returnVal);
                                        break;
                                    case Datatype.Integer:
                                        representation.Stack.Push((long)returnVal);
                                        break;
                                    case Datatype.Float:
                                        representation.Stack.Push((double)returnVal);
                                        break;
                                    case Datatype.String:
                                        representation.Stack.Push((string)returnVal);
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (ifOpCode == OperationCode.Return)
                    return;
                else if (ifOpCode == OperationCode.Throw)
                {
                    var name = representation.Buffer.ReadString(position);
                    position += 1 + name.Length;

                    var argumentTypeList = representation.Buffer.ReadOperationTypes(position);
                    position += 1 + argumentTypeList.Count;

                    if (name.StartsWith('$'))
                    {
                        var argumentList = new Stack<object>();
                        for (var index = 0; index < argumentTypeList.Count; index++)
                            argumentList.Push(representation.Stack.Pop());

                        if (name == "$System.Exception" && argumentTypeList.Count <= 1)
                        {
                            string message = null;
                            try
                            {
                                message = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message == null)
                                    throw new Exception();
                                else
                                    throw new Exception(message);
                            }
                        }
                        else if (name == "$System.InvalidOperationException" && argumentTypeList.Count <= 1)
                        {
                            string message = null;
                            try
                            {
                                message = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message == null)
                                    throw new InvalidOperationException();
                                else
                                    throw new InvalidOperationException(message);
                            }
                        }
                        else if (name == "$System.ArgumentException" && argumentTypeList.Count <= 2)
                        {
                            string message1 = null, message2 = null;
                            try
                            {
                                message1 = (string)argumentList.Pop();
                                message2 = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message1 == null && message2 == null)
                                    throw new ArgumentException();
                                else if (message1 != null && message2 == null)
                                    throw new ArgumentException(message1);
                                else if (message1 == null && message2 != null)
                                    throw new ArgumentException(message2);
                                else if (message1 != null && message2 != null)
                                    throw new ArgumentException(message1, message2);
                            }
                        }
                    }
                }

                else if (ifOpCode == OperationCode.LoadBoolean)
                {
                    representation.Stack.Push(representation.Buffer.ReadBoolean(position));
                    position++;
                }
                else if (ifOpCode == OperationCode.LoadInteger)
                {
                    representation.Stack.Push(representation.Buffer.ReadInteger(position));
                    position += 8;
                }
                else if (ifOpCode == OperationCode.LoadFloat)
                {
                    representation.Stack.Push(representation.Buffer.ReadFloat(position));
                    position += 8;
                }
                else if (ifOpCode == OperationCode.LoadString)
                {
                    var value = representation.Buffer.ReadString(position);
                    representation.Stack.Push(value);
                    position += 1 + value.Length;
                }
                else if (ifOpCode == OperationCode.LoadArgument)
                {
                    var index = representation.Buffer.ReadByte(position);
                    var value = representation.ArgumentList.GetValueOfVariableWithIndex(index);
                    representation.Stack.Push(value);
                    position++;
                }
                else if (ifOpCode == OperationCode.LoadLocal)
                {
                    var index = representation.Buffer.ReadByte(position);
                    var value = representation.LocalList.GetValueOfVariableWithIndex(index);
                    representation.Stack.Push(value);
                    position++;
                }

                else if (ifOpCode == OperationCode.SetArgument)
                {
                    var index = representation.Buffer.ReadByte(position);
                    representation.ArgumentList.ChangeVariable(index, representation.Stack.Pop());
                    position++;
                }
                else if (ifOpCode == OperationCode.SetLocal)
                {
                    var index = representation.Buffer.ReadByte(position);
                    if (representation.LocalList.HasVariableWithIndex(index))
                        representation.LocalList.ChangeVariable(index, representation.Stack.Pop());
                    else
                    {
                        var value = representation.Stack.Pop();
                        var datatype = GetDatatypeOfValue(value);
                        representation.LocalList.AddVariable(index, datatype, value);
                    }

                    CheckLocals(representation.Function.LocalTypes, representation.LocalList);
                    position++;
                }

                else if (ifOpCode == OperationCode.Add)
                {
                    object val2 = representation.Stack.Pop(), val1 = representation.Stack.Pop();
                    representation.Stack.Push(Add(val2, val1));
                }
                else if (ifOpCode == OperationCode.Subtract)
                {
                    object val2 = representation.Stack.Pop(), val1 = representation.Stack.Pop();
                    representation.Stack.Push(Subtract(val2, val1));
                }
                else if (ifOpCode == OperationCode.Multiply)
                {
                    object val2 = representation.Stack.Pop(), val1 = representation.Stack.Pop();
                    representation.Stack.Push(Multiply(val2, val1));
                }
                else if (ifOpCode == OperationCode.Divide)
                {
                    object val2 = representation.Stack.Pop(), val1 = representation.Stack.Pop();
                    representation.Stack.Push(Divide(val2, val1));
                }
                else if (ifOpCode == OperationCode.Modulus)
                {
                    object val2 = representation.Stack.Pop(), val1 = representation.Stack.Pop();
                    representation.Stack.Push(Modulus(val2, val1));
                }
            }
        }

        private void RunFunction(FunctionRepresentation representation)
        {
            var position = 0;
            // Using recursion, will return to the same place when called back.
            // var returnPositionStack = new Stack<int>();

            while (position < representation.Buffer.Stream.Count)
            {
                var operationCode = representation.Buffer.ReadOperationCode(position++);

                if (operationCode == OperationCode.Call)
                {
                    var returnType = representation.Buffer.ReadOperationType(position++);

                    var name = representation.Buffer.ReadString(position);
                    position += 1 + name.Length;

                    var argumentTypeList = representation.Buffer.ReadOperationTypes(position);
                    position += 1 + argumentTypeList.Count;

                    if (name.StartsWith('$'))
                    {
                        if (name == "$System.Console.Write" && returnType == Datatype.Void)
                        {
                            switch (argumentTypeList[0])
                            {
                                case Datatype.Boolean:
                                    Console.Write((bool)representation.Stack.Pop());
                                    break;
                                case Datatype.Integer:
                                    Console.Write((long)representation.Stack.Pop());
                                    break;
                                case Datatype.Float:
                                    Console.Write((double)representation.Stack.Pop());
                                    break;
                                case Datatype.String:
                                    Console.Write((string)representation.Stack.Pop());
                                    break;
                            }
                        }
                        else if (name == "$System.Console.WriteLine" && returnType == Datatype.Void)
                        {
                            switch (argumentTypeList[0])
                            {
                                case Datatype.Boolean:
                                    Console.WriteLine((bool)representation.Stack.Pop());
                                    break;
                                case Datatype.Integer:
                                    Console.WriteLine((long)representation.Stack.Pop());
                                    break;
                                case Datatype.Float:
                                    Console.WriteLine((double)representation.Stack.Pop());
                                    break;
                                case Datatype.String:
                                    Console.WriteLine((string)representation.Stack.Pop());
                                    break;
                            }
                        }
                        else if (name == "$System.Console.Read" && returnType == Datatype.Void)
                            Console.Read();
                        else if (name == "$System.Console.ReadLine" && returnType == Datatype.String)
                            representation.Stack.Push(Console.ReadLine());

                        else
                            throw new InvalidOperationException($"Function with name of {name} was not found!");
                    }
                    else
                    {
                        var function = GetFunction(name, returnType, argumentTypeList);

                        var valueList = new List<object>();
                        for (var index = 0; index < argumentTypeList.Count; index++)
                            valueList.Add(representation.Stack.Pop());

                        for (var index = 0; index < argumentTypeList.Count; index++)
                        {
                            var argumentType = argumentTypeList[index];
                            object value = null;

                            switch (argumentType)
                            {
                                case Datatype.Boolean:
                                    value = (bool)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.Integer:
                                    value = (long)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.Float:
                                    value = (double)valueList[valueList.Count - 1 - index];
                                    break;
                                case Datatype.String:
                                    value = (string)valueList[valueList.Count - 1 - index];
                                    break;
                            }

                            function.ArgumentList.AddVariable(index, argumentType, value);
                        }

                        RunFunction(function);

                        if (function.Function.ReturnType != Datatype.Void)
                        {
                            object returnVal = null;
                            try
                            {
                                returnVal = function.Stack.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                switch (function.Function.ReturnType)
                                {
                                    case Datatype.Boolean:
                                        representation.Stack.Push((bool)returnVal);
                                        break;
                                    case Datatype.Integer:
                                        representation.Stack.Push((long)returnVal);
                                        break;
                                    case Datatype.Float:
                                        representation.Stack.Push((double)returnVal);
                                        break;
                                    case Datatype.String:
                                        representation.Stack.Push((string)returnVal);
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (operationCode == OperationCode.Return)
                    return;
                else if (operationCode == OperationCode.Throw)
                {
                    var name = representation.Buffer.ReadString(position);
                    position += 1 + name.Length;

                    var argumentTypeList = representation.Buffer.ReadOperationTypes(position);
                    position += 1 + argumentTypeList.Count;

                    if (name.StartsWith('$'))
                    {
                        var argumentList = new Stack<object>();
                        for (var index = 0; index < argumentTypeList.Count; index++)
                            argumentList.Push(representation.Stack.Pop());

                        if (name == "$System.Exception" && argumentTypeList.Count <= 1)
                        {
                            string message = null;
                            try
                            {
                                message = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message == null)
                                    throw new Exception();
                                else
                                    throw new Exception(message);
                            }
                        }
                        else if (name == "$System.InvalidOperationException" && argumentTypeList.Count <= 1)
                        {
                            string message = null;
                            try
                            {
                                message = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message == null)
                                    throw new InvalidOperationException();
                                else
                                    throw new InvalidOperationException(message);
                            }
                        }
                        else if (name == "$System.ArgumentException" && argumentTypeList.Count <= 2)
                        {
                            string message1 = null, message2 = null;
                            try
                            {
                                message1 = (string)argumentList.Pop();
                                message2 = (string)argumentList.Pop();
                            }
                            catch
                            { }
                            finally
                            {
                                if (message1 == null && message2 == null)
                                    throw new ArgumentException();
                                else if (message1 != null && message2 == null)
                                    throw new ArgumentException(message1);
                                else if (message1 == null && message2 != null)
                                    throw new ArgumentException(message2);
                                else if (message1 != null && message2 != null)
                                    throw new ArgumentException(message1, message2);
                            }
                        }
                    }
                }

                else if (operationCode == OperationCode.LoadBoolean)
                {
                    representation.Stack.Push(representation.Buffer.ReadBoolean(position));
                    position++;
                }
                else if (operationCode == OperationCode.LoadInteger)
                {
                    representation.Stack.Push(representation.Buffer.ReadInteger(position));
                    position += 8;
                }
                else if (operationCode == OperationCode.LoadFloat)
                {
                    representation.Stack.Push(representation.Buffer.ReadFloat(position));
                    position += 8;
                }
                else if (operationCode == OperationCode.LoadString)
                {
                    var value = representation.Buffer.ReadString(position);
                    representation.Stack.Push(value);
                    position += 1 + value.Length;
                }
                else if (operationCode == OperationCode.LoadArgument)
                {
                    var index = representation.Buffer.ReadByte(position);
                    var value = representation.ArgumentList.GetValueOfVariableWithIndex(index);
                    representation.Stack.Push(value);
                    position++;
                }
                else if (operationCode == OperationCode.LoadLocal)
                {
                    var index = representation.Buffer.ReadByte(position);
                    var value = representation.LocalList.GetValueOfVariableWithIndex(index);
                    representation.Stack.Push(value);
                    position++;
                }

                else if (operationCode == OperationCode.SetArgument)
                {
                    var index = representation.Buffer.ReadByte(position);
                    representation.ArgumentList.ChangeVariable(index, representation.Stack.Pop());
                    position++;
                }
                else if (operationCode == OperationCode.SetLocal)
                {
                    var index = representation.Buffer.ReadByte(position);
                    if (representation.LocalList.HasVariableWithIndex(index))
                        representation.LocalList.ChangeVariable(index, representation.Stack.Pop());
                    else
                    {
                        var value = representation.Stack.Pop();
                        var datatype = GetDatatypeOfValue(value);
                        representation.LocalList.AddVariable(index, datatype, value);
                    }

                    CheckLocals(representation.Function.LocalTypes, representation.LocalList);
                    position++;
                }

                else if (operationCode == OperationCode.Add)
                {
                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    representation.Stack.Push(Add(value1, value2));
                }
                else if (operationCode == OperationCode.Subtract)
                {
                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    representation.Stack.Push(Subtract(value1, value2));
                }
                else if (operationCode == OperationCode.Multiply)
                {
                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    representation.Stack.Push(Multiply(value1, value2));
                }
                else if (operationCode == OperationCode.Divide)
                {
                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    representation.Stack.Push(Divide(value1, value2));
                }
                else if (operationCode == OperationCode.Modulus)
                {
                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    representation.Stack.Push(Modulus(value1, value2));
                }

                else if (operationCode == OperationCode.Equal)
                {
                    var next = GetPositionOfNextConditional(representation.Conditionals, position);

                    object value2 = representation.Stack.Pop(), value1 = representation.Stack.Pop();
                    var equal = ObjectsEqual(value1, value2);

                    if (equal)
                    {
                        if (next.type == KeywordType.Else)
                        {
                            RunConditional(position, next.position, representation);
                            var positionEnd = GetPositionOfEndConditional(representation.Conditionals, position);
                            position = 1 + positionEnd;
                        }
                        else if (next.type == KeywordType.EndIf)
                        {
                            RunConditional(position, next.position, representation);
                            position = 1 + next.position;
                        }
                    }
                    else
                    {
                        if (next.type == KeywordType.Else)
                        {
                            position = 1 + next.position;
                            var positionEnd = GetPositionOfEndConditional(representation.Conditionals, position);
                            RunConditional(position, positionEnd, representation);
                            position = 1 + positionEnd;
                        }
                        else if (next.type == KeywordType.EndIf)
                            position = 1 + next.position;
                    }
                }
            }
        }

        public void Run()
        {
            if (_ran)
                throw new InvalidOperationException($"Attempting to run the {nameof(Interpreter)} more than once!");
            _ran = true;

            RunFunction(GetFunction("main", Datatype.Void, new List<Datatype>()));
        }
    }
}