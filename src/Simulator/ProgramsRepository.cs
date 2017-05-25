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

        public static IReadOnlyDictionary<uint, uint> ClockDependendRandomDotsProgram
        {
            get { return createClockDependendRandomDotsProgram(); }
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
                            .Movli(R.T3, 1)
                            .Movli(R.T4, 31)
                            .Sub(R.T4, R.T4, R.T1)
                            .Shl(R.T4, R.T3, R.T4)
                            .Ldrx(R.T3, R.G0, R.T0)
                            .Or(R.T3, R.T3, R.T4)
                            .Strx(R.T3, R.G0, R.T0)

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

        private static IReadOnlyDictionary<uint, uint> createClockDependendRandomDotsProgram()
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

                            .Rnd(R.T2)              // T2 <- rnd 0..31
                            .Modi(R.T2, R.T2, 32)
                            .Push(R.T2)

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
                            .Movli(R.T3, 1)
                            .Movli(R.T4, 31)
                            .Sub(R.T4, R.T4, R.T1)
                            .Shl(R.T4, R.T3, R.T4)
                            .Ldrx(R.T3, R.G0, R.T0)
                            .Or(R.T3, R.T3, R.T4)
                            .Strx(R.T3, R.G0, R.T0)

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

            var programEntry = builder.CreateLabel();

            var createBoard = builder.CreateLabel();

            var createBoard_loop1_start = builder.CreateLabel();
            var createBoard_loop1_end = builder.CreateLabel();
            var createBoard_loop2_start = builder.CreateLabel();
            var createBoard_loop2_end = builder.CreateLabel();
            var createBoard_loop3_start = builder.CreateLabel();
            var createBoard_loop3_end = builder.CreateLabel();

            var prg = builder
                
                .Jmp(programEntry)

                .Data(Enumerable.Repeat(0, 20).ToArray())

                .MarkLabel(programEntry)

                    .Movi(R.G0, Computer.VIDEO_START)     // G0 <- video ram start
                    .Movli(R.G1, 1)     // G1 <- board array

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
                    .Movi(R.T7, 0x80100000)

                    .MarkLabel(createBoard_loop2_start)
                        .Bge(R.T0, R.T1, createBoard_loop2_end)

                        .Add(R.T2, R.G1, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc(R.T0)
                        .Jmp(createBoard_loop2_start)
                    .MarkLabel(createBoard_loop2_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)                    

                    .MarkLabel(createBoard_loop3_start)
                        .Bge(R.T0, R.T1, createBoard_loop3_end)

                        .Ldrx(R.T7, R.G1, R.T0)
                        .Strx(R.T7, R.G0, R.T0)

                        .Inc(R.T0)
                        .Jmp(createBoard_loop3_start)
                    .MarkLabel(createBoard_loop3_end)

                    //epilog
                    .Pop(R.Ra)
                    .Pop(R.Fp)
                    .Jmpr(R.Ra)

                ;

            return prg.Build();
        }
    }
}
