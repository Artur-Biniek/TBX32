namespace Tbx32.Core
{
    public enum OpCode
    {
        Hlt = 0,

        // Address format - OPCODE | RA | ADDRESS
        Ld,
        St,

        Jmp,
        Jr,
        Jal,

        // Offset format - OPCODE | RA | RB | OFFSET
        Ldr,
        Str,

        Movi,
        Addi,         
        Shli,
        Shri,
        Muli,
        Divi,

        Nop = 0b111110,

        Xtd = 0b111111
    }
}
