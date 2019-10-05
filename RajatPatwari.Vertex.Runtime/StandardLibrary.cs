using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RajatPatwari.Vertex.Runtime.StandardLibrary
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class VertexStandardLibraryFunctionAttribute : Attribute
    {
        public FunctionDeclaration Declaration { get; }

        public VertexStandardLibraryFunctionAttribute(string qualifiedName, Datatype @return, params Datatype[] parameters) =>
            Declaration = new FunctionDeclaration(qualifiedName, @return, parameters);
    }

    internal static class StandardLibraryCaller
    {
        private static readonly IList<(FunctionDeclaration declaration, MethodInfo implementation)> functions;

        static StandardLibraryCaller()
        {
            functions = new List<(FunctionDeclaration declaration, MethodInfo implementation)>();
            foreach (var method in typeof(StandardLibraryImplementation).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var stdAttribute = method.GetCustomAttribute<VertexStandardLibraryFunctionAttribute>()
                    ?? throw new InvalidOperationException(nameof(VertexStandardLibraryFunctionAttribute));
                functions.Add((stdAttribute.Declaration, method));
            }
        }

        public static (bool returns, object? value) CallFunction(FunctionDeclaration declaration, params object[] parameterValues)
        {
            foreach (var function in functions)
                if (function.declaration.Equals(declaration))
                {
                    var @return = function.implementation.Invoke(null, parameterValues);
                    if (function.declaration.Return != Datatype.Void && @return != null)
                        return (true, @return);
                    else
                        return (false, null);
                }

            throw new InvalidOperationException(nameof(declaration));
        }
    }

    internal static class StandardLibraryImplementation
    {
        #region Environment

        [VertexStandardLibraryFunction("std.env::exit", Datatype.Void)]
        public static void Exit() =>
            Environment.Exit(0);

        [VertexStandardLibraryFunction("std.env::date", Datatype.String)]
        public static string Date() =>
            DateTime.Now.ToShortDateString();

        [VertexStandardLibraryFunction("std.env::time", Datatype.String)]
        public static string Time() =>
            DateTime.Now.ToLongTimeString();

        #endregion

        #region Cast

        [VertexStandardLibraryFunction("std.cst::to_bl", Datatype.Boolean, Datatype.Integer)]
        public static bool ToBool(long value) =>
            value switch
            {
                0L => false,
                1L => true,
                _ => throw new ArgumentException(nameof(value))
            };

        [VertexStandardLibraryFunction("std.cst::to_bl", Datatype.Boolean, Datatype.String)]
        public static bool ToBool(string value) =>
            value switch
            {
                "false" => false,
                "true" => true,
                _ => throw new ArgumentException(nameof(value))
            };

        [VertexStandardLibraryFunction("std.cst::to_int", Datatype.Integer, Datatype.Boolean)]
        public static long ToInt(bool value) =>
            value switch
            {
                false => 0L,
                true => 1L
            };

        [VertexStandardLibraryFunction("std.cst::to_int", Datatype.Integer, Datatype.Float)]
        public static long ToInt(double value) =>
            (long)value;

        [VertexStandardLibraryFunction("std.cst::to_int", Datatype.Integer, Datatype.String)]
        public static long ToInt(string value) =>
            Convert.ToInt64(value);

        [VertexStandardLibraryFunction("std.cst::to_fl", Datatype.Float, Datatype.Integer)]
        public static double ToFloat(long value) =>
            value;

        [VertexStandardLibraryFunction("std.cst::to_fl", Datatype.Float, Datatype.String)]
        public static double ToFloat(string value) =>
            Convert.ToDouble(value);

        [VertexStandardLibraryFunction("std.cst::to_str", Datatype.String, Datatype.Boolean)]
        public static string ToStr(bool value) =>
            value switch
            {
                false => "false",
                true => "true"
            };

        [VertexStandardLibraryFunction("std.cst::to_str", Datatype.String, Datatype.Integer)]
        public static string ToStr(long value) =>
            value.ToString();

        [VertexStandardLibraryFunction("std.cst::to_str", Datatype.String, Datatype.Float)]
        public static string ToStr(double value) =>
            value.ToString();

        #endregion

        #region Operation

        // TODO

        #endregion

        #region Comparsion

        [VertexStandardLibraryFunction("std.cmp::eq", Datatype.Boolean, Datatype.Boolean, Datatype.Boolean)]
        public static bool Equal(bool value1, bool value2) =>
            value1 == value2;

        [VertexStandardLibraryFunction("std.cmp::eq", Datatype.Boolean, Datatype.Integer, Datatype.Integer)]
        public static bool Equal(long value1, long value2) =>
            value1 == value2;

        [VertexStandardLibraryFunction("std.cmp::eq", Datatype.Boolean, Datatype.Float, Datatype.Float)]
        public static bool Equal(double value1, double value2) =>
            value1 == value2;

        [VertexStandardLibraryFunction("std.cmp::eq", Datatype.Boolean, Datatype.String, Datatype.String)]
        public static bool Equal(string value1, string value2) =>
            value1 == value2;

        [VertexStandardLibraryFunction("std.cmp::gt", Datatype.Boolean, Datatype.Integer, Datatype.Integer)]
        public static bool Greater(long value1, long value2) =>
            value1 > value2;

        [VertexStandardLibraryFunction("std.cmp::gt", Datatype.Boolean, Datatype.Float, Datatype.Float)]
        public static bool Greater(double value1, double value2) =>
            value1 > value2;

        [VertexStandardLibraryFunction("std.cmp::lt", Datatype.Boolean, Datatype.Integer, Datatype.Integer)]
        public static bool Less(long value1, long value2) =>
            value1 < value2;

        [VertexStandardLibraryFunction("std.cmp::lt", Datatype.Boolean, Datatype.Float, Datatype.Float)]
        public static bool Less(double value1, double value2) =>
            value1 < value2;

        [VertexStandardLibraryFunction("std.cmp::ge", Datatype.Boolean, Datatype.Integer, Datatype.Integer)]
        public static bool GreaterEquals(long value1, long value2) =>
            value1 >= value2;

        [VertexStandardLibraryFunction("std.cmp::ge", Datatype.Boolean, Datatype.Float, Datatype.Float)]
        public static bool GreaterEquals(double value1, double value2) =>
            value1 >= value2;

        [VertexStandardLibraryFunction("std.cmp::le", Datatype.Boolean, Datatype.Integer, Datatype.Integer)]
        public static bool LessEquals(long value1, long value2) =>
            value1 <= value2;

        [VertexStandardLibraryFunction("std.cmp::le", Datatype.Boolean, Datatype.Float, Datatype.Float)]
        public static bool LessEquals(double value1, double value2) =>
            value1 <= value2;

        #endregion

        #region String Functions

        [VertexStandardLibraryFunction("std.sfn::len", Datatype.Integer, Datatype.String)]
        public static long Length(string value) =>
            value.Length;

        [VertexStandardLibraryFunction("std.sfn::char", Datatype.String, Datatype.String, Datatype.Integer)]
        public static string Char(string value1, long value2) =>
            value1[(int)value2].ToString();

        [VertexStandardLibraryFunction("std.sfn::concat", Datatype.String, Datatype.String, Datatype.String)]
        public static string Concat(string value1, string value2) =>
            value1 + value2;

        [VertexStandardLibraryFunction("std.sfn::sub", Datatype.String, Datatype.String, Datatype.Integer)]
        public static string Substring(string value1, long value2) =>
            value1.Substring((int)value2);

        [VertexStandardLibraryFunction("std.sfn::sub", Datatype.String, Datatype.String, Datatype.Integer, Datatype.Integer)]
        public static string Substring(string value1, long value2, long value3) =>
            value1.Substring((int)value2, (int)value3);

        [VertexStandardLibraryFunction("std.sfn::rem", Datatype.String, Datatype.String, Datatype.Integer)]
        public static string Remove(string value1, long value2) =>
            value1.Remove((int)value2);

        [VertexStandardLibraryFunction("std.sfn::rem", Datatype.String, Datatype.String, Datatype.Integer, Datatype.Integer)]
        public static string Remove(string value1, long value2, long value3) =>
            value1.Remove((int)value2, (int)value3);

        #endregion

        #region Math

        // TODO

        #endregion

        #region Error

        [VertexStandardLibraryFunction("std.err::inv_op", Datatype.Void)]
        public static void InvalidOperation() =>
            throw new InvalidOperationException();

        [VertexStandardLibraryFunction("std.err::inv_op", Datatype.Void, Datatype.String)]
        public static void InvalidOperation(string value) =>
            throw new InvalidOperationException(value);

        [VertexStandardLibraryFunction("std.err::arg", Datatype.Void, Datatype.String)]
        public static void Argument(string value) =>
            throw new ArgumentException(value);

        [VertexStandardLibraryFunction("std.err::arg_range", Datatype.Void, Datatype.String)]
        public static void ArgumentRange(string value) =>
            throw new ArgumentOutOfRangeException(value);

        #endregion

        #region System IO

        [VertexStandardLibraryFunction("std.sio::ln_str", Datatype.String)]
        public static string LineString() =>
            Environment.NewLine;

        [VertexStandardLibraryFunction("std.sio::clear", Datatype.Void)]
        public static void Clear() =>
            Console.Clear();

        [VertexStandardLibraryFunction("std.sio::read", Datatype.Void)]
        public static void Read() =>
            Console.ReadKey();

        [VertexStandardLibraryFunction("std.sio::readln", Datatype.String)]
        public static string ReadLine() =>
            Console.ReadLine();

        [VertexStandardLibraryFunction("std.sio::write", Datatype.Void, Datatype.String)]
        public static void Write(string value) =>
            Console.Write(value);

        [VertexStandardLibraryFunction("std.sio::writeln", Datatype.Void, Datatype.String)]
        public static void WriteLine(string value) =>
            Console.WriteLine(value);

        #endregion
    }
}