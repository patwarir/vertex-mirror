using RajatPatwari.Vertex.Runtime.AbstractSyntaxTree;
using RajatPatwari.Vertex.Runtime.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.Representation
{
    public enum OperationCode : byte
    {
        Undefined,

        Call,
        Return,

        Throw,

        LoadBoolean,
        LoadInteger,
        LoadFloat,
        LoadString,
        LoadArgument,
        LoadLocal,
        
        SetArgument,
        SetLocal,

        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus,

        Equal,
        Else,
        EndIf
    }

    public sealed class Buffer
    {
        public IList<byte> Stream { get; } = new List<byte>();

        public byte ReadByte(int position) =>
            Stream[position];

        public OperationCode ReadOperationCode(int position) =>
            (OperationCode)ReadByte(position);

        public bool ReadBoolean(int position) =>
            BitConverter.ToBoolean(Stream.ToArray(), position);

        public long ReadInteger(int position) =>
            BitConverter.ToInt64(Stream.ToArray(), position);

        public double ReadFloat(int position) =>
            BitConverter.ToDouble(Stream.ToArray(), position);

        public string ReadString(int position)
        {
            var length = ReadByte(position++);

            var returnString = new StringBuilder();
            for (var index = position; index < position + length; index++)
                returnString.Append((char)ReadByte(index));
            return returnString.ToString();
        }

        public Datatype ReadOperationType(int position) =>
            (Datatype)ReadByte(position);

        public IList<Datatype> ReadOperationTypes(int position)
        {
            var length = ReadByte(position++);

            var operationTypes = new List<Datatype>();
            for (var index = position; index < position + length; index++)
                operationTypes.Add(ReadOperationType(index));
            return operationTypes;
        }

        public void WriteByte(byte value) =>
            Stream.Add(value);

        public void WriteOperationCode(OperationCode value) =>
            WriteByte((byte)value);

        public void WriteBoolean(bool value) =>
            BitConverter.GetBytes(value).ToList().ForEach(WriteByte);

        public void WriteInteger(long value) =>
            BitConverter.GetBytes(value).ToList().ForEach(WriteByte);

        public void WriteFloat(double value) =>
            BitConverter.GetBytes(value).ToList().ForEach(WriteByte);

        public void WriteString(string value)
        {
            WriteByte((byte)value.Length);
            value.ToList().ForEach(character => WriteByte((byte)character));
        }

        public void WriteOperationType(Datatype value) =>
            WriteByte((byte)value);

        public void WriteOperationTypes(IList<Datatype> value)
        {
            WriteByte((byte)value.Count);
            value.ToList().ForEach(WriteOperationType);
        }
    }

    public readonly struct Variable
    {
        public int Index { get; }

        public Datatype Datatype { get; }

        public object Value { get; }

        public Variable(int index, Datatype datatype, object value)
        {
            Index = index;
            Datatype = datatype;

            Value = null;

            if (Datatype == Datatype.Boolean)
                Value = bool.Parse(value.ToString());
            else if (Datatype == Datatype.Integer)
                Value = long.Parse(value.ToString());
            else if (Datatype == Datatype.Float)
                Value = double.Parse(value.ToString());
            else if (Datatype == Datatype.String)
                Value = value.ToString();

            if (Value == null)
                throw new InvalidOperationException($"Invalid {nameof(Variable)}!");
        }

        public override string ToString() =>
            $"{Index}|{Datatype}|{Value}";
    }

    public sealed class VariableList
    {
        public IList<Variable> Variables { get; } = new List<Variable>();

        public void AddVariable(int index, Datatype datatype, object value) =>
            Variables.Add(new Variable(index, datatype, value));

        public bool HasVariableWithIndex(int index)
        {
            foreach (var variable in Variables)
                if (variable.Index == index)
                    return true;
            return false;
        }

        public void ChangeVariable(int index, object newValue)
        {
            if (!HasVariableWithIndex(index))
                throw new InvalidOperationException($"No {nameof(Variable)} with {nameof(index)} {index}!");

            for (var varIndex = 0; varIndex < Variables.Count; varIndex++)
                if (Variables[varIndex].Index == index)
                    Variables[varIndex] = new Variable(index, Variables[varIndex].Datatype, newValue);
        }

        public object GetValueOfVariableWithIndex(int index) =>
            Variables[index].Value;
    }

    public sealed class ConditionalRepresentation
    {
        public Conditional Conditional { get; } = new Conditional();

        public int BufferPosition { get; } = -1;

        public ConditionalRepresentation(Conditional conditional, int bufferPosition)
        {
            Conditional = conditional;
            BufferPosition = bufferPosition;
        }

        public override string ToString() =>
            $"{BufferPosition}|{Conditional}";
    }

    public sealed class FunctionRepresentation
    {
        public Function Function { get; set; } = new Function();

        public Buffer Buffer { get; set; } = new Buffer();

        public VariableList ArgumentList { get; set; } = new VariableList();

        public VariableList LocalList { get; set; } = new VariableList();

        public IList<ConditionalRepresentation> Conditionals { get; set; } = new List<ConditionalRepresentation>();

        public Stack<object> Stack { get; set; } = new Stack<object>();

        public override string ToString() =>
            $"{Buffer.Stream.Count}|{Function}";
    }
}