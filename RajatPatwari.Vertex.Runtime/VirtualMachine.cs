using System;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
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

    public interface IValue
    {
        public Datatype Datatype { get; }
    }

    public readonly struct ScalarValue<T> : IValue
        where T : struct
    {
        public Datatype Datatype { get; }

        public T Value { get; }

        internal ScalarValue(in T value)
        {
            Value = value;
            Datatype = value switch
            {
                bool _ => Datatype.Boolean,
                sbyte _ => Datatype.TinySigned,
                byte _ => Datatype.TinyUnsigned,
                short _ => Datatype.ShortSigned,
                ushort _ => Datatype.ShortUnsigned,
                int _ => Datatype.MediumSigned,
                uint _ => Datatype.MediumUnsigned,
                long _ => Datatype.LongSigned,
                ulong _ => Datatype.LongUnsigned,
                float _ => Datatype.FloatSingle,
                double _ => Datatype.FloatDouble,
                char _ => Datatype.Character,
                _ => throw new ArgumentException(nameof(value))
            };
        }
    }

    public sealed class StringValue : IValue
    {
        public Datatype Datatype { get; } = Datatype.String;

        public string Value { get; } = string.Empty;

        internal StringValue(string value) =>
            Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static class ValueFactory
    {
        public static ScalarValue<T> CreateScalarValue<T>(in T value)
            where T : struct =>
            new ScalarValue<T>(value);

        public static StringValue CreateStringValue(string value) =>
            new StringValue(value) ?? throw new ArgumentNullException(nameof(value));
    }

    public enum OperationCode : byte
    {
        NoOperation,

        JumpAlways,
        JumpTrue,
        JumpFalse,

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

    public readonly struct Label
    {
        public string Name { get; }

        public int Position { get; }

        public Label(string name, int position)
        {
            Name = name;
            Position = position;
        }
    }
}