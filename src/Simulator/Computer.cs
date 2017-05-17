using System.Collections.Generic;

namespace ArturBiniek.Tbx32.Simulator
{
    public class Computer
    {
        #region Memory Map
        public const uint VIDEO_END     = 0x001FFFFF;
        public const uint VIDEO_START   = 0x001FFFE0;

        public const uint SEG_DISP      = 0x001FFFDF;

        public const uint GAME_PAD      = 0x001FFFDE;

        public const uint TICKS_HIGH    = 0x001FFFDD;
        public const uint TICKS_LOW     = 0x001FFFDC;

        public const uint STACK_BOTTOM  = 0x001FFEFF;
        #endregion

        uint _PC;
        Memory _ram = new Memory();
        Registers _regs = new Registers();

        public Computer()
        {
            Reset();
        }

        public void Reset()
        {
            _PC = 0u;
            _ram.Reset();
            _regs.Reset();
        }

        public void LoadProgram(Dictionary<uint, uint> program)
        {
            foreach (var data in program)
            {
                _ram[data.Key] = data.Value;
            }
        }

        
    }
}
