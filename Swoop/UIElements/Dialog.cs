using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Swoop.UIElements {
    internal class Dialog : UIElement {

        public Dialog(Vector2 position, Vector2 size, string title)
            : base(position, size) {
            this.enable_render_target = true;
            build(position, size);
        }

        public Dialog(Vector2 size, string title) 
            : base((SwoopGame.resolution * 0.5f) - (size * 0.5f), size) {
            this.enable_render_target = true;
            build((SwoopGame.resolution * 0.5f) - (size * 0.5f), size);

        }

        void build(Vector2 position, Vector2 size) {
            sub_elements = new UIElementManager(position, size);
            sub_elements.add_element("test_button", new Button("test", Vector2.One * -5f));
            sub_elements.add_element("test_label", new Label("this is a test label for testing\n" +
                "all sorts of different text\n\nit's also a sub-element of a dialog box UI element and this is actually a really long string which goes way beyond the edge of this dialog box but is being line wrapped\nabcdefghijklmnopqrstuvwxyz1234567890-=!@#$%^&*(){}:\"\\|<>?,./;\'[]~`", Vector2.One * 30, (size - Vector2.One * 60)));
        }

        public override void update() {
            sub_elements.update();
        }

        public override void draw_rt() {
            Drawing.fill_rect_outline(Vector2.One, size, Color.Black, Color.White, 1f);
            sub_elements.sub_draw(draw_target);
        }

        public override void draw() {
            Drawing.image(draw_target, position, size);
        }
    }
}
