using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SwoopLib.UIElements
{
    public class TitleBar : UIElement
    {
        public string left_text { get; set; } = string.Empty;
        public string right_text { get; set; } = string.Empty;

        bool auto_height = false;
        public TitleBar(string name, XYPair position, int width) : base(name, position, new XYPair(width, Drawing.measure_string_profont("A").Y + 3)) {
            auto_height = true;
            can_be_focused = false;
        }
        public TitleBar(string name, XYPair position, XYPair size) : base(name, position, size) {
            auto_height = false;
            can_be_focused = false;
        }

        internal override void update() {
            if (!Swoop.maximized) {
                Window.moving_window = clicking;
            }
        }

        internal override void draw() {
            Drawing.fill_rect_outline(position + XYPair.One, position + size, Swoop.UI_background_color, Swoop.UI_color_base, 1f);

            if (!string.IsNullOrEmpty(left_text))
                Drawing.text(left_text, position + (XYPair.Down * 3) + (XYPair.Right * 3), Swoop.foreground_flip_color_disabled);
            if (!string.IsNullOrEmpty(right_text)) {
                float right_text_width = Drawing.measure_string_profont(right_text).X;
                Drawing.text(right_text,
                    position + (XYPair.UnitY * 3) + (XYPair.UnitX * size.X) - (XYPair.UnitX * (right_text_width + 6f))
                    , Swoop.foreground_flip_color_disabled);
            }
        }
        internal override void draw_rt() { }
        internal override void added() { }

        internal override void handle_focused_input() { }
    }
}
