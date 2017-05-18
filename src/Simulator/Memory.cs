using System.Collections.Generic;

namespace ArturBiniek.Tbx32.Simulator
{
    public class Memory
    {
        Dictionary<uint, int> _ram = new Dictionary<uint, int>();

        public int UsedCellsCount
        {
            get
            {
                return _ram.Keys.Count;
            }
        }

        public void Reset()
        {
            _ram = new Dictionary<uint, int>();
        }

        public int this[uint address]
        {
            get
            {
                if (!_ram.ContainsKey(address))
                {
                    _ram[address] = 0;
                }

                return _ram[address];
            }

            set
            {
                _ram[address] = value;
            }
        }
    }
}
