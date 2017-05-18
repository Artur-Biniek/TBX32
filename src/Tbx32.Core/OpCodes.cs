namespace Tbx32.Core
{
    public enum OpCode
    {
        // Offset format - OPCODE | RA | RB | OFFSET
        Addi = 0xF, 
        Subi,
        Movi,
        Shli,
        Shri,
    }
}
