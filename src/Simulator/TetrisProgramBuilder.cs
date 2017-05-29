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

            public Labels(CodeBuilder builder)
            {
                InitGameProc = builder.CreateLabel();
                CreateBoardProc = builder.CreateLabel();
                GenerateTetrominoProc = builder.CreateLabel();
                CanMoveBlockProc = builder.CreateLabel();
                MergeBlockProc = builder.CreateLabel();
                BlitBlockProc = builder.CreateLabel();
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


        public const Register G__CURRENT_BLOCK = R.G6;
        public const Register G__CURRENT_ROTATION = R.G5;
        public const Register G__CURRENT_ROW = R.G4;
        public const Register G__CURRENT_COL = R.G3;

        public const Register G__VIDEO_START = R.G0;

        public const short CONSTVAL_BOARD_HEIGHT = 20;

        const short CONSTVAL_BOARD_HORIZONTAL_SHIFT = 11;
        const short CONSTVAL_BOARD_VERTICAL_SHIFT = 6;

        public const uint CONSTVAL_FULL_LINE_MASK = 0x03FF;
        public const uint CONSTVAL_COPY_LINE_MASK = ~(CONSTVAL_FULL_LINE_MASK << CONSTVAL_BOARD_HORIZONTAL_SHIFT);

        public static IReadOnlyDictionary<uint, uint> Create()
        {
            var builder = new CodeBuilder();
            var procedureLabels = new Labels(builder);

            var programEntry = builder.CreateLabel();
            var gameLoop = builder.CreateLabel();

            var prg = builder

                .Jmp(programEntry)

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


                .MarkLabel(programEntry)
                    .Movi_(G__VIDEO_START, Computer.VIDEO_START)
                    .Push_(R.Fp)
                    .Jal(R.Ra, procedureLabels.InitGameProc)
                    
                    .Movli(R.S0, 0)
                .MarkLabel(gameLoop)

                .Push_(R.Fp)
                    .Push_(G__CURRENT_ROTATION)
                    .Push_(G__CURRENT_COL)                    
                    .Push_(G__CURRENT_ROW)
                    .Jal(R.Ra, procedureLabels.BlitBlockProc)

                    .Movli(R.T1, 15)
                    .Inc_(R.S0)
                    .Mov_(G__CURRENT_ROW, R.S0)
                    .Blt(R.S0, R.T1, gameLoop)

                .Hlt();

            create_INIT_GAME(ref prg, procedureLabels);
            create_CREATE_BOARD(ref prg, procedureLabels);
            create_GENERATE_TETROMINO(ref prg, procedureLabels);
            create_CAN_MOVE_BLOCK(ref prg, procedureLabels);
            create_MERGE_BLOCK(ref prg, procedureLabels);
            create_BLIT_BLOCK(ref prg, procedureLabels);

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

                    //code
                    .Movli(R.T0, 0)
                    .Movli(R.T1, 32)
                    .Movi_(R.T7, 0xFFFFFFFF)

                    .MarkLabel(loop1_start)
                        .Bge(R.T0, R.T1, loop1_end)

                        .Add(R.T2, G__VIDEO_START, R.T0)
                        .Str(R.T7, R.T2)

                        .Inc_(R.T0)
                        .Jmp(loop1_start)
                    .MarkLabel(loop1_end)

                    .Movli(R.T0, 0)
                    .Movli(R.T1, 20)
                    .Movi_(R.T7, 0)

                    .MarkLabel(loop2_start)
                        .Bge(R.T0, R.T1, loop2_end)

                        .Str(R.T7, R.T0, (short)__BOARD)

                        .Inc_(R.T0)
                        .Jmp(loop2_start)
                    .MarkLabel(loop2_end)

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

                    // return true
                    .Movli(R.V, 1)

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
                    // i (T0) should still have correct value after prev loop
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
    }
}
