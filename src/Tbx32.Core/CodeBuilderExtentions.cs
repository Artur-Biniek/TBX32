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
    }
}
