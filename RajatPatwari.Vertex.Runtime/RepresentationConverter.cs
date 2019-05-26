using RajatPatwari.Vertex.Runtime.AbstractSyntaxTree;
using RajatPatwari.Vertex.Runtime.Representation;
using RajatPatwari.Vertex.Runtime.Token;
using System;
using System.Collections.Generic;

namespace RajatPatwari.Vertex.Runtime
{
    public sealed class RepresentationConverter
    {
        private bool _ran = false;

        private readonly IList<ITreeObject> _treeObjects;

        public IList<FunctionRepresentation> FunctionRepresentations { get; } = new List<FunctionRepresentation>();

        public RepresentationConverter(IList<ITreeObject> treeObjects) =>
            _treeObjects = treeObjects;

        private Function GetMain()
        {
            foreach (var treeObject in _treeObjects)
                if (treeObject is Function function && function.Name.Value == "main" && function.ReturnType == Datatype.Void)
                    return function;
            throw new InvalidOperationException($"No \"main\" {nameof(Function)}!");
        }

        private IList<Function> GetNonMainFunctions()
        {
            var nonMainFunctions = new List<Function>();
            foreach (var treeObject in _treeObjects)
                if (treeObject is Function function && function.Name.Value != "main")
                    nonMainFunctions.Add(function);
            return nonMainFunctions;
        }

        private void ConvertFunction(FunctionRepresentation functionRepresentation)
        {
            foreach (var treeObject in functionRepresentation.Function.TreeObjects)
            {
                if (treeObject is Conditional conditional)
                {
                    if (conditional.Type == KeywordType.Equal)
                    {
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Equal);
                        functionRepresentation.Conditionals.Add(new ConditionalRepresentation(conditional,
                            functionRepresentation.Buffer.Stream.Count - 1));
                    }
                    else if (conditional.Type == KeywordType.Else)
                    {
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Else);
                        functionRepresentation.Conditionals.Add(new ConditionalRepresentation(conditional,
                            functionRepresentation.Buffer.Stream.Count - 1));
                    }
                    else if (conditional.Type == KeywordType.EndIf)
                    {
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.EndIf);
                        functionRepresentation.Conditionals.Add(new ConditionalRepresentation(conditional,
                            functionRepresentation.Buffer.Stream.Count - 1));
                    }

                    foreach (var expression in conditional.Expressions)
                    {
                        if (expression is CallExpression callExpresssion)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.Call);
                            functionRepresentation.Buffer.WriteOperationType(callExpresssion.ReturnType);
                            functionRepresentation.Buffer.WriteString($"{(callExpresssion.Runtime ? "$" : string.Empty)}{callExpresssion.Name}");
                            functionRepresentation.Buffer.WriteOperationTypes(callExpresssion.ParameterTypes);
                        }
                        else if (expression is OperationExpression operationExpression)
                        {
                            if (operationExpression.Type == KeywordType.Return)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Return);

                            else if (operationExpression.Type == KeywordType.Add)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Add);
                            else if (operationExpression.Type == KeywordType.Subtract)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Subtract);
                            else if (operationExpression.Type == KeywordType.Multiply)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Multiply);
                            else if (operationExpression.Type == KeywordType.Divide)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Divide);
                            else if (operationExpression.Type == KeywordType.Modulus)
                                functionRepresentation.Buffer.WriteOperationCode(OperationCode.Modulus);
                        }
                        else if (expression is ThrowExpression throwExpression)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.Throw);
                            functionRepresentation.Buffer.WriteString($"{(throwExpression.Runtime ? "$" : string.Empty)}{throwExpression.Name}");
                            functionRepresentation.Buffer.WriteOperationTypes(throwExpression.ParameterTypes);
                        }
                        else if (expression is LoadSetExpression loadSetExpression)
                        {
                            if (loadSetExpression.LoadSet == KeywordType.Load)
                            {
                                if (loadSetExpression.Type == KeywordType.Boolean)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadBoolean);
                                    functionRepresentation.Buffer.WriteBoolean((bool)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.Integer)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadInteger);
                                    functionRepresentation.Buffer.WriteInteger((long)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.Float)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadFloat);
                                    functionRepresentation.Buffer.WriteFloat((double)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.String)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadString);
                                    functionRepresentation.Buffer.WriteString((string)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.Argument)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadArgument);
                                    functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.Local)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadLocal);
                                    functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                                }
                            }
                            else if (loadSetExpression.LoadSet == KeywordType.Set)
                            {
                                if (loadSetExpression.Type == KeywordType.Argument)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.SetArgument);
                                    functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                                }
                                else if (loadSetExpression.Type == KeywordType.Local)
                                {
                                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.SetLocal);
                                    functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                                }
                            }
                        }
                    }
                }
                else if (treeObject is CallExpression call)
                {
                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.Call);
                    functionRepresentation.Buffer.WriteOperationType(call.ReturnType);
                    functionRepresentation.Buffer.WriteString($"{(call.Runtime ? "$" : string.Empty)}{call.Name}");
                    functionRepresentation.Buffer.WriteOperationTypes(call.ParameterTypes);
                }
                else if (treeObject is OperationExpression operationExpression)
                {
                    if (operationExpression.Type == KeywordType.Return)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Return);

                    else if (operationExpression.Type == KeywordType.Add)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Add);
                    else if (operationExpression.Type == KeywordType.Subtract)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Subtract);
                    else if (operationExpression.Type == KeywordType.Multiply)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Multiply);
                    else if (operationExpression.Type == KeywordType.Divide)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Divide);
                    else if (operationExpression.Type == KeywordType.Modulus)
                        functionRepresentation.Buffer.WriteOperationCode(OperationCode.Modulus);
                }
                else if (treeObject is ThrowExpression throwExpression)
                {
                    functionRepresentation.Buffer.WriteOperationCode(OperationCode.Throw);
                    functionRepresentation.Buffer.WriteString($"{(throwExpression.Runtime ? "$" : string.Empty)}{throwExpression.Name}");
                    functionRepresentation.Buffer.WriteOperationTypes(throwExpression.ParameterTypes);
                }
                else if (treeObject is LoadSetExpression loadSetExpression)
                {
                    if (loadSetExpression.LoadSet == KeywordType.Load)
                    {
                        if (loadSetExpression.Type == KeywordType.Boolean)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadBoolean);
                            functionRepresentation.Buffer.WriteBoolean((bool)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.Integer)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadInteger);
                            functionRepresentation.Buffer.WriteInteger((long)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.Float)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadFloat);
                            functionRepresentation.Buffer.WriteFloat((double)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.String)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadString);
                            functionRepresentation.Buffer.WriteString((string)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.Argument)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadArgument);
                            functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.Local)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.LoadLocal);
                            functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                        }
                    }
                    else if (loadSetExpression.LoadSet == KeywordType.Set)
                    {
                        if (loadSetExpression.Type == KeywordType.Argument)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.SetArgument);
                            functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                        }
                        else if (loadSetExpression.Type == KeywordType.Local)
                        {
                            functionRepresentation.Buffer.WriteOperationCode(OperationCode.SetLocal);
                            functionRepresentation.Buffer.WriteByte((byte)(long)loadSetExpression.Value.Value);
                        }
                    }
                }
            }

            FunctionRepresentations.Add(functionRepresentation);
        }

        public void Run()
        {
            if (_ran)
                throw new InvalidOperationException($"Attempting to run the {nameof(RepresentationConverter)} more than once!");
            _ran = true;

            ConvertFunction(new FunctionRepresentation { Function = GetMain() });

            foreach (var function in GetNonMainFunctions())
                ConvertFunction(new FunctionRepresentation { Function = function });
        }
    }
}