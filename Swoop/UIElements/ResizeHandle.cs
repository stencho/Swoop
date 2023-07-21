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
        public ResizeHandle(string name, Vector2 position, Vector2 size) : base(name, position, size) {
            can_be_focused = false;
            ignore_dialog = true;
        } 

        internal override void added() {

        }

        internal override void update() {
            Window.resizing_window = clicking;
        }

        internal override void draw() {
            Drawing.rect(position, position + size, Swoop.get_color(this), 1f);
            var mid = position + (size * 0.5f);
            var qs = size * 0.25f;
            Drawing.tri(
                mid + (Vector2.UnitX * qs.X) + (Vector2.UnitY * qs.Y),
                mid + (Vector2.UnitX * qs.X) - (Vector2.UnitY * qs.Y),
                mid - (Vector2.UnitX * qs.X) + (Vector2.UnitY * qs.Y),
                Swoop.get_color(this), 1f);
        }

        internal override void draw_rt() {

        }

    }
}
