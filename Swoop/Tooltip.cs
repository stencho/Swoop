using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib {
    public class Tooltip {
        //Static/manager class
        public static class Manager {
            static Tooltip? current_tooltip = null;
            static Tooltip? previous_tooltip = null;
            static bool tooltip_changed => current_tooltip != previous_tooltip;

            public static bool delay_show { get; set; } = false;
            public static int delay_show_ms { get; set; } = 300;
            public static double delay_timer = 0;

            public enum KeepOnScreenMode {
                Flip, KeepInCorner
            }
            public static KeepOnScreenMode keep_on_screen_mode = KeepOnScreenMode.Flip;

            public static void update() {

                current_tooltip = null;
                if (UIElementManager.manager_under_mouse != null && UIElementManager.manager_under_mouse.element_under_mouse != null) {
                    if (UIElementManager.manager_under_mouse.element_under_mouse.tooltip != null)
                        current_tooltip = UIElementManager.manager_under_mouse.element_under_mouse.tooltip;
                }

                if (current_tooltip != null) {
                    Swoop.render_target_overlay.draw.register_action("tooltip", current_tooltip.draw_tooltip);
                } else {
                    Swoop.render_target_overlay.draw.unregister_action("tooltip");
                }
                if (delay_show) {
                    delay_timer += Swoop.game_time.ElapsedGameTime.TotalMilliseconds;
                    if (tooltip_changed) delay_timer = 0;
                }

                previous_tooltip = current_tooltip;
            }

            public static void draw() {
            }
        }

        XYPair margin = XYPair.One * 4f;

        //Individual tooltips
        string _title = "";
        public string title {
            get { return _title; }
            set { _title = value; }
        }

        string _text = "";
        public string text {
            get { return _text; }
            set { _text = value; }
        }

        public Tooltip(string text) {
            this.text = text;
        }
        public Tooltip(string title, string text) {
            this.text = text;
            this.title = title;
        }

        ~Tooltip() {
        }

        public void draw_tooltip() {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(text)) return;
            if (Manager.delay_show && Manager.delay_show_ms > Manager.delay_timer) return;

            XYPair position = Input.cursor_pos.ToXYPair();
            XYPair total_size, title_size, text_size;
            total_size = title_size = text_size = XYPair.Zero;            

            if (!string.IsNullOrEmpty(title))
                title_size = Drawing.measure_string_profont_xy(this.title) + (XYPair.Down * 2f);
            if (!string.IsNullOrEmpty(text))
                text_size = Drawing.measure_string_profont_xy(this.text);
            
            total_size = margin + title_size.Y_only + text_size.Y_only + margin;

            if (title_size.X > text_size.X)
                total_size += title_size.X_only;
            else
                total_size += text_size.X_only;

            XYPair offset = XYPair.One * 12f;

            if (Manager.keep_on_screen_mode == Manager.KeepOnScreenMode.KeepInCorner) {
                if (position.X + offset.X + total_size.X > Swoop.resolution.X) {
                    offset -= ((position + (XYPair.One * 12f) + total_size) - Swoop.resolution).X_only;
                }
                if (position.Y + offset.Y + total_size.Y > Swoop.resolution.Y) {
                    offset -= ((position + (XYPair.One * 12f) + total_size) - Swoop.resolution).Y_only;
                }
            } else if ( Manager.keep_on_screen_mode == Manager.KeepOnScreenMode.Flip) {
                if (Input.cursor_pos.X > Swoop.resolution.X / 2) {
                    offset -= ((XYPair.One * 12f) + total_size).X_only;
                }
                if (Input.cursor_pos.Y > Swoop.resolution.Y / 2) {
                    offset -= ((XYPair.One * 12f) + total_size).Y_only;
                }
            }

            Drawing.fill_rect_outline(position + offset, position + offset + total_size, Swoop.UI_background_color, Swoop.UI_color, 1f);

            if (!string.IsNullOrEmpty(title))
                Drawing.text(title, position + offset + margin, Swoop.UI_highlight_color);

            Drawing.text(text, position + offset + margin + title_size.Y_only, Swoop.UI_color);
        }

    }
}
