using Microsoft.Xna.Framework;
using static MGRawInputLib.RawInputTypes;

namespace MGRawInputLib {
    public enum MouseButtons { Left, Right, Middle, X1, X2, ScrollUp, ScrollDown, None }

    public static class RawInputMouse {
        static RawInputMouseState mouse_state = new RawInputMouseState();

        public static void reset_scroll_delta() {
            mouse_state.ScrollDelta = 0;
        }

        static Point _mouse_delta_acc;
        static void accumulate_mouse_delta(int x, int y) {
            _mouse_delta_acc += new Point(x,y);
        }

        static Point get_and_clear_mouse_delta() {
            var ret = _mouse_delta_acc;
            _mouse_delta_acc = Point.Zero;
            return ret;
        }

        internal static void update_rawinput(RawInputTypes.RawInputMouse ri) {

            if (ri.Flags.HasFlag(RawMouseFlags.RELATIVE)) {
                accumulate_mouse_delta(ri.LastX, ri.LastY);
            } 
            
            if (ri.data.ButtonFlags != RawMouseButtons.None) {
                switch (ri.data.ButtonFlags) {
                    case RawMouseButtons.LeftDown:
                        mouse_state.LeftButton = true;
                        break;
                    case RawMouseButtons.LeftUp:
                        mouse_state.LeftButton = false;
                        break;
                    case RawMouseButtons.RightDown:
                        mouse_state.RightButton = true;
                        break;
                    case RawMouseButtons.RightUp:
                        mouse_state.RightButton = false;
                        break;
                    case RawMouseButtons.MiddleDown:
                        mouse_state.MiddleButton = true;
                        break;
                    case RawMouseButtons.MiddleUp:
                        mouse_state.MiddleButton = false;
                        break;
                    case RawMouseButtons.Button4Down:
                        mouse_state.XButton1 = true;
                        break;
                    case RawMouseButtons.Button4Up:
                        mouse_state.XButton1 = false;
                        break;
                    case RawMouseButtons.Button5Down:
                        mouse_state.XButton2 = true;
                        break;
                    case RawMouseButtons.Button5Up:
                        mouse_state.XButton2 = false;
                        break;
                    case RawMouseButtons.MouseWheel:
                        mouse_state.ScrollDelta += ri.data.ScrollDelta;
                        break;
                }
            }

        }

        public static RawInputMouseState GetState() {
            mouse_state.Delta = get_and_clear_mouse_delta();

            return mouse_state.GetState();
        }
    }

    public struct RawInputMouseState {
        Point Position { get; set; }
        int X => Position.X; int Y => Position.Y;
        public Point Delta { get; set; }
        public int ScrollDelta { get; set; }

        public bool LeftButton { get; set; } 
        public bool MiddleButton { get; set; }
        public bool RightButton { get; set; }
        public bool XButton2 { get; set; }
        public bool XButton1 { get; set; }

        public bool ScrollUp => ScrollDelta > 0;
        public bool ScrollDown => ScrollDelta < 0;

        internal RawInputMouseState GetState() {
            return new RawInputMouseState {
                Position = this.Position, Delta = this.Delta,
                LeftButton = this.LeftButton, MiddleButton = this.MiddleButton, RightButton = this.RightButton,
                XButton2 = this.XButton2, XButton1 = this.XButton1,
                ScrollDelta = this.ScrollDelta
            };
        }

        internal string info() {
            return $@"{Delta.X}x{Delta.Y}
[Left] {LeftButton} [Right] {RightButton}
[Middle] {MiddleButton} [X1] {XButton1} [X2] {XButton2}
[ScrollUp] {ScrollUp} [ScrollDown] {ScrollDown} [ScrollDelta] {ScrollDelta}";
        }
    }
}
