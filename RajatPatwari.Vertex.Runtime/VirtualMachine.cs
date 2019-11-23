using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public sealed class Scalar
    {
        private object _value;

        private bool _isDefined;

        public object Value
        {
            get
            {
                if (!IsDefined)
                    throw new InvalidOperationException($"!{nameof(IsDefined)}");
                return _value;
            }
        }

        public Datatype Datatype { get; }

        public bool IsDefined =>
            _isDefined && _value != null;

        private Scalar(Datatype datatype)
        {
            _value = null;
            Datatype = datatype;
            _isDefined = false;
        }

        private Scalar(object value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            Datatype = value switch
            {
                bool _ => Datatype.Boolean,
                long _ => Datatype.Integer,
                double _ => Datatype.Float,
                string _ => Datatype.String,
                _ => throw new ArgumentException(nameof(value))
            };
            _isDefined = true;
        }

        public void DefineValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (IsDefined)
                throw new InvalidOperationException(nameof(IsDefined));

            _value = Datatype switch
            {
                Datatype.Boolean => (bool)value,
                Datatype.Integer => (long)value,
                Datatype.Float => (double)value,
                Datatype.String => (string)value,
                _ => throw new InvalidOperationException(nameof(Datatype))
            };
            _isDefined = true;
        }

        public void Undefine()
        {
            if (!IsDefined)
                throw new InvalidOperationException($"!{nameof(IsDefined)}");

            _value = null;
            _isDefined = false;
        }

        public override string ToString() =>
            $"T:{Datatype}|D:{IsDefined}|V:{_value ?? "NULL"}";

        public static implicit operator Scalar(bool value) =>
            new Scalar(value);

        public static implicit operator Scalar(long value) =>
            new Scalar(value);

        public static implicit operator Scalar(double value) =>
            new Scalar(value);

        public static implicit operator Scalar(string value) =>
            new Scalar(value ?? throw new ArgumentNullException(nameof(value)));

        public static implicit operator Scalar(Datatype value) =>
            new Scalar(value);
    }

    public sealed class ScalarCollection : IEnumerable<Scalar>
    {
        private readonly IList<Scalar> _scalars = new List<Scalar>();

        public bool IsConstant { get; }

        public int Count =>
            _scalars.Count;

        public ScalarCollection(bool isConstant) =>
            IsConstant = isConstant;

        public Scalar Get(int index) =>
            _scalars[index];

        public void Append(Scalar value) =>
            _scalars.Add(value ?? throw new ArgumentNullException(nameof(value)));

        public void Prepend(Scalar value) =>
            _scalars.Insert(0, value ?? throw new ArgumentNullException(nameof(value)));

        public void Update(int index, Scalar value)
        {
            if (IsConstant)
                throw new InvalidOperationException(nameof(IsConstant));
            _scalars[index] = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void DefineAt(int index, object value)
        {
            if (IsConstant)
                throw new InvalidOperationException(nameof(IsConstant));
            _scalars[index].DefineValue(value ?? throw new ArgumentNullException(nameof(value)));
        }

        public void Undefine()
        {
            if (IsConstant)
                throw new InvalidOperationException(nameof(IsConstant));
            foreach (var scalar in _scalars)
                scalar.Undefine();
        }

        public IEnumerable<Datatype> GetDatatypes() =>
            _scalars.Select(scalar => scalar.Datatype);

        public IEnumerable<object> GetValues() =>
            _scalars.Select(scalar => scalar.Value);

        public override string ToString() =>
            $"Count = {Count}";

        public IEnumerator<Scalar> GetEnumerator() =>
            _scalars.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _scalars.GetEnumerator();
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

        LoadGlobal,
        LoadParameter,
        LoadConstant,
        LoadLocal,

        SetParameter,
        SetLocal
    }

    public sealed class Label
    {
        public string Name { get; }

        public int Position { get; }

        public Label(string name, int position)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Position = position;
        }

        public override string ToString() =>
            $"{Name} = {Position}";
    }

    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _buffer = new List<byte>();

        public int Count =>
            _buffer.Count;

        public byte Read(int position) =>
            _buffer[position];

        public void Write(byte value) =>
            _buffer.Add(value);

        // TODO: Add in all the reads and writes.

        public override string ToString() =>
            $"Count = {Count}";

        public IEnumerator<byte> GetEnumerator() =>
            _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _buffer.GetEnumerator();
    }

    public sealed class Function
    {
        private Scalar _return;

        private Buffer _buffer;

        private ScalarCollection _constants, _locals;

        private Stack<Scalar> _stack;

        private IList<Label> _labels;

        private Delegate _delegate;

        public static readonly IEnumerable<Datatype> NoParameters = Enumerable.Empty<Datatype>();

        public string Name { get; }

        internal bool IsRuntime { get; }

        public Scalar Return
        {
            get => _return;
            set => _return = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ScalarCollection Parameters { get; } = new ScalarCollection(false);

        public Buffer Buffer
        {
            get
            {
                if (IsRuntime || _delegate != null)
                    throw new InvalidOperationException(nameof(IsRuntime));
                return _buffer ?? throw new InvalidOperationException(nameof(Buffer));
            }
        }

        public ScalarCollection Constants
        {
            get
            {
                if (IsRuntime || _delegate != null)
                    throw new InvalidOperationException(nameof(IsRuntime));
                return _constants ?? throw new InvalidOperationException(nameof(Constants));
            }
        }

        public ScalarCollection Locals
        {
            get
            {
                if (IsRuntime || _delegate != null)
                    throw new InvalidOperationException(nameof(IsRuntime));
                return _locals ?? throw new InvalidOperationException(nameof(Locals));
            }
        }

        public Stack<Scalar> Stack
        {
            get
            {
                if (IsRuntime || _delegate != null)
                    throw new InvalidOperationException(nameof(IsRuntime));
                return _stack ?? throw new InvalidOperationException(nameof(Stack));
            }
        }

        public IList<Label> Labels
        {
            get
            {
                if (IsRuntime || _delegate != null)
                    throw new InvalidOperationException(nameof(IsRuntime));
                return _labels ?? throw new InvalidOperationException(nameof(Labels));
            }
        }

        internal Delegate Delegate
        {
            get
            {
                if (!IsRuntime || _buffer != null || _constants != null
                    || _locals != null || _stack != null || _labels != null)
                    throw new InvalidOperationException($"!{nameof(IsRuntime)}");
                return _delegate ?? throw new InvalidOperationException(nameof(Delegate));
            }
            set
            {
                if (!IsRuntime || _buffer != null || _constants != null
                    || _locals != null || _stack != null || _labels != null)
                    throw new InvalidOperationException($"!{nameof(IsRuntime)}");
                _delegate = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        internal Function(string name, bool isRuntime)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRuntime = isRuntime;
        }

        public Function(string name)
            : this(name, false) =>
            SetNewBuffer();

        private void SetNewBuffer()
        {
            if (IsRuntime || _delegate != null)
                throw new InvalidOperationException(nameof(IsRuntime));

            if (_constants == null)
                _constants = new ScalarCollection(true);
            if (_locals == null)
                _locals = new ScalarCollection(false);
            if (_stack == null)
                _stack = new Stack<Scalar>();
            if (_labels == null)
                _labels = new List<Label>();

            _buffer = new Buffer();
        }

        private void InnerRunRuntime(params object[] values)
        {
            if (!IsRuntime)
                throw new InvalidOperationException($"!{nameof(IsRuntime)}");
            if (_delegate == null)
                throw new InvalidOperationException(nameof(_delegate));
            if (Return.IsDefined)
                throw new InvalidOperationException($"{nameof(Return)}.{nameof(Return.IsDefined)}");
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Parameters.Count)
                throw new ArgumentException($"{nameof(values)}.{nameof(values.Length)}");

            for (var i = 0; i < Parameters.Count; i++)
                Parameters.DefineAt(i, values[i] ?? throw new ArgumentNullException(nameof(values)));

            var parameters = Parameters.GetValues().ToArray();
            if (Return.Datatype != Datatype.Void)
                Return.DefineValue(_delegate.DynamicInvoke(parameters));
            else
                _delegate.DynamicInvoke(parameters);
        }

        internal (bool returns, object value) RunRuntime(params object[] values)
        {
            InnerRunRuntime(values);
            Parameters.Undefine();

            if (!Return.IsDefined)
                return (false, null);
            else
            {
                var temp = Return.Value;
                Return.Undefine();
                return (true, temp);
            }
        }

        public override string ToString() =>
            $"{Name}({string.Join(',', Parameters.GetDatatypes().Select(datatype => datatype.ToString()))}) -> {Return.Datatype}";
    }

    public sealed class Package
    {
        public string Name { get; }

        internal bool IsRuntime { get; }

        public ScalarCollection Globals { get; }

        public IList<Function> Functions { get; } = new List<Function>();

        internal Package(string name, bool isRuntime)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRuntime = isRuntime;
            if (!isRuntime)
                Globals = new ScalarCollection(true);
        }

        public Package(string name)
            : this(name, false)
        { }

        internal Function FindBySignature(string name, bool isRuntime, Datatype @return, IEnumerable<Datatype> parameters)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            foreach (var function in Functions)
                if (function.Name == name && function.IsRuntime == isRuntime && function.Return.Datatype == @return
                    && function.Parameters.GetDatatypes().SequenceEqual(parameters))
                    return function;
            return null;
        }

        public Function FindBySignature(string name, Datatype @return, IEnumerable<Datatype> parameters) =>
            FindBySignature(name, false, @return, parameters);

        public static (string package, string function) SplitQualifiedName(string qualifiedName)
        {
            if (qualifiedName == null)
                throw new ArgumentNullException(nameof(qualifiedName));
            return (qualifiedName.Remove(qualifiedName.IndexOf(':')), qualifiedName.Substring(qualifiedName.LastIndexOf(':') + 1));
        }

        public override string ToString() =>
            $"{Name}|{Functions.Count}";
    }
}