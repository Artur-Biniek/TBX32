namespace Tbx32.Core
{
    public static class CodeBuilderExtentions
    {
        public static CodeBuilder Mov_(this CodeBuilder builder, Register target, Register source)
        {
            return builder.Addi(target, source, 0);
        }

        public static CodeBuilder Push_(this CodeBuilder builder, Register reg)
        {
            return builder.Str(reg, R.Sp).Addi(R.Sp, R.Sp, -1);
        }

        public static CodeBuilder Pop_(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(R.Sp, R.Sp, +1).Ldr(reg, R.Sp);
        }

        public static CodeBuilder Inc_(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(reg, reg, +1);
        }

        public static CodeBuilder Dec_(this CodeBuilder builder, Register reg)
        {
            return builder.Addi(reg, reg, -1);
        }

        public static CodeBuilder Movi_(this CodeBuilder builder, Register reg, int value)
        {
            return builder
                    .Movhi(reg, (short)((value & 0xFFFF0000) >> 16))
                    .Ori(reg, reg, (short)(value & 0x0000FFFF));
        }

        public static CodeBuilder Movi_(this CodeBuilder builder, Register reg, uint value)
        {
            return Movi_(builder, reg, (int)value);
        }

        public static CodeBuilder Rnd_(this CodeBuilder builder, Register reg, short value)
        {
            return builder.Rnd(reg).Modi(reg, reg, value);
        }
    }
}
