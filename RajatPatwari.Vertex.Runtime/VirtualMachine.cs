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

    public sealed class Scalar
    {
        private bool _isDefined;

        private object? _value;

        public bool IsDefined =>
            _isDefined && _value != null;

        public object? Value =>
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
            _isDefined = false;
            _value = null;
        }

        public override string ToString() =>
            $"T:{Datatype}|D:{IsDefined}|V:{_value ?? "[NULL]"}";

        public static explicit operator Scalar(Datatype datatype) =>
            new Scalar(datatype);

        public static explicit operator Scalar(bool value) =>
            new Scalar(value);

        public static explicit operator Scalar(long value) =>
            new Scalar(value);

        public static explicit operator Scalar(double value) =>
            new Scalar(value);

        public static explicit operator Scalar(string value) =>
            new Scalar(value ?? throw new ArgumentNullException(nameof(value)));
    }

    public sealed class ScalarCollection : IEnumerable<Scalar>
    {
        private readonly IList<Scalar> _scalars = new List<Scalar>();

        public int Count =>
            _scalars.Count;

        public bool IsConstant { get; }

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

        public void UndefineAll()
        {
            if (IsConstant)
                throw new InvalidOperationException(nameof(IsConstant));

            foreach (var scalar in _scalars)
                scalar.Undefine();
        }

        public IEnumerable<Datatype> GetDatatypes() =>
            _scalars.Select(scalar => scalar.Datatype);

        public IEnumerable<object?> GetValues() =>
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
        JumpFalse,

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

    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _buffer = new List<byte>();

        public int Length =>
            _buffer.Count;

        public byte Read(int position) =>
            _buffer[position];

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

        public Function ReadFunction(int position)
        {
            var name = ReadString(position);
            position += 1 + name.Length;

            var parameters = ReadDatatypes(position);
            var datatypes = parameters.ToList();
            position += 1 + datatypes.Count;

            var @return = ReadDatatype(position++);

            var function = new Function(name, (Scalar)@return);
            foreach (var datatype in datatypes)
                function.Parameters.Append((Scalar)datatype);
            return function;
        }

        public bool ReadBoolean(int position) =>
            BitConverter.ToBoolean(_buffer.ToArray(), position);

        public long ReadInteger(int position) =>
            BitConverter.ToInt64(_buffer.ToArray(), position);

        public double ReadFloat(int position) =>
            BitConverter.ToDouble(_buffer.ToArray(), position);

        public string ReadString(int position)
        {
            var length = Read(position++);

            var builder = new StringBuilder();
            for (var index = position; index < position + length; index++)
                builder.Append((char)Read(index));
            return builder.ToString();
        }

        public void Write(byte value) =>
            _buffer.Add(value);

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

        public void WriteFunction(Function function)
        {
            WriteString(function.Name);
            WriteDatatypes(function.Parameters.GetDatatypes());
            WriteDatatype(function.Return.Datatype);
        }

        public void WriteFunction(string name, IEnumerable<Datatype> parameters, Datatype @return)
        {
            WriteString(name);
            WriteDatatypes(parameters);
            WriteDatatype(@return);
        }

        public void WriteBoolean(bool value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteInteger(long value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteFloat(double value) =>
            BitConverter.GetBytes(value).ToList().ForEach(Write);

        public void WriteString(string value)
        {
            Write((byte)value.Length);
            value.ToList().ForEach(character => Write((byte)character));
        }

        public override string ToString() =>
            $"Length = {Length}";

        public IEnumerator<byte> GetEnumerator() =>
            _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _buffer.GetEnumerator();
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

        public override string ToString() =>
            $"{Name} = {Position}";
    }

    public sealed class Function
    {
        private Scalar _return;

        private readonly Buffer? _buffer;

        private readonly ScalarCollection? _constants;

        private ScalarCollection? _locals;

        private Stack<Scalar>? _stack;

        private readonly IList<Label>? _labels;

        private Delegate? _delegate;

        public static readonly Scalar Void = (Scalar)Datatype.Void;

        public static readonly IEnumerable<Datatype> NoParameters = Enumerable.Empty<Datatype>();

        public string Name { get; }

        internal bool IsRuntime { get; }

        public Scalar Return
        {
            get => _return;
            internal set => _return = value ?? throw new ArgumentNullException(nameof(value));
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
                if (!IsRuntime || _buffer != null || _constants != null || _locals != null || _stack != null || _labels != null)
                    throw new InvalidOperationException($"!{nameof(IsRuntime)}");

                return _delegate ?? throw new InvalidOperationException(nameof(Delegate));
            }
            set
            {
                if (!IsRuntime || _buffer != null || _constants != null || _locals != null || _stack != null || _labels != null)
                    throw new InvalidOperationException($"!{nameof(IsRuntime)}");

                _delegate = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        private Function(string name, Scalar @return, bool isRuntime)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _return = @return ?? throw new ArgumentNullException(nameof(@return));
            IsRuntime = isRuntime;
        }

        public Function(string name, Scalar @return)
            : this(name, @return, false)
        {
            if (IsRuntime || _delegate != null)
                throw new InvalidOperationException(nameof(IsRuntime));

            _buffer = new Buffer();
            _constants = new ScalarCollection(true);
            _locals = new ScalarCollection(false);
            _stack = new Stack<Scalar>();
            _labels = new List<Label>();
        }

        public static (string package, string function) SplitQualifiedName(string qualifiedName)
        {
            if (qualifiedName == null)
                throw new ArgumentNullException(nameof(qualifiedName));

            return (qualifiedName.Remove(qualifiedName.IndexOf(':')), qualifiedName.Substring(qualifiedName.IndexOf(':') + 1));
        }

        public int GetLabelPosition(string labelName) =>
            Labels.Single(label => label.Name == labelName).Position;

        public void Reset()
        {
            _return.Undefine();
            Parameters.UndefineAll();
            _locals = new ScalarCollection(false);
            _stack = new Stack<Scalar>();
        }

        private static Datatype GetDatatypeFromType(Type type) =>
            type?.Name switch
            {
                "Void" => Datatype.Void,
                "Boolean" => Datatype.Boolean,
                "Int64" => Datatype.Integer,
                "Double" => Datatype.Float,
                "String" => Datatype.String,
                _ => throw new ArgumentException(nameof(type))
            };

        internal static object[] Flatten(Stack<Scalar> stack, int numberParameters)
        {
            var list = new List<object>();
            while (stack.Count > 0 && numberParameters > 0)
            {
                list.Insert(0, stack.Pop().Value ?? throw new InvalidOperationException(nameof(stack)));
                numberParameters--;
            }
            return list.ToArray();
        }

        internal static Function MakeRuntimeFunction(string name, Delegate @delegate)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));

            var function = new Function(name, (Scalar)GetDatatypeFromType(@delegate.Method.ReturnType), true) { Delegate = @delegate };
            foreach (var parameter in @delegate.Method.GetParameters().Select(parameter => GetDatatypeFromType(parameter.ParameterType)))
                function.Parameters.Append((Scalar)parameter);
            return function;
        }

        internal (bool returns, object? value) RunRuntime(object?[] values)
        {
            if (!IsRuntime)
                throw new InvalidOperationException($"!{nameof(IsRuntime)}");
            if (_delegate == null)
                throw new InvalidOperationException(nameof(_delegate));
            if (Return == null)
                throw new InvalidOperationException(nameof(Return));
            if (Return.IsDefined)
                throw new InvalidOperationException($"{nameof(Return)}.{nameof(Return.IsDefined)}");
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Parameters?.Count)
                throw new ArgumentException($"{nameof(values)}.{nameof(values.Length)}");

            for (var i = 0; i < Parameters.Count; i++)
                Parameters.DefineAt(i, values[i] ?? throw new ArgumentNullException(nameof(values)));

            var parameters = Parameters.GetValues().ToArray();
            if (Return.Datatype != Datatype.Void)
                Return.DefineValue(_delegate.DynamicInvoke(parameters) ?? throw new InvalidOperationException(nameof(Return)));
            else
                _delegate.DynamicInvoke(parameters);

            Parameters.UndefineAll();

            if (!Return.IsDefined)
                return (false, null);
            else
            {
                var @return = Return.Value;
                Return.Undefine();
                return (true, @return);
            }
        }

        public override string ToString() =>
            $"{Name}({string.Join(',', Parameters.GetDatatypes().Select(datatype => datatype.ToString()))}) -> {Return.Datatype}";
    }

    public sealed class Package
    {
        public string Name { get; }

        internal bool IsRuntime { get; }

        public ScalarCollection? Globals { get; }

        public IList<Function> Functions { get; } = new List<Function>();

        private Package(string name, bool isRuntime)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRuntime = isRuntime;
            if (!isRuntime)
                Globals = new ScalarCollection(true);
        }

        public Package(string name)
            : this(name, false)
        { }

        internal static Package MakeRuntimePackage(string name, (string name, Delegate @delegate)[] functions)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (functions == null)
                throw new ArgumentNullException(nameof(functions));

            var package = new Package($"std.{name}", true);
            foreach (var (functionName, @delegate) in functions)
                package.Functions.Add(Function.MakeRuntimeFunction(functionName, @delegate));
            return package;
        }

        internal Function? FindBySignature(string name, bool isRuntime, Datatype @return, IEnumerable<Datatype> parameters)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            foreach (var function in Functions)
                if (function != null && function.Name == name && function.IsRuntime == isRuntime
                    && function.Return.Datatype == @return && function.Parameters.GetDatatypes().SequenceEqual(parameters))
                    return function;

            return null;
        }

        public Function? FindBySignature(string name, Datatype @return, IEnumerable<Datatype> parameters) =>
            FindBySignature(name ?? throw new ArgumentNullException(nameof(name)), false, @return, parameters ?? throw new ArgumentNullException(nameof(parameters)));

        public override string ToString() =>
            $"{Name}|{Functions.Count}";
    }
}