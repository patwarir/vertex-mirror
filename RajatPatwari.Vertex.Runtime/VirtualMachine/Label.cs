namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Label
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
}