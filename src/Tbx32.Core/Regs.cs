namespace Tbx32.Core
{
    public static class R
    {
        // temporary values registers - functions can overwrite values
        public const Register T0 = Register.R0;
        public const Register T1 = Register.R1;
        public const Register T2 = Register.R2;
        public const Register T3 = Register.R3;
        public const Register T4 = Register.R4;
        public const Register T5 = Register.R5;
        public const Register T6 = Register.R6;
        public const Register T7 = Register.R7;

        // holds function's result if it returns one value
        public const Register V = Register.R8;

        // saved registers - must be preserved between function calls
        public const Register S0 = Register.R9;
        public const Register S1 = Register.R10;
        public const Register S2 = Register.R11;
        public const Register S3 = Register.R12;

        // global registers - must be preserved between function calls
        public const Register G0 = Register.R13;
        public const Register G1 = Register.R14;
        public const Register G2 = Register.R15;
        public const Register G3 = Register.R16;
        public const Register G4 = Register.R17;
        public const Register G5 = Register.R18;
        public const Register G6 = Register.R19;
        public const Register G7 = Register.R20;

        // more temporaries
        public const Register T8 = Register.R21;
        public const Register T9 = Register.R22;
        public const Register T10 = Register.R23;
        public const Register T11 = Register.R24;
        public const Register T12 = Register.R25;
        public const Register T13 = Register.R26;
        public const Register T14 = Register.R27;

        // assembler reserved register
        public const Register AsmRes = Register.R28;

        // frame pointer register
        public const Register Fp = Register.R29;

        // stack pointer register
        public const Register Sp = Register.R30;

        // return address - convention to store return address with jump-and-link / jump-register
        public const Register Ra = Register.R31;
    }
}
