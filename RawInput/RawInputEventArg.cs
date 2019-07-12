using System;

namespace RawInput_dll
{
    public class RawInputEventArg : EventArgs
    {
        public RawInputEventArg(int x, int y, int touchId, bool down)
        {
            this.X = x;
            this.Y = y;
            this.TouchId = touchId;
            this.IsDown = down;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int TouchId { get; private set; }
        public bool IsDown { get; private set; }
    }
}
