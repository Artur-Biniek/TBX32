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

        private CodeBuilder push(uint instruction)
        {
            _code[_index++] = instruction;

            return this;
        }

        public CodeBuilder Addi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Addi, target, source, value));
        }

        public CodeBuilder Subi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Subi, target, source, value));
        }

        public CodeBuilder Movi(Register target, short value)
        {
            return push(createOffsetType(OpCode.Movi, target, Register.R0, value));
        }

        public CodeBuilder Shli(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Shli, target, source, value));
        }
    }
}
