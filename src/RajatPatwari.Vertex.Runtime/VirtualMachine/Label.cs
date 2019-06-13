using System;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public readonly struct Label : IComparable<Label>
    {
        public string Name { get; }

        public ushort Position { get; }

        public Label(string name, ushort position)
        {
            Name = name;
            Position = position;
        }

        public int CompareTo(Label other) =>
            Position.CompareTo(other.Position);
    }
}