namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public enum OperationCode : byte
    {
        NoOperation,

        JumpAlways,
        JumpTrue,
        JumpFalse,

        Call,
        Return,

        Pop,
        Clear,
        Duplicate,
        Rotate,

        LoadLiteral,

        LoadGlobal,
        LoadParameter,
        LoadConstant,
        LoadLocal,

        SetParameter,
        SetLocal
    }
}