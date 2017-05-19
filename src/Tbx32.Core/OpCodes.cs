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
        Addi, 
        Movi,
        Mov,
        Shli,
        Shri,
        Muli,
        Divi,
    }
}
