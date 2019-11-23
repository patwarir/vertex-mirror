using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;

namespace RajatPatwari.Vertex.Runtime.User
{
    public static class Program
    {
        public static void Main()
        {
            var (_, value) = StandardLibrary.ExecuteFunctionByQualifiedName("std.sfn:cat", Datatype.String,
                new Datatype[] { Datatype.String, Datatype.String }, "Hello, ", "World!");
            Console.WriteLine(value);
        }
    }
}