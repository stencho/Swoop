using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class ToggleButton : UIElement {
        string _text_on = "Enabled";
        string _text_off = "Disabled";

        public string text => (toggled_on ? _text_on : _text_off);

        bool click_highlight = false;

        XYPair margin = (XYPair.UnitY * 2f) + (XYPair.UnitX * 5);

        public bool toggled_on;

        public Action<ToggleButton, bool> toggled;

        public ToggleButton(string name, string on_text, string off_text, XYPair position, XYPair size) : base(name, position, size) {
            _text_on = on_text;
            _text_off = off_text;
        }

        public ToggleButton(string name, string on_text, string off_text, XYPair position) : base(name, position, XYPair.Zero) {
            _text_on = on_text;
            _text_off = off_text;
            this.position = position;
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont(text);
        }

        public ToggleButton(string name, XYPair position) : base(name, position, XYPair.Zero) {
            this.position = position;
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont(text);
        }

        public void change_text(string on_text, string off_text) {
            _text_on = on_text;
            _text_off = off_text;
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont(text);
        }
        public void change_text() {
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont(text);
        }

        internal override void update() {
            bool interacted = false;

            if (!clicking && was_clicking && mouse_over) interacted = true;            

            if (is_focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter)) 
                interacted = true;

            if (interacted) { 
                toggled_on = !toggled_on;
                change_text();
                if (toggled != null) toggled(this, toggled_on);
            }

            click_highlight = false;
            if ((is_focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter)) || (mouse_over && !mouse_down)) {
                click_highlight = true;
            }

        }

        internal override void draw() {
            Drawing.fill_rect_outline(position + XYPair.One, position + size, toggled_on ? Swoop.get_color(this) : Swoop.UI_background_color, Swoop.get_color(this), 1f);
            Drawing.text(text, position + margin, toggled_on ? Swoop.UI_background_color : Swoop.get_color(this));

            Drawing.rect(position + (XYPair.One*2), (position + size) - (XYPair.One), click_highlight ? Swoop.get_color(this) : Swoop.UI_background_color, click_highlight ? 3f : 1f);
        }

        internal override void draw_rt() { }
        internal override void added() { }
    }
}
