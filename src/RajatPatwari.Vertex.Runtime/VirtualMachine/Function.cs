using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Function
    {
        public string Name { get; }

        public Datatype Return { get; }

        public ScalarList Parameters { get; } = new ScalarList();

        public ScalarList Constants { get; } = new ScalarList(true);

        public ScalarList Locals { get; } = new ScalarList();

        public Buffer Buffer { get; } = new Buffer();

        public Stack<object> Stack { get; } = new Stack<object>();

        public IList<Label> Labels { get; } = new List<Label>();

        public Function(string name, Datatype @return)
        {
            Name = name;
            Return = @return;
        }
    }
}