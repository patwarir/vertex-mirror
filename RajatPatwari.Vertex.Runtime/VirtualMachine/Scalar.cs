using System;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Scalar
    {
        private bool _isDefined;
        
        private object? _value;

        public bool IsDefined =>
            _isDefined && _value != null;

        public object Value =>
            IsDefined ? _value : throw new InvalidOperationException($"!{nameof(IsDefined)}");
        
        public Datatype Datatype { get; }

        private Scalar(Datatype datatype)
        {
            _isDefined = false;
            _value = null;
            Datatype = datatype;
        }

        private Scalar(object value)
        {
            _isDefined = true;
            _value = value ?? throw new ArgumentNullException(nameof(value));
            Datatype = value switch
            {
                bool _ => Datatype.Boolean,
                long _ => Datatype.Integer,
                double _ => Datatype.Float,
                string _ => Datatype.String,
                _ => throw new ArgumentException(nameof(value))
            };
        }

        public void DefineValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (IsDefined)
                throw new InvalidOperationException(nameof(IsDefined));

            _isDefined = true;
            _value = Datatype switch
            {
                Datatype.Boolean => (bool)value,
                Datatype.Integer => (long)value,
                Datatype.Float => (double)value,
                Datatype.String => (string)value,
                _ => throw new InvalidOperationException(nameof(Datatype))
            };
        }

        public void Undefine()
        {
            if (!IsDefined)
                throw new InvalidOperationException($"!{nameof(IsDefined)}");

            _isDefined = false;
            _value = null;
        }

        public override string ToString() =>
            $"T:{Datatype}|D:{IsDefined}|V:{_value ?? "[NULL]"}";
        
        public static explicit operator Scalar(Datatype datatype) =>
            new Scalar(datatype);

        public static implicit operator Scalar(bool value) =>
            new Scalar(value);

        public static implicit operator Scalar(long value) =>
            new Scalar(value);

        public static implicit operator Scalar(double value) =>
            new Scalar(value);

        public static implicit operator Scalar(string value) =>
            new Scalar(value ?? throw new ArgumentNullException(nameof(value)));
    }
}