using System;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.Token
{
    public enum TokenType
    {
        Undefined,
        Symbol,
        Keyword,
        Literal,
        Identifier
    }

    public abstract class Token
    {
        public TokenType TokenType { get; } = TokenType.Undefined;

        protected Token(TokenType tokenType) =>
            TokenType = tokenType;

        public override string ToString() =>
            TokenType.ToString();
    }

    public enum SymbolType
    {
        Undefined,
        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        Comma,
        Colon,
        Period,
        RuntimeReference // $
    }

    public sealed class Symbol : Token
    {
        public SymbolType Type { get; } = SymbolType.Undefined;

        public Symbol(SymbolType type)
            : base(TokenType.Symbol) =>
            Type = type;

        public override string ToString() =>
            Type.ToString();
    }

    public enum KeywordType
    {
        Undefined,
        Function, // func
        Call,
        Return, // ret

        Load, // ld
        Set,

        Throw,

        Void,
        Boolean, // bool
        Integer, // int
        Float,
        String, // str
        Local, // loc
        Argument, // arg

        Add,
        Subtract, // sub
        Multiply, // mul
        Divide, // div
        Modulus, // mod

        If,
        Equal, // equ
        Else,
        EndIf // end
    }

    public sealed class Keyword : Token
    {
        public KeywordType Type { get; } = KeywordType.Undefined;

        public Keyword(KeywordType type)
            : base(TokenType.Keyword) =>
            Type = type;

        public override string ToString() =>
            Type.ToString();
    }

    public enum Datatype
    {
        Undefined,
        Void,
        Boolean,
        Integer,
        Float,
        String
    }

    public sealed class Literal : Token
    {
        public Datatype Datatype { get; } = Datatype.Undefined;

        public object Value { get; }

        public Literal(Datatype datatype, object value)
            : base(TokenType.Literal)
        {
            Datatype = datatype;
            Value = null;

            if (Datatype == Datatype.Boolean)
                Value = bool.Parse(value.ToString());
            else if (Datatype == Datatype.Integer)
                Value = long.Parse(value.ToString());
            else if (Datatype == Datatype.Float)
                Value = double.Parse(value.ToString());
            else if (Datatype == Datatype.String)
                Value = value.ToString();

            if (Value == null)
                throw new InvalidOperationException($"Invalid {nameof(Literal)}!");
        }

        public override string ToString() =>
            $"{Datatype}|{Value}";
    }

    public sealed class Identifier : Token
    {
        public string Value { get; }

        public Identifier(string value)
            : base(TokenType.Identifier)
        {
            var stringBuilder = new StringBuilder();

            var index = 0;
            if (!char.IsLetter(value[index]))
                throw new InvalidOperationException($"Invalid {nameof(Identifier)}!");
            stringBuilder.Append(value[index++]);

            while (index < value.Length)
            {
                if (char.IsLetterOrDigit(value[index]) || value[index] == '.' || value[index] == '_')
                    stringBuilder.Append(value[index++]);
                else
                    throw new InvalidOperationException($"Invalid {nameof(Identifier)}!");
            }

            Value = stringBuilder.ToString();
        }

        public override string ToString() =>
            Value;
    }
}