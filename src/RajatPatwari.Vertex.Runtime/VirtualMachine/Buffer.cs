using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _stream = new List<byte>();

        public ushort Length =>
            (ushort)_stream.Count;

        public byte ReadByte(ushort position) =>
            _stream[position];

        public OperationCode ReadOperationCode(ushort position) =>
            (OperationCode)ReadByte(position);

        public Datatype ReadDatatype(ushort position) =>
            (Datatype)ReadByte(position);

        public char ReadCharacter(ushort position) =>
            (char)ReadByte(position);

        public bool ReadBoolean(ushort position) =>
            BitConverter.ToBoolean(_stream.ToArray(), position);

        public long ReadInteger(ushort position) =>
            BitConverter.ToInt64(_stream.ToArray(), position);

        public double ReadFloat(ushort position) =>
            BitConverter.ToDouble(_stream.ToArray(), position);

        public string ReadString(ushort position)
        {
            var length = ReadByte(position++);

            var builder = new StringBuilder();
            for (ushort index = position; index < position + length; index++)
                builder.Append(ReadCharacter(index));
            return builder.ToString();
        }

        public IEnumerable<Datatype> ReadDatatypes(ushort position)
        {
            var length = ReadByte(position++);

            var datatypes = new List<Datatype>();
            for (ushort index = position; index < position + length; index++)
                datatypes.Add(ReadDatatype(index));
            return datatypes;
        }

        public void WriteByte(byte value) =>
            _stream.Add(value);

        public void WriteOperationCode(OperationCode value) =>
            WriteByte((byte)value);

        public void WriteDatatype(Datatype value) =>
            WriteByte((byte)value);

        public void WriteCharacter(char value) =>
            WriteByte((byte)value);

        public void WriteBoolean(bool value)
        {
            foreach (var @byte in BitConverter.GetBytes(value))
                WriteByte(@byte);
        }

        public void WriteInteger(long value)
        {
            foreach (var @byte in BitConverter.GetBytes(value))
                WriteByte(@byte);
        }

        public void WriteFloat(double value)
        {
            foreach (var @byte in BitConverter.GetBytes(value))
                WriteByte(@byte);
        }

        public void WriteString(string value)
        {
            WriteByte((byte)value.Length);
            foreach (var character in value.AsSpan())
                WriteCharacter(character);
        }

        public void WriteDatatypes(IEnumerable<Datatype> value)
        {
            WriteByte((byte)value.Count());
            foreach (var datatype in value)
                WriteDatatype(datatype);
        }

        public IEnumerator<byte> GetEnumerator() =>
            _stream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _stream.GetEnumerator();
    }
}