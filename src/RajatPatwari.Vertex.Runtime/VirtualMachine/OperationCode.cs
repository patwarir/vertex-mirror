namespace RajatPatwari.Vertex.Runtime.VirtualMachine
{
    public enum OperationCode : byte
    {
        NoOperation = 0x00,

        JumpAlways = 0x01,
        JumpTrue = 0x02,
        JumpFalse = 0x03,

        Call = 0x04,
        Return = 0x05,

        Throw = 0x06,

        Pop = 0x07,
        Duplicate = 0x08,

        LoadBoolean = 0x09,
        LoadInteger = 0x0a,
        LoadFloat = 0x0b,
        LoadString = 0x0c,

        LoadParameter = 0x0d,
        LoadConstant = 0x0e,
        LoadLocal = 0x0f,

        SetParameter = 0x10,
        SetLocal = 0x11,

        IncrementParameter = 0x12,
        IncrementLocal = 0x13,

        DecrementParameter = 0x14,
        DecrementLocal = 0x15,

        Negate = 0x16,

        Add = 0x17,
        Subtract = 0x18,
        Multiply = 0x19,
        Divide = 0x1a,
        Modulate = 0x1b,

        CheckEquals = 0x1c,

        CheckGreater = 0x1d,
        CheckLess = 0x1e,

        CheckGreaterEquals = 0x1f,
        CheckLessEquals = 0x20
    }
}