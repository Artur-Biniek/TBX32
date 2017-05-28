using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;
using Tbx32.Core;

namespace Tests.Simulator
{
    [TestFixture]
    public class ComputerTests
    {
        [Test]
        public void StackPointerShouldStartWithCorrectValue()
        {
            Computer comp = new Computer();

            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);

            comp.Run();

            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);
        }

        [Test]
        public void StackPointerShouldResetWithComputerReset()
        {
            Computer comp = new Computer();

            comp.LoadProgram(new CodeBuilder().Addi(R.Sp, R.Sp, -1).Build());

            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);

            comp.Run();
            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM - 1);

            comp.Reset();
            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);
        }

        [Test]
        public void ProgramCounterShouldIncrementOnEveryNonJumpInstruction()
        {
            var someadds = new CodeBuilder()
                    .Addi(R.T0, R.T1, -100)
                    .Addi(R.T0, R.T1, -100)
                    .Addi(R.T0, R.T1, -100)
                    .Addi(R.T0, R.T1, -100)
                    .Addi(R.T0, R.T1, -100)
                    .Build();

            Computer comp = new Computer();

            comp.LoadProgram(someadds);

            Assert.That(comp.PC, Is.Zero);

            comp.Step();

            Assert.That(comp.PC, Is.EqualTo(1u));

            comp.Step();

            Assert.That(comp.PC, Is.EqualTo(2u));
        }

        [Test]
        public void InstructionRegisterLoadsCorrectInstruction()
        {
            var someadds = new CodeBuilder()
                    .Addi(R.T0, R.T1, -100)
                    .Addi(R.Ra, R.Sp, 300)
                    .Build();

            Computer comp = new Computer();

            comp.LoadProgram(someadds);

            Assert.That(comp.PC, Is.Zero);
            Assert.That(comp.IR, Is.Zero);

            comp.Step();

            Assert.That(comp.PC, Is.EqualTo(1u));
            Assert.That(CodeBuilder.ExtractOpCode(comp.IR), Is.EqualTo(OpCode.Addi));
            Assert.That(CodeBuilder.ExtractRegA(comp.IR), Is.EqualTo(R.T0));
            Assert.That(CodeBuilder.ExtractRegB(comp.IR), Is.EqualTo(R.T1));
            Assert.That(CodeBuilder.ExtractOffset(comp.IR), Is.EqualTo(-100));

            comp.Step();

            Assert.That(comp.PC, Is.EqualTo(2u));
            Assert.That(CodeBuilder.ExtractOpCode(comp.IR), Is.EqualTo(OpCode.Addi));
            Assert.That(CodeBuilder.ExtractRegA(comp.IR), Is.EqualTo(R.Ra));
            Assert.That(CodeBuilder.ExtractRegB(comp.IR), Is.EqualTo(R.Sp));
            Assert.That(CodeBuilder.ExtractOffset(comp.IR), Is.EqualTo(300));
        }

        [Test]
        public void AddImmediateInstructionShouldWork()
        {
            var someadds = new CodeBuilder()
                    .Addi(Register.R1, Register.R0, 1)
                    .Addi(Register.R2, Register.R1, 2)
                    .Addi(Register.R3, Register.R2, 3)
                    .Addi(Register.R4, Register.R3, 4)
                    .Addi(Register.R5, Register.R4, 5)
                    .Addi(Register.R6, Register.R5, 6)
                    .Addi(Register.R7, Register.R6, -5)
                    .Addi(Register.R8, Register.R7, -4)
                    .Addi(Register.R9, Register.R8, -3)
                    .Addi(Register.R10, Register.R9, -2)
                    .Addi(Register.R11, Register.R10, -1)
                    .Build();

            Computer comp = new Computer();

            comp.LoadProgram(someadds);
            Assert.That(comp[Register.R0], Is.EqualTo(0));
            Assert.That(comp[Register.R1], Is.EqualTo(0));

            comp.Step();
            Assert.That(comp[Register.R0], Is.EqualTo(0));
            Assert.That(comp[Register.R1], Is.EqualTo(1));

            comp.Step();
            Assert.That(comp[Register.R1], Is.EqualTo(1));
            Assert.That(comp[Register.R2], Is.EqualTo(3));

            comp.Step();
            Assert.That(comp[Register.R2], Is.EqualTo(3));
            Assert.That(comp[Register.R3], Is.EqualTo(6));

            comp.Step();
            Assert.That(comp[Register.R3], Is.EqualTo(6));
            Assert.That(comp[Register.R4], Is.EqualTo(10));

            comp.Step();
            Assert.That(comp[Register.R4], Is.EqualTo(10));
            Assert.That(comp[Register.R5], Is.EqualTo(15));

            comp.Step();
            Assert.That(comp[Register.R5], Is.EqualTo(15));
            Assert.That(comp[Register.R6], Is.EqualTo(21));

            comp.Step();
            Assert.That(comp[Register.R6], Is.EqualTo(21));
            Assert.That(comp[Register.R7], Is.EqualTo(16));

            comp.Step();
            Assert.That(comp[Register.R7], Is.EqualTo(16));
            Assert.That(comp[Register.R8], Is.EqualTo(12));

            comp.Step();
            Assert.That(comp[Register.R8], Is.EqualTo(12));
            Assert.That(comp[Register.R9], Is.EqualTo(9));

            comp.Step();
            Assert.That(comp[Register.R9], Is.EqualTo(9));
            Assert.That(comp[Register.R10], Is.EqualTo(7));

            comp.Step();
            Assert.That(comp[Register.R10], Is.EqualTo(7));
            Assert.That(comp[Register.R11], Is.EqualTo(6));
        }

        [Test]
        public void SubImmediateThroughAddImmediateInstructionShouldWork()
        {
            var comp = new Computer();

            var prg = new CodeBuilder()
                    .Movli(Register.R30, 0)
                    .Addi(Register.R31, Register.R30, -10)
                    .Addi(Register.R29, Register.R31, 20)
                    .Build();

            comp.LoadProgram(prg);

            comp.Step();
            Assert.That(comp[Register.R30], Is.EqualTo(0));

            comp.Step();
            Assert.That(comp[Register.R31], Is.EqualTo(-10));

            comp.Step();
            Assert.That(comp[Register.R29], Is.EqualTo(10));
        }

        [Test]
        public void MovLowImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            for (int i = 0; i < 32; i++)
            {
                prg.Movli((Register)i, (short)((i + 1) * 2));
            }

            comp.LoadProgram(prg.Build());

            for (int i = 0; i < 32; i++)
            {
                comp.Step();

                Assert.That(comp[(Register)i], Is.EqualTo((i + 1) * 2));
            }
        }

        [Test]
        public void MovHighImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            for (int i = 0; i < 32; i++)
            {
                prg.Movhi((Register)i, (short)((i + 1) * 2));
            }

            comp.LoadProgram(prg.Build());

            for (int i = 0; i < 32; i++)
            {
                comp.Step();

                Assert.That(comp[(Register)i], Is.EqualTo(((i + 1) * 2) << 16));
            }
        }

        [Test]
        public void ShlImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();
            var expected = 1;

            prg.Movli(Register.R0, (short)expected);
            for (int i = 0; i < 32; i++)
            {
                prg.Shli(Register.R31, Register.R0, (short)i);
            }

            comp.LoadProgram(prg.Build());

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                comp.Step();

                expected = 1 << i;

                Assert.That(comp[Register.R31], Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShrImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();
            var start = 1 << 31;
            var expected = 0;

            prg.Movli(0, 1);
            prg.Shli(Register.R0, Register.R0, 31);

            for (int i = 0; i < 31; i++)
            {
                prg.Shri(Register.R30, Register.R0, (short)i);
            }

            comp.LoadProgram(prg.Build());

            comp.Step();
            comp.Step();
            for (int i = 0; i < 31; i++)
            {
                comp.Step();

                expected = (int)((uint)start >> i);

                Assert.That(comp[Register.R30], Is.EqualTo(expected));
            }
        }

        [Test]
        public void DivImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R10, 13);
            prg.Divi(Register.R11, Register.R10, 3);

            comp.LoadProgram(prg.Build());
            comp.Step();
            comp.Step();

            Assert.That(comp[Register.R11], Is.EqualTo(4));
        }


        [Test]
        [TestCase(10, -1)]
        [TestCase(10, 3)]
        [TestCase(10, 4)]
        [TestCase(-10, -3)]
        [TestCase(0, 4)]
        public void ModImmediateInstructionShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R10, a);
            prg.Modi(Register.R11, Register.R10, b);

            comp.LoadProgram(prg.Build());
            comp.Step();
            comp.Step();

            Assert.That(comp[Register.R11], Is.EqualTo(a % b));
        }

        [Test]
        [TestCase(-98, 23)]
        [TestCase(98, -23)]
        [TestCase(12, 34)]
        [TestCase(-68, -84)]
        public void MulImmediateInstructionShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R10, a);
            prg.Muli(Register.R11, Register.R10, b);

            comp.LoadProgram(prg.Build());
            comp.Step();
            comp.Step();

            Assert.That(comp[Register.R11], Is.EqualTo(a * b));
        }

        [Test]
        [TestCase(-300, 500)]
        [TestCase(1300, 14)]
        [TestCase(int.MinValue, int.MaxValue)]
        public void LdInstructionShouldWork(int val1, int val2)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var lbl1 = prg.CreateLabel();
            var lbl2 = prg.CreateLabel();

            prg.Ld(Register.R15, lbl1)
               .Ld(Register.R16, lbl2)
               .SetOrg(0xFFFE)
               .MarkLabel(lbl1)
               .Data(val1)
               .MarkLabel(lbl2)
               .Data(val2);

            comp.LoadProgram(prg.Build());

            Assert.That(comp[0xFFFE], Is.EqualTo(val1));
            Assert.That(comp[0xFFFF], Is.EqualTo(val2));

            comp.Step();
            Assert.That(comp[Register.R15], Is.EqualTo(val1));

            comp.Step();
            Assert.That(comp[Register.R16], Is.EqualTo(val2));
        }

        [Test]
        public void LdrWithOffsetShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.CreateLabel(0x100 - 1);
            prg.CreateLabel(0x100 + 1);

            prg
                .Movli(R.T1, 0x100)
                .Ldr(R.T4, R.T1, -1)
                .Ldr(R.T5, R.T1, +1)
                .Hlt()
                .SetOrg(0x100 - 1)
                .Data(11)
                .SetOrg(0x100 + 1)
                .Data(12);


            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.T4], Is.EqualTo(11));
            Assert.That(comp[R.T5], Is.EqualTo(12));
        }

        [Test]
        [TestCase(112u, 5340)]
        [TestCase(1784u, 4130)]
        [TestCase(100u, -1234)]
        public void StInstructionShouldWork(uint addr, short value)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var variable = prg.CreateLabel();

            prg.Movli(Register.R17, value)
               .St(Register.R17, variable)
               .SetOrg(addr)
               .MarkLabel(variable);

            comp.LoadProgram(prg.Build());
            comp.Step();
            comp.Step();

            Assert.That(comp[addr], Is.EqualTo(value));
        }

        [Test]
        public void JmpInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();
            var target2 = prg.CreateLabel();

            prg.Movli(Register.R19, 20)
               .Jmp(target1)
               .Movli(Register.R20, 11)
               .MarkLabel(target1)
               .Movli(Register.R20, 21)
               .Jmp(target2)
               .Movli(Register.R21, 12)
               .MarkLabel(target2)
               .Movli(Register.R21, 22);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R19], Is.EqualTo(20));
            Assert.That(comp[Register.R20], Is.EqualTo(21));
            Assert.That(comp[Register.R21], Is.EqualTo(22));
        }

        [Test]
        public void JmprInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg.Movli(Register.R19, 0x100)
               .Jmpr(Register.R19)
               .Movli(Register.R20, 11)
               .SetOrg(0x100)
               .MarkLabel(target1)
               .Movli(Register.R21, 21);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R20], Is.EqualTo(0));
            Assert.That(comp[Register.R21], Is.EqualTo(21));
        }

        [Test]
        public void JalInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg.Movli(Register.R19, 11)
               .Jal(Register.R19, target1)
               .Movli(Register.R20, 11)
               .SetOrg(0x100)
               .MarkLabel(target1)
               .Movli(Register.R21, 21);

            comp.LoadProgram(prg.Build());

            comp.Step();
            Assert.That(comp[Register.R19], Is.EqualTo(11));
            var oldpc = comp.PC;
            comp.Step();
            Assert.That(comp[Register.R19], Is.EqualTo(oldpc + 1));
            comp.Step();
            Assert.That(comp[Register.R21], Is.EqualTo(21));

            Assert.That(comp[Register.R20], Is.EqualTo(0));
        }

        [Test]
        public void JalAndJrWorkingToghether()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();
            var target2 = prg.CreateLabel();

            prg.Movli(Register.R19, 11)
               .Jal(R.Ra, target1)
               .Movli(Register.R20, 12)
               .Jmp(target2)
               .SetOrg(0x100)
               .MarkLabel(target1)
               .Movli(Register.R21, 21)
               .Jmpr(R.Ra)
               .MarkLabel(target2);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R20], Is.EqualTo(12));
            Assert.That(comp[Register.R21], Is.EqualTo(21));
        }


        [Test]
        public void MovShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R0, 1234)
               .Mov_(Register.R1, Register.R0)
               .Mov_(Register.R2, Register.R1)
               .Mov_(Register.R3, Register.R2);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R0], Is.EqualTo(1234));
            Assert.That(comp[Register.R1], Is.EqualTo(1234));
            Assert.That(comp[Register.R2], Is.EqualTo(1234));
            Assert.That(comp[Register.R3], Is.EqualTo(1234));
        }

        [Test]
        public void StrShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R0, 1234)
               .Movli(Register.R1, 0x1111)
               .Str(Register.R0, Register.R1);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[0x1111], Is.EqualTo(1234));
        }

        [Test]
        public void StrWithOffsetShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R0, 666)
               .Movli(Register.R1, 0x1111)
               .Addi(Register.R0, Register.R0, -1)
               .Str(Register.R0, Register.R1, -1)
               .Addi(Register.R0, Register.R0, 2)
               .Str(Register.R0, Register.R1, +1);


            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[0x1111 - 1], Is.EqualTo(666 - 1));
            Assert.That(comp[0x1111 + 1], Is.EqualTo(666 + 1));
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        public void AddShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, a)
               .Movli(Register.R21, b)
               .Add(Register.R22, Register.R20, Register.R21)
               .Add(Register.R23, Register.R21, Register.R20);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R22], Is.EqualTo(a + b));
            Assert.That(comp[Register.R23], Is.EqualTo(b + a));
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        public void SubShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, a)
               .Movli(Register.R21, b)
               .Sub(Register.R22, Register.R20, Register.R21)
               .Sub(Register.R23, Register.R21, Register.R20);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R22], Is.EqualTo(a - b));
            Assert.That(comp[Register.R23], Is.EqualTo(b - a));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(124)]
        [TestCase(14)]
        [TestCase(-124)]
        [TestCase(0xFFFF)]
        public void ShlShouldWork(int a)
        {
            for (int i = 0; i < 31; i++)
            {
                var prg = new CodeBuilder();
                var comp = new Computer();

                var skip = prg.CreateLabel();
                var val = prg.CreateLabel();

                prg.Jmp(skip)
                   .MarkLabel(val)
                   .Data(a)
                   .MarkLabel(skip)
                   .Ld(Register.R15, val)
                   .Movli(Register.R16, (short)i)
                   .Shl(Register.R17, Register.R15, Register.R16);

                comp.LoadProgram(prg.Build());
                comp.Run();

                Assert.That(comp[Register.R17], Is.EqualTo(a << i));
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(124)]
        [TestCase(14)]
        [TestCase(-124)]
        [TestCase(0xFFFF)]
        public void ShrShouldWork(int a)
        {
            for (int i = 0; i < 31; i++)
            {
                var prg = new CodeBuilder();
                var comp = new Computer();

                var skip = prg.CreateLabel();
                var val = prg.CreateLabel();

                prg.Jmp(skip)
                   .MarkLabel(val)
                   .Data(a)
                   .MarkLabel(skip)
                   .Ld(Register.R15, val)
                   .Movli(Register.R16, (short)i)
                   .Shr(Register.R17, Register.R15, Register.R16);

                comp.LoadProgram(prg.Build());
                comp.Run();

                Assert.That(comp[Register.R17], Is.EqualTo((int)((uint)a >> i)));
            }
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(0, -312345)]
        public void MulShouldWork(int a, int b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var skip = prg.CreateLabel();
            var albl = prg.CreateLabel();
            var blbl = prg.CreateLabel();

            prg.Jmp(skip)
               .MarkLabel(albl)
               .Data(a)
               .MarkLabel(blbl)
               .Data(b)
               .MarkLabel(skip)
               .Ld(Register.R15, albl)
               .Ld(Register.R16, blbl)
               .Mul(Register.R17, Register.R15, Register.R16);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R17], Is.EqualTo(a * b));
        }

        [Test]
        [TestCase(24, 345)]
        [TestCase(145, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(-1024, -4)]
        [TestCase(0, -312345)]
        public void DivShouldWork(int a, int b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var skip = prg.CreateLabel();
            var albl = prg.CreateLabel();
            var blbl = prg.CreateLabel();

            prg.Jmp(skip)
               .MarkLabel(albl)
               .Data(a)
               .MarkLabel(blbl)
               .Data(b)
               .MarkLabel(skip)
               .Ld(Register.R15, albl)
               .Ld(Register.R16, blbl)
               .Div(Register.R17, Register.R15, Register.R16);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R17], Is.EqualTo(a / b));
        }

        [Test]
        [TestCase(24, 345)]
        [TestCase(145, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(-1024, -4)]
        [TestCase(0, -312345)]
        public void ModShouldWork(int a, int b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var skip = prg.CreateLabel();
            var albl = prg.CreateLabel();
            var blbl = prg.CreateLabel();

            prg.Jmp(skip)
               .MarkLabel(albl)
               .Data(a)
               .MarkLabel(blbl)
               .Data(b)
               .MarkLabel(skip)
               .Ld(Register.R15, albl)
               .Ld(Register.R16, blbl)
               .Mod(Register.R17, Register.R15, Register.R16);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R17], Is.EqualTo(a % b));
        }

        [Test]
        public void NopShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();
            var oldVals = new int[32];

            for (int i = 0; i < 32; i++)
            {
                oldVals[i] = comp[(Register)i];
            }

            prg.Nop()
                .Nop()
                .Nop()
                .Nop();

            comp.LoadProgram(prg.Build());

            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 1);

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 2);

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 3);

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 4);
        }

        [Test]
        public void HltShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();
            var oldVals = new int[32];

            for (int i = 0; i < 32; i++)
            {
                oldVals[i] = comp[(Register)i]; ;
            }

            prg.Hlt()
                .Movli(Register.R0, 1)
                .Movli(Register.R1, 2)
                .Movli(Register.R2, 3);

            comp.LoadProgram(prg.Build());

            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);

            comp.Step();
            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);

            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);

            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);

            for (int i = 0; i < 32; i++)
            {
                Assert.That(comp[(Register)i] == oldVals[i]);
            }
            Assert.That(comp.PC == 0);
        }

        [Test]
        public void HltIsEncodedAsZero()
        {
            Assert.That((uint)OpCode.Hlt == 0);
        }

        [Test]
        public void BrzDoesNotJumpWhenRegisterIsNotZero()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, 1)
                .Movli(R.G1, 55)
                .Brz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == 88);
        }

        [Test]
        public void BrzJumpsWhenRegisterIsZero()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, 0)
                .Movli(R.G1, 55)
                .Brz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == 55);
        }

        [Test]
        public void BrnzDoesNotJumpWhenRegisterIsZero()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, 0)
                .Movli(R.G1, 55)
                .Brnz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == 88);
        }

        [Test]
        public void BrnzJumpsWhenRegisterIsNonZero()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, 1)
                .Movli(R.G1, 55)
                .Brnz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == 55);
        }

        [Test]
        [TestCase(-1, 88)]
        [TestCase(0, 88)]
        [TestCase(1, 55)]
        public void BrgzJumpsWhenRegisterIsGreaterThanZero(short value, short ecpect)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, value)
                .Movli(R.G1, 55)
                .Brgz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == ecpect);
        }

        [Test]
        [TestCase(-1, 55)]
        [TestCase(0, 88)]
        [TestCase(1, 88)]
        public void BrlzJumpsWhenRegisterIsLessThanZero(short value, short ecpect)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.G0, value)
                .Movli(R.G1, 55)
                .Brlz(R.G0, target1)
                .Movli(R.G1, 88)
                .MarkLabel(target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G1] == ecpect);
        }

        [Test]
        public void BeqJumpsWhenRegistersAreEqual()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.T0, 1)
                .Movli(R.T1, 1)
                .Beq(R.T0, R.T1, target1)
                .Hlt()
                .MarkLabel(target1)
                .Movli(R.G0, 432)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == 432);
        }

        [Test]
        public void BeqCanJumpBackWhenRegistersAreEqual()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var actualStart = prg.CreateLabel();
            var target1 = prg.CreateLabel();

            prg
                .Jmp(actualStart)
                .MarkLabel(target1)
                .Movli(R.G0, -765)
                .Hlt()
                .MarkLabel(actualStart)
                .Movli(R.T0, 1)
                .Movli(R.T1, 1)
                .Beq(R.T0, R.T1, target1)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == -765);
        }

        [Test]
        public void BneqJumpsWhenRegistersAreNotEqual()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.T0, 1)
                .Movli(R.T1, 0)
                .Bneq(R.T0, R.T1, target1)
                .Hlt()
                .MarkLabel(target1)
                .Movli(R.G0, 1432)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == 1432);
        }

        [Test]
        public void BneqDoesNotJumpWhenRegistersAreEqual()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var target1 = prg.CreateLabel();

            prg
                .Movli(R.T0, 12)
                .Movli(R.T1, 12)
                .Bneq(R.T0, R.T1, target1)
                .Movli(R.G0, 1432)
                .Hlt()
                .MarkLabel(target1)
                .Movli(R.G0, 5674)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == 1432);
        }

        [Test]
        [TestCase(0, 0, true)]
        [TestCase(10, 10, true)]
        [TestCase(-10, -10, true)]
        [TestCase(10, 0, true)]
        [TestCase(-3, -56, true)]
        [TestCase(0, 10, false)]
        [TestCase(-10, -5, false)]
        [TestCase(-5, 5, false)]
        [TestCase(0, 1, false)]
        [TestCase(34, 12, true)]
        public void BgeJumpsWhenRegistersAreGreaterOrEqual(short left, short right, bool shouldJump)
        {
            const short VALUE_WHEN_NO_JUMP = 1234;
            const short VALUE_WHEN_JUMMPED = 9876;

            var prg = new CodeBuilder();
            var comp = new Computer();

            var jumpLabel = prg.CreateLabel();

            prg
                .Movli(R.T0, left)
                .Movli(R.T1, right)
                .Bge(R.T0, R.T1, jumpLabel)
                .Movli(R.G0, VALUE_WHEN_NO_JUMP)
                .Hlt()
                .MarkLabel(jumpLabel)
                .Movli(R.G0, VALUE_WHEN_JUMMPED)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == (shouldJump ? VALUE_WHEN_JUMMPED : VALUE_WHEN_NO_JUMP));
        }

        [Test]
        [TestCase(0, 0, true)]
        [TestCase(10, 10, true)]
        [TestCase(-10, -10, true)]
        [TestCase(10, 0, false)]
        [TestCase(-3, -56, false)]
        [TestCase(0, 10, true)]
        [TestCase(-10, -5, true)]
        [TestCase(-5, 5, true)]
        [TestCase(0, 1, true)]
        [TestCase(34, 12, false)]
        public void BleJumpsWhenRegistersAreLessOrEqual(short left, short right, bool shouldJump)
        {
            const short VALUE_WHEN_NO_JUMP = 1234;
            const short VALUE_WHEN_JUMMPED = 9876;

            var prg = new CodeBuilder();
            var comp = new Computer();

            var jumpLabel = prg.CreateLabel();

            prg
                .Movli(R.T0, left)
                .Movli(R.T1, right)
                .Ble(R.T0, R.T1, jumpLabel)
                .Movli(R.G0, VALUE_WHEN_NO_JUMP)
                .Hlt()
                .MarkLabel(jumpLabel)
                .Movli(R.G0, VALUE_WHEN_JUMMPED)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == (shouldJump ? VALUE_WHEN_JUMMPED : VALUE_WHEN_NO_JUMP));
        }

        [Test]
        [TestCase(0, 0, false)]
        [TestCase(10, 10, false)]
        [TestCase(-10, -10, false)]
        [TestCase(10, 0, true)]
        [TestCase(-3, -56, true)]
        [TestCase(0, 10, false)]
        [TestCase(-10, -5, false)]
        [TestCase(-5, 5, false)]
        [TestCase(0, 1, false)]
        [TestCase(34, 12, true)]
        public void BgtJumpsWhenFirstRegisterIsGreaterThanTheOther(short left, short right, bool shouldJump)
        {
            const short VALUE_WHEN_NO_JUMP = 1234;
            const short VALUE_WHEN_JUMMPED = 9876;

            var prg = new CodeBuilder();
            var comp = new Computer();

            var jumpLabel = prg.CreateLabel();

            prg
                .Movli(R.T0, left)
                .Movli(R.T1, right)
                .Bgt(R.T0, R.T1, jumpLabel)
                .Movli(R.G0, VALUE_WHEN_NO_JUMP)
                .Hlt()
                .MarkLabel(jumpLabel)
                .Movli(R.G0, VALUE_WHEN_JUMMPED)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == (shouldJump ? VALUE_WHEN_JUMMPED : VALUE_WHEN_NO_JUMP));
        }

        [Test]
        [TestCase(0, 0, false)]
        [TestCase(10, 10, false)]
        [TestCase(-10, -10, false)]
        [TestCase(10, 0, false)]
        [TestCase(-3, -56, false)]
        [TestCase(0, 10, true)]
        [TestCase(-10, -5, true)]
        [TestCase(-5, 5, true)]
        [TestCase(0, 1, true)]
        [TestCase(34, 12, false)]
        public void BltJumpsWhenFistRegisterIsLessThenTheOther(short left, short right, bool shouldJump)
        {
            const short VALUE_WHEN_NO_JUMP = 1234;
            const short VALUE_WHEN_JUMMPED = 9876;

            var prg = new CodeBuilder();
            var comp = new Computer();

            var jumpLabel = prg.CreateLabel();

            prg
                .Movli(R.T0, left)
                .Movli(R.T1, right)
                .Blt(R.T0, R.T1, jumpLabel)
                .Movli(R.G0, VALUE_WHEN_NO_JUMP)
                .Hlt()
                .MarkLabel(jumpLabel)
                .Movli(R.G0, VALUE_WHEN_JUMMPED)
                .Hlt();

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[R.G0] == (shouldJump ? VALUE_WHEN_JUMMPED : VALUE_WHEN_NO_JUMP));
        }

        [Test]
        public void NegShouldWorkCorrectly()
        {
            for (short i = -10; i < 10; i++)
            {
                var prg = new CodeBuilder();
                var comp = new Computer();

                var jumpLabel = prg.CreateLabel();

                prg
                    .Movli(R.T0, i)
                    .Neg(R.T1, R.T0)
                    .Hlt();

                comp.LoadProgram(prg.Build());
                comp.Run();

                Assert.That(comp[R.T1] == -i);
            }
        }

        [Test]
        public void NotShouldWorkCorrectly()
        {
            for (short i = -10; i < 10; i++)
            {
                var prg = new CodeBuilder();
                var comp = new Computer();

                var jumpLabel = prg.CreateLabel();

                prg
                    .Movli(R.T0, i)
                    .Not(R.T1, R.T0)
                    .Hlt();

                comp.LoadProgram(prg.Build());
                comp.Run();

                Assert.That(comp[R.T1] == ~i);
            }
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        [TestCase(1, -1)]
        public void AndShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, a)
               .Movli(Register.R21, b)
               .And(Register.R22, Register.R20, Register.R21);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R22], Is.EqualTo(a & b));
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        [TestCase(1, -1)]
        public void OrShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, a)
               .Movli(Register.R21, b)
               .Or(Register.R22, Register.R20, Register.R21);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R22], Is.EqualTo(a | b));
        }

        [Test]
        [TestCase(124, 345)]
        [TestCase(14, 345)]
        [TestCase(124, -34)]
        [TestCase(-12, -3)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        [TestCase(1, -1)]
        public void XorShouldWork(short a, short b)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, a)
               .Movli(Register.R21, b)
               .Xor(Register.R22, Register.R20, Register.R21);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R22], Is.EqualTo(a ^ b));
        }

        [Test]
        [TestCase(345)]
        [TestCase(-34)]
        [TestCase(-3)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void AndImmediateShouldWork(short a)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, 0b10101010)
               .Andi(Register.R21, Register.R20, a);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R21], Is.EqualTo(0b10101010 & a));
        }

        [Test]
        [TestCase(345)]
        [TestCase(-34)]
        [TestCase(-3)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void OrImmediateShouldWork(short a)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, 0b10101010)
               .Ori(Register.R21, Register.R20, a);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R21], Is.EqualTo(0b10101010 | (ushort)a));
        }

        [Test]
        [TestCase(345)]
        [TestCase(-34)]
        [TestCase(-3)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void XorImmediateShouldWork(short a)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movli(Register.R20, 0b10101010)
               .Xori(Register.R21, Register.R20, a);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R21], Is.EqualTo(0b10101010 ^ a));
        }

        [Test]
        [TestCase(345)]
        [TestCase(-34)]
        [TestCase(-3)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(0x0BCD0123)]
        [TestCase(0x0001FFFF)]
        [TestCase(0x000F0000)]
        [TestCase(0x0A0B0C0D)]
        [TestCase(0x00008000)]
        public void MoviPseudoInstructionShouldWork(int a)
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            prg.Movi_(Register.R20, a);

            comp.LoadProgram(prg.Build());
            comp.Run();

            Assert.That(comp[Register.R20], Is.EqualTo(a));
        }

        [Test]
        public void LdrxAndStrxInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            var loopStart = prg.CreateLabel();

            prg
                .Movi_(R.G3, 0x100)
                .Movi_(R.G5, 0x200)
                
                .Movi_(R.T1, 0)
                .Movi_(R.T2, 10)

                .MarkLabel(loopStart)
                .Ldrx(R.S0, R.G3, R.T1)
                .Strx(R.S0, R.G5, R.T1)
                .Inc_(R.T1)
                .Blt(R.T1, R.T2, loopStart)

                .Hlt()

                .SetOrg(0x100)
                .Data(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
                
                ;

            comp.LoadProgram(prg.Build());
            comp.Run();

            for(int i = 0; i < 10; i++)
            {
                Assert.That(comp[(uint)(0x100 + i)] == i + 1);
                Assert.That(comp[(uint)(0x200 + i)] == i + 1);
            }
        }
    }
}
