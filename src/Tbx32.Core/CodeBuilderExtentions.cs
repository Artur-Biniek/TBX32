namespace Tbx32.Core
{
    public static class CodeBuilderExtentions
    {
        public static CodeBuilder Mov(this CodeBuilder builder, Register target, Register source)
        {
            return builder.Addi(target, source, 0);
        }

        public static CodeBuilder Push(this CodeBuilder builder, Register reg)
        {
            return builder.Str(reg, R.Sp).Addi(R.Sp, R.Sp, -1);
        }

        public static CodeBuilder Pop(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(R.Sp, R.Sp, +1).Ldr(reg, R.Sp);
        }

        public static CodeBuilder Inc(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(reg, reg, +1);
        }

        public static CodeBuilder Dec(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(reg, reg, -1);
        }

        public static CodeBuilder Movi(this CodeBuilder builder, Register reg, int value)
        {
            return builder
                    .Movhi(reg, (short)((value & 0xFFFF0000) >> 16))
                    .Ori(reg, reg, (short)(value & 0x0000FFFF));
        }

        public static CodeBuilder Movi(this CodeBuilder builder, Register reg, uint value)
        {
            return Movi(builder, reg, (int)value);
        }
    }
}
