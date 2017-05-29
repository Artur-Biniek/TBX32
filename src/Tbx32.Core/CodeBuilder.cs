using System;
using System.Collections.Generic;
using System.Linq;

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

        private uint createOffsetType(OpCode opcode, Register reg1, Register reg2, uint oldPc, Label target)
        {
            uint res = 0u;

            uint address = _labels[target];
            var nextInstrAddress = oldPc;

            uint offset = (address > nextInstrAddress) ? address - nextInstrAddress : nextInstrAddress - address;

            if (offset > short.MaxValue || -offset < short.MinValue)
            {
                throw new InvalidOperationException("Offset out of 16bit signed value range");
            }

            short value = (short)(address > nextInstrAddress ? offset : -offset);

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

            uint address = lbl == null ? 0u : _labels[lbl];

            return createAddressTypeInternal(opcode, reg, address);
        }

        private uint createAddressTypeInternal(OpCode opcode, Register reg, uint address)
        {
            uint res = 0u;

            if ((address & ~0x001FFFFF) != 0)
            {
                throw new InvalidOperationException("Address out of 21 bit addresable range");
            }

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

        public CodeBuilder Data(params uint[] datapoints)
        {
            return Data(datapoints.Select(p => (int)p).ToArray());
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

        public CodeBuilder Movli(Register target, short value)
        {
            return push(createOffsetType(OpCode.Movli, target, Register.R0, value));
        }

        public CodeBuilder Movhi(Register target, short value)
        {
            return push(createOffsetType(OpCode.Movhi, target, Register.R0, value));
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

        public CodeBuilder Modi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Modi, target, source, value));
        }

        public CodeBuilder Andi(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Andi, target, source, value));
        }

        public CodeBuilder Ori(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Ori, target, source, value));
        }

        public CodeBuilder Xori(Register target, Register source, short value)
        {
            return push(createOffsetType(OpCode.Xori, target, source, value));
        }

        public CodeBuilder Ld(Register target, Label source)
        {
            return pushDelayed(() => createAddressType(OpCode.Ld, target, source));
        }

        public CodeBuilder Ld(Register target, uint source)
        {
            return push(createAddressTypeInternal(OpCode.Ld, target, source));
        }

        public CodeBuilder Ldr(Register target, Register source, short offset = 0)
        {
            return push(createOffsetType(OpCode.Ldr, target, source, offset));
        }

        public CodeBuilder St(Register source, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.St, source, target));
        }

        public CodeBuilder St(Register source, uint target)
        {
            return push(createAddressTypeInternal(OpCode.St, source, target));
        }

        public CodeBuilder Rnd(Register target)
        {
            return push(createAddressType(OpCode.Rnd, target, null));
        }

        public CodeBuilder Str(Register source, Register target, short offset = 0)
        {
            return push(createOffsetType(OpCode.Str, source, target, offset));
        }

        public CodeBuilder Beq(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Beq, left, right, addrOfNextInstruction, target));
        }

        public CodeBuilder Bneq(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Bneq, left, right, addrOfNextInstruction, target));
        }

        public CodeBuilder Bge(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Bge, left, right, addrOfNextInstruction, target));
        }

        public CodeBuilder Ble(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Ble, left, right, addrOfNextInstruction, target));
        }

        public CodeBuilder Bgt(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Bgt, left, right, addrOfNextInstruction, target));
        }

        public CodeBuilder Blt(Register left, Register right, Label target)
        {
            var addrOfNextInstruction = _index + 1;
            return pushDelayed(() => createOffsetType(OpCode.Blt, left, right, addrOfNextInstruction, target));
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

        public CodeBuilder Brgz(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brgz, reg, target));
        }

        public CodeBuilder Brgez(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brgez, reg, target));
        }

        public CodeBuilder Brlz(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brlz, reg, target));
        }

        public CodeBuilder Brlez(Register reg, Label target)
        {
            return pushDelayed(() => createAddressType(OpCode.Brlez, reg, target));
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

        public CodeBuilder And(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.And, target, leftSource, rightSource));
        }

        public CodeBuilder Or(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Or, target, leftSource, rightSource));
        }

        public CodeBuilder Xor(Register target, Register leftSource, Register rightSource)
        {
            return push(createXtdType(XtdOpCode.Xor, target, leftSource, rightSource));
        }

        public CodeBuilder Not(Register target, Register source)
        {
            return push(createXtdType(XtdOpCode.Not, target, source, 0));
        }

        public CodeBuilder Neg(Register target, Register source)
        {
            return push(createXtdType(XtdOpCode.Neg, target, source, 0));
        }

        public CodeBuilder Ldrx(Register target, Register source, Register offset)
        {
            return push(createXtdType(XtdOpCode.Ldrx, target, source, offset));
        }

        public CodeBuilder Strx(Register target, Register source, Register offset)
        {
            return push(createXtdType(XtdOpCode.Strx, target, source, offset));
        }

        public CodeBuilder Nop()
        {
            return push(createAddressType(OpCode.Nop, 0, null));
        }

        public CodeBuilder Hlt()
        {
            return push(createAddressType(OpCode.Hlt, 0, null));
        }

        public CodeBuilder Brk()
        {
            return push(createAddressType(OpCode.Brk, 0, null));
        }
    }
}
