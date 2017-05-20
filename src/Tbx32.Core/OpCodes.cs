namespace Tbx32.Core
{
    public enum OpCode
    {
        // Address format - OPCODE | RA | ADDRESS
        Ld,
        St,
        Jmp,
        Jr,
        Jal,

        // Offset format - OPCODE | RA | RB | OFFSET
        Str,
        Mov,

        Movi,
        Addi,         
        Shli,
        Shri,
        Muli,
        Divi,

        Xtd = 0b111111
    }
}
