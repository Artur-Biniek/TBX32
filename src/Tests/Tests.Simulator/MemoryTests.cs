using ArturBiniek.Tbx32.Simulator;
using NUnit.Framework;

namespace Tests.Simulator
{
    [TestFixture]
    public class MemoryTests
    {
        [Test]
        public void WrittenValueShouldBeRecoverable()
        {
            Memory ram = new Memory();

            for (uint i = 0; i < 32; i++)
            {
                var expected = (int)i + 5;

                ram[i] = expected;

                Assert.That(ram[i], Is.EqualTo(expected));
            }
        }

        [Test]
        public void ResetShouldBringValuesToZero()
        {
            Memory ram = new Memory();

            for (uint i = 0; i < 2000000; i += 100000)
            {
                var expected = i + 5;

                ram[i] = (int)expected;

                Assert.That(ram[i], Is.EqualTo(expected));
            }

            ram.Reset();

            for (uint i = 0; i < 2000000; i += 100000)
            {
                Assert.That(ram[i], Is.EqualTo(0));
            }
        }

        [Test]
        public void WrittenValueShouldBeOverridable()
        {
            Memory ram = new Memory();

            for (uint i = 0; i < 2000000; i += 100000)
            {
                var expected = i + 5;

                ram[i] = (int)expected;
                ram[i]++;

                Assert.That(ram[i], Is.EqualTo(expected + 1));
            }
        }

        [Test]
        public void UsageCountShouldShowCorrectValue()
        {
            Memory ram = new Memory();

            Assert.That(ram.UsedCellsCount, Is.Zero);

            for (uint i = 0; i < 200; i++)
            {
                var expected = i + 5;

                ram[i] = (int)expected;
                ram[i]++;
                ram[i] = 0;
                ram[i] += 34;

                Assert.That(ram[i], Is.EqualTo(34));
                Assert.That(ram.UsedCellsCount, Is.EqualTo(i + 1));
            }

            ram.Reset();

            Assert.That(ram.UsedCellsCount, Is.Zero);
        }
    }
}
