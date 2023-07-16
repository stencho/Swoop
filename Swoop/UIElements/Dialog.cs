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

        public Dialog(Vector2 position, Vector2 size, string title, Action<Dialog, UIElementManager>? build_action)
            : base(position, size) {
            this.enable_render_target = true;
            this.title = title;
            build(position, size, build_action);
        }

        public Dialog(Vector2 position, Vector2 size, Action<Dialog, UIElementManager>? build_action)
            : base(position, size) {
            this.enable_render_target = true;
            build(position, size, build_action);
        }

        void build(Vector2 position, Vector2 size, Action<Dialog, UIElementManager>? build_action) {
            sub_elements = new UIElementManager(position, size.ToPoint());

            if (build_action != null) build_action(this, sub_elements);
        }

        internal override void added() {

        }

        internal override void update() {
            sub_elements.update();
        }

        internal override void draw_rt() {
            Drawing.fill_rect(Vector2.Zero, size.X, size.Y, Color.Black);
            sub_elements.sub_draw(draw_target);
            Drawing.rect(Vector2.One, size, Color.White, 1f);
        }

        internal override void draw() {
            Drawing.image(draw_target, position, size);

            Vector2 tl = position + (Vector2.UnitX * 6) - (Vector2.UnitY * Drawing.measure_string_profont("A").Y);

            if (!String.IsNullOrWhiteSpace(title)) {
                Drawing.fill_rect_outline(tl - (Vector2.UnitX * 3) - (Vector2.UnitY * 1), tl + Drawing.measure_string_profont(title) + (Vector2.UnitX * 3) + (Vector2.UnitY * 1), Color.Black, Color.White, 1f);
                Drawing.text(title, tl, Color.White);
            }
        }
    }
}
