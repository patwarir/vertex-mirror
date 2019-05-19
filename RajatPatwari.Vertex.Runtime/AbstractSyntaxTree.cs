using RajatPatwari.Vertex.Runtime.Token;
using System.Collections.Generic;
using System.Text;

namespace RajatPatwari.Vertex.Runtime.AbstractSyntaxTree
{
    public interface ITreeObject
    { }

    public abstract class Expression : ITreeObject
    { }

    public sealed class CallExpression : Expression
    {
        public Identifier Name { get; set; }

        public bool Runtime { get; set; } = false;

        public Datatype ReturnType { get; set; }

        public IList<Datatype> ParameterTypes { get; set; } = new List<Datatype>();

        public override string ToString()
        {
            var returnString = new StringBuilder($"{(Runtime ? "$" : string.Empty)}{Name}|{ReturnType}|");
            foreach (var parameter in ParameterTypes)
                returnString.Append($"{parameter},");
            return returnString.Append("|").ToString();
        }
    }

    public sealed class LoadSetExpression : Expression
    {
        public KeywordType LoadSet { get; set; }

        public KeywordType Type { get; set; }

        public Literal Value { get; set; }

        public override string ToString() =>
            $"{LoadSet}|{Type}|{Value}";
    }

    public sealed class ThrowExpression : Expression
    {
        public Identifier Name { get; set; }

        public bool Runtime { get; set; }

        public IList<Datatype> ParameterTypes { get; set; } = new List<Datatype>();

        public override string ToString()
        {
            var returnString = new StringBuilder($"{(Runtime ? "$" : string.Empty)}{Name}|");
            foreach (var parameter in ParameterTypes)
                returnString.Append($"{parameter},");
            return returnString.Append("|").ToString();
        }
    }

    public sealed class OperationExpression : Expression // ret or operation
    {
        public KeywordType Type { get; set; }

        public override string ToString() =>
            Type.ToString();
    }

    public sealed class Conditional : ITreeObject // equal, else or endif
    {
        public KeywordType Type { get; set; }

        public IList<Expression> Expressions { get; set; } = new List<Expression>();

        public override string ToString() =>
            Type.ToString();
    }

    public sealed class Function : ITreeObject
    {
        public Identifier Name { get; set; }

        public Datatype ReturnType { get; set; }

        public IList<Datatype> ArgumentTypes { get; set; } = new List<Datatype>();

        public IList<Datatype> LocalTypes { get; set; } = new List<Datatype>();

        public IList<ITreeObject> TreeObjects { get; set; } = new List<ITreeObject>();

        public override string ToString()
        {
            var returnString = new StringBuilder($"{Name}|{ReturnType}|");

            foreach (var argument in ArgumentTypes)
                returnString.Append($"{argument},");
            returnString.Append("|");

            foreach (var local in LocalTypes)
                returnString.Append($"{local},");
            return returnString.Append("|").ToString();
        }
    }
}