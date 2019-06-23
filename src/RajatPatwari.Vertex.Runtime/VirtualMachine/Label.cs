using System;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public readonly struct Label : IEquatable<Label>, IComparable<Label>
    {
        public string Name { get; }

        public ushort Position { get; }

        public Label(string name, ushort position)
        {
            Name = name;
            Position = position;
        }

        public bool Equals(Label other) =>
            Name == other.Name && Position == other.Position;

        public override bool Equals(object obj) =>
            obj is Label label && Equals(label);

        public override int GetHashCode() =>
            base.GetHashCode();

        public int CompareTo(Label other) =>
            Position.CompareTo(other.Position);
    }
}