namespace Tbx32.Core
{
    public enum OpCode
    {
        // Address format - OPCODE | RA | ADDRESS
        Ld,
        St,

        // Offset format - OPCODE | RA | RB | OFFSET
        Addi = 0xF, 
        Subi,
        Movi,
        Shli,
        Shri,
        Muli,
        Divi,
    }
}
