using System;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public readonly struct Scalar : IEquatable<Scalar>
    {
        public Datatype Datatype { get; }

        public object Value { get; }

        public Scalar(Datatype datatype, object value)
        {
            Datatype = datatype;

            Value = null;
            switch (Datatype)
            {
                case Datatype.Boolean:
                    Value = (bool)value;
                    break;
                case Datatype.Integer:
                    Value = (long)value;
                    break;
                case Datatype.Float:
                    Value = (double)value;
                    break;
                case Datatype.String:
                    Value = (string)value;
                    break;
            }

            if (Value == null)
                throw new ArgumentException($"{nameof(value)} is invalid for {nameof(datatype)} {datatype}!");
        }

        public bool Equals(Scalar scalar)
        {
            if (Datatype != scalar.Datatype)
                return false;

            switch (Datatype)
            {
                case Datatype.Boolean:
                    return (bool)Value == (bool)scalar.Value;
                case Datatype.Integer:
                    return (long)Value == (long)scalar.Value;
                case Datatype.Float:
                    return (double)Value == (double)scalar.Value;
                case Datatype.String:
                    return (string)Value == (string)scalar.Value;
            }

            throw new InvalidOperationException("Cannot check equality!");
        }
    }
}