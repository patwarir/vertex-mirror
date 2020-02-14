using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
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

        internal Function FindBySignature(string name, bool isRuntime, Datatype @return,
            IEnumerable<Datatype> parameters)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            foreach (var function in Functions)
                if (function != null && function.Name == name && function.IsRuntime == isRuntime
                    && function.Return.Datatype == @return
                    && function.Parameters.GetDatatypes().SequenceEqual(parameters))
                    return function;

            return null;
        }

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

        public Function FindBySignature(string name, Datatype @return, IEnumerable<Datatype> parameters) =>
            FindBySignature(name ?? throw new ArgumentNullException(nameof(name)), false, @return,
                parameters ?? throw new ArgumentNullException(nameof(parameters)));
        
        public override string ToString() =>
            $"{Name}|{Functions.Count}";
    }
}