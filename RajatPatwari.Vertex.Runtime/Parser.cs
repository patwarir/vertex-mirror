using RajatPatwari.Vertex.Runtime.AbstractSyntaxTree;
using RajatPatwari.Vertex.Runtime.Token;
using System;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class Parser
    {
        private bool _ran = false;

        private readonly IList<Token.Token> _tokens;

        private int _position = 0;

        public IList<ITreeObject> TreeObjects { get; } = new List<ITreeObject>();

        public Parser(IList<Token.Token> tokens) =>
            _tokens = tokens;

        private IList<Token.Token> ReadParenthesisBracketList()
        {
            // Assumed to be at LeftParenthesis.
            _position++;

            var list = new List<Token.Token>();
            while (_position < _tokens.Count)
            {
                if (_tokens[_position] is Symbol symbol
                    && (symbol.Type == SymbolType.RightParenthesis || symbol.Type == SymbolType.RightBracket))
                    break;
                else if (_tokens[_position] is Symbol commaSymbol && commaSymbol.Type == SymbolType.Comma)
                    _position++;
                else
                    list.Add(_tokens[_position++]);
            }

            _position++;
            return list;
        }

        private static Datatype GetDatatypeFromToken(Token.Token token)
        {
            var keyword = (Keyword)token;

            if (keyword.Type == KeywordType.Void)
                return Datatype.Void;
            else if (keyword.Type == KeywordType.Boolean)
                return Datatype.Boolean;
            else if (keyword.Type == KeywordType.Integer)
                return Datatype.Integer;
            else if (keyword.Type == KeywordType.Float)
                return Datatype.Float;
            else if (keyword.Type == KeywordType.String)
                return Datatype.String;

            throw new InvalidOperationException($"Invalid {nameof(token)}!");
        }

        private static IList<Datatype> GetDatatypesFromTokens(IList<Token.Token> tokens)
        {
            var list = new List<Datatype>();
            foreach (var token in tokens)
                list.Add(GetDatatypeFromToken(token));
            return list;
        }

        public void Run()
        {
            if (_ran)
                throw new InvalidOperationException($"Attempting to run the {nameof(Parser)} more than once!");
            _ran = true;

            Function currentFunction = null;

            while (_position < _tokens.Count)
            {
                if (_tokens[_position].TokenType == TokenType.Keyword && _tokens[_position] is Keyword keyword)
                {
                    _position++;

                    if (keyword.Type == KeywordType.Function)
                    {
                        var name = (Identifier)_tokens[_position++];
                        var argumentList = GetDatatypesFromTokens(ReadParenthesisBracketList());

                        _position++;
                        var returnType = GetDatatypeFromToken(_tokens[_position++]);

                        var localList = GetDatatypesFromTokens(ReadParenthesisBracketList());

                        currentFunction = new Function
                        {
                            Name = name,
                            ReturnType = returnType,
                            ArgumentTypes = argumentList,
                            LocalTypes = localList
                        };
                    }
                    else if (keyword.Type == KeywordType.Call)
                    {
                        var callExpression = new CallExpression();
                        callExpression.ReturnType = GetDatatypeFromToken(_tokens[_position++]);

                        if (_tokens[_position] is Symbol symbol && symbol.Type == SymbolType.RuntimeReference)
                        {
                            callExpression.Runtime = true;
                            _position++;
                        }

                        callExpression.Name = (Identifier)_tokens[_position++];
                        callExpression.ParameterTypes = GetDatatypesFromTokens(ReadParenthesisBracketList());

                        currentFunction.TreeObjects.Add(callExpression);
                    }
                    else if (keyword.Type == KeywordType.Return)
                    {
                        currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Return });
                        TreeObjects.Add(currentFunction);
                        currentFunction = null;
                    }


                }

                else
                    _position++;
            }
        }
    }
}