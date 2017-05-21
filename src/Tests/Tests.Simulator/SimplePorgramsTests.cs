using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;
using Tbx32.Core;

namespace Tests.Simulator
{
    [TestFixture]
    public class SimplePorgramsTests
    {
        [Test]
        [TestCase(3, 4)]
        [TestCase(6, 1)]
        [TestCase(-3, 4)]
        [TestCase(-31, -94)]
        [TestCase(-3, 0)]
        [TestCase(0, 4)]
        [TestCase(0, 0)]
        public void SimpleSumAndSubOfTwoNumbersFunctionBeingCalled(short a, short b)
        {
            var comp = new Computer();
            var builder = new CodeBuilder();

            var addFunction = builder.CreateLabel();
            var subFunction = builder.CreateLabel();

            var prg = builder
                .Movi(R.Fp, 0x23) // simpulate some inital FP value

                // program start

                .Push(R.Fp)

                .Movi(R.T0, a)
                .Movi(R.T1, b)

                .Push(R.T1)
                .Push(R.T0)

                .Jal(R.Ra, addFunction) // call add function

                .Mov(R.V1, R.V0) // stash sumation result into V1


                .Push(R.Fp)

                .Movi(R.T0, a)
                .Movi(R.T1, b)

                .Push(R.T1)
                .Push(R.T0)

                .Jal(R.Ra, subFunction) // call sub function

                .Hlt()

                .MarkLabel(addFunction)
                    // prolog
                    .Mov(R.Fp, R.Sp)
                    .Push(R.Ra)

                    // function code
                    .Ldr(R.T3, R.Fp, 1)
                    .Ldr(R.T4, R.Fp, 2)
                    .Add(R.V0, R.T3, R.T4)

                    // epilog
                    .Pop(R.Ra)
                    .Addi(R.Sp, R.Sp, 2)
                    .Pop(R.Fp)
                    .Jr(R.Ra)

                .MarkLabel(subFunction)
                    // prolog
                    .Mov(R.Fp, R.Sp)
                    .Push(R.Ra)

                    // function code
                    .Ldr(R.T3, R.Fp, 1)
                    .Ldr(R.T4, R.Fp, 2)
                    .Sub(R.V0, R.T3, R.T4)

                    // epilog
                    .Pop(R.Ra)
                    .Addi(R.Sp, R.Sp, 2)
                    .Pop(R.Fp)
                    .Jr(R.Ra);

            comp.LoadProgram(prg.Build());

            comp.Run();

            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);
            Assert.That(comp[R.Fp] == 0x23);
            Assert.That(comp[R.V1] == a + b);
            Assert.That(comp[R.V0] == a - b);
        }
    }
}
