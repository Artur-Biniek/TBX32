namespace Tbx32.Core
{
    public enum OpCode
    {
        Hlt = 0,
        
        Brk, 

        // Address format - OPCODE | RA | ADDRESS
        Ld,             // Load
        St,             // Store

        Rnd,            // Random value

        Jmp,            // Jump (unconditionaly)
        Jmpr,           // Jump Register (unconditionally)
        Jal,            // Jump And Link (unconditinoally)

        Brz,            // Branch if Zero
        Brnz,           // Branch if Not Zero
        Brgz,           // Branch if Greater than Zero
        Brlz,           // Branch if Less than Zero

        // Offset format - OPCODE | RA | RB | OFFSET
        Ldr,            // Load Register
        Str,            // Store Register

        Beq,            // Branch Equal
        Bneq,           // Branch Not Equal
        Bgt,            // Branch Greater
        Bge,            // Branch Greater or Equal
        Blt,            // Branch Less
        Ble,            // Branch Less or Equal        

        Movli,
        Movhi,
        Addi,         
        Shli,
        Shri,
        Muli,
        Divi,
        Modi,
        Andi,
        Ori,
        Xori,

        Nop = 0b111110,

        Xtd = 0b111111      // Extended. Opcode contained in least significant bits
    }
}
