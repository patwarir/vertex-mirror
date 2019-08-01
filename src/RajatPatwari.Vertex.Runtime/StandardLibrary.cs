using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public static class StandardLibrary
    {
        public static IEnumerable<string> Names { get; } = new[]
        {
            "env::exit", "env::pause",
            "env::date", "env::time",

            "sfn::len", "sfn::sub", "sfn::rem",

            "op::add", "op::sub", "op::mul",
            "op::div", "op::mod", "op::pow",

            "math::abs", "math::neg",
            "math::max", "math::min",

            "cmp::eq", "cmp::gt", "cmp::lt",
            "cmp::ge", "cmp::le",

            "io::write", "io::writeln",
            "io::read", "io::readln",
            "io::ln_str",

            "ex::arg", "ex::arg_null",
            "ex::arg_range", "ex::inv_op"
        };
    }
}