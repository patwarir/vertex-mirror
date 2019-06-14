using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Parser
    {
        private readonly string _code;

        public IList<Function> Functions { get; } = new List<Function>();

        public Parser(string code) =>
            _code = code;

        private string TrimTabs(string line) =>
            line.AsSpan(line.LastIndexOf('\t') + 1).ToString();

        private Datatype GetDatatype(string datatype)
        {
            switch (datatype.ToLowerInvariant())
            {
                case "void":
                    return Datatype.Void;
                case "bl":
                    return Datatype.Boolean;
                case "int":
                    return Datatype.Integer;
                case "fl":
                    return Datatype.Float;
                case "str":
                    return Datatype.String;
            }

            throw new ArgumentException($"{nameof(datatype)} {datatype} is invalid!");
        }

        private object GetDefaultValue(Datatype datatype)
        {
            switch (datatype)
            {
                case Datatype.Boolean:
                    return false;
                case Datatype.Integer:
                    return 0L;
                case Datatype.Float:
                    return 0.0D;
                case Datatype.String:
                    return string.Empty;
            }

            throw new ArgumentException($"{nameof(datatype)} {datatype} is invalid!");
        }

        private string GetStringLiteral(string literalWithQuotes) =>
            literalWithQuotes.Substring(1, literalWithQuotes.LastIndexOf('"') - 1);

        public void Run()
        {
            Function current = null;
            bool inConstantBlock = false, inLocalBlock = false, inCommentBlock = false;
            byte constantIndex = 0, localIndex = 0;

            foreach (var line in _code.Split(Environment.NewLine).Select(TrimTabs))
            {
                if (line.StartsWith("/*"))
                    inCommentBlock = true;
                else if (line.StartsWith("*/"))
                    inCommentBlock = false;
                if (!inCommentBlock)
                {
                    if (line.StartsWith("//"))
                    { }

                    else if (line.StartsWith("func "))
                    {
                        var name = line.Substring(5, line.IndexOf('(') - 5);
                        var @return = line.Substring(line.LastIndexOf(':') + 2, line.LastIndexOf('{') - line.LastIndexOf(':') - 3);
                        current = new Function(name, GetDatatype(@return));

                        var stringListOfParameters = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);

                        byte index = 0;

                        foreach (var stringParameter in stringListOfParameters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                        {
                            var datatype = GetDatatype(stringParameter);
                            current.Parameters.Add(index, datatype, GetDefaultValue(datatype));
                            index++;
                        }
                    }
                    else if (!inConstantBlock && !inLocalBlock && line == "}")
                    {
                        Functions.Add(current);
                        current = null;
                    }

                    else if (line.StartsWith("cn "))
                    {
                        constantIndex = 0;
                        inConstantBlock = true;
                    }
                    else if (!inLocalBlock && line == "}")
                        inConstantBlock = false;

                    else if (inConstantBlock)
                    {
                        if (bool.TryParse(line, out var bl))
                            current.Constants.Add(constantIndex++, Datatype.Boolean, bl);
                        else if (long.TryParse(line, out var @int))
                            current.Constants.Add(constantIndex++, Datatype.Integer, @int);
                        else if (double.TryParse(line, out var fl))
                            current.Constants.Add(constantIndex++, Datatype.Float, fl);
                        else if (line.StartsWith('"'))
                            current.Constants.Add(constantIndex++, Datatype.String, GetStringLiteral(line));
                    }

                    else if (line.StartsWith("lc "))
                    {
                        localIndex = 0;
                        inLocalBlock = true;
                    }
                    else if (!inConstantBlock && line == "}")
                        inLocalBlock = false;

                    else if (inLocalBlock)
                    {
                        var datatype = GetDatatype(line);
                        current.Locals.Add(localIndex++, datatype, GetDefaultValue(datatype));
                    }

                    else if (line.StartsWith("lb "))
                        current.Labels.Add(new Label(line.Substring(3, line.Length - 4), current.Buffer.Length));

                    else if (line.StartsWith("nop "))
                        current.Buffer.WriteOperationCode(OperationCode.NoOperation);

                    else if (line.StartsWith("jmp "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.JumpAlways);
                        current.Buffer.WriteString(line.Substring(4, line.Length - 4));
                    }
                    else if (line.StartsWith("jmp.a "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.JumpAlways);
                        current.Buffer.WriteString(line.Substring(6, line.Length - 6));
                    }

                    else if (line.StartsWith("jmp.t "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.JumpTrue);
                        current.Buffer.WriteString(line.Substring(6, line.Length - 6));
                    }
                    else if (line.StartsWith("jmp.f "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.JumpFalse);
                        current.Buffer.WriteString(line.Substring(6, line.Length - 6));
                    }

                    else if (line.StartsWith("call "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.Call);
                        var start = line.Substring(5);

                        current.Buffer.WriteDatatype(GetDatatype(start.Substring(0, start.IndexOf(' '))));
                        current.Buffer.WriteString(start.Substring(start.IndexOf(' ') + 1, start.IndexOf('(') - start.IndexOf(' ') - 1));

                        var stringListOfParameters = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                        IList<Datatype> datatypes = new List<Datatype>();

                        foreach (var stringParameter in stringListOfParameters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                            datatypes.Add(GetDatatype(stringParameter));

                        current.Buffer.WriteDatatypes(datatypes);
                    }
                    else if (line.StartsWith("ret "))
                        current.Buffer.WriteOperationCode(OperationCode.Return);

                    else if (line.StartsWith("trw "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.Throw);
                        var start = line.Substring(4);

                        current.Buffer.WriteString(start.Substring(0, start.IndexOf('(')));

                        var stringListOfParameters = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                        IList<Datatype> datatypes = new List<Datatype>();

                        foreach (var stringParameter in stringListOfParameters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                            datatypes.Add(GetDatatype(stringParameter));

                        current.Buffer.WriteDatatypes(datatypes);
                    }
                    else if (line.StartsWith("throw "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.Throw);
                        var start = line.Substring(6);

                        current.Buffer.WriteString(start.Substring(0, start.IndexOf('(')));

                        var stringListOfParameters = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                        IList<Datatype> datatypes = new List<Datatype>();

                        foreach (var stringParameter in stringListOfParameters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                            datatypes.Add(GetDatatype(stringParameter));

                        current.Buffer.WriteDatatypes(datatypes);
                    }

                    else if (line.StartsWith("pop "))
                        current.Buffer.WriteOperationCode(OperationCode.Pop);
                    else if (line.StartsWith("dup "))
                        current.Buffer.WriteOperationCode(OperationCode.Duplicate);

                    else if (line.StartsWith("ld.bl"))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadBoolean);
                        var value = line.Substring(5);

                        if (value == string.Empty)
                            current.Buffer.WriteBoolean((bool)GetDefaultValue(Datatype.Boolean));
                        else
                            current.Buffer.WriteBoolean(bool.Parse(value.AsSpan(1).ToString()));
                    }
                    else if (line.StartsWith("ld.int"))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadInteger);
                        var value = line.Substring(6);

                        if (value == string.Empty)
                            current.Buffer.WriteInteger((long)GetDefaultValue(Datatype.Integer));
                        else
                            current.Buffer.WriteInteger(long.Parse(value.AsSpan(1).ToString()));
                    }
                    else if (line.StartsWith("ld.fl"))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadFloat);
                        var value = line.Substring(5);

                        if (value == string.Empty)
                            current.Buffer.WriteFloat((double)GetDefaultValue(Datatype.Float));
                        else
                            current.Buffer.WriteFloat(double.Parse(value.AsSpan(1).ToString()));
                    }
                    else if (line.StartsWith("ld.str"))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadString);
                        var value = line.Substring(6);

                        if (value == string.Empty)
                            current.Buffer.WriteString((string)GetDefaultValue(Datatype.String));
                        else
                            current.Buffer.WriteString(GetStringLiteral(value.AsSpan(1).ToString()));
                    }

                    else if (line.StartsWith("ld.pr "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("ld.arg "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }

                    else if (line.StartsWith("ld.cn "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadConstant);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("ld.lc "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.LoadLocal);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(6)));
                    }

                    else if (line.StartsWith("st.pr "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.SetParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("st.arg "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.SetParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }

                    else if (line.StartsWith("st.lc "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.SetLocal);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(6)));
                    }

                    else if (line.StartsWith("inc.pr "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.IncrementParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }
                    else if (line.StartsWith("inc.arg "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.IncrementParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(8)));
                    }

                    else if (line.StartsWith("inc.lc "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.IncrementLocal);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }

                    else if (line.StartsWith("dec.pr "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.DecrementParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }
                    else if (line.StartsWith("dec.arg "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.DecrementParameter);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(8)));
                    }

                    else if (line.StartsWith("dec.lc "))
                    {
                        current.Buffer.WriteOperationCode(OperationCode.DecrementLocal);
                        current.Buffer.WriteByte(byte.Parse(line.Substring(7)));
                    }

                    else if (line.StartsWith("neg "))
                        current.Buffer.WriteOperationCode(OperationCode.Negate);

                    else if (line.StartsWith("add "))
                        current.Buffer.WriteOperationCode(OperationCode.Add);
                    else if (line.StartsWith("sub "))
                        current.Buffer.WriteOperationCode(OperationCode.Subtract);
                    else if (line.StartsWith("mul "))
                        current.Buffer.WriteOperationCode(OperationCode.Multiply);
                    else if (line.StartsWith("div "))
                        current.Buffer.WriteOperationCode(OperationCode.Divide);
                    else if (line.StartsWith("mod "))
                        current.Buffer.WriteOperationCode(OperationCode.Modulate);

                    else if (line.StartsWith("c.eq "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckEquals);
                    else if (line.StartsWith("if.eq "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckEquals);

                    else if (line.StartsWith("c.gt "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckGreater);
                    else if (line.StartsWith("if.gt "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckGreater);

                    else if (line.StartsWith("c.lt "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckLess);
                    else if (line.StartsWith("if.lt "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckLess);

                    else if (line.StartsWith("c.ge "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckGreaterEquals);
                    else if (line.StartsWith("if.ge "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckGreaterEquals);

                    else if (line.StartsWith("c.le "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckLessEquals);
                    else if (line.StartsWith("if.le "))
                        current.Buffer.WriteOperationCode(OperationCode.CheckLessEquals);
                }
            }
        }
    }
}