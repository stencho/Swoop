using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.Effects {
    public class DitherEffect
    {
        static Effect shader;
        public static Effect effect => shader;

        public int dither_size = 1;

        public Color color_a = Color.White;
        public Color color_b = Color.Black;

        public bool clip_b = false;

        public void configure_shader(Vector2 top_left, Vector2 bottom_right)
        {
            shader.Parameters["clip_b"].SetValue(clip_b);

            shader.Parameters["color_a"].SetValue(color_a.ToVector4());
            shader.Parameters["color_b"].SetValue(color_b.ToVector4());

            shader.Parameters["top_left"].SetValue(top_left);
            shader.Parameters["bottom_right"].SetValue(bottom_right);
        }

        public DitherEffect(ContentManager content)
        {
            if (shader == null) shader = content.Load<Effect>("effects/dither");
        }
    }
}
