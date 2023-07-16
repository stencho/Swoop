using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Swoop.UIElements {
    internal class Panel : UIElement {

        public Action build_action;

        public Panel(Vector2 position, Vector2 size) : base(position, size) {
            this.enable_render_target = true;
            build(position, size);
        }

        void build(Vector2 position, Vector2 size) {
            if (build_action != null) build_action();            
        }

        public override void update() {
            sub_elements.update();
        }

        public override void draw_rt() {
            Drawing.fill_rect(Vector2.Zero, size.X, size.Y, Color.Black);
            sub_elements.sub_draw(draw_target);
            Drawing.rect(Vector2.One, size, Color.White, 1f);
        }

        public override void draw() {
            Drawing.image(draw_target, position, size);
        }
    }
}
