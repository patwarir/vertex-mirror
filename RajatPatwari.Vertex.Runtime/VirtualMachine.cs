using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public enum Datatype : byte
    {
        Void,
        Boolean,
        Integer,
        Float,
        String
    }

    public enum OperationCode : byte
    {
        NoOperation,

        JumpAlways,
        JumpTrue,

        Call,
        Return,

        Pop,
        Clear,
        Duplicate,
        Rotate,

        LoadLiteral,

        LoadParameter,
        LoadLocal,

        SetParameter,
        SetLocal
    }

    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _stream = new List<byte>();

        public int Length =>
            _stream.Count;

        private byte Read(int position) =>
            _stream[position];

        public Datatype ReadDatatype(int position) =>
            (Datatype)Read(position);

        public IEnumerable<Datatype> ReadDatatypes(int position)
        {
            var length = Read(position++);

            var datatypes = new List<Datatype>();
            for (var index = position; index < position + length; index++)
                datatypes.Add(ReadDatatype(index));
            return datatypes;
        }

        public OperationCode ReadOperationCode(int position) =>
            (OperationCode)Read(position);

        public byte ReadIndex(int position) =>
            Read(position);

        public string ReadIdentifier(int position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length; index++)
                builder.Append((char)Read(index));
            return builder.ToString();
        }

        public FunctionDeclaration ReadDeclaration(int position)
        {
            var qualifiedName = ReadIdentifier(position);
            position += 1 + qualifiedName.Length;

            var parameters = ReadDatatypes(position);
            position += 1 + parameters.Count();

            var @return = ReadDatatype(position++);
            return new FunctionDeclaration(qualifiedName, @return, parameters);
        }

        #region Literal Reads

        public bool ReadBoolean(int position) =>
            BitConverter.ToBoolean(_stream.ToArray(), position);

        public long ReadInteger(int position) =>
            BitConverter.ToInt64(_stream.ToArray(), position);

        public double ReadFloat(int position) =>
            BitConverter.ToDouble(_stream.ToArray(), position);

        public string ReadString(int position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length * 2; index += 2)
                builder.Append(BitConverter.ToChar(_stream.ToArray(), index));
            return builder.ToString();
        }

        #endregion

        private void Write(byte value) =>
            _stream.Add(value);

        public void WriteDatatype(Datatype value) =>
            Write((byte)value);

        public void WriteDatatypes(IEnumerable<Datatype> value)
        {
            var datatypes = value.ToList();
            Write((byte)datatypes.Count);
            datatypes.ForEach(WriteDatatype);
        }

        public void WriteOperationCode(OperationCode value) =>
            Write((byte)value);

        public void WriteIndex(byte value) =>
            Write(value);

        public void WriteIdentifier(string value)
        {
            Write((byte)value.Length);
            value.ToList().ForEach(@char => Write((byte)@char));
        }

        public void WriteDeclaration(FunctionDeclaration declaration)
        {
            WriteIdentifier(declaration.QualifiedName);
            WriteDatatypes(declaration.Parameters);
            WriteDatatype(declaration.Return);
        }

        #region Literal Writes

        public void WriteBoolean(bool value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteInteger(long value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteFloat(double value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteString(string value)
        {
            Write((byte)value.Length);
            value.ToList().ForEach(@char => BitConverter.GetBytes(@char).ToList().ForEach(Write));
        }

        #endregion

        public IEnumerator<byte> GetEnumerator() =>
            _stream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _stream.GetEnumerator();
    }

    public readonly struct FunctionDeclaration : IEquatable<FunctionDeclaration>
    {
        public string QualifiedName { get; }

        public Datatype Return { get; }

        public IEnumerable<Datatype> Parameters { get; }

        public FunctionDeclaration(string qualifiedName, Datatype @return, IEnumerable<Datatype> parameters)
        {
            QualifiedName = qualifiedName;
            Return = @return;
            Parameters = parameters;
        }

        public bool Equals(FunctionDeclaration declaration) =>
            QualifiedName == declaration.QualifiedName && Return == declaration.Return
                && Parameters.SequenceEqual(declaration.Parameters);

        public override bool Equals(object? obj) =>
            obj is FunctionDeclaration declaration && Equals(declaration);

        public override int GetHashCode() =>
            base.GetHashCode();
    }

    public sealed class FunctionImplementation
    {
        #region Non-Volatile Data

        public FunctionDeclaration Declaration { get; }

        public Buffer Buffer { get; } = new Buffer();

        public IList<(string name, int position)> Labels { get; } = new List<(string name, int position)>();

        #endregion

        #region Volatile Data

        public (Datatype datatype, object value) ReturnValue { get; set; }

        public IList<(Datatype datatype, object value)> ParameterValues { get; private set; } = new List<(Datatype datatype, object value)>();

        public IList<(Datatype datatype, object value)> Locals { get; private set; } = new List<(Datatype datatype, object value)>();

        public Stack<(Datatype datatype, object value)> Stack { get; private set; } = new Stack<(Datatype datatype, object value)>();

        #endregion

        public FunctionImplementation(string qualifedName, Datatype @return, params Datatype[] parameterTypes) =>
            Declaration = new FunctionDeclaration(qualifedName, @return, parameterTypes);

        public void Reset()
        {
            ReturnValue = default;
            ParameterValues = new List<(Datatype datatype, object value)>();
            Locals = new List<(Datatype datatype, object value)>();
            Stack = new Stack<(Datatype datatype, object value)>();
        }

        public int GetLabelPosition(string name) =>
            Labels.Single(label => label.name == name).position;
    }
}