using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class Panel : UIElement {

        public Action<UIElementManager> build_action;

        public Panel(string name, Vector2 position, Vector2 size, Action<Panel, UIElementManager>? build_action) : base(name, position, size) {
            this.enable_render_target = true;
            build(position, size, build_action);
        }

        void build(Vector2 position, Vector2 size, Action<Panel, UIElementManager>? build_action) {
            sub_elements = new UIElementManager(position, size.ToPoint());
            if (build_action != null) build_action(this, sub_elements);            
        }

        internal override void update() {
            sub_elements.update();
        }

        internal override void draw_rt() {
            Drawing.fill_rect(Vector2.Zero, size.X, size.Y, Color.Black);
            sub_elements.sub_draw(draw_target);
            Drawing.rect(Vector2.One, size, Swoop.UIColor, 1f);
        }

        internal override void draw() {
            Drawing.image(draw_target, position, size);
        }

        internal override void added() {}
    }
}
