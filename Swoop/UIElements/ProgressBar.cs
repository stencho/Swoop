using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
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

            float relative_mouse_value = mouse_relative.ToVector2().X / size.ToVector2().X;
            
            float.Clamp(relative_mouse_value, 0f, 1f);
            if (invert) relative_mouse_value = 1f - relative_mouse_value;

            if (!vertical) {

                if (!invert) {
                    Drawing.fill_rect(
                        position,
                        (position + size.Y_only) + (size.X_only * draw_value()),
                        Swoop.get_color(this));

                    if (!string.IsNullOrWhiteSpace(text))
                        Drawing.text(text, (position + (XYPair.Up * 12f)).ToVector2(), Swoop.UI_background_color);

                } else {
                    Drawing.fill_rect(
                        position + (size.X_only * (1.0f - draw_value())),
                        (position + (size.X_only * (1.0f - draw_value())) + size.Y_only) + (size.X_only * draw_value()),
                        Swoop.get_color(this));
                }

                Drawing.end();



                if (clickable && mouse_down && mouse_over) {
                    if (!invert) {
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                (position) + (size.X_only * draw_value()),
                                position + size.Y_only + mouse_relative.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);
                        } else {
                            Drawing.fill_rect_dither(
                                position  + mouse_relative.X_only,
                                position + size.Y_only + (size.X_only * draw_value()),
                                Swoop.get_color(this), Swoop.UI_background_color);
                        }

                        Drawing.line(
                            position + mouse_relative.X_only,
                            position + size.Y_only + mouse_relative.X_only,
                            Swoop.get_color(this), 1f);
                    } else {
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                position  + mouse_relative.X_only,
                                (position) + size.Y_only + (size.X_only * (1f-draw_value())),
                                Swoop.get_color(this), Swoop.UI_background_color);
                        } else {
                            Drawing.fill_rect_dither(
                                position + (size.X_only * (1f - draw_value())),
                                position + size.Y_only + mouse_relative.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);
                        }

                        Drawing.line(
                            position + mouse_relative.X_only,
                            position + size.Y_only + mouse_relative.X_only,
                            Swoop.get_color(this), 1f);
                    }
                    
                } else {
                }

                if (clickable) {
                    //Drawing.text_inverting("test", position.ToVector2(), position.ToVector2() + Drawing.measure_string_profont("test"), Swoop.get_color(this), Swoop.UI_background_color);
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


        bool clicking = false;
        bool was_clicking = false;

        internal override void update() {
            if (!visible) return;

            float relative_mouse_value = mouse_relative.ToVector2().X / size.ToVector2().X;
            float.Clamp(relative_mouse_value, 0f, 1f);

            if (clickable && mouse_down) clicking = true;
            if (!mouse_down) clicking = false;

            //mouse just released
            if (was_clicking && !clicking) {
                if (mouse_over) {
                    if (!invert)
                        _value = relative_mouse_value;
                    else
                        _value = 1f - relative_mouse_value;
                }
            }

            was_clicking = clicking;
        }

        internal override void handle_focused_input() { }
    }
}
