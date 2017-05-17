using System.Collections.Generic;

namespace Tbx32.Core
{
    public class CodeBuilder
    {
        private uint _index;
        private Dictionary<uint, uint> _code = new Dictionary<uint, uint>();

        private uint createOffsetType(OpCode opcode, Register reg1, Register reg2, short value)
        {
            uint res = 0u;

            res = (uint)opcode << 26
                | (uint)reg1 << 21
                | (uint)reg2 << 16
                | (ushort)value;

            return res;
        }

        #region Instruction Parts Extraction
        public static OpCode ExtractOpCode(uint instruction)
        {
            return (OpCode)(instruction >> 26);
        }

        public static Register ExtractRegA(uint instruction)
        {
            return (Register)((instruction >> 21) & 0b11111);
        }

        public static Register ExtractRegB(uint instruction)
        {
            return (Register)((instruction >> 16) & 0b11111);
        }

        public static Register ExtractRegC(uint instruction)
        {
            return (Register)((instruction >> 11) & 0b11111);
        }

        public static short ExtractOffset(uint instruction)
        {
            return (short)(instruction & 0xFFFF);
        }

        public static uint ExtractAddress(uint instruction)
        {
            return instruction & 0x001FFFFF;
        }
        #endregion

        public IReadOnlyDictionary<uint, uint> Build()
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<uint, uint>(_code);
        }

        public CodeBuilder Addi(Register result, Register source, short value)
        {
            _code[_index] = createOffsetType(OpCode.Addi, result, source, value);

            _index++;

            return this;
        }

        public CodeBuilder Subi(Register result, Register source, short value)
        {
            _code[_index] = createOffsetType(OpCode.Subi, result, source, value);

            _index++;

            return this;
        }
    }
}
