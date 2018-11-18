using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static IReadOnlyDictionary<uint, uint> BouncingBallProgram
        {
            get { return createBouncingBallProgram(); }
        }

        public static IReadOnlyDictionary<uint, uint> Tetris
        {
            get { return TetrisProgramBuilder.Create(); }
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
                            .Push_(R.Fp)
                            .Push_(R.S0)
                            .Push_(R.S0)
                            .Jal(R.Ra, putPixel)
                            .Addi(R.S0, R.S0, 1)
                            .Jmp(whileLoop)

                        .MarkLabel(exitLoop)
                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov_(R.Fp, R.Sp)
                            .Push_(R.Ra)

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
                            .Pop_(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop_(R.Fp)
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
                            .Movli(R.T1, 200)       // T1 <- 200ms delay
                            .Blt(R.T0, R.T1, whileLoop) // jump back to the begining if less than 200ms
                            .Ldr(R.S2, R.G1)

                            .Push_(R.Fp)

                            .Push_(R.S0)

                            .Rnd(R.T2)              // T2 <- rnd 0..31
                            .Modi(R.T2, R.T2, 32)
                            .Push_(R.T2)

                            .Jal(R.Ra, putPixel)
                            .Addi(R.S0, R.S0, 1)
                            .Jmp(whileLoop)

                        .MarkLabel(exitLoop)
                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov_(R.Fp, R.Sp)
                            .Push_(R.Ra)

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
                            .Pop_(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop_(R.Fp)
                            .Jmpr(R.Ra)

                        .MarkLabel(video)
                            .Data((int)Computer.VIDEO_START)
                        .MarkLabel(lowClock)
                            .Data((int)Computer.TICKS_LOW)

                        .Build();

            return prg;
        }

        private static IReadOnlyDictionary<uint, uint> createBouncingBallProgram()
        {
            var builder = new CodeBuilder();

            var video = builder.CreateLabel();
            var putPixel = builder.CreateLabel();
            var whileLoop = builder.CreateLabel();

            var if1end = builder.CreateLabel();
            var if2end = builder.CreateLabel();
            var if3end = builder.CreateLabel();
            var if4end = builder.CreateLabel();

            var putPixel_if1end = builder.CreateLabel();
            var putPixel_if1else = builder.CreateLabel();

            var prg = builder

                        .Movi_(R.G0, Computer.VIDEO_START)

                        .Movli(R.S0, 0)  // x
                        .Movli(R.S1, 7)  // y
                        .Movli(R.S2, 1)  // dx
                        .Movli(R.S3, 1)  // dy

                        .Movli(R.T0, 1)

                        .MarkLabel(whileLoop)
                            .Dec_(R.T0)
                            .Brnz(R.T0, whileLoop)


                            .Movli(R.T0, 0)
                            .Movli(R.T1, -1)
                            .Bneq(R.S0, R.T0, if1end)
                            .Bneq(R.S2, R.T1, if1end)
                            .Movli(R.S2, 1)

                        .MarkLabel(if1end)

                            .Movli(R.T0, 19)
                            .Movli(R.T1, 1)
                            .Bneq(R.S0, R.T0, if2end)
                            .Bneq(R.S2, R.T1, if2end)
                            .Movli(R.S2, -1)

                        .MarkLabel(if2end)

                            .Movli(R.T0, 0)
                            .Movli(R.T1, -1)
                            .Bneq(R.S1, R.T0, if3end)
                            .Bneq(R.S3, R.T1, if3end)
                            .Movli(R.S3, 1)

                        .MarkLabel(if3end)

                            .Movli(R.T0, 31)
                            .Movli(R.T1, 1)
                            .Bneq(R.S1, R.T0, if4end)
                            .Bneq(R.S3, R.T1, if4end)
                            .Movli(R.S3, -1)

                        .MarkLabel(if4end)

                            .Push_(R.S1)

                            .Add(R.S0, R.S0, R.S2)
                            .Add(R.S1, R.S1, R.S3)

                            .Push_(R.Fp)
                            .Push_(R.S0)
                            .Push_(R.S1)
                            .Jal(R.Ra, putPixel)

                            .Pop_(R.T1)
                            .Movli(R.T0, 0xFFF)
                            .Strx(R.T0, R.G0, R.T1)


                            .Movli(R.T0, 80)

                            .Jmp(whileLoop)

                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov_(R.Fp, R.Sp)
                            .Push_(R.Ra)

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
                            .Pop_(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop_(R.Fp)
                            .Jmpr(R.Ra)

                            .SetOrg(Computer.VIDEO_START)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                            .Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF).Data(0x00000FFF)
                        .Build();

            return prg;
        }


        public static IReadOnlyDictionary<uint, uint> Test0
        {
            get { return test0(); }
        }

        private static IReadOnlyDictionary<uint, uint> test0()
        {
            var builder = new CodeBuilder();
            var d0 = builder.CreateLabel();
            var d1 = builder.CreateLabel();
            var d2 = builder.CreateLabel();
            var d3 = builder.CreateLabel();
            var d4 = builder.CreateLabel();
            var d5 = builder.CreateLabel();
            var start = builder.CreateLabel();

            var prg = builder
                .Jmp(start)
                .SetOrg(10)
                .MarkLabel(start)


                    .Ldr(Register.R1, Register.R2, -5)
                    

                    .Hlt()

//.SetOrg(0x001FFFE0)
.SetOrg(15)

                    .MarkLabel(d1)
                   

                    .Data(666).MarkLabel(d2)
                    .Data(0).MarkLabel(d3)
                    .Data(0).MarkLabel(d4)
                    .Data(0).MarkLabel(d5)
                
                ;

            var built = prg.Build();
            
            return built;
        }

        public static string ToString(IReadOnlyDictionary<uint, uint> prg)
        {
            var max = prg.Keys.Max();
            var sb = new StringBuilder();

            for (uint i = 0; i <= max; i++)
            {
                if (prg.ContainsKey(i))
                {
                    sb.Append(string.Format("{0:X} ", prg[i]));
                }
                else
                {
                    sb.Append("0 ");
                }
            }

            return sb.ToString();
        }
    }
}
