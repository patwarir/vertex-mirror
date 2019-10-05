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

        LoadParameter,
        LoadLocal,

        SetParameter,
        SetLocal
    }

    public sealed class Buffer : IEnumerable<byte>
    {
        private readonly IList<byte> _stream = new List<byte>();

        public int Length =>
            _stream.Count;

        private byte Read(int position) =>
            _stream[position];

        public Datatype ReadDatatype(int position) =>
            (Datatype)Read(position);

        public OperationCode ReadOperationCode(int position) =>
            (OperationCode)Read(position);

        private void Write(byte value) =>
            _stream.Add(value);

        public void WriteDatatype(Datatype value) =>
            Write((byte)value);

        public void WriteOperationCode(OperationCode value) =>
            Write((byte)value);

        public IEnumerator<byte> GetEnumerator() =>
            _stream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _stream.GetEnumerator();
    }

    public readonly struct FunctionDeclaration : IEquatable<FunctionDeclaration>
    {
        public string QualifiedName { get; }

        public Datatype Return { get; }

        public IEnumerable<Datatype> Parameters { get; }

        public FunctionDeclaration(string qualifiedName, Datatype @return, IEnumerable<Datatype> parameters)
        {
            QualifiedName = qualifiedName;
            Return = @return;
            Parameters = parameters;
        }

        public bool Equals(FunctionDeclaration declaration) =>
            QualifiedName == declaration.QualifiedName && Return == declaration.Return
                && Parameters.SequenceEqual(declaration.Parameters);

        public override bool Equals(object? obj) =>
            obj is FunctionDeclaration declaration && Equals(declaration);

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}