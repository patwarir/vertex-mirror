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
            Conditional currentConditional = null;

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

                        if (currentConditional != null)
                            currentConditional.Expressions.Add(callExpression);
                        else
                            currentFunction.TreeObjects.Add(callExpression);
                    }
                    // Return cannot be inside an if block.
                    else if (keyword.Type == KeywordType.Return)
                    {
                        currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Return });
                        TreeObjects.Add(currentFunction);
                        currentFunction = null;
                    }

                    else if (keyword.Type == KeywordType.Load)
                    {
                        _position++;

                        var type = (Keyword)_tokens[_position];
                        var loadSetExpression = new LoadSetExpression
                        {
                            LoadSet = KeywordType.Load
                        };

                        if (type.Type == KeywordType.Boolean)
                        {
                            loadSetExpression.Type = KeywordType.Boolean;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Boolean, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Integer)
                        {
                            loadSetExpression.Type = KeywordType.Integer;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Float)
                        {
                            loadSetExpression.Type = KeywordType.Float;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Float, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.String)
                        {
                            loadSetExpression.Type = KeywordType.String;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.String, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Local)
                        {
                            loadSetExpression.Type = KeywordType.Local;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Argument)
                        {
                            loadSetExpression.Type = KeywordType.Argument;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }

                        if (currentConditional != null)
                            currentConditional.Expressions.Add(loadSetExpression);
                        else
                            currentFunction.TreeObjects.Add(loadSetExpression);

                        _position++;
                    }
                    else if (keyword.Type == KeywordType.Set)
                    {
                        _position++;

                        var type = (Keyword)_tokens[_position];
                        var loadSetExpression = new LoadSetExpression
                        {
                            LoadSet = KeywordType.Set
                        };

                        if (type.Type == KeywordType.Boolean)
                        {
                            loadSetExpression.Type = KeywordType.Boolean;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Boolean, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Integer)
                        {
                            loadSetExpression.Type = KeywordType.Integer;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Float)
                        {
                            loadSetExpression.Type = KeywordType.Float;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Float, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.String)
                        {
                            loadSetExpression.Type = KeywordType.String;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.String, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Local)
                        {
                            loadSetExpression.Type = KeywordType.Local;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }
                        else if (type.Type == KeywordType.Argument)
                        {
                            loadSetExpression.Type = KeywordType.Argument;
                            _position++;
                            loadSetExpression.Value = new Literal(Datatype.Integer, ((Literal)_tokens[_position]).Value);
                        }

                        if (currentConditional != null)
                            currentConditional.Expressions.Add(loadSetExpression);
                        else
                            currentFunction.TreeObjects.Add(loadSetExpression);

                        _position++;
                    }

                    else if (keyword.Type == KeywordType.Throw)
                    {
                        var throwExpression = new ThrowExpression();

                        if (_tokens[_position] is Symbol symbol && symbol.Type == SymbolType.RuntimeReference)
                        {
                            throwExpression.Runtime = true;
                            _position++;
                        }

                        throwExpression.Name = (Identifier)_tokens[_position++];
                        throwExpression.ParameterTypes = GetDatatypesFromTokens(ReadParenthesisBracketList());

                        if (currentConditional != null)
                            currentConditional.Expressions.Add(throwExpression);
                        else
                            currentFunction.TreeObjects.Add(throwExpression);
                    }

                    else if (keyword.Type == KeywordType.Add)
                    {
                        if (currentConditional != null)
                            currentConditional.Expressions.Add(new OperationExpression { Type = KeywordType.Add });
                        else
                            currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Add });
                    }
                    else if (keyword.Type == KeywordType.Subtract)
                    {
                        if (currentConditional != null)
                            currentConditional.Expressions.Add(new OperationExpression { Type = KeywordType.Subtract });
                        else
                            currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Subtract });
                    }
                    else if (keyword.Type == KeywordType.Multiply)
                    {
                        if (currentConditional != null)
                            currentConditional.Expressions.Add(new OperationExpression { Type = KeywordType.Multiply });
                        else
                            currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Multiply });
                    }
                    else if (keyword.Type == KeywordType.Divide)
                    {
                        if (currentConditional != null)
                            currentConditional.Expressions.Add(new OperationExpression { Type = KeywordType.Divide });
                        else
                            currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Divide });
                    }
                    else if (keyword.Type == KeywordType.Modulus)
                    {
                        if (currentConditional != null)
                            currentConditional.Expressions.Add(new OperationExpression { Type = KeywordType.Modulus });
                        else
                            currentFunction.TreeObjects.Add(new OperationExpression { Type = KeywordType.Modulus });
                    }

                    else if (keyword.Type == KeywordType.If)
                    {
                        _position++;

                        Conditional conditional = new Conditional();
                        var ifType = (Keyword)_tokens[_position];

                        // Nested if blocks are not allowed.
                        if (ifType.Type == KeywordType.Equal)
                        {
                            conditional.Type = KeywordType.Equal;

                            if (currentConditional != null)
                            {
                                currentFunction.TreeObjects.Add(currentConditional);
                                currentConditional = null;
                            }

                            currentConditional = conditional;
                        }
                        // Only if and else are supported, not elif.
                        else if (ifType.Type == KeywordType.Else)
                        {
                            conditional.Type = KeywordType.Else;

                            if (currentConditional != null)
                            {
                                currentFunction.TreeObjects.Add(currentConditional);
                                currentConditional = null;
                            }

                            currentConditional = conditional;
                        }
                        else if (ifType.Type == KeywordType.EndIf)
                        {
                            conditional.Type = KeywordType.EndIf;

                            currentFunction.TreeObjects.Add(currentConditional);
                            currentConditional = null;

                            currentFunction.TreeObjects.Add(new Conditional { Type = KeywordType.EndIf });
                        }
                    }
                }

                else
                    _position++;
            }
        }
    }
}