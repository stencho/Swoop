using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace Swoop.UIElements {
    internal class MouseDeltaDisplay : UIElement {
        Vector2 base_delta;

        InputHandler input = new InputHandler();

        public MouseDeltaDisplay(Vector2 position, Vector2 size) : base(position, size) {
            this.position = position;
            this.size = size;
        }
        
        public override void update() {
            input.update();
            if (input != null)
                base_delta = input.mouse_delta.ToVector2();
        }

        public override void draw() {
            SDF.draw_centered(Drawing.sdf_circle, position, size, Color.Black, Color.White, 0.99f, 0.1f, 1f,false);

            Drawing.line_rounded_ends(position + (Vector2.Normalize(base_delta) * ((size / 2f) * 0.75f)), position + (Vector2.Normalize(base_delta) * ((size/2f) * 0.9f)) , Color.MonoGameOrange, 6f);
        }
        public override void draw_rt() { }
    }
}
