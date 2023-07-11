using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Swoop.UIElements
{
    internal class TitleBar : UIElement
    {
        public string left_text { get; set; } = string.Empty;
        public string right_text { get; set; } = string.Empty;
        //bottom text

        public TitleBar(Vector2 position, Vector2 size) : base(position, size) {
        }

        public override void update() {
            Window.moving_window = clicking;
        }

        public override void draw() {
            Drawing.fill_rect_outline(position, position + size, Color.Black, Color.White, 1f);

            if (!string.IsNullOrEmpty(left_text))
                Drawing.text(left_text, position + Vector2.One * 3, Color.White);
            if (!string.IsNullOrEmpty(right_text)) {
                float right_text_width = Drawing.measure_string_profont(right_text).X;
                Drawing.text(right_text,
                    position + (Vector2.UnitY * 3) + (Vector2.UnitX * size.X) - (Vector2.UnitX * (right_text_width + 3f))
                    , Color.White);
            }
        }
        public override void draw_rt() { }
    }
}
