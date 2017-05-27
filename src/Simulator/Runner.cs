using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ArturBiniek.Tbx32.Simulator
{
    internal class Runner
    {
        private Computer _comp;

        Timer _timer;
        private Action<int[]> _callback;

        public Runner(IReadOnlyDictionary<uint, uint> bouncingBallProgram, Action<int[]> callback)
        {
            _comp = new Computer();
            _comp.LoadProgram(bouncingBallProgram);

            var t = Task.Factory.StartNew(TimerLoop);
        }

        public async void TimerLoop()
        {
            bool status = true;
            DateTime last = DateTime.Now;

            while (status)
            {
                if (DateTime.Now - last < TimeSpan.FromMilliseconds(2)) continue;

                lock (_comp)
                {
                    status = _comp.Step();

                    if (!status) _timer.Dispose();
                }

                last = DateTime.Now;
            }
        }

        public int[] GetRam()
        {
            lock (_comp)
            {
                int[] arr = new int[32];
                for (int i = 0; i < 32; i++)
                {
                    arr[i] = _comp[Computer.VIDEO_START + (uint)i];
                }

                return arr;
            }
        }
    }
}