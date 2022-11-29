using proj_Bomberman;
using System;

namespace proj_Bomberman
{
    public delegate void StringDel(string message);
    public delegate void PlayerDel(Player obj);
    public delegate void DisposeBombDel(Bomb obj, int row, int col);
    public delegate void DisposeBlastDel(Blast obj, int index);

    public static class GlobalVar {
        public const int CANVAS_PADDING_LEFT = 105;
        public const int CANVAS_PADDING_TOP = 90;
        public const int BLOCK_SIZE = 24;
    }
}