using RajatPatwari.Vertex.Runtime.VirtualMachine;
using System;
using System.Collections.Generic;
using System.IO;

namespace RajatPatwari.Vertex.Runtime
{
    public static class StandardLibrary
    {
        private static readonly IList<Package> packages = new List<Package>();

        static StandardLibrary()
        {
            if (packages == null)
                throw new InvalidOperationException(nameof(packages));

            packages.Add(Package.MakeRuntimePackage("env", new (string, Delegate)[]
            {
                ("exit", (Action)(() => Environment.Exit(0))),
                ("date", (Func<string>)DateTime.Now.ToShortDateString),
                ("time", (Func<string>)DateTime.Now.ToLongTimeString)
            }));

            packages.Add(Package.MakeRuntimePackage("cast", new (string, Delegate)[]
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
            }));

            packages.Add(Package.MakeRuntimePackage("opr", new (string, Delegate)[]
            {
                ("neg", (Func<bool, bool>)(value => !value)),
                ("neg", (Func<long, long>)(value => -value)),
                ("neg", (Func<double, double>)(value => -value)),
                ("to_dec", (Func<string, long>)(value =>
                    long.Parse(value, System.Globalization.NumberStyles.HexNumber))),
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
                ("lgb", (Func<long, long, double>)((value1, value2) => Math.Log(value1, value2))),
                ("lgb", (Func<double, double, double>)((value1, value2) => Math.Log(value1, value2)))
            }));

            packages.Add(Package.MakeRuntimePackage("cmp", new (string, Delegate)[]
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
            }));

            packages.Add(Package.MakeRuntimePackage("sfn", new (string, Delegate)[]
            {
                ("len", (Func<string, long>)(value => value.Length)),
                ("char", (Func<string, long, string>)((value, index) => value[(int)index].ToString())),
                ("is_emp", (Func<string, bool>)string.IsNullOrWhiteSpace),
                ("cat", (Func<string, string, string>)string.Concat),
                ("rep", (Func<string, long, string>)((value, times) => new string(value[0], (int)times))),
                ("sub", (Func<string, long, string>)((value, index) => value.Substring((int)index))),
                ("sub", (Func<string, long, long, string>)((value, index, length) =>
                    value.Substring((int)index, (int)length))),
                ("rem", (Func<string, long, string>)((value, index) => value.Remove((int)index))),
                ("rem", (Func<string, long, long, string>)((value, index, length) =>
                    value.Remove((int)index, (int)length)))
            }));

            packages.Add(Package.MakeRuntimePackage("math", new (string, Delegate)[]
            {
                // TODO: Add in the functions for the math package.
            }));

            packages.Add(Package.MakeRuntimePackage("err", new (string, Delegate)[]
            {
                ("inv_op", (Action)(() => throw new InvalidOperationException())),
                ("inv_op", (Action<string>)(value => throw new InvalidOperationException(value))),
                ("arg", (Action<string>)(value => throw new ArgumentException(value))),
                ("arg_range", (Action<string>)(value => throw new ArgumentOutOfRangeException(value)))
            }));

            packages.Add(Package.MakeRuntimePackage("io", new (string, Delegate)[]
            {
                ("ln_str", (Func<string>)(() => Environment.NewLine)),
                ("clear", (Action)Console.Clear),
                ("read", (Action)(() => Console.ReadKey())),
                ("readln", (Func<string>)Console.ReadLine),
                ("write", (Action<string>)Console.Write),
                ("writeln", (Action<string>)Console.WriteLine)
            }));

            packages.Add(Package.MakeRuntimePackage("file", new (string, Delegate)[]
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
            }));
        }

        internal static Function FindBySignature(string package, string function, Datatype @return,
            IEnumerable<Datatype> parameters)
        {
            foreach (var stdPackage in packages)
                if (stdPackage.IsRuntime && stdPackage.Name == package)
                    return stdPackage.FindBySignature(function, true, @return, parameters);
            return null;
        }

        internal static Function FindBySignature(string qualifiedName, Datatype @return,
            IEnumerable<Datatype> parameters)
        {
            var (package, function) = Function.SplitQualifiedName(qualifiedName);
            return FindBySignature(package, function, @return, parameters);
        }

        public static (bool returns, object? value) Execute(string qualifiedName, Datatype @return,
            IEnumerable<Datatype> parameters, params object?[] values) =>
            FindBySignature(qualifiedName ?? throw new ArgumentNullException(nameof(qualifiedName)), @return,
                    parameters ?? throw new ArgumentNullException(nameof(parameters)))
                .RunRuntime(values ?? throw new ArgumentNullException(nameof(values)));
    }
}