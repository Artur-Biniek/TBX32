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
            public CodeBuilder.Label MergeBlockProc { get; private set; }
            public CodeBuilder.Label BlitBlockProc { get; private set; }
            public CodeBuilder.Label GameLoopProc { get; private set; }
            public CodeBuilder.Label CheckLinesProc { get; private set; }

            public Labels(CodeBuilder builder)
            {
                InitGameProc = builder.CreateLabel();
                CreateBoardProc = builder.CreateLabel();
                GenerateTetrominoProc = builder.CreateLabel();
                CanMoveBlockProc = builder.CreateLabel();
                MergeBlockProc = builder.CreateLabel();
                BlitBlockProc = builder.CreateLabel();
                GameLoopProc = builder.CreateLabel();
                CheckLinesProc = builder.CreateLabel();
            }
        }

        public const uint __BOARD = 1;
        public const uint __BLOCKS_DATA = __BOARD + 20;
        public const uint __START_LOCATION_DATA = __BLOCKS_DATA + 28;
        public const uint __NEXT_BLOCK = __START_LOCATION_DATA + 28;
        public const uint __NEXT_ROTATION = __NEXT_BLOCK + 1;
        public const uint __PLAYING = __NEXT_ROTATION + 1;
        public const uint __FULL_LINE_MASK = __PLAYING + 1;
        public const uint __COPY_LINE_MASK = __FULL_LINE_MASK + 1;
        public const uint __CURRENT_DELAY = __COPY_LINE_MASK + 1;

        public const Register G__CURRENT_BLOCK = R.G6;
        public const Register G__CURRENT_ROTATION = R.G5;
        public const Register G__CURRENT_ROW = R.G4;
        public const Register G__CURRENT_COL = R.G3;

        public const Register G__LAST_KEYBOARD = R.G2;
        public const Register G__LAST_UPDATE_TIME = R.G1;
        public const Register G__VIDEO_START = R.G0;

        public const short CONSTVAL_BOARD_HEIGHT = 20;
        public const short CONSTVAL_BOARD_WIDTH = 10;

        const short CONSTVAL_BOARD_HORIZONTAL_SHIFT = 11;
        const short CONSTVAL_BOARD_VERTICAL_SHIFT = 6;

        public const uint CONSTVAL_FULL_LINE_MASK = 0x03FF;
        public const uint CONSTVAL_COPY_LINE_MASK = ~(CONSTVAL_FULL_LINE_MASK << CONSTVAL_BOARD_HORIZONTAL_SHIFT);

        public const short CONSTVAL_REPEAT_RATE_SLOW = 200;
        public const short CONSTVAL_REPEAT_RATE_FAST = 100;
        public const short CONSTVAL_REPEAT_RATE_RAPID = 40;

        public static IReadOnlyDictionary<uint, uint> Create()
        {
            var builder = new CodeBuilder();
            var procedureLabels = new Labels(builder);

            var programEntry = builder.CreateLabel();

            var prg = builder

                .Jmp(procedureLabels.GameLoopProc)

                // int[20] Board ;
                .SetOrg(__BOARD)
                .Data(Enumerable.Repeat(0, 20).ToArray())

                // const uint[28] BLOCKS_DATA;
                .SetOrg(__BLOCKS_DATA)
                .Data(
                    0x0F00, 0x2222, 0x00f0, 0x4444, // I-block
                    0x8E00, 0x6440, 0x0E20, 0x44C0, // J-block
                    0x2E00, 0x4460, 0x0E80, 0xC440, // L-block
                    0x6600, 0x6600, 0x6600, 0x6600, // O-block
                    0x6C00, 0x4620, 0x06C0, 0x8C40, // S-block
                    0x4E00, 0x4640, 0x0E40, 0x4C40, // T-block
                    0xC600, 0x2640, 0x0C60, 0x4C80)  // Z-block

                // const int[28] START_LOCATION_DATA;
                .SetOrg(__START_LOCATION_DATA)
                .Data(
                    -1, 0, -2, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0,
                    0, 0, 0, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0,
                    0, 0, -1, 0)

                // uint NextBlock;
                .SetOrg(__NEXT_BLOCK)
                .Data(0)

                // uint NextRotation;
                .SetOrg(__NEXT_ROTATION)
                .Data(0)

                // bool Playing;
                .SetOrg(__PLAYING)
                .Data(0)

                //  const uint FULL_LINE_MASK = 0x03FF;
                .SetOrg(__FULL_LINE_MASK)
                .Data(CONSTVAL_FULL_LINE_MASK)

                // const uint COPY_LINE_MASK = ~(FULL_LINE_MASK << BOARD_HORIZONTAL_SHIFT);
                .SetOrg(__COPY_LINE_MASK)
                .Data(CONSTVAL_COPY_LINE_MASK)

                // int _curDelay = 2500;
                .SetOrg(__CURRENT_DELAY)
                .Data(2500)
                ;

            create_INIT_GAME(ref prg, procedureLabels);
            create_CREATE_BOARD(ref prg, procedureLabels);
            create_GENERATE_TETROMINO(ref prg, procedureLabels);
            create_CAN_MOVE_BLOCK(ref prg, procedureLabels);
            create_MERGE_BLOCK(ref prg, procedureLabels);
            create_BLIT_BLOCK(ref prg, procedureLabels);
            create_CHECK_LINES(ref prg, procedureLabels);
            create_GAME_LOOP(ref prg, procedureLabels);

            return prg.Build();
        }

        private static void create_INIT_GAME(ref CodeBuilder builder, Labels labels)
        {
            builder
                .MarkLabel(labels.InitGameProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

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

            builder
              .MarkLabel(labels.CreateBoardProc)

                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // const int zero = 0
                    .Movli(R.T14, 0)

                    // const int walls = 0x00200400
                    .Movi_(R.T13, 0x00200400)

                    // const int floor = 0x003FFC00
                    .Movi_(R.T12, 0x003FFC00)

                    // for (int i = 0; i < BOARD_HEIGHT; i += 1)
                    .Movli(R.T0, 0)
                    .Movli(R.T1, CONSTVAL_BOARD_HEIGHT)
                    .MarkLabel(loop1_start)
                    .Bge(R.T0, R.T1, loop1_end)
                        // {
                        // _board[i] = zero;
                        .Str(R.T14, R.T0, (short)__BOARD)

                        // _screenMemory[i + BOARD_VERTICAL_SHIFT] = 0x00200400;
                        .Addi(R.T2, R.T0, CONSTVAL_BOARD_VERTICAL_SHIFT)
                        .Strx(R.T13, G__VIDEO_START, R.T2)

                    .Inc_(R.T0)
                    .Jmp(loop1_start)
                    .MarkLabel(loop1_end)
                    // }

                  

                    // _screenMemory[i + BOARD_VERTICAL_SHIFT] = 0x003FFC00;
                    .Addi(R.T2, R.T0, CONSTVAL_BOARD_VERTICAL_SHIFT)
                    .Strx(R.T12, G__VIDEO_START, R.T2)

                   

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
            var outerLoopStart = builder.CreateLabel();
            var outerLoopEnd = builder.CreateLabel();

            var innerLoopStart = builder.CreateLabel();
            var innerLoopEnd = builder.CreateLabel();

            var endIf = builder.CreateLabel();

            var returnFalse = builder.CreateLabel();
            var returnTrue = builder.CreateLabel();

            var epilog = builder.CreateLabel();

            var mask = R.T14;
            var block = R.T13;
            var x = R.T12;
            var y = R.T11;
            var drow = R.T10;
            var dcol = R.T9;
            var boardWidth = R.T8;
            var boardHeight = R.T7;

            var r = R.T0;
            var c = R.T1;
            var loopLimit = R.T2;

            var tmp1 = R.T3;
            var tmp2 = R.T4;
            var tmp3 = R.T5;

            builder
                .MarkLabel(labels.CanMoveBlockProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)


                    // load drow & dcol args to dedicated registers
                    .Ldr(dcol, R.Fp, 1)
                    .Ldr(drow, R.Fp, 2)

                    // uint mask = 0x8000;
                    .Movi_(mask, 0x8000)

                    // init useful constants
                    .Movli(boardWidth, CONSTVAL_BOARD_WIDTH)
                    .Movli(boardHeight, CONSTVAL_BOARD_HEIGHT)

                    // uint block = BLOCKS_DATA[_curBlock + drot];
                    .Ldr(R.AsmRes, R.Fp, 3)
                    .Add(R.AsmRes, G__CURRENT_BLOCK, R.AsmRes)
                    .Ldr(block, R.AsmRes, (short)__BLOCKS_DATA)

                    // for (int r = 0; r < 4; r++)
                    .Movli(loopLimit, 4)
                    .Movli(r, 0)
                    .MarkLabel(outerLoopStart)
                    .Bge(r, loopLimit, outerLoopEnd)
                        // {

                        // for (int c = 0; c < 4; c++)
                        .Movli(c, 0)
                        .MarkLabel(innerLoopStart)
                        .Bge(c, loopLimit, innerLoopEnd)
                            // {

                            // if ((mask & block) != 0)
                            .And(tmp1, mask, block)
                            .Brz(tmp1, endIf)
                                // {
                                // x = c + dcol;
                                .Add(x, c, dcol)

                                // y = r + drow;
                                .Add(y, r, drow)

                                // if (x < 0) return false;
                                .Brlz(x, returnFalse)

                                // if (y < 0) return false;
                                .Brlz(y, returnFalse)

                                // if (x >= BOARD_WIDTH) return false;
                                .Bge(x, boardWidth, returnFalse)

                                // if (y >= BOARD_HEIGHT) return false;
                                .Bge(y, boardHeight, returnFalse)

                                // var line = _board[y];
                                .Ldr(tmp1, y, (short)__BOARD)

                                //var bit = BOARD_WIDTH - 1 - x;
                                .Addi(tmp2, boardWidth, -1)
                                .Sub(tmp2, tmp2, x)

                                // if ((line & (1 << bit)) != 0) return false;
                                .Movli(tmp3, 1)
                                .Shl(tmp3, tmp3, tmp2)
                                .And(tmp3, tmp1, tmp3)
                                .Brnz(tmp3, returnFalse)

                            .MarkLabel(endIf)
                            // }

                            // mask >>= 1;
                            .Shri(mask, mask, 1)

                        .Inc_(c)
                        .Jmp(innerLoopStart)
                        .MarkLabel(innerLoopEnd)
                    // }

                    .Inc_(r)
                    .Jmp(outerLoopStart)
                    .MarkLabel(outerLoopEnd)
                    // }

                    // return true;
                    .MarkLabel(returnTrue)
                    .Movli(R.V, 1)
                    .Jmp(epilog)

                    .MarkLabel(returnFalse)
                    .Movli(R.V, 0)

                    .MarkLabel(epilog)
                    //epilog
                    .Pop_(R.Ra)
                    .Addi(R.Sp, R.Sp, 3)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
            ;
        }

        private static void create_MERGE_BLOCK(ref CodeBuilder builder, Labels labels)
        {
            var forLoopStart = builder.CreateLabel();
            var forLoopContinue = builder.CreateLabel();
            var forLoopEnd = builder.CreateLabel();
            var elseBranch = builder.CreateLabel();
            var endIf = builder.CreateLabel();

            var mask = R.T14;
            var offset = R.T13;
            var shft = R.T12;
            var mem = R.T11;
            var strip = R.T10;
            var block = R.T9;


            builder
                .MarkLabel(labels.MergeBlockProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // uint mask = 0xF000;
                    .Movi_(mask, 0xF000)

                    // int offset = 6; 
                    .Movli(offset, 6)

                    // int shft = 0; 
                    .Movli(shft, 0)

                    // uint block = BLOCKS_DATA[_curBlock + _curRotation];
                    .Add(R.T0, G__CURRENT_BLOCK, G__CURRENT_ROTATION)
                    .Ldr(block, R.T0, (short)__BLOCKS_DATA)

                    // T0 --> i
                    // T1 --> _curRow + 4
                    // i = _curRow;

                    .Addi(R.T1, G__CURRENT_ROW, 4)

                    //for (i = _curRow; i < _curRow + 4; i++, mask >>= 4, offset -= 4)
                    .Mov_(R.T0, G__CURRENT_ROW)
                    .MarkLabel(forLoopStart)
                        // {                      
                        .Bge(R.T0, R.T1, forLoopEnd)

                        // if (i < 0 || i >= BOARD_HEIGHT) continue;
                        .Brlz(R.T0, forLoopContinue)
                        .Movli(R.AsmRes, CONSTVAL_BOARD_HEIGHT)
                        .Bge(R.T0, R.AsmRes, forLoopContinue)

                        // mem = _board[i];
                        .Ldr(mem, R.T0, (short)__BOARD)

                        // strip = (uint)(block & mask);
                        .And(strip, block, mask)

                        // shft = offset + _curCol;
                        .Add(shft, offset, G__CURRENT_COL)

                        // if (shft >= 0) {
                        .Brlz(shft, elseBranch)

                        // strip >>= shft;
                        .Shr(strip, strip, shft)
                        .Jmp(endIf)

                        // } else {
                        .MarkLabel(elseBranch)

                        // strip <<= (-shft);
                        .Neg(R.AsmRes, shft)
                        .Shl(strip, strip, R.AsmRes)

                        // } 
                        .MarkLabel(endIf)

                        // mem |= strip;
                        .Or(mem, mem, strip)

                        // _board[i] = mem
                        .Str(mem, R.T0, (short)__BOARD)

                        // ... i++, mask >>= 4, offset -= 4)
                        .MarkLabel(forLoopContinue)
                        .Inc_(R.T0)
                        .Shri(mask, mask, 4)
                        .Addi(offset, offset, -4)
                        .Jmp(forLoopStart)

                    .MarkLabel(forLoopEnd)

                    //epilog
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
        }

        private static void create_BLIT_BLOCK(ref CodeBuilder builder, Labels labels)
        {
            var forLoopStart1 = builder.CreateLabel();
            var forLoopEnd1 = builder.CreateLabel();
            var forLoopStart2 = builder.CreateLabel();
            var forLoopContinue2 = builder.CreateLabel();
            var forLoopEnd2 = builder.CreateLabel();
            var forLoopStart3 = builder.CreateLabel();
            var forLoopEnd3 = builder.CreateLabel();
            var elseBranch = builder.CreateLabel();
            var endIf = builder.CreateLabel();

            var mask = R.T14;
            var offset = R.T13;
            var shft = R.T12;
            var mem = R.T11;
            var strip = R.T10;
            var block = R.T9;
            var rot = R.T8;
            var col = R.T7;
            var row = R.T6;

            builder
                .MarkLabel(labels.BlitBlockProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)

                    // load arguments from stack
                    .Ldr(row, R.Fp, 1)
                    .Ldr(col, R.Fp, 2)
                    .Ldr(rot, R.Fp, 3)

                    // uint mask = 0xF000;
                    .Movi_(mask, 0xF000)

                    // int offset = 6; 
                    .Movli(offset, 6)

                    // int shft = 0; 
                    .Movli(shft, 0)

                    // uint block = BLOCKS_DATA[_curBlock + rot];
                    .Add(R.T0, G__CURRENT_BLOCK, rot)
                    .Ldr(block, R.T0, (short)__BLOCKS_DATA)


                    // for (i = 0; i < row; i++)
                    .Movli(R.T0, 0)
                    .Mov_(R.T1, row)
                    .MarkLabel(forLoopStart1)
                        // {
                        .Bge(R.T0, R.T1, forLoopEnd1)

                        // var maskedLine = _screenMemory[i + BOARD_VERTICAL_SHIFT] & COPY_LINE_MASK;
                        .Addi(R.T3, R.T0, CONSTVAL_BOARD_VERTICAL_SHIFT)
                        .Ldrx(R.T4, G__VIDEO_START, R.T3)
                        .Ld(R.T5, __COPY_LINE_MASK)
                        .And(R.T4, R.T4, R.T5)

                        // _screenMemory[i + BOARD_VERTICAL_SHIFT] = maskedLine | (_board[i] << BOARD_HORIZONTAL_SHIFT);
                        .Ldr(mem, R.T0, (short)__BOARD)
                        .Shli(R.AsmRes, mem, CONSTVAL_BOARD_HORIZONTAL_SHIFT)
                        .Or(R.T4, R.T4, R.AsmRes)
                        .Strx(R.T4, G__VIDEO_START, R.T3)

                        .Inc_(R.T0)
                        .Jmp(forLoopStart1)
                    .MarkLabel(forLoopEnd1)
                    // }


                    .Addi(R.T1, row, 4)
                    // for (i = row; i < row + 4; i++, mask >>= 4, offset -= 4)
                    .Mov_(R.T0, row)
                    .MarkLabel(forLoopStart2)
                        // {                      
                        .Bge(R.T0, R.T1, forLoopEnd2)

                        // if (i < 0 || i >= BOARD_HEIGHT) continue;
                        .Brlz(R.T0, forLoopContinue2)
                        .Movli(R.AsmRes, CONSTVAL_BOARD_HEIGHT)
                        .Bge(R.T0, R.AsmRes, forLoopContinue2)

                        // mem = _board[i];
                        .Ldr(mem, R.T0, (short)__BOARD)

                        // strip = (uint)(block & mask);
                        .And(strip, block, mask)

                        // shft = offset + col;
                        .Add(shft, offset, col)

                        // if (shft >= 0) {
                        .Brlz(shft, elseBranch)

                            // strip >>= shft;
                            .Shr(strip, strip, shft)
                            .Jmp(endIf)

                        // } else {
                        .MarkLabel(elseBranch)

                            // strip <<= (-shft);
                            .Neg(R.AsmRes, shft)
                            .Shl(strip, strip, R.AsmRes)

                        // } 
                        .MarkLabel(endIf)

                        // mem |= strip;
                        .Or(mem, mem, strip)

                        // var maskedLine = _screenMemory[i + BOARD_VERTICAL_SHIFT] & COPY_LINE_MASK;
                        .Addi(R.T3, R.T0, CONSTVAL_BOARD_VERTICAL_SHIFT)
                        .Ldrx(R.T4, G__VIDEO_START, R.T3)
                        .Ld(R.T5, __COPY_LINE_MASK)
                        .And(R.T4, R.T4, R.T5)

                        // _screenMemory[i + BOARD_VERTICAL_SHIFT] = maskedLine | (mem << BOARD_HORIZONTAL_SHIFT);
                        .Shli(R.AsmRes, mem, CONSTVAL_BOARD_HORIZONTAL_SHIFT)
                        .Or(R.T4, R.T4, R.AsmRes)
                        .Strx(R.T4, G__VIDEO_START, R.T3)

                        // ... i++, mask >>= 4, offset -= 4)
                        .MarkLabel(forLoopContinue2)
                        .Inc_(R.T0)
                        .Shri(mask, mask, 4)
                        .Addi(offset, offset, -4)
                        .Jmp(forLoopStart2)

                    .MarkLabel(forLoopEnd2)

                    // for (i = row + 4; i < BOARD_HEIGHT; i++)
                    // i (T0) should still have correct value after prev loop
                    .Movli(R.T1, CONSTVAL_BOARD_HEIGHT)
                    .MarkLabel(forLoopStart3)
                        // {
                        .Bge(R.T0, R.T1, forLoopEnd3)

                        // var maskedLine = _screenMemory[i + BOARD_VERTICAL_SHIFT] & COPY_LINE_MASK;
                        .Addi(R.T3, R.T0, CONSTVAL_BOARD_VERTICAL_SHIFT)
                        .Ldrx(R.T4, G__VIDEO_START, R.T3)
                        .Ld(R.T5, __COPY_LINE_MASK)
                        .And(R.T4, R.T4, R.T5)

                        // _screenMemory[i + BOARD_VERTICAL_SHIFT] = maskedLine | (_board[i] << BOARD_HORIZONTAL_SHIFT);
                        .Ldr(mem, R.T0, (short)__BOARD)
                        .Shli(R.AsmRes, mem, CONSTVAL_BOARD_HORIZONTAL_SHIFT)
                        .Or(R.T4, R.T4, R.AsmRes)
                        .Strx(R.T4, G__VIDEO_START, R.T3)

                        .Inc_(R.T0)
                        .Jmp(forLoopStart3)
                    .MarkLabel(forLoopEnd3)

                    //epilog
                    .Pop_(R.Ra)
                    .Addi(R.Sp, R.Sp, 3)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
        }

        private static void create_CHECK_LINES(ref CodeBuilder builder, Labels labels)
        {
            var outterForStart = builder.CreateLabel();
            var outterForEnd = builder.CreateLabel();

            var ifEnd = builder.CreateLabel();

            var innerForStart = builder.CreateLabel();
            var innerForEnd = builder.CreateLabel();

            var row = R.T14;
            var next = R.T13;

            builder.
                MarkLabel(labels.CheckLinesProc)
                    //prolog
                    .Mov_(R.Fp, R.Sp)
                    .Push_(R.Ra)
                    .Push_(R.S0)

                    // uint res = 0;
                    .Xor(R.V, R.V, R.V)

                    // for (int row = BOARD_HEIGHT - 1; row >= 0; row--)
                    .Movli(row, CONSTVAL_BOARD_HEIGHT)
                    .Dec_(row)
                    .MarkLabel(outterForStart)
                    .Brlz(row, outterForEnd)
                        // {
                        // if ((_board[row] & FULL_LINE_MASK) == FULL_LINE_MASK)
                        .Ldr(R.T0, row, (short)__BOARD)
                        .Ld(R.T1, __FULL_LINE_MASK)
                        .And(R.T0, R.T0, R.T1)
                        .Bneq(R.T0, R.T1, ifEnd)
                            // {
                            //for (int next = row; next > 1; next--)
                            .Mov_(next, row)
                            .Movli(R.S0, 1)
                            .MarkLabel(innerForStart)
                            .Ble(next, R.S0, innerForEnd)
                                // {
                                // _board[next] = (_board[next - 1] & COPY_LINE_MASK);
                                .Addi(R.T0, next, -1)
                                .Ldr(R.T0, R.T0, (short)__BOARD)
                                .Ld(R.T1, __COPY_LINE_MASK)
                                .And(R.T1, R.T0, R.T1)
                                .Str(R.T1, next, (short)__BOARD)

                            .Dec_(next)
                            .Jmp(innerForStart)
                            .MarkLabel(innerForEnd)
                            // }

                            //  row++; // recheck current row;   
                            .Inc_(row)

                            //  res++; // add line to result
                            .Inc_(R.V)
                        .MarkLabel(ifEnd)
                    // }

                    .Dec_(row)
                    .Jmp(outterForStart)
                    .MarkLabel(outterForEnd)
                    // }

                    //epilog
                    .Pop_(R.S0)
                    .Pop_(R.Ra)
                    .Pop_(R.Fp)
                    .Jmpr(R.Ra);
            ;
        }

        private static void create_GAME_LOOP(ref CodeBuilder builder, Labels labels)
        {
            var gameLoopStart = builder.CreateLabel();
            var gameLoopEnd = builder.CreateLabel();

            var ifUpdateTimeStart = builder.CreateLabel();
            var ifUpdateTimeEnd = builder.CreateLabel();

            var ifUpdateTimeCanMoveElse = builder.CreateLabel();
            var ifUpdateTimeCanMoveEnd = builder.CreateLabel();

            var escapeCheckEnd = builder.CreateLabel();

            var ifLeftOuterEnd = builder.CreateLabel();
            var ifLeftOldKeyElseIf = builder.CreateLabel();
            var ifLeftOldKeyEnd = builder.CreateLabel();

            var ifRightOuterEnd = builder.CreateLabel();
            var ifRightOldKeyElseIf = builder.CreateLabel();
            var ifRightOldKeyEnd = builder.CreateLabel();

            var ifDownOuterEnd = builder.CreateLabel();
            var ifDownOldKeyElseIf = builder.CreateLabel();
            var ifDownOldKeyEnd = builder.CreateLabel();

            var ifRotateOuterEnd = builder.CreateLabel();
            var ifRotateOldKeyElseIf = builder.CreateLabel();
            var ifRotateOldKeyEnd = builder.CreateLabel();

            var ifMovesEnd = builder.CreateLabel();
            var ifMovesStart = builder.CreateLabel();

            var time = R.S0;
            var keystate = R.S1;
            var oldstate = R.S2;

            var moveleft = R.T14;
            var moveright = R.T13;
            var movedown = R.T12;
            var moverotate = R.T11;
            var ncol = R.T10;
            var nrow = R.T9;
            var nrot = R.T8;

            builder.

                MarkLabel(labels.GameLoopProc)

                    // setup stack bottom
                    .Movi_(R.Sp, Computer.RAM_TOP)

                    .Movi_(G__VIDEO_START, Computer.VIDEO_START)

                    .Push_(R.Fp)
                    .Jal(R.Ra, labels.InitGameProc)

                .MarkLabel(gameLoopStart)

                // var keystate = _getKeys();
                .Ld(keystate, Computer.GAME_PAD)
                //.Movli(keystate, (short)KeyCodes.KEY_LEFT)

                // if ((keystate & KEY_ESC) != 0)
                .Andi(R.T0, keystate, (short)KeyCodes.KEY_ESC)
                .Brz(R.T0, escapeCheckEnd)
                // {
                .Push_(R.Fp)
                .Jal(R.Ra, labels.InitGameProc)
                // }
                .MarkLabel(escapeCheckEnd)

                // if (!_playing) continue;
                .Ld(R.T0, __PLAYING)
                .Brz(R.T0, gameLoopStart)

                // var time = _getTime();
                .Ld(time, Computer.TICKS_LOW)

                // bool moveLeft = false
                .Movli(moveleft, 0)

                // bool moveright = false
                .Movli(moveright, 0)

                // bool movedown = false
                .Movli(movedown, 0)

                // bool moverotate = false
                .Movli(moverotate, 0)

                // int ncol = _curCol
                .Mov_(ncol, G__CURRENT_COL)

                // int nrow = _curRow
                .Mov_(nrow, G__CURRENT_ROW)

                // int nrot = _curRotation
                .Mov_(nrot, G__CURRENT_ROTATION)

                // *** MOVE LEFT HANDLING ***
                // if ((keystate & KEY_LEFT) != 0)
                .Andi(R.T0, keystate, (short)KeyCodes.KEY_LEFT)
                .Brz(R.T0, ifLeftOuterEnd)
                    // {
                    //  if ((_oldKeys & KEY_LEFT) == 0)
                    .Andi(R.T0, oldstate, (short)KeyCodes.KEY_LEFT)
                    .Brnz(R.T0, ifLeftOldKeyElseIf)
                        // {
                        //  moveLeft = true;
                        .Movli(moveleft, 1)

                        //  _lastKeyboardTime = time + REPEAT_RATE_SLOW;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_SLOW)

                    .Jmp(ifLeftOldKeyEnd)
                    .MarkLabel(ifLeftOldKeyElseIf)
                    // } else if (time > _lastKeyboardTime) {
                    .Ble(time, G__LAST_KEYBOARD, ifLeftOldKeyEnd)
                        // _lastKeyboardTime = time + REPEAT_RATE_FAST;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_FAST)

                        //  moveLeft = true;
                        .Movli(moveleft, 1)

                    .MarkLabel(ifLeftOldKeyEnd)
                // }                    
                .MarkLabel(ifLeftOuterEnd)
                // }

                // *** MOVE RIGHT HANDLING ***
                // if ((keystate & KEY_RIGHT) != 0)
                .Andi(R.T0, keystate, (short)KeyCodes.KEY_RIGHT)
                .Brz(R.T0, ifRightOuterEnd)
                    // {
                    //  if ((_oldKeys & KEY_RIGHT) == 0)
                    .Andi(R.T0, oldstate, (short)KeyCodes.KEY_RIGHT)
                    .Brnz(R.T0, ifRightOldKeyElseIf)
                        // {
                        //  moveright = true;
                        .Movli(moveright, 1)

                        //  _lastKeyboardTime = time + REPEAT_RATE_SLOW;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_SLOW)

                    .Jmp(ifRightOldKeyEnd)
                    .MarkLabel(ifRightOldKeyElseIf)
                    // } else if (time > _lastKeyboardTime) {
                    .Ble(time, G__LAST_KEYBOARD, ifRightOldKeyEnd)
                        // _lastKeyboardTime = time + REPEAT_RATE_FAST;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_FAST)

                        //  moveright = true;
                        .Movli(moveright, 1)

                    .MarkLabel(ifRightOldKeyEnd)
                // }                    
                .MarkLabel(ifRightOuterEnd)
                // }


                // *** MOVE DOWN HANDLING ***
                // if ((keystate & KEY_DOWN) != 0)
                .Andi(R.T0, keystate, (short)KeyCodes.KEY_DOWN)
                .Brz(R.T0, ifDownOuterEnd)
                    // {
                    //  if ((_oldKeys & KEY_DOWN) == 0)
                    .Andi(R.T0, oldstate, (short)KeyCodes.KEY_DOWN)
                    .Brnz(R.T0, ifDownOldKeyElseIf)
                        // {
                        //  movedown = true;
                        .Movli(movedown, 1)

                        //  _lastKeyboardTime = time + REPEAT_RATE_SLOW;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_SLOW)

                    .Jmp(ifDownOldKeyEnd)
                    .MarkLabel(ifDownOldKeyElseIf)
                    // } else if (time > _lastKeyboardTime) {
                    .Ble(time, G__LAST_KEYBOARD, ifDownOldKeyEnd)
                        // _lastKeyboardTime = time + REPEAT_RATE_RAPID;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_RAPID)

                        //  movedown = true;
                        .Movli(movedown, 1)

                    .MarkLabel(ifDownOldKeyEnd)
                // }                    
                .MarkLabel(ifDownOuterEnd)
                // }

                // *** MOVE ROTATE HANDLING ***
                // if ((keystate & KEY_ROTATE) != 0)
                .Andi(R.T0, keystate, (short)KeyCodes.KEY_UP)
                .Brz(R.T0, ifRotateOuterEnd)
                    // {
                    //  if ((_oldKeys & KEY_ROTATE) == 0)
                    .Andi(R.T0, oldstate, (short)KeyCodes.KEY_UP)
                    .Brnz(R.T0, ifRotateOldKeyElseIf)
                        // {
                        //  moverotate = true;
                        .Movli(moverotate, 1)

                        //  _lastKeyboardTime = time + REPEAT_RATE_SLOW;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_SLOW)

                    .Jmp(ifRotateOldKeyEnd)
                    .MarkLabel(ifRotateOldKeyElseIf)
                    // } else if (time > _lastKeyboardTime) {
                    .Ble(time, G__LAST_KEYBOARD, ifRotateOldKeyEnd)
                        // _lastKeyboardTime = time + REPEAT_RATE_RAPID;
                        .Addi(G__LAST_KEYBOARD, time, CONSTVAL_REPEAT_RATE_RAPID)

                        //  moverotate = true;
                        .Movli(moverotate, 1)

                    .MarkLabel(ifRotateOldKeyEnd)
                // }                    
                .MarkLabel(ifRotateOuterEnd)
                // }

                // _oldKeys = keystate;
                .Mov_(oldstate, keystate)

                // if (moveLeft) { ncol--; }
                .Muli(R.T0, moveleft, -1)
                .Add(ncol, ncol, R.T0)

                // if (moveRight) {ncol++;}
                .Muli(R.T0, moveright, 1)
                .Add(ncol, ncol, R.T0)

                // if (moveDown) {    nrow++;  }
                .Muli(R.T0, movedown, 1)
                .Add(nrow, nrow, R.T0)

                // if (rotate)  {nrot = (_curRotation + 1) % 4;}
                .Muli(R.T0, moverotate, 1)
                .Add(nrot, nrot, R.T0)
                .Modi(nrot, nrot, 4)

                //if (moveLeft || moveRight || rotate || moveDown)
                .Brnz(moveleft, ifMovesStart)
                .Brnz(moveright, ifMovesStart)
                .Brnz(moverotate, ifMovesStart)
                .Brnz(movedown, ifMovesStart)
                .Jmp(ifMovesEnd)
                .MarkLabel(ifMovesStart)
                // {
                    // Save T-regs that we need after function call.
                    .Push_(nrot)
                    .Push_(nrow)
                    .Push_(ncol)

                    // if (canMoveBlock(ncol, nrow, nrot))                    
                    .Push_(R.Fp)
                    .Push_(nrot)
                    .Push_(nrow)
                    .Push_(ncol)
                    .Jal(R.Ra, labels.CanMoveBlockProc)

                    // Resotre T-regs
                    .Pop_(ncol)
                    .Pop_(nrow)
                    .Pop_(nrot)

                    .Brz(R.V, ifMovesEnd)
                    // {
                        // _curCol = ncol;
                        .Mov_(G__CURRENT_COL, ncol)
                        // _curRow = nrow;
                        .Mov_(G__CURRENT_ROW, nrow)
                        // _curRotation = nrot;
                        .Mov_(G__CURRENT_ROTATION, nrot)
                    // }
                // }
                .MarkLabel(ifMovesEnd)

                // if (time > _lastUpdateTime + _curDelay)
                .Ld(R.T0, __CURRENT_DELAY)
                .Add(R.T0, G__LAST_UPDATE_TIME, R.T0)
                .Ble(time, R.T0, ifUpdateTimeEnd)
                    // {
                    // if (canMoveBlock(_curCol, _curRow + 1, _curRotation))
                    .Push_(R.Fp)
                    .Push_(G__CURRENT_ROTATION)
                    .Addi(R.T0, G__CURRENT_ROW, 1)
                    .Push_(R.T0)
                    .Push_(G__CURRENT_COL)
                    .Jal(R.Ra, labels.CanMoveBlockProc)
                    .Brz(R.V, ifUpdateTimeCanMoveElse)
                        // {
                        //  _curRow++;
                        .Inc_(G__CURRENT_ROW)
                        .Jmp(ifUpdateTimeCanMoveEnd)
                    // } else {
                    .MarkLabel(ifUpdateTimeCanMoveElse)
                        // mergeBlock();
                        .Push_(R.Fp)
                        .Jal(R.Ra, labels.MergeBlockProc)

                        // _sevenSegDisplay[0] += checkLines(); 
                        .Push_(R.Fp)
                        .Jal(R.Ra, labels.CheckLinesProc)
                        .Ld(R.T0, Computer.SEG_DISP)
                        .Add(R.T0, R.T0, R.V)
                        .St(R.T0, Computer.SEG_DISP)

                        //generateTetromino();
                        .Push_(R.Fp)
                        .Jal(R.Ra, labels.GenerateTetrominoProc)

                    .MarkLabel(ifUpdateTimeCanMoveEnd)
                    // }

                    // _lastUpdateTime = time;
                    .Mov_(G__LAST_UPDATE_TIME, time)
                // }
                .MarkLabel(ifUpdateTimeEnd)

                // blitBlock(_curRow, _curCol, _curRotation);
                .Push_(R.Fp)
                .Push_(G__CURRENT_ROTATION)
                .Push_(G__CURRENT_COL)
                .Push_(G__CURRENT_ROW)
                .Jal(R.Ra, labels.BlitBlockProc)

                .Jmp(gameLoopStart)
                ;
        }
    }
}
