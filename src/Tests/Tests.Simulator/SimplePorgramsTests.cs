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
                .Movli(R.Fp, 0x23) // simpulate some inital FP value

                // program start

                .Push_(R.Fp)

                .Movli(R.T0, a)
                .Movli(R.T1, b)

                .Push_(R.T1)
                .Push_(R.T0)

                .Jal(R.Ra, addFunction) // call add function

                .Mov_(R.G0, R.V) // stash sumation result into G0


                .Push_(R.Fp)

                .Movli(R.T0, a)
                .Movli(R.T1, b)

                .Push_(R.T1)
                .Push_(R.T0)

                .Jal(R.Ra, subFunction) // call sub function

                .Hlt()

                .MarkLabel(addFunction)
                    // prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // function code
                    .Ldr(R.T3, R.Fp, 1)
                    .Ldr(R.T4, R.Fp, 2)
                    .Add(R.V, R.T3, R.T4)

                    // epilog
                    .Pop_(R.Ra)
                    .Addi(R.Sp, R.Sp, 2)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra)

                .MarkLabel(subFunction)
                    // prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // function code
                    .Ldr(R.T3, R.Fp, 1)
                    .Ldr(R.T4, R.Fp, 2)
                    .Sub(R.V, R.T3, R.T4)

                    // epilog
                    .Pop_(R.Ra)
                    .Addi(R.Sp, R.Sp, 2)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);

            comp.LoadProgram(prg.Build());

            comp.Run();

            Assert.That(comp[R.Sp] == Computer.STACK_BOTTOM);
            Assert.That(comp[R.Fp] == 0x23);
            Assert.That(comp[R.G0] == a + b);
            Assert.That(comp[R.V] == a - b);
        }
    }
}
