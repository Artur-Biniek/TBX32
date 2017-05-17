using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;

namespace Tests.Simulator
{
    [TestFixture]
    public class RegistersTests
    {
        [Test]
        public void WrittenValueShouldBeRecoverable()
        {
            Registers regs = new Registers();

            for (byte i = 0; i < 32; i++)
            {
                var expected = i + 5;

                regs[i] = expected;

                Assert.That(regs[i], Is.EqualTo(expected));
            }
        }

        [Test]
        public void ResetShouldBringValuesToZero()
        {
            Registers regs = new Registers();

            for (byte i = 0; i < 32; i++)
            {
                var expected = i + 5;

                regs[i] = expected;

                Assert.That(regs[i], Is.EqualTo(expected));
            }

            regs.Reset();

            for (byte i = 0; i < 32; i++)
            {
                Assert.That(regs[i], Is.EqualTo(0));
            }
        }

        [Test]
        public void WrittenValueShouldBeOverridable()
        {
            Registers regs = new Registers();

            for (byte i = 0; i < 32; i++)
            {
                var expected = i + 5;

                regs[i] = expected;
                regs[i]++;

                Assert.That(regs[i], Is.EqualTo(expected + 1));
            }
        }
    }
}
