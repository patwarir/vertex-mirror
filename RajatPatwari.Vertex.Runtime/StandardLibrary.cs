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
            len.Parameters.Add(new Scalar(Datatype.String, string.Empty));
            sfn.Add(len);

            var sub = new Function("sub", Datatype.String);
            sub.Parameters.Add(new Scalar(Datatype.String, string.Empty));
            sub.Parameters.Add(new Scalar(Datatype.MediumSigned, default(int)));
            sfn.Add(sub);

            var rem = new Function("rem", Datatype.String);
            rem.Parameters.Add(new Scalar(Datatype.String, string.Empty));
            rem.Parameters.Add(new Scalar(Datatype.MediumSigned, default(int)));
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
                new Function("read", Datatype.Void),
                new Function("readln", Datatype.String)
            };

            var write = new Function("write", Datatype.Void);
            write.Parameters.Add(new Scalar(Datatype.String, string.Empty));
            io.Add(write);

            var writeln = new Function("writeln", Datatype.Void);
            writeln.Parameters.Add(new Scalar(Datatype.String, string.Empty));
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

        private static (string packageName, Function function) FindBySignature(string qualifiedName, Datatype @return, IList<Datatype> parameters)
        {
            string packageName = qualifiedName.Remove(qualifiedName.LastIndexOf("::")),
                functionName = qualifiedName.Substring(qualifiedName.LastIndexOf("::") + 2);
            return (packageName, packages.First(package => package.Name == packageName).FindBySignature(functionName, @return, parameters));
        }

        private static bool CheckType(Type type, Datatype datatype) =>
            datatype switch
            {
                // TODO: Switch these to typeof(void), etc.
                Datatype.Void => type.FullName == "System.Void",

                Datatype.Boolean => type.FullName == "System.Boolean",

                Datatype.TinySigned => type.FullName == "System.SByte",
                Datatype.TinyUnsigned => type.FullName == "System.Byte",

                Datatype.ShortSigned => type.FullName == "System.Int16",
                Datatype.ShortUnsigned => type.FullName == "System.UInt16",

                Datatype.MediumSigned => type.FullName == "System.Int32",
                Datatype.MediumUnsigned => type.FullName == "System.UInt32",

                Datatype.LongSigned => type.FullName == "System.Int64",
                Datatype.LongUnsigned => type.FullName == "System.UInt64",

                Datatype.FloatSingle => type.FullName == "System.Single",
                Datatype.FloatDouble => type.FullName == "System.Double",

                Datatype.Character => type.FullName == "System.Char",
                Datatype.String => type.FullName == "System.String",

                _ => false
            };

        private static bool CheckParameterTypes(IEnumerable<ParameterInfo> parameterTypes, IEnumerable<Datatype> parameterDatatypes)
        {
            if (parameterTypes.Count() != parameterDatatypes.Count())
                return false;

            for (var i = 0; i < parameterTypes.Count(); i++)
                if (!CheckType(parameterTypes.ElementAt(i).ParameterType, parameterDatatypes.ElementAt(i)))
                    return false;

            return true;
        }

        private static (bool @return, object? value) CallStandardLibraryFunction((string packageName, Function function) wrapper)
        {
            foreach (var innerClass in typeof(StandardLibraryImpl).GetNestedTypes())
                if (innerClass.GetCustomAttribute<VertexPackageAttribute>()?.Name == wrapper.packageName)
                    foreach (var method in innerClass.GetMethods())
                        if (method.GetCustomAttribute<VertexFunctionAttribute>()?.Name == wrapper.function.Name
                            && CheckType(method.ReturnType, wrapper.function.Return)
                            && CheckParameterTypes(method.GetParameters(), wrapper.function.Parameters.GetDatatypes()))
                        {
                            // TODO: Return stuff from methods if possible.
                            Console.WriteLine("Found: " + method);
                            return (false, null);
                        }

            throw new InvalidOperationException();
        }

        public static (bool @return, object? value) FindAndCall(string qualifiedName, Datatype @return, IList<Datatype> parameters) =>
            CallStandardLibraryFunction(FindBySignature(qualifiedName, @return, parameters));
    }

    #region Attributes

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VertexPackageAttribute : Attribute
    {
        public string Name { get; }

        public VertexPackageAttribute(string name) =>
            Name = name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class VertexFunctionAttribute : Attribute
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
                Thread.Sleep(TimeSpan.MaxValue);

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

            [VertexFunction("read")]
            public static void Read() =>
                Console.Read();

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