using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;
using Tbx32.Core;

namespace Tests.Simulator
{
    [TestFixture]
    public class SimplePorgramsTests
    {
        [Test]
        public void SimpleSumTwoNumbersFunctionBeingCalled()
        {
            var comp = new Computer();
            var builder = new CodeBuilder();

            var addFunction = builder.CreateLabel();

            var prg = builder
                .Movi(R.T0, 5)
                .Movi(R.T1, 6)
                .Str(R.T1, R.Sp) // push T1
                .Addi(R.Sp, R.Sp, -1)
                .Str(R.T0, R.Sp) // push T0
                .Addi(R.Sp, R.Sp, -1)
                .Str(R.Fp, R.Sp) // push Fp
                .Addi(R.Sp, R.Sp, -1)
                .Jal(R.Ra, addFunction) // call addFunction
                .Hlt()
                .Str(R.Ra, R.Sp) // push Ra
                .Addi(R.Sp, R.Sp, -1)
                .Mov(R.Fp, R.Sp);
              //  .Ld(R.T3, )



        }
    }
}
