using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.UIElements {
    public class GDICanvas : UIElement {
        RenderableGDIBitmap gdi_bitmap;
        public bool manual_texture_update = false;

        public GDICanvas(string name, XYPair position, XYPair size, Action<System.Drawing.Graphics> draw_action) : base(name, position, size) {
            gdi_bitmap = new RenderableGDIBitmap(size.X, size.Y, draw_action);
            can_be_focused = false;
        }

        public void update_texture() {
            gdi_bitmap.copy_bitmap_to_texture();
        }


        internal override void draw() {
            gdi_bitmap.update_canvas();

            if (!manual_texture_update) {
                gdi_bitmap.copy_bitmap_to_texture();
            }

            Drawing.image(gdi_bitmap.texture, position, size);
            Drawing.rect(position, position + size, Swoop.get_color(this), 1f);
        }

        internal override void draw_rt() {

        }

        internal override void handle_focused_input() {
        }

        internal override void update() {
        }
        internal override void added() { }
    }
}
