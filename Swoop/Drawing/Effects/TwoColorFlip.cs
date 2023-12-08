using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.Effects {
    public class TwoColorFlip: ManagedEffect {       

        public void configure_shader(Vector2 top_left, Vector2 bottom_right, Color color_a, Color color_b) {
            //set_param("color_a", color_a);
            //set_param("color_b", color_b);
            //set_param("screen_texture", Drawing.main_render_target);
        }

        /// <summary>
        /// A pixel shader which works out if a pixel on the screen is closer to a or b, 
        /// then draws the input with the opposite color
        /// </summary>
        /// <param name="color_a"></param>
        /// <param name="color_b"></param>
        public TwoColorFlip(ContentManager content) {
            load_shader_file(content, "effects/two_color_flip");
        }
    }
}
