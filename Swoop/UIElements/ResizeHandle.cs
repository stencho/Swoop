using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using SwoopLib;

namespace SwoopLib.UIElements {
    public class ResizeHandle : UIElement {
        public ResizeHandle(string name, XYPair position, XYPair size) : base(name, position, size) {
            can_be_focused = false;
            ignore_dialog = true;
        } 

        internal override void added() {

        }

        internal override void update() {
            if (!Swoop.maximized)
                Window.resizing_window = clicking;
        }

        internal override void draw() {
            Drawing.fill_rect_outline(position, position + size, Swoop.UI_background_color, Swoop.get_color(this), 1f);
            var mid = position + (size * 0.5f);
            var qs = size * 0.25f;
            Drawing.tri(
                mid + (XYPair.UnitX * qs.X) + (XYPair.UnitY * qs.Y),
                mid + (XYPair.UnitX * qs.X) - (XYPair.UnitY * qs.Y),
                mid - (XYPair.UnitX * qs.X) + (XYPair.UnitY * qs.Y),
                Swoop.get_color(this), 1f);
        }

        internal override void draw_rt() {

        }

        internal override void handle_focused_input() { }
    }
}
