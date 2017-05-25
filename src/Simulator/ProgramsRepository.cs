using System.Collections.Generic;
using System.Linq;
using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    public static class ProgramsRepository
    {
        public static IReadOnlyDictionary<uint, uint> DiagonalLineProgram
        {
            get { return createDiagonalLineProgram(); }
        }

        public static IReadOnlyDictionary<uint, uint> DiagonalLineProgramWithTimeDependance
        {
            get { return createClockDependendDiagonalLineProgram(); }
        }

        public static IReadOnlyDictionary<uint, uint> Tetris
        {
            get { return createTetris(); }
        }

        private static IReadOnlyDictionary<uint, uint> createDiagonalLineProgram()
        {
            var builder = new CodeBuilder();

            var video = builder.CreateLabel();
            var putPixel = builder.CreateLabel();
            var whileLoop = builder.CreateLabel();
            var exitLoop = builder.CreateLabel();

            var prg = builder

                        .Ld(R.G0, video)
                        .Movli(R.S0, 0)
                        .Movli(R.S1, 31)

                        .MarkLabel(whileLoop)
                            .Bgt(R.S0, R.S1, exitLoop)
                            .Push(R.Fp)
                            .Push(R.S0)
                            .Push(R.S0)
                            .Jal(R.Ra, putPixel)
                            .Addi(R.S0, R.S0, 1)
                            .Jmp(whileLoop)

                        .MarkLabel(exitLoop)
                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov(R.Fp, R.Sp)
                            .Push(R.Ra)

                            .Ldr(R.T0, R.Fp, 1)        // T0 <- x
                            .Ldr(R.T1, R.Fp, 2)        // T1 <- y
                            .Add(R.T2, R.G0, R.T0)     // T2 <- VIDEO + x
                            .Movli(R.T3, 1)
                            .Movli(R.T4, 31)
                            .Sub(R.T4, R.T4, R.T1)
                            .Shl(R.T4, R.T3, R.T4)
                            .Ldr(R.T3, R.T2)
                            .Or(R.T3, R.T3, R.T4)
                            .Str(R.T3, R.T2)

                            // epilog
                            .Pop(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop(R.Fp)
                            .Jmpr(R.Ra)

                        .MarkLabel(video)
                            .Data((int)Computer.VIDEO_START)

                        .Build();

            return prg;
        }

        private static IReadOnlyDictionary<uint, uint> createClockDependendDiagonalLineProgram()
        {
            var builder = new CodeBuilder();

            var video = builder.CreateLabel();
            var lowClock = builder.CreateLabel();
            var putPixel = builder.CreateLabel();
            var whileLoop = builder.CreateLabel();
            var exitLoop = builder.CreateLabel();

            var prg = builder

                        .Ld(R.G0, video)
                        .Ld(R.G1, lowClock)
                        .Movli(R.S0, 0)
                        .Movli(R.S1, 31)
                        .Ldr(R.S2, R.G1)           // S2 <- old time

                        .MarkLabel(whileLoop)
                            .Bgt(R.S0, R.S1, exitLoop)
                            .Ldr(R.T0, R.G1)        // T0 <- current time
                            .Sub(R.T0, R.T0, R.S2)  // T0 <- old time - current time
                            .Movli(R.T1, 1000)       // T1 <- 1000ms delay
                            .Blt(R.T0, R.T1, whileLoop) // jump back to the begining if less than 1000ms
                            .Ldr(R.S2, R.G1)
                            .Push(R.Fp)
                            .Push(R.S0)
                            .Push(R.S0)
                            .Jal(R.Ra, putPixel)
                            .Addi(R.S0, R.S0, 1)
                            .Jmp(whileLoop)

                        .MarkLabel(exitLoop)
                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov(R.Fp, R.Sp)
                            .Push(R.Ra)

                            .Ldr(R.T0, R.Fp, 1)        // T0 <- x
                            .Ldr(R.T1, R.Fp, 2)        // T1 <- y
                            .Add(R.T2, R.G0, R.T0)     // T2 <- VIDEO + x
                            .Movli(R.T3, 1)
                            .Movli(R.T4, 31)
                            .Sub(R.T4, R.T4, R.T1)
                            .Shl(R.T4, R.T3, R.T4)
                            .Ldr(R.T3, R.T2)
                            .Or(R.T3, R.T3, R.T4)
                            .Str(R.T3, R.T2)

                            // epilog
                            .Pop(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop(R.Fp)
                            .Jmpr(R.Ra)

                        .MarkLabel(video)
                            .Data((int)Computer.VIDEO_START)
                        .MarkLabel(lowClock)
                            .Data((int)Computer.TICKS_LOW)

                        .Build();

            return prg;
        }

        private static IReadOnlyDictionary<uint, uint> createTetris()
        {
            var builder = new CodeBuilder();

            var createBoard = builder.CreateLabel();
            var createBoard_const1 = builder.CreateLabel();
            var createBoard_const2 = builder.CreateLabel();
            var createBoard_loop1_start = builder.CreateLabel();
            var createBoard_loop1_end = builder.CreateLabel();
            var createBoard_loop2_start = builder.CreateLabel();
            var createBoard_loop2_end = builder.CreateLabel();

            var video = builder.CreateLabel();
            var board = builder.CreateLabel();

            var prg = builder.Nop()

                .Ld(R.G0, video)
                .Ld(R.G1, board)

                .Push(R.Fp)
                .Jal(R.Ra, createBoard)




                .Hlt()

                .MarkLabel(createBoard)
                    //prolog
                    .Mov(R.Fp, R.Sp)
                    .Push(R.Ra)

                    //code
                    .Movli(R.T0, 0)
                    .Movli(R.T1, 32)
                    .Movli(R.T7, 1)

                    .MarkLabel(createBoard_loop1_start)
                        .Bge(R.T0, R.T1, createBoard_loop1_end)

                        .Add(R.T2, R.G0, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc(R.T0)
                        .Jmp(createBoard_loop1_start)
                    .MarkLabel(createBoard_loop1_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)
                    .Ld(R.T7, createBoard_const1)

                    .MarkLabel(createBoard_loop2_start)
                        .Bge(R.T0, R.T1, createBoard_loop2_end)

                        .Add(R.T2, R.G0, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc(R.T0)
                        .Jmp(createBoard_loop2_start)
                    .MarkLabel(createBoard_loop2_end)

                    //epilog
                    .Pop(R.Ra)
                    .Pop(R.Fp)
                    .Jmpr(R.Ra)

                    .MarkLabel(createBoard_const1)
                        .Data(0x80100000)
                    .MarkLabel(createBoard_const2)
                        .Data(0xFFF00000)

                .MarkLabel(video)
                    .Data(Computer.VIDEO_START)



                .MarkLabel(board)
                    .Data(Enumerable.Repeat(0, 20).ToArray());
                ;

            return prg.Build();
        }
    }
}
