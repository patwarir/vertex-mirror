using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    #region Operation Codes and Datatypes

    public enum OperationCode : byte
    {
        NoOperation,

        JumpAlways,
        JumpTrue,
        JumpFalse,

        Call,
        Return,

        Cast,
        CheckType,

        Pop,
        Clear,
        Duplicate,
        Rotate,

        LoadLiteral,
        LoadParameter,
        LoadConstant,
        LoadLocal,

        SetParameter,
        SetLocal
    }

    public enum Datatype : byte
    {
        Void,

        Boolean,

        TinySigned,
        TinyUnsigned,

        ShortSigned,
        ShortUnsigned,

        MediumSigned,
        MediumUnsigned,

        LongSigned,
        LongUnsigned,

        FloatSingle,
        FloatDouble,

        Character,
        String
    }

    #endregion

    #region Scalars

    public readonly struct Scalar
    {
        public Datatype Datatype { get; }

        public object Value { get; }

        public Scalar(Datatype datatype, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Datatype = datatype;
            Value = datatype switch
            {
                Datatype.Boolean => (bool)value,

                Datatype.TinySigned => (sbyte)value,
                Datatype.TinyUnsigned => (byte)value,

                Datatype.ShortSigned => (short)value,
                Datatype.ShortUnsigned => (ushort)value,

                Datatype.MediumSigned => (int)value,
                Datatype.MediumUnsigned => (uint)value,

                Datatype.LongSigned => (long)value,
                Datatype.LongUnsigned => (ulong)value,

                Datatype.FloatSingle => (float)value,
                Datatype.FloatDouble => (double)value,

                Datatype.Character => (char)value,
                Datatype.String => (string)value,

                _ => throw new ArgumentException(nameof(datatype))
            };
        }
    }

    public sealed class ScalarList : IEnumerable<Scalar>
    {
        private readonly IList<Scalar> _scalars = new List<Scalar>();

        public bool Constant { get; }

        public ScalarList(bool constant = false) =>
            Constant = constant;

        public void Add(in Scalar value) =>
            _scalars.Add(value);

        public void Update(byte index, in Scalar value)
        {
            if (Constant)
                throw new InvalidOperationException(nameof(Constant));

            _scalars[index] = value;
        }

        public Scalar Get(byte index) =>
            _scalars[index];

        public IEnumerable<Datatype> GetDatatypes() =>
            _scalars.Select(scalar => scalar.Datatype);

        public IEnumerator<Scalar> GetEnumerator() =>
            _scalars.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _scalars.GetEnumerator();
    }

    #endregion

    #region Labels and Buffer

    public readonly struct Label
    {
        public string Name { get; }

        public ushort Position { get; }

        public Label(string name, ushort position)
        {
            Name = name;
            Position = position;
        }
    }

    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _stream = new List<byte>();

        public ushort Length =>
            (ushort)_stream.Count;

        private byte Read(ushort position) =>
            _stream[position];

        public OperationCode ReadOperationCode(ushort position) =>
            (OperationCode)Read(position);

        public Datatype ReadDatatype(ushort position) =>
            (Datatype)Read(position);

        public IEnumerable<Datatype> ReadDatatypes(ushort position)
        {
            var length = Read(position++);

            var datatypes = new List<Datatype>();
            for (var index = position; index < position + length; index++)
                datatypes.Add(ReadDatatype(index));
            return datatypes;
        }

        public byte ReadIndex(ushort position) =>
            Read(position);

        public char ReadIdentifierCharacter(ushort position) =>
            (char)Read(position);

        public string ReadIdentifier(ushort position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length; index++)
                builder.Append(ReadIdentifierCharacter(index));
            return builder.ToString();
        }

        #region Literal Reads

        public bool ReadBoolean(ushort position) =>
            BitConverter.ToBoolean(_stream.ToArray(), position);

        public sbyte ReadTinySigned(ushort position) =>
            _stream[position + 1] == 0 ? (sbyte)_stream[position] : (sbyte)-(256 - _stream[position]);

        public byte ReadTinyUnsigned(ushort position) =>
            Read(position);

        public short ReadShortSigned(ushort position) =>
            BitConverter.ToInt16(_stream.ToArray(), position);

        public ushort ReadShortUnsigned(ushort position) =>
            BitConverter.ToUInt16(_stream.ToArray(), position);

        public int ReadMediumSigned(ushort position) =>
            BitConverter.ToInt32(_stream.ToArray(), position);

        public uint ReadMediumUnsigned(ushort position) =>
            BitConverter.ToUInt32(_stream.ToArray(), position);

        public long ReadLongSigned(ushort position) =>
            BitConverter.ToInt64(_stream.ToArray(), position);

        public ulong ReadLongUnsigned(ushort position) =>
            BitConverter.ToUInt64(_stream.ToArray(), position);

        public float ReadFloatSingle(ushort position) =>
            BitConverter.ToSingle(_stream.ToArray(), position);

        public double ReadFloatDouble(ushort position) =>
            BitConverter.ToDouble(_stream.ToArray(), position);

        public char ReadCharacter(ushort position) =>
            BitConverter.ToChar(_stream.ToArray(), position);

        public string ReadString(ushort position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length * 2; index += 2)
                builder.Append(ReadCharacter(index));
            return builder.ToString();
        }

        #endregion

        private void Write(byte value) =>
            _stream.Add(value);

        public void WriteOperationCode(OperationCode value) =>
            Write((byte)value);

        public void WriteDatatype(Datatype value) =>
            Write((byte)value);

        public void WriteDatatypes(in IEnumerable<Datatype> value)
        {
            var datatypes = value.ToList();
            Write((byte)datatypes.Count);
            datatypes.ForEach(WriteDatatype);
        }

        public void WriteIndex(byte value) =>
            Write(value);

        public void WriteIdentifierCharacter(char value) =>
            Write((byte)value);

        public void WriteIdentifier(string value)
        {
            Write((byte)value.Length);
            value.ToList().ForEach(WriteIdentifierCharacter);
        }

        #region Literal Writes

        public void WriteBoolean(bool value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteTinySigned(sbyte value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteTinyUnsigned(byte value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteShortSigned(short value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteShortUnsigned(ushort value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteMediumSigned(int value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteMediumUnsigned(uint value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteLongSigned(long value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteLongUnsigned(ulong value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteFloatSingle(float value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteFloatDouble(double value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteCharacter(char value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteString(string value)
        {
            Write((byte)value.Length);
            value.ToList().ForEach(WriteCharacter);
        }

        #endregion

        public IEnumerator<byte> GetEnumerator() =>
            _stream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _stream.GetEnumerator();
    }

    #endregion

    #region Functions

    public sealed class Function
    {
        public string Name { get; }

        public Datatype Return { get; }

        public ScalarList Parameters { get; } = new ScalarList();

        public ScalarList Constants { get; } = new ScalarList(true);

        public ScalarList Locals { get; } = new ScalarList();

        public Buffer Buffer { get; } = new Buffer();

        public Stack<Scalar> Stack { get; } = new Stack<Scalar>();

        public IList<Label> Labels { get; } = new List<Label>();

        public Function(string name, Datatype @return)
        {
            Name = name;
            Return = @return;
        }
    }

    public sealed class Package : IEnumerable<Function>
    {
        private readonly IList<Function> _functions = new List<Function>();

        public string Name { get; }

        public Package(string name) =>
            Name = name;

        public void Add(in Function function) =>
            _functions.Add(function);

        public Function FindBySignature(string name, Datatype @return, IList<Datatype> parameters) =>
            _functions.First(function => function.Name == name && function.Return == @return
                && function.Parameters.GetDatatypes().SequenceEqual(parameters));

        public IEnumerator<Function> GetEnumerator() =>
            _functions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _functions.GetEnumerator();
    }

    #endregion
}