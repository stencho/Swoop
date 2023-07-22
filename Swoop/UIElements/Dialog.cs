using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib.UIElements {
    public class Dialog : UIElement {
        string title = "";

        public Dialog(string name, XYPair position, XYPair size, string title, Action<Dialog, UIElementManager>? build_action)
            : base(name, position, size) {
            this.enable_render_target = true;
            this.title = title;
            build(position, size, build_action);
        }

        public Dialog(string name, XYPair position, XYPair size, Action<Dialog, UIElementManager>? build_action)
            : base(name, position, size) {
            this.enable_render_target = true;
            build(position, size, build_action);
        }

        void build(XYPair position, XYPair size, Action<Dialog, UIElementManager>? build_action) {
            can_be_focused = false;
            sub_elements = new UIElementManager(position, size);

            if (build_action != null) build_action(this, sub_elements);
        }

        internal override void added() {
            parent.dialog_element = this.name;
        }

        internal override void update() {            
            this.position = parent.size / 2 - (this.size / 2);

            sub_elements.size = this.size;
            sub_elements.position = this.position;

            sub_elements.update();
        }

        internal override void draw_rt() {
            Drawing.fill_rect(XYPair.Zero, size.X, size.Y, Swoop.UI_background_color);
            sub_elements.sub_draw(draw_target);
            Drawing.rect(XYPair.One, size, Swoop.get_color(this), 1f);
        }

        internal override void draw() {
            Drawing.image(draw_target, position, size);

            XYPair tl = position + (XYPair.UnitX * 6) - (XYPair.UnitY * Drawing.measure_string_profont("A").Y);

            if (!String.IsNullOrWhiteSpace(title)) {
                Drawing.fill_rect_outline(tl - (XYPair.UnitX * 3) - (XYPair.UnitY * 1), 
                    tl + Drawing.measure_string_profont_xy(title) + (XYPair.UnitX * 3) + (XYPair.UnitY * 1),
                    Swoop.UI_background_color, Swoop.get_color(this), 1f);
                Drawing.text(title, tl, Swoop.get_color(this));
            }
        }
    }
}
