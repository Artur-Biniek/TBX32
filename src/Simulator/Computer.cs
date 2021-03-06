﻿using System;
using System.Collections.Generic;
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

        public const uint RAM_TOP = 0x001FFEFF;
        #endregion

        uint _PC;
        uint _IR;

        Memory _ram = new Memory();
        Registers _regs = new Registers();
        Random _rnd = new Random();
        private Func<KeyCodes> _keyboardController;

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

        public int this[uint memoryLocation]
        {
            get
            {
                return _ram[memoryLocation];
            }
        }

        public Computer() : this(null) { }

        public Computer(Func<KeyCodes> keyboardController)
        {
            _keyboardController = keyboardController;

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
                _ram[data.Key] = (int)data.Value;
            }
        }

        public void Run()
        {
            while (Step()) ;
        }

        public bool Step()
        {
            _IR = (uint)_ram[_PC];
            _PC++;

            if (_keyboardController != null)
            {
                _ram[GAME_PAD] = (int)_keyboardController();
            }

            var ticks = (ulong)_ram[TICKS_LOW] + ((ulong)_ram[TICKS_HIGH] << 32);

            var opcode = (OpCode)CodeBuilder.ExtractOpCode(_IR);
            var ra = CodeBuilder.ExtractRegA(_IR);
            var rb = CodeBuilder.ExtractRegB(_IR);
            var rc = CodeBuilder.ExtractRegC(_IR);
            var offset = CodeBuilder.ExtractOffset(_IR);
            var address = CodeBuilder.ExtractAddress(_IR);

            switch (opcode)
            {
                case OpCode.Ld:
                    _regs[ra] = _ram[address];
                    break;

                case OpCode.Ldr:
                    _regs[ra] = _ram[(uint)(_regs[rb] + offset)];
                    break;

                case OpCode.St:
                    _ram[address] = _regs[ra];
                    break;

                case OpCode.Rnd:
                    _regs[ra] = _rnd.Next(0, int.MaxValue);
                    break;

                case OpCode.Str:
                    _ram[(uint)(_regs[rb] + offset)] = _regs[ra];
                    break;

                case OpCode.Jmp:
                    _PC = address;
                    break;

                case OpCode.Jmpr:
                    _PC = (uint)_regs[ra];
                    break;

                case OpCode.Jal:
                    _regs[ra] = (int)_PC;
                    _PC = address;
                    break;

                case OpCode.Brz:
                    if (_regs[ra] == 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Brnz:
                    if (_regs[ra] != 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Brgz:
                    if (_regs[ra] > 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Brgez:
                    if (_regs[ra] >= 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Brlz:
                    if (_regs[ra] < 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Brlez:
                    if (_regs[ra] <= 0)
                    {
                        _PC = address;
                    }
                    break;

                case OpCode.Beq:
                    if (_regs[ra] == _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Bneq:
                    if (_regs[ra] != _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Bge:
                    if (_regs[ra] >= _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Ble:
                    if (_regs[ra] <= _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Bgt:
                    if (_regs[ra] > _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Blt:
                    if (_regs[ra] < _regs[rb])
                    {
                        _PC = (uint)(_PC + offset);
                    }
                    break;

                case OpCode.Subi:
                    _regs[ra] = _regs[rb] - offset;
                    break;

                case OpCode.Addi:
                    _regs[ra] = _regs[rb] + offset;
                    break;

                case OpCode.Movli:
                    _regs[ra] = (ushort)offset;
                    break;

                case OpCode.Movhi:
                    _regs[ra] = (ushort)offset << 16;
                    break;

                case OpCode.Shli:
                    _regs[ra] = _regs[rb] << (ushort)offset;
                    break;

                case OpCode.Shri:
                    _regs[ra] = (int)((uint)_regs[rb] >> (ushort)offset);
                    break;

                case OpCode.Muli:
                    _regs[ra] = _regs[rb] * offset;
                    break;

                case OpCode.Divi:
                    _regs[ra] = _regs[rb] / offset;
                    break;

                case OpCode.Modi:
                    _regs[ra] = _regs[rb] % offset;
                    break;

                case OpCode.Andi:
                    _regs[ra] = _regs[rb] & (ushort)offset;
                    break;

                case OpCode.Ori:
                    _regs[ra] = _regs[rb] | (ushort)offset;
                    break;

                case OpCode.Xori:
                    _regs[ra] = _regs[rb] ^ (ushort)offset;
                    break;

                case OpCode.Nop:
                    break;

                case OpCode.Hlt:
                    _PC--;
                    return false;

                case OpCode.Brk:
                    var breakHere = 0xC;
                    break;

                case OpCode.Xtd:
                    XtdOpCode fun = CodeBuilder.ExtractXtdOpCode(_IR);
                    switch (fun)
                    {
                        case XtdOpCode.Sub:
                            _regs[ra] = _regs[rb] - _regs[rc];
                            break;

                        case XtdOpCode.Add:
                            _regs[ra] = _regs[rb] + _regs[rc];
                            break;

                        case XtdOpCode.Shl:
                            _regs[ra] = _regs[rb] << _regs[rc];
                            break;

                        case XtdOpCode.Shr:
                            _regs[ra] = (int)((uint)_regs[rb] >> _regs[rc]);
                            break;

                        case XtdOpCode.Mul:
                            _regs[ra] = _regs[rb] * _regs[rc];
                            break;

                        case XtdOpCode.Div:
                            _regs[ra] = _regs[rb] / _regs[rc];
                            break;

                        case XtdOpCode.Mod:
                            _regs[ra] = _regs[rb] % _regs[rc];
                            break;

                        case XtdOpCode.And:
                            _regs[ra] = _regs[rb] & _regs[rc];
                            break;

                        case XtdOpCode.Or:
                            _regs[ra] = _regs[rb] | _regs[rc];
                            break;

                        case XtdOpCode.Xor:
                            _regs[ra] = _regs[rb] ^ _regs[rc];
                            break;

                        case XtdOpCode.Not:
                            _regs[ra] = ~_regs[rb];
                            break;

                        case XtdOpCode.Neg:
                            _regs[ra] = -_regs[rb];
                            break;

                        case XtdOpCode.Ldrx:
                            _regs[ra] = _ram[(uint)(_regs[rb] + _regs[rc])];
                            break;

                        case XtdOpCode.Strx:
                            _ram[(uint)(_regs[rb] + _regs[rc])] = _regs[ra];
                            break;
                    }
                    break;
            }

            ticks++;

            _ram[TICKS_LOW] = (int)(0x00000000FFFFFFFF & ticks);
            _ram[TICKS_HIGH] = (int)((0xFFFFFFFF00000000 & ticks) >> 32);

            return true;
        }
    }
}