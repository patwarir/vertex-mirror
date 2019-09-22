using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RajatPatwari.Vertex.Runtime
{
    public static class StandardLibrary
    {
        private static readonly IList<Package> packages = new List<Package>();

        private static void SetupEnvironment() =>
            packages.Add(new Package("std.env")
            {
                new Function("exit", Datatype.Void),
                new Function("pause", Datatype.Void),
                new Function("date", Datatype.String),
                new Function("time", Datatype.String)
            });

        private static void SetupStringFunctions()
        {
            var sfn = new Package("std.sfn");

            var len = new Function("len", Datatype.MediumSigned);
            len.Parameters.Append(new Scalar(Datatype.String, string.Empty));
            sfn.Add(len);

            var sub = new Function("sub", Datatype.String);
            sub.Parameters.Append(new Scalar(Datatype.String, string.Empty));
            sub.Parameters.Append(new Scalar(Datatype.MediumSigned, default(int)));
            sfn.Add(sub);

            var rem = new Function("rem", Datatype.String);
            rem.Parameters.Append(new Scalar(Datatype.String, string.Empty));
            rem.Parameters.Append(new Scalar(Datatype.MediumSigned, default(int)));
            sfn.Add(rem);

            packages.Add(sfn);
        }

        private static void SetupOperations()
        {
            packages.Add(new Package("std.op"));
            // TODO
        }

        private static void SetupMath()
        {
            packages.Add(new Package("std.math"));
            // TODO
        }

        private static void SetupComparisons()
        {
            packages.Add(new Package("std.cmp"));
            // TODO
        }

        private static void SetupInputOutput()
        {
            var io = new Package("std.io")
            {
                new Function("ln_str", Datatype.String),
                new Function("clear", Datatype.Void),
                new Function("read", Datatype.Void),
                new Function("readln", Datatype.String)
            };

            var write = new Function("write", Datatype.Void);
            write.Parameters.Append(new Scalar(Datatype.String, string.Empty));
            io.Add(write);

            var writeln = new Function("writeln", Datatype.Void);
            writeln.Parameters.Append(new Scalar(Datatype.String, string.Empty));
            io.Add(writeln);

            packages.Add(io);
        }

        private static void SetupExceptions()
        {
            packages.Add(new Package("std.ex"));
            // TODO
        }

        static StandardLibrary()
        {
            SetupEnvironment();
            SetupStringFunctions();
            SetupOperations();
            SetupMath();
            SetupComparisons();
            SetupInputOutput();
            SetupExceptions();
        }

        private static (string packageName, Function function) FindBySignature(string qualifiedName, Datatype @return, in IList<Datatype> parameters)
        {
            var (packageName, functionName) = Package.ParseQualifiedName(qualifiedName);
            return (packageName, packages.First(package => package.Name == packageName).FindBySignature(functionName, @return, parameters));
        }

        private static bool CheckType(in Type type, Datatype datatype) =>
            datatype switch
            {
                Datatype.Void => type == typeof(void),

                Datatype.Boolean => type == typeof(bool),

                Datatype.TinySigned => type == typeof(sbyte),
                Datatype.TinyUnsigned => type == typeof(byte),

                Datatype.ShortSigned => type == typeof(short),
                Datatype.ShortUnsigned => type == typeof(ushort),

                Datatype.MediumSigned => type == typeof(int),
                Datatype.MediumUnsigned => type == typeof(uint),

                Datatype.LongSigned => type == typeof(long),
                Datatype.LongUnsigned => type == typeof(ulong),

                Datatype.FloatSingle => type == typeof(float),
                Datatype.FloatDouble => type == typeof(double),

                Datatype.Character => type == typeof(char),
                Datatype.String => type == typeof(string),

                _ => false
            };

        private static bool CheckParameterTypes(in IEnumerable<ParameterInfo> parameterTypes, in IEnumerable<Datatype> parameterDatatypes)
        {
            if (parameterTypes.Count() != parameterDatatypes.Count())
                return false;

            for (var index = 0; index < parameterTypes.Count(); index++)
                if (!CheckType(parameterTypes.ElementAt(index).ParameterType, parameterDatatypes.ElementAt(index)))
                    return false;

            return true;
        }

        private static (bool @return, object? value) CallStandardLibraryFunction(in (string packageName, Function function) wrapper, in ScalarList parameters)
        {
            foreach (var innerClass in typeof(StandardLibraryImpl).GetNestedTypes())
                if (innerClass.GetCustomAttribute<VertexPackageAttribute>()?.Name == wrapper.packageName)
                    foreach (var method in innerClass.GetMethods())
                        if (method.GetCustomAttribute<VertexFunctionAttribute>()?.Name == wrapper.function.Name
                            && CheckType(method.ReturnType, wrapper.function.Return)
                            && CheckParameterTypes(method.GetParameters(), wrapper.function.Parameters.GetDatatypes()))
                        {
                            var @return = method.Invoke(null, parameters.GetValues().ToArray());
                            if (@return != null && wrapper.function.Return != Datatype.Void)
                                return (true, @return);
                            else
                                return (false, null);
                        }

            throw new InvalidOperationException();
        }

        public static ScalarList Flatten(in Stack<Scalar> stack, byte numberParameters)
        {
            var list = new ScalarList(true);
            while (stack.Count > 0 && numberParameters > 0)
            {
                list.Prepend(stack.Pop());
                numberParameters--;
            }
            return list;
        }

        public static (bool @return, object? value) FindAndCall(string qualifiedName, Datatype @return, in IList<Datatype> parameterDatatypes, in ScalarList parameters) =>
            CallStandardLibraryFunction(FindBySignature(qualifiedName, @return, parameterDatatypes), parameters);
    }

    #region Attributes

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class VertexPackageAttribute : Attribute
    {
        public string Name { get; }

        public VertexPackageAttribute(string name) =>
            Name = name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class VertexFunctionAttribute : Attribute
    {
        public string Name { get; }

        public VertexFunctionAttribute(string name) =>
            Name = name;
    }

    #endregion

    internal static class StandardLibraryImpl
    {
        [VertexPackage("std.env")]
        public static class Environment
        {
            [VertexFunction("exit")]
            public static void Exit() =>
                System.Environment.Exit(0);

            [VertexFunction("pause")]
            public static void Pause() =>
                Thread.Sleep(TimeSpan.FromMilliseconds(System.Math.Pow(2, 30)));

            [VertexFunction("date")]
            public static string Date() =>
                DateTime.Now.ToShortDateString();

            [VertexFunction("time")]
            public static string Time() =>
                DateTime.Now.ToLongTimeString();
        }

        [VertexPackage("std.sfn")]
        public static class StringFunctions
        {
            [VertexFunction("len")]
            public static int Length(string str) =>
                str.Length;

            [VertexFunction("sub")]
            public static string Substring(string str, int start) =>
                str.Substring(start);

            [VertexFunction("rem")]
            public static string Remove(string str, int start) =>
                str.Remove(start);
        }

        [VertexPackage("std.op")]
        public static class Operations
        {
            // TODO
        }

        [VertexPackage("std.math")]
        public static class Math
        {
            // TODO
        }

        [VertexPackage("std.cmp")]
        public static class Comparisons
        {
            // TODO
        }

        [VertexPackage("std.io")]
        public static class InputOutput
        {
            [VertexFunction("ln_str")]
            public static string LineString() =>
                System.Environment.NewLine;

            [VertexFunction("clear")]
            public static void Clear() =>
                Console.Clear();

            [VertexFunction("read")]
            public static void Read() =>
                Console.ReadKey();

            [VertexFunction("readln")]
            public static string ReadLine() =>
                Console.ReadLine();

            [VertexFunction("write")]
            public static void Write(string str) =>
                Console.Write(str);

            [VertexFunction("writeln")]
            public static void WriteLine(string str) =>
                Console.WriteLine(str);
        }

        [VertexPackage("std.ex")]
        public static class Exceptions
        {
            // TODO
        }
    }
}