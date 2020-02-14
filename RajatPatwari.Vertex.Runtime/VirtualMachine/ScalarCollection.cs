using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
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
                scalar?.Undefine();
        }

        public IEnumerable<Datatype> GetDatatypes() =>
            _scalars.Select(scalar => scalar?.Datatype ?? throw new InvalidOperationException(nameof(scalar)));

        public IEnumerable<object> GetValues() =>
            _scalars.Select(scalar => scalar?.Value ?? throw new InvalidOperationException(nameof(scalar)));

        public override string ToString() =>
            $"Count = {Count}";

        public IEnumerator<Scalar> GetEnumerator() =>
            _scalars.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _scalars.GetEnumerator();
    }
}