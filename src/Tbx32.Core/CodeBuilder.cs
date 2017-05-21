﻿using System;
using System.Collections.Generic;

namespace Tbx32.Core
{
    public class CodeBuilder
    {
        public class Label { }

        private uint _index;

        private Dictionary<Label, uint> _labels = new Dictionary<Label, uint>();
        private SortedDictionary<uint, uint> _code = new SortedDictionary<uint, uint>();
        private readonly List<Tuple<uint, Func<uint>>> _delayedConstruction = new List<Tuple<uint, Func<uint>>>();

        private uint createOffsetType(OpCode opcode, Register reg1, Register reg2, short value)
        {
            uint res = 0u;

            res = (uint)opcode << 26
                | (uint)reg1 << 21
                | (uint)reg2 << 16
                | (ushort)value;

            return res;
        }

        private uint createXtdType(XtdOpCode xtdOpCode, Register reg1, Register reg2, Register reg3)
        {
            uint res = 0u;

            res = (uint)OpCode.Xtd << 26
                | (uint)reg1 << 21
                | (uint)reg2 << 16
                | (uint)reg3 << 11
                | (uint)xtdOpCode;

            return res;
        }

        private uint createAddressType(OpCode opcode, Register reg, Label lbl)
        {
            if (lbl != null && !_labels.ContainsKey(lbl))
            {
                throw new InvalidOperationException("Usage of unmarked lablel");
            }

            uint res = 0u;
            uint address = lbl == null ? 0u : _labels[lbl];

            res = (uint)opcode << 26
                | (uint)reg << 21
                | (0x001FFFFF & address);

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

        public static XtdOpCode ExtractXtdOpCode(uint instruction)
        {
            return (XtdOpCode)(instruction & 0xF);
        }
        #endregion

        public IReadOnlyDictionary<uint, uint> Build()
        {
            foreach (var instructionCreator in _delayedConstruction)
            {
                _code[instructionCreator.Item1] = instructionCreator.Item2();
            }

            return new System.Collections.ObjectModel.ReadOnlyDictionary<uint, uint>(_code);
        }

        public Label CreateLabel(uint? address = null)
        {
            var label = new Label();

            if (address != null)
            {
                _labels[label] = address.Value;
            }

            return label;
        }

        public CodeBuilder MarkLabel(Label lbl)
        {
            if (_labels.ContainsKey(lbl))
            {
                throw new InvalidOperationException("Label already marked");
            }

            _labels[lbl] = _index;

            return this;
        }

        public CodeBuilder Data(params int[] datapoints)
        {
            foreach (var dp in datapoints)
            {
                push((uint)dp);
            }

            return this;
        }

        public CodeBuilder SetOrg(uint address)
        {
            _index = address;

            return this;
        }

        private CodeBuilder push(uint instruction)
        {
            if (_code.ContainsKey(_index))
            {
                throw new InvalidOperationException("Address already used");
            }

            _code[_index++] = instruction;

            return this;
        }

        private CodeBuilder pushDelayed(Func<uint> instruction)
        {
            _delayedConstruction.Add(new Tuple<uint, Func<uint>>(_index, instruction));

            _index++;

            return this;
        }

        public CodeBuilder Addi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Addi, target, source, value));
        }

        public CodeBuilder Movi(Register target, short value)
        {
            return push(createOffsetType(OpCode.Movi, target, Register.R0, value));
        }

        public CodeBuilder Shli(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Shli, target, source, value));
        }

        public CodeBuilder Shri(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Shri, target, source, value));
        }

        public CodeBuilder Muli(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Muli, target, source, value));
        }

        public CodeBuilder Divi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Divi, target, source, value));
        }

        public CodeBuilder Ld(Register target, Label source)
        {
            return pushDelayed(() => createAddressType(OpCode.Ld, target, source));
        }

        public CodeBuilder Ldr(Register target, Register source, short offset = 0)
        {
            return push(createOffsetType(OpCode.Ldr, target, source, offset));
        }

        public CodeBuilder St(Register source, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.St, source, target));
        }

        public CodeBuilder Str(Register source, Register target, short offset = 0)
        {
            return push(createOffsetType(OpCode.Str, source, target, offset));
        }

        public CodeBuilder Jmp(Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Jmp, (Register)0, target));
        }

        public CodeBuilder Jmpr(Register target)
        {
            return push(createAddressType(OpCode.Jmpr, target, null));
        }

        public CodeBuilder Jal(Register linkRegister, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Jal, linkRegister, target));
        }

        public CodeBuilder Brz(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brz, reg, target));
        }

        public CodeBuilder Brnz(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brnz, reg, target));
        }

        public CodeBuilder Add(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Add, target, leftSource, rightSource));
        }

        public CodeBuilder Sub(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Sub, target, leftSource, rightSource));
        }

        public CodeBuilder Shl(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Shl, target, leftSource, rightSource));
        }

        public CodeBuilder Shr(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Shr, target, leftSource, rightSource));
        }

        public CodeBuilder Mul(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Mul, target, leftSource, rightSource));
        }

        public CodeBuilder Div(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Div, target, leftSource, rightSource));
        }

        public CodeBuilder Mod(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Mod, target, leftSource, rightSource));
        }

        public CodeBuilder Nop()
        {
            return push(createAddressType(OpCode.Nop, 0, null));
        }

        public CodeBuilder Hlt()
        {
            return push(createAddressType(OpCode.Hlt, 0, null));
        }
    }
}
