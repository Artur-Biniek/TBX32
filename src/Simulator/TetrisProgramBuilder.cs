using System.Collections.Generic;
using System.Linq;
using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    public static class TetrisProgramBuilder
    {
        private class Labels
        {
            public CodeBuilder.Label InitGameProc { get; private set; }
            public CodeBuilder.Label CreateBoardProc { get; private set; }
            public CodeBuilder.Label GenerateTetrominoProc { get; private set; }
            public CodeBuilder.Label CanMoveBlockProc { get; private set; }

            public Labels(CodeBuilder builder)
            {
                InitGameProc = builder.CreateLabel();
                CreateBoardProc = builder.CreateLabel();
                GenerateTetrominoProc = builder.CreateLabel();
                CanMoveBlockProc = builder.CreateLabel();
            }
        }

        public const uint __BOARD = 1;
        public const uint __BLOCKS_DATA = __BOARD + 20;
        public const uint __START_LOCATION_DATA = __BLOCKS_DATA + 28;
        public const uint __NEXT_BLOCK = __START_LOCATION_DATA + 28;
        public const uint __NEXT_ROTATION = __NEXT_BLOCK + 1;
        public const uint __PLAYING = __NEXT_ROTATION + 1;


        public const Register G__CURRENT_BLOCK = R.G6;
        public const Register G__CURRENT_ROTATION = R.G5;
        public const Register G__CURRENT_ROW = R.G4;
        public const Register G__CURRENT_COL = R.G3;


        public const Register G__VIDEO_START = R.G0;

        public static IReadOnlyDictionary<uint, uint> Create()
        {
            var builder = new CodeBuilder();
            var procedureLabels = new Labels(builder);

            var programEntry = builder.CreateLabel();
            var gameLoop = builder.CreateLabel();


            var prg = builder

                .Jmp(programEntry)

                // var Board : int[20]
                .SetOrg(__BOARD)
                .Data(Enumerable.Repeat(0, 20).ToArray())

                // const BLOCKS_DATA : uint[28]
                .SetOrg(__BLOCKS_DATA)
                .Data(
                    0x0F00, 0x2222, 0x00f0, 0x4444, // I-block
                    0x8E00, 0x6440, 0x0E20, 0x44C0, // J-block
                    0x2E00, 0x4460, 0x0E80, 0xC440, // L-block
                    0x6600, 0x6600, 0x6600, 0x6600, // O-block
                    0x6C00, 0x4620, 0x06C0, 0x8C40, // S-block
                    0x4E00, 0x4640, 0x0E40, 0x4C40, // T-block
                    0xC600, 0x2640, 0x0C60, 0x4C80)  // Z-block

                // const START_LOCATION_DATA : int[28]
                .SetOrg(__START_LOCATION_DATA)
                .Data(
                    -1, 0, -2, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0,
                    0, 0, 0, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0)

                // var NextBlock : uint;
                .SetOrg(__NEXT_BLOCK)
                .Data(0)

                // var NextRotation : uint;
                .SetOrg(__NEXT_ROTATION)
                .Data(0)

                // var Playing : bool;
                .SetOrg(__PLAYING)
                .Data(0)


                .MarkLabel(programEntry)

                    .Movi_(G__VIDEO_START, Computer.VIDEO_START)

                    .Push_(R.Fp)
                    .Jal(R.Ra, procedureLabels.InitGameProc)


                .MarkLabel(gameLoop)

                    .Push_(R.Fp)
                    .Jal(R.Ra, procedureLabels.CreateBoardProc)

                    .Jmp(gameLoop)

                .Hlt();

            create_INIT_GAME(ref prg, procedureLabels);
            create_CREATE_BOARD(ref prg, procedureLabels);
            create_GENERATE_TETROMINO(ref prg, procedureLabels);
            create_CAN_MOVE_BLOCK(ref prg, procedureLabels);

            return prg.Build();
        }

        private static void create_INIT_GAME(ref CodeBuilder builder, Labels labels)
        {
            builder
                .MarkLabel(labels.InitGameProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    //code                    
                    // _sevenSegDisplay[0] = 0;
                    .Movli(R.T0, 0)
                    .Movi_(R.T1, Computer.SEG_DISP)
                    .Str(R.T0, R.T1)

                    // _nextBlock = _rnd.Next(7) * 4;
                    .Rnd_(R.T0, 7)
                    .Muli(R.T0, R.T0, 4)
                    .St(R.T0, __NEXT_BLOCK)

                    // _nextRotation = _rnd.Next(4);
                    .Rnd_(R.T0, 4)
                    .St(R.T0, __NEXT_ROTATION)

                    //  createBoard();
                    .Push_(R.Fp)
                    .Jal(R.Ra, labels.CreateBoardProc)

                    // generateTetromino();
                    .Push_(R.Fp)
                    .Jal(R.Ra, labels.GenerateTetrominoProc)

                    // _playing = true;
                    .Movli(R.T0, 1)
                    .St(R.T0, __PLAYING)

                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra)
                    ;
        }

        private static void create_CREATE_BOARD(ref CodeBuilder builder, Labels labels)
        {
            var loop1_start = builder.CreateLabel();
            var loop1_end = builder.CreateLabel();
            var loop2_start = builder.CreateLabel();
            var loop2_end = builder.CreateLabel();
            var loop3_start = builder.CreateLabel();
            var loop3_end = builder.CreateLabel();

            builder
              .MarkLabel(labels.CreateBoardProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    //code
                    .Movli(R.T0, 0)
                    .Movli(R.T1, 32)
                    .Movli(R.T7, 1)

                    .MarkLabel(loop1_start)
                        .Bge(R.T0, R.T1, loop1_end)

                        .Add(R.T2, G__VIDEO_START, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc_(R.T0)
                        .Jmp(loop1_start)
                    .MarkLabel(loop1_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)
                    .Movi_(R.T7, 0x80100000)

                    .MarkLabel(loop2_start)
                        .Bge(R.T0, R.T1, loop2_end)

                        .Str(R.T7, R.T0, (short)__BOARD)

                        .Inc_(R.T0)
                        .Jmp(loop2_start)
                    .MarkLabel(loop2_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)

                    .MarkLabel(loop3_start)
                        .Bge(R.T0, R.T1, loop3_end)

                        .Ldr(R.T7, R.T0, (short)__BOARD)
                        .Strx(R.T7, G__VIDEO_START, R.T0)

                        .Inc_(R.T0)
                        .Jmp(loop3_start)
                    .MarkLabel(loop3_end)

                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
        }

        private static void create_GENERATE_TETROMINO(ref CodeBuilder builder, Labels labels)
        {
            builder
                .MarkLabel(labels.GenerateTetrominoProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // _curBlock = _nextBlock;
                    .Ld(R.T0, __NEXT_BLOCK)
                    .Mov_(G__CURRENT_BLOCK, R.T0)

                    // _curRotation = _nextRotation;
                    .Ld(R.T0, __NEXT_ROTATION)
                    .Mov_(G__CURRENT_ROTATION, R.T0)

                    // _nextBlock = _rnd.Next(7) * 4;
                    .Rnd_(R.T0, 7)
                    .Muli(R.T0, R.T0, 4)
                    .St(R.T0, __NEXT_BLOCK)

                    // _nextRotation = _rnd.Next(4);
                    .Rnd_(R.T0, 4)
                    .St(R.T0, __NEXT_ROTATION)

                    // _curRow = START_LOCATION_DATA[_curBlock + _curRotation];
                    .Add(R.T0, G__CURRENT_BLOCK, G__CURRENT_ROTATION)
                    .Ldr(G__CURRENT_ROW, R.T0, (short)__START_LOCATION_DATA)

                    // _curCol = 3;
                    .Movli(G__CURRENT_COL, 3)

                    // _playing = canMoveBlock(_curCol, _curRow, _curRotation)
                    .Push_(R.Fp)
                    .Push_(G__CURRENT_ROTATION)
                    .Push_(G__CURRENT_ROW)
                    .Push_(G__CURRENT_COL)
                    .Jal(R.Ra, labels.CanMoveBlockProc)
                    .St(R.V, __PLAYING)

                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra)
                    ;
        }

        private static void create_CAN_MOVE_BLOCK(ref CodeBuilder builder, Labels labels)
        {
            builder
                .MarkLabel(labels.CanMoveBlockProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    //code
                    // return true
                    .Movli(R.V, 1)

                    //epilog
                    .Pop_(R.Ra)
                    .Addi(R.Sp, R.Sp, 3)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
            ;
        }
    }
}
