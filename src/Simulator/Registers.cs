using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    public class Registers
    {
        const int SIZE = 32;

        int[] _regs = new int[SIZE];

        public void Reset()
        {
            _regs = new int[SIZE];
        }

        public int this[int index]
        {
            get
            {
                return _regs[index];
            }

            set
            {
                _regs[index] = value;
            }
        }

        public int this[Register reg]
        {
            get
            {
                return this[(int)reg];
            }

            set
            {
                this[(int)reg] = value;
            }
        }
    }
}
