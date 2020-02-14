using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Function
    {
        private Scalar _return;

        private readonly Buffer? _buffer;

        private readonly ScalarCollection? _constants, _locals;

        private readonly Stack<Scalar>? _stack;

        private readonly IList<Label>? _labels;

        private Delegate? _delegate;

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
                if (!IsRuntime || _buffer != null || _constants != null || _locals != null || _stack != null
                    || _labels != null)
                    throw new InvalidOperationException($"!{nameof(IsRuntime)}");
                
                return _delegate ?? throw new InvalidOperationException(nameof(Delegate));
            }
            set
            {
                if (!IsRuntime || _buffer != null || _constants != null || _locals != null || _stack != null
                    || _labels != null)
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
            : this(name, false)
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
        
        internal static (string package, string function) SplitQualifiedName(string qualifiedName)
        {
            if (qualifiedName == null)
                throw new ArgumentNullException(nameof(qualifiedName));

            return (qualifiedName.Remove(qualifiedName.IndexOf(':')),
                qualifiedName.Substring(qualifiedName.IndexOf(':') + 1));
        }

        internal static Datatype GetDatatypeFromType(Type type) =>
            type?.Name switch
            {
                "Void" => Datatype.Void,
                "Boolean" => Datatype.Boolean,
                "Int64" => Datatype.Integer,
                "Double" => Datatype.Float,
                "String" => Datatype.String,
                _ => throw new ArgumentException(nameof(type))
            };

        internal static Function MakeRuntimeFunction(string name, Delegate @delegate)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));
            
            var function = new Function(name, true)
            {
                Return = (Scalar)GetDatatypeFromType(@delegate.Method.ReturnType), Delegate = @delegate
            };
            foreach (var parameter in @delegate.Method.GetParameters()
                .Select(parameter => GetDatatypeFromType(parameter.ParameterType)))
                function.Parameters.Append((Scalar)parameter);
            return function;
        }

        internal (bool returns, object? value) RunRuntime(params object?[] values)
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
                Return.DefineValue(_delegate.DynamicInvoke(parameters));
            else
                _delegate.DynamicInvoke(parameters);
            
            Parameters.UndefineAll();

            if (!Return.IsDefined)
                return (false, null);
            var @return = Return.Value;
            Return.Undefine();
            return (true, @return);
        }

        public override string ToString() =>
            $"{Name}({string.Join(',', Parameters.GetDatatypes().Select(datatype => datatype.ToString()))}) -> {Return.Datatype}";
    }
}