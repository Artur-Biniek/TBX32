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

        public int this[byte index]
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
    }
}
