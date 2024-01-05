using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace MGRawInputLib {
    public static class Input {
        static Thread control_update_thread = new Thread(new ThreadStart(update));

        public static List<InputHandler> handlers = new List<InputHandler>();
        static Game parent;

        static readonly double key_hold_time = 300;

        public static HashSet<KeyTime> pressed_keys = new HashSet<KeyTime>();

        public struct KeyTime {
            Keys _key;
            public Keys key => _key;

            DateTime _pressed_time;
            public DateTime pressed_time => _pressed_time;

            public TimeSpan time_since_press => DateTime.Now - _pressed_time;            
            public bool held => time_since_press.TotalMilliseconds >= key_hold_time;

            public KeyTime(Keys key, DateTime time) {
                this._key = key;                
                _pressed_time = time;
            }
        }

        public static string list_keys() {
            if (pressed_keys == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (KeyTime key in pressed_keys) { sb.Append(key.key.ToString()); sb.Append(", "); }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }


        public static bool key_in_pressed_list(Keys k, out KeyTime key_time) {
            foreach (var key in pressed_keys) {
                if (k == key.key) { key_time = key; return true; }
            }
            key_time = new KeyTime();
            return false;
        }

        public static void end() { run_thread = false; }

        public static bool num_lock => keyboard_state.NumLock;
        public static bool caps_lock => keyboard_state.CapsLock;

        public static KeyboardState keyboard_state {get; set; }
        public static MouseState mouse_state { get; set; }
        public static MouseState mouse_state_previous { get; set; }

        public static GamePadState gamepad_one_state { get; private set; }
        public static GamePadState gamepad_two_state { get; private set; }
        public static GamePadState gamepad_three_state { get; private set; }
        public static GamePadState gamepad_four_state { get; private set; }

        public static int frame_rate => _frame_rate;
        static int _frame_rate;

        static double _fps_timer = 0;
        static long _frame_count = 0;

        public static int fps_update_frequency_ms { get; set; } = 1000;
        public static int poll_hz { get; private set; } = 120;
        static bool limit_thread_rate = true;
        static bool use_sleep = true;
        static double thread_ms => (1000.0 / (double)poll_hz);

        static DateTime start_dt = DateTime.Now;
        static DateTime current_dt = DateTime.Now;
        static TimeSpan ts;

        static volatile bool run_thread = true;

        static bool _lock_mouse = false;
        public static bool lock_mouse {
            get { return _lock_mouse; }
            set {
                _lock_mouse = value;
                //if (parent.IsActive && !was_active) _lock_mouse = false;                
            }
        }
        static bool _was_locked = false;

        public enum input_method { MonoGame, RawInput }
        static input_method _input_method = input_method.RawInput;
        public static input_method poll_method => _input_method;

        public static void change_polling_method(input_method method) {
            if (_input_method != input_method.RawInput && method == input_method.RawInput) {
                Externs.RawInput.enable = true;
            } else if (_input_method != input_method.MonoGame && method == input_method.MonoGame) {
                Externs.RawInput.enable = false;
            }
            _input_method = method;
        }

        static bool was_active = false;

        static Point pre_lock_mouse_pos = Point.Zero;

        public static Point cursor_pos;
        public static Point cursor_pos_actual;

        public static string RAWINPUT_DEBUG_STRING = "";

        public static void hide_mouse() { parent.IsMouseVisible = false; }
        public static void show_mouse() { parent.IsMouseVisible = true; }

        public static RawInputKeyboardState ri_keyboard_state = new RawInputKeyboardState();
        public static RawInputMouseState ri_mouse_state = new RawInputMouseState();

        public static Point mouse_delta;

        public static void initialize(Game parent) {
            Input.parent = parent;

            Externs.RawInput.create_rawinput_message_loop();

            control_update_thread.Start();
        }

        static TimeSpan one_tick = new TimeSpan((long)250);
        static TimeSpan sleep_ts;
        static void update() {
            while (run_thread) {
                start_dt = DateTime.Now;

                gamepad_one_state = GamePad.GetState(PlayerIndex.One);
                gamepad_two_state = GamePad.GetState(PlayerIndex.Two);
                gamepad_three_state = GamePad.GetState(PlayerIndex.Three);
                gamepad_four_state = GamePad.GetState(PlayerIndex.Four);

                if (_input_method == input_method.RawInput) {

                    cursor_pos = Externs.get_cursor_pos_relative_to_window();
                    cursor_pos_actual = Externs.get_cursor_pos();

                    ri_keyboard_state = RawInputKeyboard.GetState();

                    ri_mouse_state = RawInputMouse.GetState();

                    mouse_delta = ri_mouse_state.Delta;

                    for (int i = 0; i < handlers.Count; i++) {
                        handlers[i].accumulate_mouse_delta(ri_mouse_state.Delta);
                        handlers[i].accumulate_scroll_delta(ri_mouse_state.ScrollDelta);
                    }

                    lock (pressed_keys) {
                        var current_keys = ri_keyboard_state.pressed_keys;

                        foreach (var kt in pressed_keys) {
                            if (!current_keys.Contains(kt.key)) pressed_keys.Remove(kt);
                        }

                        foreach (var k in current_keys) {
                            if (!key_in_pressed_list(k, out _)) {
                                pressed_keys.Add(new KeyTime(k, DateTime.Now));
                            }
                        }
                    }
                    RawInputMouse.reset_scroll_delta();
                }

                if (_input_method == input_method.MonoGame) {
                    keyboard_state = Keyboard.GetState();         
                    
                    mouse_state_previous = mouse_state;
                    mouse_state = Mouse.GetState();

                    if (!lock_mouse) mouse_delta = (mouse_state.Position - mouse_state_previous.Position);
                    else mouse_delta = (mouse_state.Position - new Point(parent.Window.ClientBounds.Size.X / 2, parent.Window.ClientBounds.Size.Y / 2));

                    if (lock_mouse && !_was_locked) mouse_delta = Point.Zero;

                    foreach (InputHandler handler in handlers) handler.accumulate_mouse_delta(mouse_delta);


                    lock (pressed_keys) {
                        var current_keys = keyboard_state.GetPressedKeys();

                        foreach (var kt in pressed_keys) {
                            if (!current_keys.Contains(kt.key)) pressed_keys.Remove(kt);
                        }

                        foreach (var k in current_keys) {
                            if (!key_in_pressed_list(k, out _)) {
                                pressed_keys.Add(new KeyTime(k, DateTime.Now));
                            }
                        }
                    }
                } 
                
                //mouse lock 
                if (parent != null && lock_mouse && !_was_locked) {
                    parent.IsMouseVisible = false;
                    if (_input_method == input_method.RawInput) {
                        pre_lock_mouse_pos = Externs.get_cursor_pos();
                    } else {
                        pre_lock_mouse_pos = mouse_state.Position;
                    }
                }
                if (lock_mouse && parent.IsActive) {
                    reset_mouse(parent.Window.ClientBounds.Size);
                    if (poll_method == input_method.RawInput) {
                        var wpos = Externs.get_window_pos();
                        Externs.set_cursor_pos(wpos.X + (parent.Window.ClientBounds.Size.X / 2), wpos.Y + (parent.Window.ClientBounds.Size.Y / 2));
                    } else {
                        //this has an annoying issue where it will not actually set the mouse position in the situation that you have
                        //A. clicked on this window while another window was focused
                        //B. not released the mouse button yet
                        //this causes issues with the camera spinning around if you, say, have left/right click bound to mouse look                
                        Mouse.SetPosition(parent.Window.ClientBounds.Size.X / 2, parent.Window.ClientBounds.Size.Y / 2);
                    }
                }
                if ((parent != null && !parent.IsActive && was_active && lock_mouse) || (!lock_mouse && _was_locked)) {
                    parent.IsMouseVisible = true;

                    if (_input_method == input_method.RawInput) {
                        Externs.set_cursor_pos(pre_lock_mouse_pos);
                    } else {
                        Mouse.SetPosition(pre_lock_mouse_pos.X, pre_lock_mouse_pos.Y);
                    }
                }

                Window.update();                

                was_active = parent.IsActive;
                _was_locked = lock_mouse;
                //FPS stuff here
                _fps_timer += ts.TotalMilliseconds;
                _frame_count++;

                if (_fps_timer >= fps_update_frequency_ms) {
                    _frame_rate = (int)(_frame_count * (1000.0 / fps_update_frequency_ms));
                    _frame_count = 0;
                    _fps_timer -= fps_update_frequency_ms;
                }
                
                if (limit_thread_rate) {
                    if (use_sleep) {
                        while (run_thread) {
                            sleep_ts = new TimeSpan((long)(((thread_ms - (DateTime.Now - start_dt).TotalMilliseconds) * 1000000f)) / 100);
                            if (sleep_ts.TotalMilliseconds > 0)
                                Thread.Sleep(sleep_ts);
                            current_dt = DateTime.Now;
                            ts = (current_dt - start_dt);
                            if (ts.TotalMilliseconds >= thread_ms) break;
                        }
                    } else {
                        while (run_thread) {
                            current_dt = DateTime.Now;
                            ts = (current_dt - start_dt);

                            if (ts.TotalMilliseconds >= thread_ms) break;
                        }
                    }
                } else {
                    current_dt = DateTime.Now;
                    ts = (current_dt - start_dt);
                    //Thread.Sleep(one_tick);
                }
            }
        }

        static void reset_mouse(Point resolution) {
        }

        public static bool is_pressed(Keys key) {
            if (_input_method == input_method.MonoGame) return keyboard_state.IsKeyDown(key);
            else if (_input_method == input_method.RawInput) return ri_keyboard_state.IsKeyDown(key);
            else return keyboard_state.IsKeyDown(key) || ri_keyboard_state.IsKeyDown(key);
        }
        
        public static bool is_pressed(MouseButtons mouse_button) {
            if (_input_method == input_method.MonoGame) return mg_mb_pressed(mouse_button);
            else if (_input_method == input_method.RawInput) return ri_mb_pressed(mouse_button);
            else return mg_mb_pressed(mouse_button) || ri_mb_pressed(mouse_button);
        }

        private static bool ri_mb_pressed(MouseButtons mouse_button) {
            switch (mouse_button) {
                case MouseButtons.Left:
                    return ri_mouse_state.LeftButton;
                case MouseButtons.Right:
                    return ri_mouse_state.RightButton;
                case MouseButtons.Middle:
                    return ri_mouse_state.MiddleButton;
                case MouseButtons.X1:
                    return ri_mouse_state.XButton1;
                case MouseButtons.X2:
                    return ri_mouse_state.XButton2;
                case MouseButtons.ScrollUp:
                    return ri_mouse_state.ScrollUp;
                case MouseButtons.ScrollDown:
                    return ri_mouse_state.ScrollDown;
                default:
                    return false;
            }
        }

        private static bool mg_mb_pressed(MouseButtons mouse_button) {
            switch (mouse_button) {
                case MouseButtons.Left:
                    return mouse_state.LeftButton == ButtonState.Pressed;
                case MouseButtons.Right:
                    return mouse_state.RightButton == ButtonState.Pressed;
                case MouseButtons.Middle:
                    return mouse_state.MiddleButton == ButtonState.Pressed;
                case MouseButtons.X1:
                    return mouse_state.XButton1 == ButtonState.Pressed;
                case MouseButtons.X2:
                    return mouse_state.XButton2 == ButtonState.Pressed;
                case MouseButtons.ScrollUp:
                    return mouse_state.ScrollWheelValue - mouse_state_previous.ScrollWheelValue > 0;
                case MouseButtons.ScrollDown:
                    return mouse_state.ScrollWheelValue - mouse_state_previous.ScrollWheelValue < 0;
                default: return false;
            }
        }


    }
}

