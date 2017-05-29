using System;

namespace Tbx32.Core
{
    [Flags]
    public enum KeyCodes
    {
        NONE = 0,
        KEY_LEFT = 1 << 0,
        KEY_UP = 1 << 1,
        KEY_RIGHT = 1 << 2,
        KEY_DOWN = 1 << 3,
        KEY_ESC = 1 << 4
    }
}
