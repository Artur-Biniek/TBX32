using System.Collections.Generic;

namespace ArturBiniek.Tbx32.Simulator
{
    public class Memory
    {
        Dictionary<uint, uint> _ram = new Dictionary<uint, uint>();

        public int UsedCellsCount
        {
            get
            {
                return _ram.Keys.Count;
            }
        }

        public void Reset()
        {
            _ram = new Dictionary<uint, uint>();
        }

        public uint this[uint address]
        {
            get
            {
                if (!_ram.ContainsKey(address))
                {
                    _ram[address] = 0u;
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
