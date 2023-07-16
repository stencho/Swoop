using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements { 
    public class Button : UIElement {
        string _text = "button";
        public string text => _text;
        Vector2 margin = (Vector2.UnitY * 1.5f) + (Vector2.UnitX * 5);

        public Action click_action = null;

        public Color color_foreground { get; set; } = Swoop.UIColor;
        public Color color_background { get; set; } = Color.Black;

        public Button(string name, string text, Vector2 position, Vector2 size) : base(name, position, size) {
            _text = text;
        }

        public Button(string name, string text, Vector2 position) : base(name, position, Vector2.Zero) {
            _text = text;
            this.position = position;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        public void change_text(string text) {
            _text = text;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        internal override void update() {
            //successful click, released left mouse while over the button and clicking
            if (!clicking && was_clicking && mouse_over) {
                if (click_action != null) click_action();
            }
        }


        internal override void draw() { 
            bool col_toggle = mouse_over && !mouse_down;
            Drawing.fill_rect_outline(position, position + size, col_toggle ? color_foreground : color_background, color_foreground, 1f);
            Drawing.text(_text, position + margin, col_toggle ? color_background : color_foreground);
        }

        internal override void draw_rt() { }
        internal override void added() { }
    }
}
