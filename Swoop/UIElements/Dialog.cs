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
        string title = "";

        public Dialog(Vector2 position, Vector2 size, string title)
            : base(position, size) {
            this.enable_render_target = true;
            this.title = title;
            build(position, size);
        }

        public Dialog(Vector2 size, string title) 
            : base((SwoopGame.resolution * 0.5f) - (size * 0.5f), size) {
            this.enable_render_target = true;
            this.title = title;
            build((SwoopGame.resolution * 0.5f) - (size * 0.5f), size);
        }

        public Dialog(Vector2 position, Vector2 size)
            : base(position, size) {
            this.enable_render_target = true;
            build(position, size);
        }

        public Dialog(Vector2 size)
            : base((SwoopGame.resolution * 0.5f) - (size * 0.5f), size) {
            this.enable_render_target = true;
            build((SwoopGame.resolution * 0.5f) - (size * 0.5f), size);
        }

        void build(Vector2 position, Vector2 size) {
            sub_elements = new UIElementManager(position, size);

            sub_elements.add_element("test_label", new Label("this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                size/2, Label.anchor_point.CENTER));

            ((Label)sub_elements.elements["test_label"]).draw_outline = true;
            ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;

            sub_elements.add_element("test_label_2", new Label("this another test", Vector2.One * 10));
            ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

            sub_elements.add_element("close", new Button("close", new Vector2(size.X / 2, size.Y - 40)));
            ((Button)sub_elements.elements["close"]).click_action = () => {
                this.parent.remove_element(this.name);
            };
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

            Vector2 tl = position + (Vector2.UnitX * 6) - (Vector2.UnitY * Drawing.measure_string_profont("A").Y);

            Drawing.fill_rect_outline(tl - (Vector2.UnitX * 3), tl + Drawing.measure_string_profont(title) + (Vector2.UnitX * 3), Color.Black, Color.White, 1f);
            Drawing.text(title, tl, Color.White);
            

        }
    }
}
