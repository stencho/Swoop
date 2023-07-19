using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class ToggleButton : UIElement {
        string _text = "button";
        public string text => _text;
        Vector2 margin = (Vector2.UnitY * 1.5f) + (Vector2.UnitX * 5);

        public bool toggled_on;

        public Action<ToggleButton, bool> toggled;

        public ToggleButton(string name, string text, Vector2 position, Vector2 size) : base(name, position, size) {
            _text = text;
        }

        public ToggleButton(string name, string text, Vector2 position) : base(name, position, Vector2.Zero) {
            _text = text;
            this.position = position;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        public void change_text(string text) {
            _text = text;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        internal override void update() {
            bool interacted = false;

            if (!clicking && was_clicking && mouse_over) interacted = true;            

            if (is_focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter)) 
                interacted = true;

            if (interacted) toggled_on = !toggled_on;
            if (interacted && toggled != null) toggled(this, toggled_on); 
        }

        internal override void draw() {
            bool m = (mouse_over && !mouse_down) || (is_focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter));

            bool col_toggle = toggled_on;

            Drawing.fill_rect_outline(position, position + size, col_toggle ? Swoop.get_color(this) : Swoop.UIBackgroundColor, Swoop.get_color(this), 1f);
            Drawing.text(_text, position + margin, col_toggle ? Swoop.UIBackgroundColor : Swoop.get_color(this));

            Drawing.rect(position + (Vector2.One * 1), (position + size) - (Vector2.One * 1), m ? Swoop.get_color(this) : Swoop.UIBackgroundColor, m ? 3f : 1f);
        }

        internal override void draw_rt() { }
        internal override void added() { }
    }
}
