using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class ProgressBar : UIElement {

        bool _clickable = false;
        public bool clickable { get { return _clickable; } set { can_be_focused = value; _clickable = value; } }

        public bool vertical = false;
        public bool invert = false;

        bool internal_text_on_left = true;

        public string text = "";


        public ProgressBar(string name, float value, XYPair position, XYPair size) : base(name, position, size) {
            this._value = value;
            min = 0; max = 1;
            clickable = false;
        }

        public ProgressBar(string name, float min, float max, float value, XYPair position, XYPair size) : base(name, position, size) {
            this.min = min; this.max = max;
            this._value = value;
            clickable = false;
        }

        float min, max, _value;

        float draw_value() {
            if (value < min) return 0f; if (value > max) return 1f;
            return (_value - min) / (max - min);
        }

        public float value { get { return this._value; } set { this._value = value; } }
        
        internal override void added() {}

        internal override void draw() {
            if (!visible) return;

            if (!vertical) {
                if (!invert) {
                    Drawing.fill_rect_dither(
                        position, 
                        (position + size.Y_only) + (size.X_only * draw_value()), 
                        Swoop.get_color(this), Swoop.UI_background_color);

                    if (!string.IsNullOrWhiteSpace(text))
                        Drawing.text(text, (position + (XYPair.Up * 12f)).ToVector2(), Swoop.UI_background_color);

                } else {
                    Drawing.fill_rect(
                        position + (size.X_only * (1.0f-draw_value())), 
                       (position + (size.X_only * (1.0f - draw_value())) + size.Y_only) + (size.X_only * draw_value()), 
                       Swoop.get_color(this));
                }


                if (!string.IsNullOrEmpty(text))
                    Drawing.text(text, (position + (XYPair.Up * 12f)).ToVector2(), Swoop.get_color(this));

                //vertical
            } else {
                if (invert) {
                    Drawing.fill_rect(
                        position + (size.Y_only - (size.Y_only * draw_value())),
                        position + size.X_only + (size.Y_only - (size.Y_only * draw_value())) + (size.Y_only * (draw_value())), Swoop.get_color(this));
                } else {
                    Drawing.fill_rect(
                        position,
                        (position + (size.Y_only * (draw_value())) + size.X_only), Swoop.get_color(this));
                }

                if (!string.IsNullOrEmpty(text))
                    Drawing.text_vertical(text, (position + size.X_only + (XYPair.Right*12f)).ToVector2(), Swoop.get_color(this));
            }
             
            Drawing.rect(position, position + size, Swoop.get_color(this), 1f); 
        }




        internal override void draw_rt() {}

        internal override void update() {
            if (!visible) return;

        }
    }
}
