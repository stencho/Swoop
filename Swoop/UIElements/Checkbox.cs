using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MGRawInputLib;

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

        internal override void update() {
            bool interacted = false;

            if (!clicking && was_clicking && mouse_over) interacted = true;
            if (is_focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                interacted = true;

            if (interacted) {
                Checked = !Checked;
                update_size();
                if (checked_changed != null) checked_changed(this, Checked);
            }
        }

        internal override void draw() {
            XYPair mid_left = position + (size.Y_only * 0.5f) ;
            Drawing.fill_rect_outline(
                mid_left - (checkbox_size.Y_only * 0.5f), mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size, 
                Swoop.UI_background_color, Swoop.get_color(this), 1f);

            if (mouse_over) {
                Drawing.rect(
                    mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One, 
                    mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - XYPair.One,
                    Swoop.get_color(this), 1f);
            }

            if (Checked) {
                Drawing.fill_rect(
                    mid_left - (checkbox_size.Y_only * 0.5f) + XYPair.One,
                    mid_left - (checkbox_size.Y_only * 0.5f) + checkbox_size - (XYPair.One * 2),
                    Swoop.get_color(this));                    
            }

            Drawing.text(_text, mid_left + checkbox_size.X_only + (XYPair.UnitX * margin) - (text_size.Y_only * 0.5f), Swoop.get_color(this));
        }

        internal override void draw_rt() {}

    }
}
