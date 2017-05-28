using System.Collections.Generic;
using System.Linq;
using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    public static class TetrisProgramBuilder
    {
        public static IReadOnlyDictionary<uint, uint> Create()
        {
            var builder = new CodeBuilder();

            var programEntry = builder.CreateLabel();

            var initGameProc = builder.CreateLabel();
            var createBoardProc = builder.CreateLabel();

            var prg = builder

                .Jmp(programEntry)

                // @1: var Board : int[20]
                .Data(Enumerable.Repeat(0, 20).ToArray())

                // @21: const BLOCKS_DATA : uint[28]
                .Data(
                    0x0F00, 0x2222, 0x00f0, 0x4444, // I-block
                    0x8E00, 0x6440, 0x0E20, 0x44C0, // J-block
                    0x2E00, 0x4460, 0x0E80, 0xC440, // L-block
                    0x6600, 0x6600, 0x6600, 0x6600, // O-block
                    0x6C00, 0x4620, 0x06C0, 0x8C40, // S-block
                    0x4E00, 0x4640, 0x0E40, 0x4C40, // T-block
                    0xC600, 0x2640, 0x0C60, 0x4C80)  // Z-bloc
                                                     // @49: 
                .Data(0xC0FFEE)


                .MarkLabel(programEntry)

                    .Movi_(R.G0, Computer.VIDEO_START)     // G0 <- video ram start
                    .Movli(R.G1, 1)     // G1 <- board array

                    .Push_(R.Fp)
                    .Jal(R.Ra, initGameProc)

                    .Push_(R.Fp)
                    .Jal(R.Ra, createBoardProc)


                    .Hlt();

            create_INIT_GAME(ref prg, initGameProc);
            create_CREATE_BOARD(ref prg, createBoardProc);

            return prg.Build();
        }

        private static void create_INIT_GAME(ref CodeBuilder builder, CodeBuilder.Label initGameProc)
        {
            builder
                .MarkLabel(initGameProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    //code                    
                    // _sevenSegDisplay[0] = 0;
                    .Movli(R.T0, 0)
                    .Movi_(R.T1, Computer.SEG_DISP)
                    .Str(R.T0, R.T1)

                    //_nextBlock = _rnd.Next(7) * 4;



                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra)
                    ;
        }

        private static void create_CREATE_BOARD(ref CodeBuilder builder, CodeBuilder.Label createBoard)
        {
            var loop1_start = builder.CreateLabel();
            var loop1_end = builder.CreateLabel();
            var loop2_start = builder.CreateLabel();
            var loop2_end = builder.CreateLabel();
            var loop3_start = builder.CreateLabel();
            var loop3_end = builder.CreateLabel();

            builder
              .MarkLabel(createBoard)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    //code
                    .Movli(R.T0, 0)
                    .Movli(R.T1, 32)
                    .Movli(R.T7, 1)

                    .MarkLabel(loop1_start)
                        .Bge(R.T0, R.T1, loop1_end)

                        .Add(R.T2, R.G0, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc_(R.T0)
                        .Jmp(loop1_start)
                    .MarkLabel(loop1_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)
                    .Movi_(R.T7, 0x80100000)

                    .MarkLabel(loop2_start)
                        .Bge(R.T0, R.T1, loop2_end)

                        .Add(R.T2, R.G1, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc_(R.T0)
                        .Jmp(loop2_start)
                    .MarkLabel(loop2_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)

                    .MarkLabel(loop3_start)
                        .Bge(R.T0, R.T1, loop3_end)

                        .Ldrx(R.T7, R.G1, R.T0)
                        .Strx(R.T7, R.G0, R.T0)

                        .Inc_(R.T0)
                        .Jmp(loop3_start)
                    .MarkLabel(loop3_end)

                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
        }
    }
}
