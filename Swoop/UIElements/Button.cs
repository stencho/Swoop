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
        XYPair margin = (XYPair.UnitY * 2f) + (XYPair.UnitX * 5);

        bool click_highlight = false;

        Action custom_draw;

        public Action click_action = null;

        bool _auto_size = false;
        public bool auto_size {
            get => _auto_size; set {

            }
        }

        public Button(string name, Action custom_draw, XYPair position, XYPair size) : base(name, position, size) {
            _text = text;
            _auto_size = false;
            
            this.custom_draw = custom_draw;
            this.enable_render_target = true;
        }

        public Button(string name, string text, XYPair position, XYPair size) : base(name, position, size) {
            _text = text;
            _auto_size = false;
        }

        public Button(string name, string text, XYPair position) : base(name, position, XYPair.Zero) {
            _text = text;
            this.position = position;
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont_pt(text);

            _auto_size = true;
        }

        public void change_text(string text) {
            _text = text;
            size = (margin * 2) - XYPair.UnitY + Drawing.measure_string_profont_pt(text);
        }


        internal override void update() {
            if (!visible) return;
            //successful click, released left mouse while over the button and clicking
            if (!clicking && was_clicking && mouse_over) {
                if (click_action != null) click_action();
            } 

            if (focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter)) {
                if (click_action != null) click_action();                
            }
            click_highlight = false;
            if ((focused && Swoop.input_handler.is_pressed(Microsoft.Xna.Framework.Input.Keys.Enter)) || (mouse_over && !mouse_down)) {
                click_highlight = true;
            }
        }


        internal override void draw() {
            if (!visible) return;

            if (custom_draw != null) {
                //Drawing.fill_rect(position + XYPair.One, position + size, click_highlight ? Swoop.get_color(this) : Swoop.UI_background_color);
                Drawing.image(draw_target, position.ToVector2(), size.ToVector2(), Color.White);
                //Drawing.rect(position + XYPair.One, position + size, Swoop.get_color(this), 1f);

            } else {
                Drawing.fill_rect_outline(position + XYPair.One, position + size, click_highlight ? Swoop.get_color(this) : Swoop.UI_background_color, Swoop.get_color(this), 1f);
                Drawing.text(_text, position + margin, click_highlight ? Swoop.UI_background_color : Swoop.get_color(this));
            }

        }

        internal override void draw_rt() {
            if (custom_draw != null) custom_draw();
        }
        internal override void added() { }

        internal override void handle_focused_input() { }
    }
}
