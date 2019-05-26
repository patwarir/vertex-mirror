using RajatPatwari.Vertex.Runtime.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Lexer
    {
        private bool _ran = false;

        private readonly string _code = string.Empty;

        private int _position = 0;

        public IList<Token.Token> Tokens { get; } = new List<Token.Token>();

        public Lexer(string code) =>
            _code = code ?? throw new ArgumentNullException(nameof(code));

        private void ReadSingleLineComment()
        {
            while (_position < _code.Length)
            {
                if (_code[_position] == '\r' || _code[_position] == '\n')
                    return;
                _position++;
            }
        }

        private void ReadMultiLineComment()
        {
            while (_position < _code.Length)
            {
                if (_code[_position] == '*' && _code[_position + 1] == '/')
                {
                    _position += 2;
                    return;
                }
                _position++;
            }
        }

        private char ReadSymbol() =>
            _code[_position++];

        private string ReadIdentifier()
        {
            var stringBuilder = new StringBuilder();

            if (!char.IsLetter(_code[_position]))
                throw new InvalidOperationException($"Invalid {nameof(Identifier)}!");
            stringBuilder.Append(_code[_position++]);

            while (_position < _code.Length)
            {
                if (char.IsLetterOrDigit(_code[_position]) || _code[_position] == '.' || _code[_position] == '_')
                    stringBuilder.Append(_code[_position++]);
                else
                    break;
            }

            return stringBuilder.ToString();
        }

        private string ReadStringLiteral()
        {
            _position++;

            var stringBuilder = new StringBuilder();
            while (_position < _code.Length)
            {
                if (_code[_position] == '"')
                {
                    _position++;
                    break;
                }

                stringBuilder.Append(_code[_position++]);
            }
            return stringBuilder.ToString();
        }

        private string ReadAlphabetic()
        {
            var stringBuilder = new StringBuilder();
            while (_position < _code.Length)
            {
                if (!char.IsLetter(_code[_position]))
                    break;
                stringBuilder.Append(_code[_position++]);
            }
            return stringBuilder.ToString();
        }

        private string ReadNumeric()
        {
            var stringBuilder = new StringBuilder();
            while (_position < _code.Length)
            {
                if (char.IsDigit(_code[_position]) || _code[_position] == '+' || _code[_position] == '-'
                    || _code[_position] == '.')
                    stringBuilder.Append(_code[_position++]);
                else
                    break;
            }
            return stringBuilder.ToString();
        }

        public void Run()
        {
            if (_ran)
                throw new InvalidOperationException($"Attempting to run the {nameof(Lexer)} more than once!");
            _ran = true;

            while (_position < _code.Length)
            {
                if (_code[_position] == '#' || (_code[_position] == '/' && _code[_position + 1] == '/'))
                    ReadSingleLineComment();
                else if (_code[_position] == '/' && _code[_position + 1] == '*')
                    ReadMultiLineComment();
                else if (_code[_position] == '(' || _code[_position] == ')' || _code[_position] == '['
                    || _code[_position] == ']' || _code[_position] == ',' || _code[_position] == ':')
                {
                    var symbolChar = ReadSymbol();
                    Symbol symbol = null;

                    if (symbolChar == '(')
                        symbol = new Symbol(SymbolType.LeftParenthesis);
                    else if (symbolChar == ')')
                        symbol = new Symbol(SymbolType.RightParenthesis);
                    else if (symbolChar == '[')
                        symbol = new Symbol(SymbolType.LeftBracket);
                    else if (symbolChar == ']')
                        symbol = new Symbol(SymbolType.RightBracket);
                    else if (symbolChar == ',')
                        symbol = new Symbol(SymbolType.Comma);
                    else if (symbolChar == ':')
                        symbol = new Symbol(SymbolType.Colon);

                    Tokens.Add(symbol ?? throw new InvalidOperationException($"Invalid {nameof(Symbol)}!"));
                }
                else if (_code[_position] == '$')
                {
                    Tokens.Add(new Symbol(SymbolType.RuntimeReference));
                    _position++;
                    Tokens.Add(new Identifier(ReadIdentifier()));
                }
                else if (_code[_position] == '"')
                    Tokens.Add(new Literal(Datatype.String, ReadStringLiteral()));
                else if (char.IsLetter(_code[_position]))
                {
                    var alphabetic = ReadAlphabetic();

                    if (alphabetic == "func")
                    {
                        Tokens.Add(new Keyword(KeywordType.Function));
                        _position++;
                        Tokens.Add(new Identifier(ReadIdentifier()));
                    }
                    else if (alphabetic == "call")
                    {
                        Tokens.Add(new Keyword(KeywordType.Call));
                        _position++;

                        var returnType = ReadAlphabetic();

                        if (returnType == "void")
                            Tokens.Add(new Keyword(KeywordType.Void));
                        else if (returnType == "bool")
                            Tokens.Add(new Keyword(KeywordType.Boolean));
                        else if (returnType == "int")
                            Tokens.Add(new Keyword(KeywordType.Integer));
                        else if (returnType == "float")
                            Tokens.Add(new Keyword(KeywordType.Float));
                        else if (returnType == "str")
                            Tokens.Add(new Keyword(KeywordType.String));

                        _position++;

                        if (_code[_position] == '$')
                        {
                            Tokens.Add(new Symbol(SymbolType.RuntimeReference));
                            _position++;
                            Tokens.Add(new Identifier(ReadIdentifier()));
                        }
                        else if (char.IsLetter(_code[_position]))
                            Tokens.Add(new Identifier(ReadIdentifier()));
                    }
                    else if (alphabetic == "ret")
                        Tokens.Add(new Keyword(KeywordType.Return));

                    else if (alphabetic == "ld")
                        Tokens.Add(new Keyword(KeywordType.Load));
                    else if (alphabetic == "set")
                        Tokens.Add(new Keyword(KeywordType.Set));

                    else if (alphabetic == "throw")
                    {
                        Tokens.Add(new Keyword(KeywordType.Throw));
                        _position++;

                        if (_code[_position] == '$')
                        {
                            Tokens.Add(new Symbol(SymbolType.RuntimeReference));
                            _position++;
                            Tokens.Add(new Identifier(ReadIdentifier()));
                        }
                    }

                    else if (alphabetic == "void")
                        Tokens.Add(new Keyword(KeywordType.Void));
                    else if (alphabetic == "bool")
                        Tokens.Add(new Keyword(KeywordType.Boolean));
                    else if (alphabetic == "int")
                        Tokens.Add(new Keyword(KeywordType.Integer));
                    else if (alphabetic == "float")
                        Tokens.Add(new Keyword(KeywordType.Float));
                    else if (alphabetic == "str")
                        Tokens.Add(new Keyword(KeywordType.String));
                    else if (alphabetic == "loc")
                        Tokens.Add(new Keyword(KeywordType.Local));
                    else if (alphabetic == "arg")
                        Tokens.Add(new Keyword(KeywordType.Argument));

                    else if (alphabetic == "add")
                        Tokens.Add(new Keyword(KeywordType.Add));
                    else if (alphabetic == "sub")
                        Tokens.Add(new Keyword(KeywordType.Subtract));
                    else if (alphabetic == "mul")
                        Tokens.Add(new Keyword(KeywordType.Multiply));
                    else if (alphabetic == "div")
                        Tokens.Add(new Keyword(KeywordType.Divide));
                    else if (alphabetic == "mod")
                        Tokens.Add(new Keyword(KeywordType.Modulus));

                    else if (alphabetic == "if")
                        Tokens.Add(new Keyword(KeywordType.If));
                    else if (alphabetic == "equ")
                        Tokens.Add(new Keyword(KeywordType.Equal));
                    else if (alphabetic == "else")
                        Tokens.Add(new Keyword(KeywordType.Else));
                    else if (alphabetic == "end")
                        Tokens.Add(new Keyword(KeywordType.EndIf));

                    else if (alphabetic == "true")
                        Tokens.Add(new Literal(Datatype.Boolean, true));
                    else if (alphabetic == "false")
                        Tokens.Add(new Literal(Datatype.Boolean, false));

                    else
                        Tokens.Add(new Identifier(alphabetic));
                }
                else if (char.IsDigit(_code[_position]) || _code[_position] == '+' || _code[_position] == '-'
                    || (_code[_position] == '.' && char.IsDigit(_code[_position + 1])))
                {
                    var number = ReadNumeric();

                    if (number.Contains('.'))
                        Tokens.Add(new Literal(Datatype.Float, number));
                    else
                        Tokens.Add(new Literal(Datatype.Integer, number));
                }
                else if (_code[_position] == '.')
                {
                    Tokens.Add(new Symbol(SymbolType.Period));
                    _position++;
                }
                else
                    _position++;
            }
        }
    }
}