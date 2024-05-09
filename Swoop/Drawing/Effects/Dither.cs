using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib.Effects {
    public class Dither : ManagedEffect {

        public bool clip_b = false;

        public void configure_shader(Vector2 top_left, Vector2 bottom_right, Color color_a, Color color_b) {
            set_param("clip_b", clip_b);

            set_param("color_a", color_a);
            set_param("color_b", color_b);

            set_param("top_left", top_left);
            set_param("bottom_right", bottom_right);
        }

        public Dither(ContentManager content) : base(content, "effects/dither") {}
    }
}
