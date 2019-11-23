using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;

namespace RajatPatwari.Vertex.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var additionPkg = new Package("std.opr", true);
            var function = new Function("add", true)
            {
                Return = new Scalar(Datatype.Integer),
                Delegate = (Func<long, long, long>)((i, j) => i + j)
            };
            function.Parameters.Append(new Scalar(Datatype.Integer));
            function.Parameters.Append(new Scalar(Datatype.Integer));
            additionPkg.Functions.Add(function);

            var addFunc = additionPkg.FindBySignature("add", true, Datatype.Integer, new[] { Datatype.Integer, Datatype.Integer });
            var (_, val) = addFunc.RunRuntime(10L, 3L);
            Console.WriteLine(val);

            Console.WriteLine(Package.SplitQualifiedname("std.opr:add"));
        }
    }
}