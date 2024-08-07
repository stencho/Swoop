﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Text;

namespace MGRawInputLib {
    public static class Input {
        static Thread control_update_thread = new Thread(new ThreadStart(update));

        public static List<InputHandler> handlers = new List<InputHandler>();
        static Game parent;

        public static double key_hold_time = 500;
        public static double repeat_rate = 50;

        public static HashSet<InputTime> pressed_inputs = new HashSet<InputTime>();

        public enum InputType {
            Key, MouseButton
        }

        public class InputTime {
            public InputType input_type = InputType.Key;
            object _trigger;
            public Keys key => input_type == InputType.Key ? (Keys)_trigger : Keys.None;
            public MouseButtons mouse_button => input_type == InputType.MouseButton ? (MouseButtons)_trigger : MouseButtons.None;

            DateTime _pressed_time;
            public DateTime pressed_time => _pressed_time;

            public TimeSpan time_since_press => DateTime.Now - _pressed_time;
            public bool held => time_since_press.TotalMilliseconds >= key_hold_time;

            internal bool was_held = false;
            bool _just_held = false;
            public bool just_held { 
                get {
                    _just_held = held && !was_held;

                    if (_just_held) {
                        was_held = true;
                    }

                    return _just_held;
                }
            }

            public TimeSpan repeat_timer;

            DateTime last_time;

            bool _handled = false;
            public bool handled => _handled;
            public void handle() { _handled = true; }
            public void unhandle() { _handled = false; }

            public bool repeat() {
                if (_handled) { last_time = DateTime.Now; return false; }

                var time = DateTime.Now;
                repeat_timer = time - last_time;

                if (repeat_timer.TotalMilliseconds >= repeat_rate) {
                    last_time = time;
                    return true;
                }

                return false;
            }

            public override string ToString() {
                if (input_type == InputType.Key) {
                    return key.ToString();
                } else if (input_type == InputType.MouseButton) {
                    return mouse_button.ToString();
                } else return "";
            }

            public string type_prefix() {
                if (input_type == InputType.Key) {
                    return "key_";
                } else if (input_type == InputType.MouseButton) {
                    return "mouse_";
                } else return "";
            }

            public InputTime() {
            }

            public InputTime(Keys trigger, DateTime time) {
                this._trigger = trigger;
                _pressed_time = time;
                last_time = pressed_time;
                input_type = InputType.Key;
            }
            public InputTime(MouseButtons trigger, DateTime time) {
                this._trigger = trigger;
                _pressed_time = time;
                last_time = pressed_time;
                input_type = InputType.MouseButton;
            }
        }

        public static string list_keys() {
            if (pressed_inputs == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (InputTime input in pressed_inputs) {
                sb.Append(input.type_prefix()); sb.Append(input.ToString().ToLower()); sb.Append(", "); 
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }


        public static bool in_pressed_list(Keys k, out InputTime input_time) {
            lock (pressed_inputs) {
                foreach (var input in pressed_inputs) {
                    if (k == input.key) { input_time = input; return true; }
                }
            }
            input_time = new InputTime();
            return false;
        }
        public static bool in_pressed_list(MouseButtons mb, out InputTime input_time) {
            lock (pressed_inputs) {
                foreach (var input in pressed_inputs) {
                    if (mb == input.mouse_button) { input_time = input; return true; }
                }
            }
            input_time = new InputTime();
            return false;
        }


        public static void end() { 
            run_thread = false;
            Externs.RawInput.destroy_rawinput_message_loop();
        }

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
        public static int poll_hz { get; private set; } = 500;
        static bool limit_thread_rate = true;
        static bool use_sleep = true;
        static double thread_ms => (1000.0 / (double)poll_hz);

        static long start_tick   = 0;
        static long current_tick = 0;
        static long time_span_ticks;

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

            control_update_thread.IsBackground = true;
            control_update_thread.Start();

            parent.Exiting += Parent_Exiting;
            parent.Disposed += Parent_Disposed;
        }

        private static void Parent_Disposed(object? sender, EventArgs e) {
            run_thread = false;
            //Externs.timeEndPeriod(3);
        }

        private static void Parent_Exiting(object? sender, EventArgs e) {
            run_thread = false;
            //Externs.timeEndPeriod(1);
        }

        static TimeSpan sleep_ts;
        static Stopwatch stopwatch = new Stopwatch();

        static HashSet<MouseButtons> get_pressed_mouse_buttons() {
            HashSet<MouseButtons> pmb = new HashSet<MouseButtons>();
            var buttons = Enum.GetValues(typeof(MouseButtons));
            
            foreach(MouseButtons b in buttons) {
                if (is_pressed(b)) pmb.Add(b);
            }

            return pmb;
        }

        static void update() {
            stopwatch.Start();
            Debug.Print(Stopwatch.IsHighResolution.ToString() + " " + Stopwatch.Frequency);
            Externs.timeBeginPeriod(5);

            //while (run_thread) {
                //Thread.Sleep(1000);
            //}

            while (run_thread) {
                start_tick = stopwatch.ElapsedTicks;
                //start_tick = current_tick;
                bool active = false;

                if (parent != null) 
                    if (parent.IsActive)
                        active = parent.IsActive;

                //gamepad_one_state = GamePad.GetState(PlayerIndex.One);
                //gamepad_two_state = GamePad.GetState(PlayerIndex.Two);
                //gamepad_three_state = GamePad.GetState(PlayerIndex.Three);
                //gamepad_four_state = GamePad.GetState(PlayerIndex.Four);

                if (_input_method == input_method.RawInput && Externs.RawInput.new_rawinput_data) {

                    cursor_pos = Externs.get_cursor_pos_relative_to_window(parent.Window);
                    cursor_pos_actual = Externs.get_cursor_pos();

                    ri_keyboard_state = RawInputKeyboard.GetState();

                    ri_mouse_state = RawInputMouse.GetState();

                    mouse_delta = ri_mouse_state.Delta;

                    for (int i = 0; i < handlers.Count; i++) {
                        handlers[i].accumulate_mouse_delta(ri_mouse_state.Delta);
                        handlers[i].accumulate_scroll_delta(ri_mouse_state.ScrollDelta);
                    }

                    lock (pressed_inputs) {
                        var current_keys = ri_keyboard_state.pressed_keys;
                        var current_mouse_buttons = get_pressed_mouse_buttons();

                        foreach (var kt in pressed_inputs) {
                            //kt.unhandle();

                            if (kt.input_type == InputType.Key && !current_keys.Contains(kt.key)) 
                                pressed_inputs.Remove(kt);
                            if (kt.input_type == InputType.MouseButton && !current_mouse_buttons.Contains(kt.mouse_button)) 
                                pressed_inputs.Remove(kt);
                        }

                        foreach (var k in current_keys) {
                            if (!in_pressed_list(k, out _)) {
                                pressed_inputs.Add(new InputTime(k, DateTime.Now));
                            }
                        }

                        foreach (var mb in current_mouse_buttons) {
                            if (!in_pressed_list(mb, out _)) {
                                pressed_inputs.Add(new InputTime(mb, DateTime.Now));
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


                    lock (pressed_inputs) {
                        var current_keys = keyboard_state.GetPressedKeys();

                        foreach (var kt in pressed_inputs) {
                            if (!current_keys.Contains(kt.key)) pressed_inputs.Remove(kt);
                        }

                        foreach (var k in current_keys) {
                            if (!in_pressed_list(k, out _)) {
                                pressed_inputs.Add(new InputTime(k, DateTime.Now));
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
                if (lock_mouse && active) {
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

                if ((parent != null && !active && was_active && lock_mouse) || (!lock_mouse && _was_locked)) {
                    parent.IsMouseVisible = true;

                    if (_input_method == input_method.RawInput) {
                        Externs.set_cursor_pos(pre_lock_mouse_pos);
                    } else {
                        Mouse.SetPosition(pre_lock_mouse_pos.X, pre_lock_mouse_pos.Y);
                    }
                }
                

                Window.update();                

                was_active = active;
                _was_locked = lock_mouse;

                if (limit_thread_rate) {
                    if (use_sleep) {                            
                        while (true) {
                            current_tick = stopwatch.ElapsedTicks;
                            time_span_ticks = (current_tick - start_tick);
                            sleep_ts = new TimeSpan((long)(thread_ms * 10000.0) - time_span_ticks);

                            if (sleep_ts.Ticks > 0)
                                Thread.Sleep(sleep_ts);

                            //current_tick = DateTime.Now.Ticks;
                            //break;
                            if (time_span_ticks / 10000.0 >= thread_ms) 
                                break;
                        }

                    } else {
                        while (true) {
                            current_tick = stopwatch.ElapsedTicks;
                            time_span_ticks = (current_tick - start_tick);

                            if (time_span_ticks / 10000.0 >= thread_ms) break;
                        }

                    }
                }

                current_tick = stopwatch.ElapsedTicks;
                time_span_ticks = (current_tick - start_tick);
                //FPS stuff here

                //if (time_span_ticks / 10000.0 < fps_update_frequency_ms)
                _fps_timer += time_span_ticks / 10000.0;

                _frame_count++;

                if (_fps_timer >= fps_update_frequency_ms) {
                    _frame_rate = (int)(_frame_count * (1000.0 / (double)_fps_timer));
                    _frame_count = 0;
                    _fps_timer -= fps_update_frequency_ms;
                    //_fps_timer = 0;
                }

                //start_tick = stopwatch.ElapsedTicks;

                //start_tick = current_tick;
            }
            Externs.timeEndPeriod(5);
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

