namespace Tbx32.Core
{
    public enum OpCode
    {
        Hlt = 0,

        // Address format - OPCODE | RA | ADDRESS
        Ld,         // Load
        St,         // Store

        Jmp,        // Jump (unconditionaly)
        Jmpr,       // Jump Register (unconditionally)
        Jal,        // Jump And Link (unconditinoally)

        Brz,        // Branch if Zero
        Brnz,       // Branch if Not Zero

        // Offset format - OPCODE | RA | RB | OFFSET
        Ldr,        // Load Register
        Str,        // Store Register

        Movi,
        Addi,         
        Shli,
        Shri,
        Muli,
        Divi,

        Nop = 0b111110,

        Xtd = 0b111111      // Extended. Opcode contained in least significant bits
    }
}
