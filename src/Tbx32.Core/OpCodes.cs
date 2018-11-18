namespace Tbx32.Core
{
    public enum OpCode
    {
        Hlt = 0,
        
        Brk, 

        // Address format - OPCODE(6) | RA(5) | ADDRESS(21)
        Ld,             // Load
        St,             // Store

        Rnd,            // Random value

        Jmp,            // Jump (unconditionaly)
        Jmpr,           // Jump Register (unconditionally)
        Jal,            // Jump And Link (unconditinoally)

        Brz,            // Branch if Zero
        Brnz,           // Branch if Not Zero
        Brgz,           // Branch if Greater than Zero
        Brgez,          // Branch if Greater or Equal Zero
        Brlz,           // Branch if Less than Zero
        Brlez,          // Branch if Less or Equal Zero

        // Offset format - OPCODE(6) | RA(5) | RB(5) | OFFSET(16)
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

        // ALU codes - should follow same order and values in 4 LSBs as those in XtdOpCodes
        Addi = 0x21,        
        Shli,
        Shri,
        Muli,
        Divi,
        Modi,
        Andi,
        Ori,
        Xori,

        Nop = 0b111110,

        // Offset format - OPCODE(6) | RA(5) | RB(5) | RC(5) | OFFSET(11)
        Xtd = 0b111111      // Extended. Opcode contained in least significant bits         
    }
}
