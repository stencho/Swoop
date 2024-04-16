using Microsoft.Xna.Framework;

namespace MGRawInputLib {
    public static class Window {
        public static bool is_active = false;
        public static bool mouse_over_window = false;
        static Point relative_mouse;

        public static Action<Point>? resize_start;
        public static Action<Point>? resize_end;

        public static Rectangle window_rect = Rectangle.Empty;

        static GameWindow parent_window;
        public static void init(GameWindow parent_game_window) {
            parent_window = parent_game_window;
        }

        public static void update() {
            window_rect = Externs.get_window_rect();
            var cp = Externs.get_cursor_pos();

            if (Input.mouse_delta.X != 0 || Input.mouse_delta.Y != 0) {
                mouse_over_window = Externs.window_under_cursor();

                if (moving_window && Input.is_pressed(MouseButtons.Left)) {
                    Externs.MoveWindow(Externs.actual_window_handle,
                        cp.X - relative_mouse.X,
                        cp.Y - relative_mouse.Y,
                        window_rect.Width, window_rect.Height, false);

                } else if (resizing_window && Input.is_pressed(MouseButtons.Left)) {
                    Externs.MoveWindow(Externs.actual_window_handle,
                        window_rect.X, window_rect.Y,
                    start_size.X - (relative_mouse.X - (cp.X - window_rect.X)),
                    start_size.Y - (relative_mouse.Y - (cp.Y - window_rect.Y)), false);

                }
            }
        }

        static Point start_size = Point.Zero;
        static bool _rz_wnd = false;
        public static bool resizing_window {
            get {
                return _rz_wnd;
            }
            set {
                if (_rz_wnd == false && value == true) {
                    relative_mouse = Input.cursor_pos;
                    start_size = Externs.get_window_rect().Size - Externs.get_client_area_offset(parent_window);

                    if (resize_start != null) resize_start(Externs.get_window_rect().Size);
                }
                if (_rz_wnd == true && value == false) {
                    if (resize_end != null) resize_end(Externs.get_window_rect().Size);
                }
                _rz_wnd = value;
            }
        }

        static bool _mv_wnd = false;
        public static bool moving_window {
            get {
                return _mv_wnd;
            }
            set {
                if (_mv_wnd == false && value == true)
                    relative_mouse = Externs.get_cursor_pos() - Externs.get_window_pos();

                _mv_wnd = value;
            }
        }
    }
}
