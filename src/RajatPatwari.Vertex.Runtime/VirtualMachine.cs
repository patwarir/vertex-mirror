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
        Throw,
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

        StoreParameter,
        StoreLocal
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

    public static class DatatypeExtensions
    {
        public static bool IsTiny(this Datatype datatype) =>
            datatype == Datatype.TinySigned || datatype == Datatype.TinyUnsigned;

        public static bool IsShort(this Datatype datatype) =>
            datatype == Datatype.ShortSigned || datatype == Datatype.ShortUnsigned;

        public static bool IsMedium(this Datatype datatype) =>
            datatype == Datatype.MediumSigned || datatype == Datatype.MediumUnsigned;

        public static bool IsLong(this Datatype datatype) =>
            datatype == Datatype.LongSigned || datatype == Datatype.LongUnsigned;

        public static bool IsInteger(this Datatype datatype) =>
            datatype.IsTiny() || datatype.IsShort() || datatype.IsMedium() || datatype.IsLong();

        public static bool IsFloat(this Datatype datatype) =>
            datatype == Datatype.FloatSingle || datatype == Datatype.FloatDouble;

        public static bool IsNumeric(this Datatype datatype) =>
            datatype.IsInteger() || datatype.IsFloat();
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
            if (datatype == Datatype.Void)
                throw new ArgumentException(nameof(datatype));

            Datatype = datatype;

            Value = null;
            switch (datatype)
            {
                case Datatype.Boolean:
                    Value = (bool)value;
                    break;

                case Datatype.TinySigned:
                    Value = (sbyte)value;
                    break;
                case Datatype.TinyUnsigned:
                    Value = (byte)value;
                    break;

                case Datatype.ShortSigned:
                    Value = (short)value;
                    break;
                case Datatype.ShortUnsigned:
                    Value = (ushort)value;
                    break;

                case Datatype.MediumSigned:
                    Value = (int)value;
                    break;
                case Datatype.MediumUnsigned:
                    Value = (uint)value;
                    break;

                case Datatype.LongSigned:
                    Value = (long)value;
                    break;
                case Datatype.LongUnsigned:
                    Value = (ulong)value;
                    break;

                case Datatype.FloatSingle:
                    Value = (float)value;
                    break;
                case Datatype.FloatDouble:
                    Value = (double)value;
                    break;

                case Datatype.Character:
                    Value = (char)value;
                    break;
                case Datatype.String:
                    Value = (string)value;
                    break;
            }

            if (Value == null)
                throw new InvalidOperationException("Invalid Scalar!");
        }

        public static object Check(Datatype datatype, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (datatype == Datatype.Void)
                throw new ArgumentException(nameof(datatype));

            switch (datatype)
            {
                case Datatype.Boolean:
                    return (bool)value;

                case Datatype.TinySigned:
                    return (sbyte)value;
                case Datatype.TinyUnsigned:
                    return (byte)value;

                case Datatype.ShortSigned:
                    return (short)value;
                case Datatype.ShortUnsigned:
                    return (ushort)value;

                case Datatype.MediumSigned:
                    return (int)value;
                case Datatype.MediumUnsigned:
                    return (uint)value;

                case Datatype.LongSigned:
                    return (long)value;
                case Datatype.LongUnsigned:
                    return (ulong)value;

                case Datatype.FloatSingle:
                    return (float)value;
                case Datatype.FloatDouble:
                    return (double)value;

                case Datatype.Character:
                    return (char)value;
                case Datatype.String:
                    return (string)value;
            }

            throw new InvalidOperationException("Invalid Scalar!");
        }
    }

    public sealed class ScalarList : IEnumerable<Scalar>
    {
        private readonly IList<Scalar> _scalars = new List<Scalar>();

        public bool Constant { get; }

        public ScalarList(bool constant = false) =>
            Constant = constant;

        public void Add(Datatype datatype, object value) =>
            _scalars.Add(new Scalar(datatype, value));

        public void Update(byte index, object value)
        {
            if (Constant)
                throw new InvalidOperationException(nameof(Constant));

            _scalars[index] = new Scalar(_scalars[index].Datatype, value);
        }

        public object GetValue(byte index) =>
            _scalars[index].Value;

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
            ReadTinyUnsigned(position);

        public char ReadIdentifierCharacter(ushort position) =>
            (char)Read(position);

        public string ReadIdentifier(ushort position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length; index++)
                builder.Append(ReadIdentifierCharacter(position));
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

        public void WriteDatatypes(IEnumerable<Datatype> value)
        {
            var datatypes = value.ToList();
            Write((byte)datatypes.Count);
            datatypes.ForEach(WriteDatatype);
        }

        public void WriteIndex(byte value) =>
            WriteTinyUnsigned(value);

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

        public string Package { get; }

        public Datatype Return { get; }

        public ScalarList Parameters { get; } = new ScalarList();

        public ScalarList Constants { get; } = new ScalarList(true);

        public ScalarList Locals { get; } = new ScalarList();

        public Buffer Buffer { get; } = new Buffer();

        public Stack<object> Stack { get; } = new Stack<object>();

        public IList<Label> Labels { get; } = new List<Label>();

        public Function(string name, string package, Datatype @return)
        {
            Name = name;
            Package = package;
            Return = @return;
        }
    }

    #endregion
}