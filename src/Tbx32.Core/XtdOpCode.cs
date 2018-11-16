namespace Tbx32.Core
{
    public enum XtdOpCode
    {
        // Extended format - OPCODE(6) | RA(5) | RB(5) | RC(5) | XTD (11)
        // First 16 values (0x0 to 0xF) reserved for ALU functions
        // Currently only last 6 bits of XTD are used due to microcode decoding logic simplification. 7 bit address to lookup.

        Sub,
        Add,
        Shl,
        Shr,
        Mul,
        Div,
        Mod,
        And,
        Or,
        Xor,

        Not,
        Neg,

        Reserved1 = 0xE,
        Reserved2 = 0xF,

        Ldrx = 0x10,
        Strx,        
    }
}
