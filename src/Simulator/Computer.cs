﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    public class Computer
    {
        #region Memory Map
        public const uint VIDEO_END = 0x001FFFFF;
        public const uint VIDEO_START = 0x001FFFE0;

        public const uint SEG_DISP = 0x001FFFDF;

        public const uint GAME_PAD = 0x001FFFDE;

        public const uint TICKS_HIGH = 0x001FFFDD;
        public const uint TICKS_LOW = 0x001FFFDC;

        public const uint STACK_BOTTOM = 0x001FFEFF;
        #endregion

        uint _PC;
        uint _IR;
        Memory _ram = new Memory();
        Registers _regs = new Registers();

        public uint PC
        {
            get { return _PC; }
        }

        public uint IR
        {
            get { return _IR; }
        }

        public int this[Register reg]
        {
            get
            {
                return _regs[reg];
            }
        }

        public Computer()
        {
            Reset();
        }

        public void Reset()
        {
            _PC = 0u;
            _ram.Reset();
            _regs.Reset();
        }

        public void LoadProgram(IReadOnlyDictionary<uint, uint> program)
        {
            foreach (var data in program)
            {
                _ram[data.Key] = data.Value;
            }
        }

        public void Step()
        {
            _IR = _ram[_PC];
            _PC++;

            var opcode = CodeBuilder.ExtractOpCode(_IR);
            var ra = CodeBuilder.ExtractRegA(_IR);
            var rb = CodeBuilder.ExtractRegB(_IR);
            var rc = CodeBuilder.ExtractRegC(_IR);
            var offset = CodeBuilder.ExtractOffset(_IR);

            switch (opcode)
            {
                case OpCode.Addi:
                    _regs[ra] = _regs[rb] + offset;
                    break;

                case OpCode.Subi:
                    _regs[ra] = _regs[rb] - offset;
                    break;

                case OpCode.Movi:
                    _regs[ra] = offset;
                    break;
            }
        }
    }
}