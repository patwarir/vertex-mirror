using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public sealed class ScalarList : IEnumerable<Scalar>
    {
        private IList<Scalar> _list = new List<Scalar>();

        public bool Constant { get; } = false;

        public ScalarList(bool constant = false) =>
            Constant = constant;

        public void Add(byte index, Datatype datatype, object value) =>
            _list.Insert(index, new Scalar(datatype, value));

        public void Change(byte index, object value)
        {
            if (Constant)
                throw new InvalidOperationException($"{nameof(Constant)}!");

            _list[index] = new Scalar(_list[index].Datatype, value);
        }

        public object GetValue(byte index) =>
            _list[index].Value;

        public IEnumerable<Datatype> GetDatatypes() =>
            _list.Select(scalar => scalar.Datatype);

        public IEnumerator<Scalar> GetEnumerator() =>
            _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _list.GetEnumerator();
    }
}