namespace RajatPatwari.Vertex.Runtime
{
    public static class Program
    {
        public static void Main() =>
            StandardLibrary.FindAndCall("std.io::writeln", VirtualMachine.Datatype.Void,
                new System.Collections.Generic.List<VirtualMachine.Datatype>()
                    { VirtualMachine.Datatype.String });
    }
}