using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Swoop.UIElements {
    public class MouseDeltaDriftTest : UIElement {
        Vector2 center => (size * 0.5f);

        Vector2 MonoGame_test_mouse_pos;
        Vector2 RawInput_test_mouse_pos;

        InputHandler inputh = new InputHandler();

        public MouseDeltaDriftTest(Vector2 position, Vector2 size) : base(position, size) {
            MonoGame_test_mouse_pos = center;
            RawInput_test_mouse_pos = center;
            enable_render_target = true;
        }

        public override void update() {
            inputh.update();

            if (clicking) {
                MonoGame_test_mouse_pos += inputh.mouse_delta.ToVector2();
            } 
            if (!mouse_down) {
                MonoGame_test_mouse_pos = Vector2.LerpPrecise(MonoGame_test_mouse_pos, center, 25f * (float)(1000.0 / SwoopGame.target_fps / 1000.0));
                RawInput_test_mouse_pos = Vector2.LerpPrecise(RawInput_test_mouse_pos, center, 25f * (float)(1000.0 / SwoopGame.target_fps / 1000.0));
            }
            if (clicking && !was_clicking) {
                Input.lock_mouse = true;
            }
            if (!mouse_down && mouse_was_down) {
                Input.lock_mouse = false;            
            }
        }

        public override void draw_rt() {
            Drawing.graphics_device.Clear(Color.Black);
            //SDF.draw_centered(Drawing.sdf_circle, center, Vector2.One * 7f, Color.White);
            SDF.draw_centered(Drawing.sdf_circle, MonoGame_test_mouse_pos, Vector2.One * 7f, Color.MonoGameOrange);
            SDF.draw_centered(Drawing.sdf_circle, RawInput_test_mouse_pos, Vector2.One * 7f, Color.LightBlue);            
        }

        public override void draw() {
            Drawing.image(draw_target, position, size);
            Drawing.rect(position, position + size, Color.White, 1f);
        }

    }
}
