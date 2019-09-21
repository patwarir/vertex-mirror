using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public void Add(Scalar value) =>
            _scalars.Add(value);

        public void Update(byte index, Scalar value)
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

        // TODO: Finish the Reads.

        private void Write(byte value) =>
            _stream.Add(value);

        // TODO: Finish the Writes.

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

        public void Add(Function function) =>
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