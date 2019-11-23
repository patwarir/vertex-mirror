using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RajatPatwari.Vertex.Runtime
{
    public static class StandardLibrary
    {
        private static readonly IList<Package> packages = new List<Package>();

        private static Function MakeStdFunc(string name, Datatype @return, IEnumerable<Datatype> parameters, Delegate @delegate)
        {
            var function = new Function(name, true)
            {
                Return = @return,
                Delegate = @delegate
            };
            foreach (var parameter in parameters)
                function.Parameters.Append(parameter);
            return function;
        }

        private static Function MakeStdFunc(string name, Delegate @delegate)
        {
            static Datatype GetDatatypeFromType(Type type) =>
                type.Name switch
                {
                    "Void" => Datatype.Void,
                    "Boolean" => Datatype.Boolean,
                    "Int64" => Datatype.Integer,
                    "Double" => Datatype.Float,
                    "String" => Datatype.String,
                    _ => throw new ArgumentException(nameof(type))
                };

            var @return = GetDatatypeFromType(@delegate.Method.ReturnType);
            var parameters = @delegate.Method.GetParameters().Select(parameter => GetDatatypeFromType(parameter.ParameterType));
            return MakeStdFunc(name, @return, parameters, @delegate);
        }

        private static void AddStdPkg(string name, (string name, Delegate @delegate)[] funcs)
        {
            var package = new Package($"std.{name}", true);
            foreach (var func in funcs)
                package.Functions.Add(MakeStdFunc(func.name, func.@delegate));
            packages.Add(package);
        }

        static StandardLibrary()
        {
            AddStdPkg("env", new (string, Delegate)[]
            {
                ("exit", (Action)(() => Environment.Exit(0))),
                ("date", (Func<string>)DateTime.Now.ToShortDateString),
                ("time", (Func<string>)DateTime.Now.ToLongTimeString)
            });

            AddStdPkg("cast", new (string, Delegate)[]
            {
                ("to_bl", (Func<long, bool>)(value => value switch
                {
                    0L => false,
                    1L => true,
                    _ => throw new ArgumentException(nameof(value))
                })),
                ("to_bl", (Func<double, bool>)(value => value switch
                {
                    0.0 => false,
                    1.0 => true,
                    _ => throw new ArgumentException(nameof(value))
                })),
                ("to_bl", (Func<string, bool>)(value => value switch
                {
                    "false" => false,
                    "true" => true,
                    _ => throw new ArgumentException(nameof(value))
                })),
                ("to_int", (Func<bool, long>)(value => value switch
                {
                    false => 0L,
                    true => 1L
                })),
                ("to_int", (Func<double, long>)(value => (long)value)),
                ("to_int", (Func<string, long>)long.Parse),
                ("to_fl", (Func<bool, double>)(value => value switch
                {
                    false => 0.0,
                    true => 1.0
                })),
                ("to_fl", (Func<long, double>)(value => value)),
                ("to_fl", (Func<string, double>)double.Parse),
                ("to_str", (Func<bool, string>)(value => value.ToString().ToLower())),
                ("to_str", (Func<long, string>)(value => value.ToString())),
                ("to_str", (Func<double, string>)(value => value.ToString()))
            });

            AddStdPkg("opr", new (string, Delegate)[]
            {
                ("neg", (Func<bool, bool>)(value => !value)),
                ("neg", (Func<long, long>)(value => -value)),
                ("neg", (Func<double, double>)(value => -value)),
                ("to_dec", (Func<string, long>)(value => long.Parse(value, System.Globalization.NumberStyles.HexNumber))),
                ("to_hex", (Func<long, string>)(value => value.ToString("X"))),
                ("add", (Func<long, long, long>)((value1, value2) => value1 + value2)),
                ("add", (Func<double, double, double>)((value1, value2) => value1 + value2)),
                ("sub", (Func<long, long, long>)((value1, value2) => value1 - value2)),
                ("sub", (Func<double, double, double>)((value1, value2) => value1 - value2)),
                ("mul", (Func<long, long, long>)((value1, value2) => value1 * value2)),
                ("mul", (Func<double, double, double>)((value1, value2) => value1 * value2)),
                ("div", (Func<long, long, long>)((value1, value2) => value1 / value2)),
                ("div", (Func<double, double, double>)((value1, value2) => value1 / value2)),
                ("mod", (Func<long, long, long>)((value1, value2) => value1 % value2)),
                ("mod", (Func<double, double, double>)((value1, value2) => value1 % value2)),
                ("pwb", (Func<long, long, double>)((value1, value2) => Math.Pow(value1, value2))),
                ("pwb", (Func<double, double, double>)((value1, value2) => Math.Pow(value1, value2))),
                ("rtb", (Func<long, long, double>)((value1, value2) => Math.Pow(value1, 1.0 / value2))),
                ("rtb", (Func<double, double, double>)((value1, value2) => Math.Pow(value1, 1.0 / value2))),
                ("lgb", (Func<long, long, double>)((value1, value2) => Math.Log(value2, value1))),
                ("lgb", (Func<double, double, double>)((value1, value2) => Math.Log(value2, value1)))
            });

            AddStdPkg("cmp", new (string, Delegate)[]
            {
                ("eq", (Func<bool, bool, bool>)EqualityComparer<bool>.Default.Equals),
                ("eq", (Func<long, long, bool>)EqualityComparer<long>.Default.Equals),
                ("eq", (Func<double, double, bool>)EqualityComparer<double>.Default.Equals),
                ("eq", (Func<string, string, bool>)EqualityComparer<string>.Default.Equals),
                ("gt", (Func<long, long, bool>)((value1, value2) => value1 > value2)),
                ("gt", (Func<double, double, bool>)((value1, value2) => value1 > value2)),
                ("lt", (Func<long, long, bool>)((value1, value2) => value1 < value2)),
                ("lt", (Func<double, double, bool>)((value1, value2) => value1 < value2)),
                ("ge", (Func<long, long, bool>)((value1, value2) => value1 >= value2)),
                ("ge", (Func<double, double, bool>)((value1, value2) => value1 >= value2)),
                ("le", (Func<long, long, bool>)((value1, value2) => value1 <= value2)),
                ("le", (Func<double, double, bool>)((value1, value2) => value1 <= value2))
            });

            AddStdPkg("sfn", new (string, Delegate)[]
            {
                ("len", (Func<string, long>)(value => value.Length)),
                ("char", (Func<string, long, string>)((value, index) => value[(int)index].ToString())),
                ("is_emp", (Func<string, bool>)string.IsNullOrWhiteSpace),
                ("cat", (Func<string, string, string>)string.Concat),
                ("rep", (Func<string, long, string>)((value, times) => new string(value[0], (int)times))),
                ("sub", (Func<string, long, string>)((value, index) => value.Substring((int)index))),
                ("sub", (Func<string, long, long, string>)((value, index, length) => value.Substring((int)index, (int)length))),
                ("rem", (Func<string, long, string>)((value, index) => value.Remove((int)index))),
                ("rem", (Func<string, long, long, string>)((value, index, length) => value.Remove((int)index, (int)length)))
            });

            AddStdPkg("math", new (string, Delegate)[]
            {
                // TODO: Add in the functions for the math package.
            });

            AddStdPkg("err", new (string, Delegate)[]
            {
                ("inv_op", (Action)(() => throw new InvalidOperationException())),
                ("inv_op", (Action<string>)(value => throw new InvalidOperationException(value))),
                ("arg", (Action<string>)(value => throw new ArgumentException(value))),
                ("arg_range", (Action<string>)(value => throw new ArgumentOutOfRangeException(value)))
            });
            
            AddStdPkg("io", new (string, Delegate)[]
            {
                ("ln_str", (Func<string>)(() => Environment.NewLine)),
                ("clear", (Action)Console.Clear),
                ("read", (Action)(() => Console.ReadKey())),
                ("readln", (Func<string>)Console.ReadLine),
                ("write", (Action<string>)Console.Write),
                ("writeln", (Action<string>)Console.WriteLine)
            });

            AddStdPkg("file", new (string, Delegate)[]
            {
                ("read", (Func<string, string>)File.ReadAllText),
                ("write", (Action<string, string>)File.AppendAllText),
                ("dir_str", (Func<string>)(() => Path.DirectorySeparatorChar.ToString())),
                ("cur_dir", (Func<string>)Directory.GetCurrentDirectory),
                ("combine", (Func<string, string, string>)Path.Combine),
                ("exists", (Func<string, bool>)File.Exists),
                ("dir_exists", (Func<string, bool>)Directory.Exists),
                ("path", (Func<string, string>)Path.GetFullPath),
                ("dir_path", (Func<string, string>)Path.GetDirectoryName),
                ("make", (Action<string>)(value => File.Create(value))),
                ("make_dir", (Action<string>)(value => Directory.CreateDirectory(value))),
                ("delete", (Action<string>)File.Delete),
                ("delete_dir", (Action<string>)Directory.Delete)
            });
        }

        internal static Function FindFunctionBySignature(string package, string function, Datatype @return, IEnumerable<Datatype> parameters)
        {
            foreach (var stdPackage in packages)
                if (stdPackage.IsRuntime && stdPackage.Name == package)
                    return stdPackage.FindBySignature(function, true, @return, parameters);
            return null;
        }

        internal static Function FindFunctionBySignature(string qualifiedName, Datatype @return, IEnumerable<Datatype> parameters)
        {
            var (package, function) = Package.SplitQualifiedName(qualifiedName);
            return FindFunctionBySignature(package, function, @return, parameters);
        }

        public static (bool returns, object value) ExecuteFunctionByQualifiedName(string qualifiedName, Datatype @return, IEnumerable<Datatype> parameters, params object[] values) =>
            FindFunctionBySignature(qualifiedName ?? throw new ArgumentNullException(nameof(qualifiedName)), @return,
                parameters ?? throw new ArgumentNullException(nameof(parameters))).RunRuntime(values ?? throw new ArgumentNullException(nameof(values)));
    }
}