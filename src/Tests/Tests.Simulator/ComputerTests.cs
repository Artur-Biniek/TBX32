using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;
using Tbx32.Core;

namespace Tests.Simulator
{
    [TestFixture]
    public class ComputerTests
    {
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
                    .Subi(R.Ra, R.Sp, 300)
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
            Assert.That(CodeBuilder.ExtractOpCode(comp.IR), Is.EqualTo(OpCode.Subi));
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
        public void SubImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder()
                    .Subi(Register.R31, Register.R30, 10)
                    .Subi(Register.R29, Register.R31, -20)
                    .Build();

            Computer comp = new Computer();

            comp.LoadProgram(prg);

            comp.Step();
            Assert.That(comp[Register.R31], Is.EqualTo(-10));

            comp.Step();
            Assert.That(comp[Register.R29], Is.EqualTo(10));
        }

        [Test]
        public void MovImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();

            for (int i = 0; i < 32; i++)
            {
                prg.Movi((Register)i, (short)((i + 1) * 2));
            }

            comp.LoadProgram(prg.Build());

            for (int i = 0; i < 32; i++)
            {
                comp.Step();

                Assert.That(comp[(Register)i], Is.EqualTo((i + 1) * 2));
            }
        }

        [Test]
        public void ShlImmediateInstructionShouldWork()
        {
            var prg = new CodeBuilder();
            var comp = new Computer();
            var expected = 1;

            prg.Movi(Register.R0, (short)expected);
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

            prg.Movi(0, 1);
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
    }
}
