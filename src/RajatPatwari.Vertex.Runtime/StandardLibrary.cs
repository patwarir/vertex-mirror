using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public static class StandardLibrary
    {
        public static IEnumerable<string> Names { get; } = new[]
        {
            "std.env.date", "std.env.time",

            "std.sfn.len", "std.sfn.sub", "std.sfn.rem",

            "std.op.add", "std.op.sub", "std.op.mul",
            "std.op.div", "std.op.mod", "std.op.pow",

            "std.math.abs", "std.math.neg",
            "std.math.max", "std.math.min",

            "std.cmp.eq", "std.cmp.gt", "std.cmp.lt",
            "std.cmp.ge", "std.cmp.le",

            "std.io.write", "std.io.writeln",
            "std.io.read", "std.io.readln",

            "std.ex.arg", "std.ex.arg_null",
            "std.ex.arg_range", "std.ex.inv_op"
        };
    }
}