namespace RajatPatwari.Vertex.Runtime.StandardLibrary
{
    public static class Temp
    {
        public static string[] Names { get; } =
        {
            "std_env.date", "std_env.time",

            "std_sfn.len", "std_sfn.sub",
            "std_sfn.rem", "std_sfn.concat",

            "std_op.add", "std_op.sub", "std_op.mul",
            "std_op.div", "std_op.mod", "std_op.pow",

            "std_cmp.eq", "std_cmp.gt", "std_cmp.lt",
            "std_cmp.ge", "std_cmp.le",

            "std_io.write", "std_io.writeln",
            "std_io.read", "std_io.readln",

            "std_ex.arg", "std_ex.arg_null",
            "std_ex.arg_range", "std_ex.inv_op"
        };
    }

    /*public static class Environment
    {
        public static string Date() =>
            DateTime.Now.ToShortDateString();

        public static string Time() =>
            DateTime.Now.ToLongTimeString();
    }

    public static class StringFunctions
    {
        public static uint Length(string str) =>
            (uint)str.Length;

        public static string Substring(string str, uint startPosition, int length) =>
            length == -1 ? str.Substring((int)startPosition) : str.Substring((int)startPosition, length);

        public static string Remove(string str, uint startPosition, int length) =>
            length == -1 ? str.Remove((int)startPosition) : str.Remove((int)startPosition, length);
    }

    public static class Math
    {
        public static int AbsoluteValue(int value) =>
            System.Math.Abs(value);

        public static int Negate(int value) =>
            -value;

        public static int Maximum(int value1, int value2) =>
            System.Math.Max(value1, value2);

        public static int Minimum(int value1, int value2) =>
            System.Math.Min(value1, value2);
    }

    public static class InputOutput
    {
        public static void Write(string str) =>
            Console.Write(str);

        public static void WriteLine(string str) =>
            Console.WriteLine(str);

        public static void Read() =>
            Console.Read();

        public static string ReadLine() =>
            Console.ReadLine();
    }

    public static class Exception
    {
        public static System.Exception Argument(string message) =>
            new ArgumentException(message);

        public static System.Exception ArgumentNull(string message) =>
            new ArgumentNullException(message);

        public static System.Exception ArgumentRange(string message) =>
            new ArgumentOutOfRangeException(message);

        public static System.Exception InvalidOperation(string message) =>
            new InvalidOperationException(message);
    }*/
}