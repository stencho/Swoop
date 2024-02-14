using Microsoft.Xna.Framework.Input;
using System.Text;
using static MGRawInputLib.RawInputTypes;

namespace MGRawInputLib {
    public static class RawInputKeyboard {
        static RawInputKeyboardState keyboard_state = new RawInputKeyboardState();

        internal static void update_rawinput(RAWINPUT ri) {
            
            if (!ri.Data.Keyboard.Flags.HasFlag(RawInputKeyFlags.UP)) {
                keyboard_state.pressed_keys.Add(RIMGKeys.monogame_key(ri.Data.Keyboard.VirtualKey));
            } else if (ri.Data.Keyboard.Flags.HasFlag(RawInputKeyFlags.UP)) {
                keyboard_state.pressed_keys.Remove(RIMGKeys.monogame_key(ri.Data.Keyboard.VirtualKey));
            }

        }
    
        public static RawInputKeyboardState GetState() {
            return new RawInputKeyboardState(keyboard_state.pressed_keys);
        }        
    }

    public struct RawInputKeyboardState {
        public bool CapsLock => (Externs.GetKeyState(Externs.vk_states.VK_CAPITAL) & 0xFFFF) != 0;
        public bool NumLock => (Externs.GetKeyState(Externs.vk_states.VK_NUMLOCK) & 0xFFFF) != 0;
        public bool ScrollLock => (Externs.GetKeyState(Externs.vk_states.VK_SCROLL) & 0xFFFF) != 0;

        public HashSet<Keys> pressed_keys = new HashSet<Keys>();

        public RawInputKeyboardState() { }
        public RawInputKeyboardState(HashSet<Keys> keys) { pressed_keys = new HashSet<Keys>(keys); }
        public RawInputKeyboardState(Keys[] keys) { pressed_keys = new HashSet<Keys>(keys); }

        public Keys[] GetPressedKeys() { return pressed_keys.ToArray(); }
                        
        public bool IsKeyUp(Keys key) => !pressed_keys.Contains(key);
        public bool IsKeyDown(Keys key) => pressed_keys.Contains(key) && Window.is_active;

        internal RawInputKeyboardState GetState() => new RawInputKeyboardState(pressed_keys);
        
        public string list_keys() {
            if (pressed_keys == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach(Keys key in pressed_keys) { sb.Append(key.ToString()); sb.Append(", "); }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }

    //awful awful function to simply switch between VirtualKeys and MonoGame Keys
    //this has to happen at least once, so it's happening above in RawInputKeyboard
    //and RIKeyboardState is storing MonoGame Keys instead of VirtualKeys as they're
    //practically useless above this point

    internal static class RIMGKeys {
        public static Keys monogame_key(this VirtualKeys key) {
            switch (key) {
                case VirtualKeys.Backspace: return Keys.Back;
                case VirtualKeys.Tab: return Keys.Tab;
                case VirtualKeys.Clear: return Keys.OemClear;
                case VirtualKeys.Return: return Keys.Enter;
                case VirtualKeys.Shift: return Keys.LeftShift;
                case VirtualKeys.Control: return Keys.LeftControl;
                case VirtualKeys.Alt: return Keys.LeftAlt;
                case VirtualKeys.Pause: return Keys.Pause;
                case VirtualKeys.CapsLock: return Keys.CapsLock;
                case VirtualKeys.Kana: return Keys.Kana;
                case VirtualKeys.Escape: return Keys.Escape;
                case VirtualKeys.Convert: return Keys.ImeConvert;
                case VirtualKeys.NonConvert: return Keys.ImeNoConvert;
                case VirtualKeys.Space: return Keys.Space;
                case VirtualKeys.PageUp: return Keys.PageUp;
                case VirtualKeys.PageDown: return Keys.PageDown;
                case VirtualKeys.End: return Keys.End;
                case VirtualKeys.Home: return Keys.Home;
                case VirtualKeys.Left: return Keys.Left;
                case VirtualKeys.Up: return Keys.Up;
                case VirtualKeys.Right: return Keys.Right;
                case VirtualKeys.Down: return Keys.Down;
                case VirtualKeys.Select: return Keys.Select;
                case VirtualKeys.Print: return Keys.Print;
                case VirtualKeys.Execute: return Keys.Execute;
                case VirtualKeys.PrintScreen: return Keys.PrintScreen;
                case VirtualKeys.Insert: return Keys.Insert;
                case VirtualKeys.Delete: return Keys.Delete;
                case VirtualKeys.Help: return Keys.Help;
                case VirtualKeys.D0: return Keys.D0;
                case VirtualKeys.D1: return Keys.D1;
                case VirtualKeys.D2: return Keys.D2;
                case VirtualKeys.D3: return Keys.D3;
                case VirtualKeys.D4: return Keys.D4;
                case VirtualKeys.D5: return Keys.D5;
                case VirtualKeys.D6: return Keys.D6;
                case VirtualKeys.D7: return Keys.D7;
                case VirtualKeys.D8: return Keys.D8;
                case VirtualKeys.D9: return Keys.D9;
                case VirtualKeys.A: return Keys.A;
                case VirtualKeys.B: return Keys.B;
                case VirtualKeys.C: return Keys.C;
                case VirtualKeys.D: return Keys.D;
                case VirtualKeys.E: return Keys.E;
                case VirtualKeys.F: return Keys.F;
                case VirtualKeys.G: return Keys.G;
                case VirtualKeys.H: return Keys.H;
                case VirtualKeys.I: return Keys.I;
                case VirtualKeys.J: return Keys.J;
                case VirtualKeys.K: return Keys.K;
                case VirtualKeys.L: return Keys.L;
                case VirtualKeys.M: return Keys.M;
                case VirtualKeys.N: return Keys.N;
                case VirtualKeys.O: return Keys.O;
                case VirtualKeys.P: return Keys.P;
                case VirtualKeys.Q: return Keys.Q;
                case VirtualKeys.R: return Keys.R;
                case VirtualKeys.S: return Keys.S;
                case VirtualKeys.T: return Keys.T;
                case VirtualKeys.U: return Keys.U;
                case VirtualKeys.V: return Keys.V;
                case VirtualKeys.W: return Keys.W;
                case VirtualKeys.X: return Keys.X;
                case VirtualKeys.Y: return Keys.Y;
                case VirtualKeys.Z: return Keys.Z;
                case VirtualKeys.LeftWindows: return Keys.LeftWindows;
                case VirtualKeys.RightWindows: return Keys.RightWindows;
                case VirtualKeys.Menu: return Keys.Apps;
                case VirtualKeys.Sleep: return Keys.Sleep;
                case VirtualKeys.Numpad0: return Keys.NumPad0;
                case VirtualKeys.Numpad1: return Keys.NumPad1;
                case VirtualKeys.Numpad2: return Keys.NumPad2;
                case VirtualKeys.Numpad3: return Keys.NumPad3;
                case VirtualKeys.Numpad4: return Keys.NumPad4;
                case VirtualKeys.Numpad5: return Keys.NumPad5;
                case VirtualKeys.Numpad6: return Keys.NumPad6;
                case VirtualKeys.Numpad7: return Keys.NumPad7;
                case VirtualKeys.Numpad8: return Keys.NumPad8;
                case VirtualKeys.Numpad9: return Keys.NumPad9;
                case VirtualKeys.Multiply: return Keys.Multiply;
                case VirtualKeys.Add: return Keys.Add;
                case VirtualKeys.Separator: return Keys.Separator;
                case VirtualKeys.Subtract: return Keys.Subtract;
                case VirtualKeys.Decimal: return Keys.Decimal;
                case VirtualKeys.Divide: return Keys.Divide;
                case VirtualKeys.F1: return Keys.F1;
                case VirtualKeys.F2: return Keys.F2;
                case VirtualKeys.F3: return Keys.F3;
                case VirtualKeys.F4: return Keys.F4;
                case VirtualKeys.F5: return Keys.F5;
                case VirtualKeys.F6: return Keys.F6;
                case VirtualKeys.F7: return Keys.F7;
                case VirtualKeys.F8: return Keys.F8;
                case VirtualKeys.F9: return Keys.F9;
                case VirtualKeys.F10: return Keys.F10;
                case VirtualKeys.F11: return Keys.F11;
                case VirtualKeys.F12: return Keys.F12;
                case VirtualKeys.F13: return Keys.F13;
                case VirtualKeys.F14: return Keys.F14;
                case VirtualKeys.F15: return Keys.F15;
                case VirtualKeys.F16: return Keys.F16;
                case VirtualKeys.F17: return Keys.F17;
                case VirtualKeys.F18: return Keys.F18;
                case VirtualKeys.F19: return Keys.F19;
                case VirtualKeys.F20: return Keys.F20;
                case VirtualKeys.F21: return Keys.F21;
                case VirtualKeys.F22: return Keys.F22;
                case VirtualKeys.F23: return Keys.F23;
                case VirtualKeys.F24: return Keys.F24;
                case VirtualKeys.NumLock: return Keys.NumLock;
                case VirtualKeys.ScrollLock: return Keys.Scroll;
                case VirtualKeys.LeftShift: return Keys.LeftShift;
                case VirtualKeys.RightShift: return Keys.RightShift;
                case VirtualKeys.LeftControl: return Keys.LeftControl;
                case VirtualKeys.RightControl: return Keys.RightControl;
                case VirtualKeys.BrowserBack: return Keys.BrowserBack;
                case VirtualKeys.BrowserForward: return Keys.BrowserForward;
                case VirtualKeys.BrowserRefresh: return Keys.BrowserRefresh;
                case VirtualKeys.BrowserStop: return Keys.BrowserStop;
                case VirtualKeys.BrowserSearch: return Keys.BrowserSearch;
                case VirtualKeys.BrowserFavorites: return Keys.BrowserFavorites;
                case VirtualKeys.BrowserHome: return Keys.BrowserHome;
                case VirtualKeys.VolumeMute: return Keys.VolumeMute;
                case VirtualKeys.VolumeDown: return Keys.VolumeDown;
                case VirtualKeys.VolumeUp: return Keys.VolumeUp;
                case VirtualKeys.MediaNextTrack: return Keys.MediaNextTrack;
                case VirtualKeys.MediaPrevTrack: return Keys.MediaPreviousTrack;
                case VirtualKeys.MediaStop: return Keys.MediaStop;
                case VirtualKeys.MediaPlayPause: return Keys.MediaPlayPause;
                case VirtualKeys.LaunchMail: return Keys.LaunchMail;
                case VirtualKeys.LaunchMediaSelect: return Keys.SelectMedia;
                case VirtualKeys.LaunchApplication1: return Keys.LaunchApplication1;
                case VirtualKeys.LaunchApplication2: return Keys.LaunchApplication2;
                case VirtualKeys.Colons: return Keys.OemSemicolon;
                case VirtualKeys.Plus: return Keys.OemPlus;
                case VirtualKeys.Comma: return Keys.OemComma;
                case VirtualKeys.Minus: return Keys.OemMinus;
                case VirtualKeys.Period: return Keys.OemPeriod;
                case VirtualKeys.Question: return Keys.OemQuestion;
                case VirtualKeys.Tilde: return Keys.OemTilde;
                case VirtualKeys.OpenBracket: return Keys.OemOpenBrackets;
                case VirtualKeys.CloseBracket: return Keys.OemCloseBrackets;
                case VirtualKeys.Pipe: return Keys.OemPipe;
                case VirtualKeys.Quotes: return Keys.OemQuotes;
                case VirtualKeys.ICOHelp: return Keys.Help;
                case VirtualKeys.ProcessKey: return Keys.ProcessKey;
                case VirtualKeys.ATTN: return Keys.Attn;
                case VirtualKeys.EREOF: return Keys.EraseEof;
                case VirtualKeys.Play: return Keys.Play;
                case VirtualKeys.Zoom: return Keys.Zoom;
                case VirtualKeys.OEMClear: return Keys.OemClear;
                default: return Keys.None;
            }
        }
    }
}
