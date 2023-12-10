using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib.Effects {
    public class InvertingText: ManagedEffect {
        XYPair position;
        
        string _text = "text";
        public string text {
            get { return _text; }
            set { _text = value; build_text_rt(); }
        }

        AutoRenderTarget render_target;

        void build_text_rt() {
            var size = Drawing.measure_string_profont_xy(_text);

            render_target.size = size;
        }



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
        public InvertingText(ContentManager content, string text) {
            render_target = new AutoRenderTarget();
            this.text = text;

            load_shader_file(content, "effects/two_color_flip");
        }
    }
}
