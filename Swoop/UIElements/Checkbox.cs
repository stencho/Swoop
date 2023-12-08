using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MGRawInputLib;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography;

namespace SwoopLib.UIElements {
    public class Checkbox : UIElement {
        string _text;
        public string text { get { return _text; } set { _text = value; update_size(); } }

        XYPair checkbox_size = XYPair.One * 12f;
        XYPair text_size = XYPair.Zero;
        
        const int margin = 4;

        public bool Checked { get; set; } = false;
        bool text_taller_than_box = false;

        public Action<Checkbox, bool> checked_changed;

        void update_size() {
            text_size = Drawing.measure_string_profont_xy(text);

            if (text_size.Y > checkbox_size.Y) {
                text_taller_than_box = true;
                size = text_size + (XYPair.UnitX * (checkbox_size.X + margin));
            } else {
                text_taller_than_box = false;
                size = checkbox_size + (XYPair.UnitX * (text_size.X + margin));
            }
        }

        public Checkbox(string name, string text, XYPair position) : base(name, position, XYPair.Zero) {
            this.text = text;
        }

        internal override void added() { }
        bool interacted = false;
        bool key_down = false;

        internal override void update() {
            if (!visible) return;

            if (!clicking && was_clicking && mouse_over) interacted = true;

            if (interacted) {
                Checked = !Checked;
                update_size();
                if (checked_changed != null) checked_changed(this, Checked);
            }
            interacted = false;
        }

        internal override void draw() {
            if (!visible) return;
            XYPair mid_left = position + (size.Y_only * 0.5f) ;
            Drawing.fill_rect_outline(
                mid_left - (checkbox_size.Y_only * 0.5f), mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size, 
                Swoop.UI_background_color, Swoop.get_color(this), 1f);

            if (mouse_over || key_down) {
                if (!Checked && (mouse_down || key_down)) {

                    Drawing.fill_rect_dither(
                        mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                        mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - (XYPair.One * 2),
                        Swoop.get_color(this), Swoop.UI_background_color);
                } else if (Checked && (mouse_down || key_down)) {

                    Drawing.fill_rect_dither(
                        mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                        mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - (XYPair.One * 2),
                        Swoop.get_color(this), Swoop.UI_background_color);
                } else {

                    if (Checked) {
                        Drawing.fill_rect(
                            mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                            mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - (XYPair.One * 2),
                            Swoop.get_color(this));
                    }

                    Drawing.rect(
                        mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                        mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - XYPair.One,
                        Swoop.get_color(this), 1f);
                }
            } else {
                if (Checked) {
                    Drawing.fill_rect(
                        mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                        mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - (XYPair.One * 2),
                        Swoop.get_color(this));
                }
            }

            Drawing.text(_text, mid_left + checkbox_size.X_only + (XYPair.UnitX * margin) - (text_size.Y_only * 0.5f), Swoop.get_color(this));
        }

        internal override void draw_rt() {}

        internal override void handle_focused_input() {
            key_down = (Swoop.input_handler.is_pressed(Keys.Enter) || Swoop.input_handler.is_pressed(Keys.Space)) 
                     || (Swoop.input_handler.just_released(Keys.Space) || Swoop.input_handler.just_released(Keys.Enter));

            if (Swoop.input_handler.just_released(Keys.Space) || Swoop.input_handler.just_released(Keys.Enter)) {
                interacted = true;
            }
        }
    }
}
