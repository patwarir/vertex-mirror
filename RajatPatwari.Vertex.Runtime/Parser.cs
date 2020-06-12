using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Parser
    {
        private readonly string _code;

        private Package? _package;

        public Package Package =>
            _package ?? throw new InvalidOperationException(nameof(Package));

        public Parser(string code) =>
            _code = code;

        private static string TrimTab(string line) =>
            line.Substring(line.LastIndexOf('\t') + 1);

        private static Datatype GetDatatype(string datatype) =>
            datatype switch
            {
                "vd" => Datatype.Void,
                "bl" => Datatype.Boolean,
                "int" => Datatype.Integer,
                "fl" => Datatype.Float,
                "str" => Datatype.String,
                _ => throw new ArgumentException(nameof(datatype))
            };

        private static string GetStringLiteral(string @string) =>
            @string[1..@string.LastIndexOf('"')];

        private static Scalar GetValue(Datatype datatype, string value) =>
            datatype switch
            {
                Datatype.Boolean => (Scalar)bool.Parse(value),
                Datatype.Integer => (Scalar)long.Parse(value),
                Datatype.Float => (Scalar)double.Parse(value),
                Datatype.String => (Scalar)GetStringLiteral(value),
                _ => throw new ArgumentException(nameof(value))
            };

        public void Run()
        {
            Function? current = null;
            bool inGlobalBlock = false, inConstantBlock = false, inLocalBlock = false, inCommentBlock = false;

            foreach (var line in _code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(TrimTab))
            {
                if (string.IsNullOrWhiteSpace(line))
                { }
                else if (line.StartsWith("/*"))
                    inCommentBlock = true;
                else if (line.Contains("*/") && inCommentBlock)
                    inCommentBlock = false;

                else if (!inCommentBlock)
                {
                    if (line.StartsWith("//"))
                    { }
                    else if (line.StartsWith("#"))
                    { } // TODO: Implement this.

                    else if (line.StartsWith("pkg ") && _package == null)
                        _package = new Package(line.Substring(4));

                    else if (line.StartsWith("gl ") && _package?.Globals?.Count == 0 && current == null)
                        inGlobalBlock = true;
                    else if (line.StartsWith('}') && current == null && !inConstantBlock && !inLocalBlock)
                        inGlobalBlock = false;
                    else if (inGlobalBlock)
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        _package?.Globals?.Append(GetValue(GetDatatype(parts[0]), parts[1]));
                    }

                    else if (line.StartsWith("fn "))
                    {
                        current = new Function(line[3..line.IndexOf('(')],
                            (Scalar)GetDatatype(line.Substring(line.LastIndexOf("->", StringComparison.Ordinal) + 3,
                                line.LastIndexOf('{') - line.LastIndexOf("->", StringComparison.Ordinal) - 4)));

                        var listParameters = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                        foreach (var parameter in listParameters.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(parameter => parameter.Trim()))
                            current.Parameters.Append((Scalar)GetDatatype(parameter));
                    }
                    else if (line.StartsWith('}') && current != null && !inConstantBlock && !inLocalBlock)
                    {
                        _package?.Functions.Add(current);
                        current = null;
                    }

                    else if (line.StartsWith("cn "))
                        inConstantBlock = true;
                    else if (line.StartsWith('}') && inConstantBlock && !inLocalBlock)
                        inConstantBlock = false;
                    else if (inConstantBlock)
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        current?.Constants.Append(GetValue(GetDatatype(parts[0]), parts[1]));
                    }

                    else if (line.StartsWith("lc "))
                        inLocalBlock = true;
                    else if (line.StartsWith('}') && inLocalBlock && !inConstantBlock)
                        inLocalBlock = false;
                    else if (inLocalBlock)
                        current?.Locals.Append((Scalar)GetDatatype(line));

                    else if (line.StartsWith("lb "))
                        current?.Labels.Add(new Label(line[3..], current.Buffer.Length));

                    else if (line.StartsWith("nop"))
                        current?.Buffer.WriteOperationCode(OperationCode.NoOperation);

                    else if (line.StartsWith("jmp.a "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.JumpAlways);
                        current?.Buffer.WriteString(line[6..]);
                    }

                    else if (line.StartsWith("jmp.t "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.JumpTrue);
                        current?.Buffer.WriteString(line[6..]);
                    }
                    else if (line.StartsWith("jmp.f "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.JumpFalse);
                        current?.Buffer.WriteString(line[6..]);
                    }

                    else if (line.StartsWith("call "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.Call);

                        var listParameters = line[(line.IndexOf('(') + 1)..line.IndexOf(')')];
                        IList<Datatype> datatypes = new List<Datatype>();
                        foreach (var datatype in listParameters.Split(", ", StringSplitOptions.RemoveEmptyEntries))
                            datatypes.Add(GetDatatype(datatype));

                        current?.Buffer.WriteFunction(line[5..line.IndexOf('(')], datatypes,
                            GetDatatype(line[(line.LastIndexOf("->", StringComparison.Ordinal) + 3)..]));
                    }
                    else if (line.StartsWith("ret"))
                        current?.Buffer.WriteOperationCode(OperationCode.Return);

                    else if (line.StartsWith("pop"))
                        current?.Buffer.WriteOperationCode(OperationCode.Pop);
                    else if (line.StartsWith("clr"))
                        current?.Buffer.WriteOperationCode(OperationCode.Clear);
                    else if (line.StartsWith("dup"))
                        current?.Buffer.WriteOperationCode(OperationCode.Duplicate);
                    else if (line.StartsWith("rot"))
                        current?.Buffer.WriteOperationCode(OperationCode.Rotate);

                    else if (line.StartsWith("ld.lt "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.LoadLiteral);
                        
                        var start = line.Substring(6);

                        var datatype = GetDatatype(start.Remove(start.IndexOf(' ')));
                        current?.Buffer.WriteDatatype(datatype);

                        var value = GetValue(datatype, start.Substring(start.IndexOf(' ') + 1));
                        switch (value.Datatype)
                        {
                            case Datatype.Boolean: current?.Buffer.WriteBoolean((bool)value.Value); break;
                            case Datatype.Integer: current?.Buffer.WriteInteger((long)value.Value); break;
                            case Datatype.Float: current?.Buffer.WriteFloat((double)value.Value); break;
                            case Datatype.String: current?.Buffer.WriteString((string)value.Value); break;
                            default: throw new InvalidOperationException(nameof(OperationCode.LoadLiteral));
                        }
                    }

                    else if (line.StartsWith("ld.gl "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.LoadGlobal);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("ld.pr "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.LoadParameter);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("ld.cn "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.LoadConstant);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("ld.lc "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.LoadLocal);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }

                    else if (line.StartsWith("st.pr "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.SetParameter);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }
                    else if (line.StartsWith("st.lc "))
                    {
                        current?.Buffer.WriteOperationCode(OperationCode.SetLocal);
                        current?.Buffer.Write(byte.Parse(line.Substring(6)));
                    }
                }
            }
        }
    }
}