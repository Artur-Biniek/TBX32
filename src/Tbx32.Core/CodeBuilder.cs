using System;
using System.Collections.Generic;

namespace Tbx32.Core
{
    public class CodeBuilder
    {
        public class Label { }

        private uint _index;

        private Dictionary<Label, uint> _labels = new Dictionary<Label, uint>();
        private Dictionary<uint, uint> _code = new Dictionary<uint, uint>();
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

        private uint createAddressType(OpCode opcode, Register reg, Label lbl)
        {
            if (!_labels.ContainsKey(lbl))
            {
                throw new InvalidOperationException("Usage of unmarked lablel");
            }

            uint res = 0u;
            uint address = _labels[lbl];

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
        #endregion

        public IReadOnlyDictionary<uint, uint> Build()
        {
            foreach (var instructionCreator in _delayedConstruction)
            {
                _code[instructionCreator.Item1] = instructionCreator.Item2();
            }

            return new System.Collections.ObjectModel.ReadOnlyDictionary<uint, uint>(_code);
        }

        public Label CreateLabel()
        {
            return new Label();
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
    }
}
