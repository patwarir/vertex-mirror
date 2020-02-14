using System.Collections;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _buffer = new List<byte>();

        public int Count =>
            _buffer.Count;

        public byte Read(int position) =>
            _buffer[position];

        public void Write(byte value) =>
            _buffer.Add(value);
        
        // TODO: Finish all the reads and writes.

        public override string ToString() =>
            $"Count = {Count}";

        public IEnumerator<byte> GetEnumerator() =>
            _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _buffer.GetEnumerator();
    }
}