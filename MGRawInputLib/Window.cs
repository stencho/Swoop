using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MGRawInputLib {
    public static class Window {
        public enum sizing_dir { VERTICAL, HORIZONTAL, FREE }
        public static sizing_dir resize_direction = sizing_dir.FREE;

        public static Point relative_mouse;
        public static Rectangle start_size;

        public static void update() {
            if (moving_window) {
                var rec = Externs.get_window_rect();
                var cp = Externs.get_cursor_pos();

                if (Input.mouse_delta.X != 0 || Input.mouse_delta.Y != 0) {
                    Externs.MoveWindow(Externs.actual_window_handle,
                        cp.X - relative_mouse.X,
                        cp.Y - relative_mouse.Y,
                        rec.Width, rec.Height, false);
                }
            } 
        }

        static bool _mv_wnd = false;
        public static bool moving_window {
            get {
                return _mv_wnd;
            }
            set {
                if (_mv_wnd != value && value)
                    relative_mouse = Externs.get_cursor_pos() - Externs.get_window_pos();

                _mv_wnd = value;
            }
        }
    }
}
