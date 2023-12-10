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

        public Action<float> value_changed;

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

            var clamped_mouse_relative = XYPair.clamp(mouse_relative, XYPair.Zero, size);
            float relative_mouse_value = 0f;

            if (!vertical) relative_mouse_value = clamped_mouse_relative.ToVector2().X / size.ToVector2().X;
            else relative_mouse_value = clamped_mouse_relative.ToVector2().Y / size.ToVector2().Y;

            if (invert) relative_mouse_value = 1f - relative_mouse_value;

            relative_mouse_value = float.Clamp(relative_mouse_value, 0f, 1f);

            XYPair size_X_scaled = (size.X_only * draw_value());
            XYPair size_Y_scaled = (size.Y_only * draw_value());

            //Draw bar background
            Drawing.fill_rect(
                position,
                position + size.Y_only,
                Swoop.UI_background_color);

            //Drawing a horizontal bar
            if (!vertical) {
                //Draw main bar
                if (!invert) {
                    Drawing.fill_rect(
                        position,
                        (position + size.Y_only) + size_X_scaled,
                        Swoop.get_color(this));

                } else {
                    Drawing.fill_rect(
                        position + (size.X_only * (1.0f - draw_value())),
                        (position + (size.X_only * (1.0f - draw_value())) + size.Y_only) + size_X_scaled,
                        Swoop.get_color(this));
                }

                Drawing.end();

                //Draw dithered overlay while clicking
                if (clicking) {
                    if (!invert) {
                        //determine which side of the end of the bar the mouse cursor is on and
                        //draw dithered rectangle from the end of the bar to where the cursor is
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                (position) + size_X_scaled,
                                position + size.Y_only + clamped_mouse_relative.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);

                            //if we're clicking past the end of the bar, cap off the dithering
                            Drawing.line(
                                position + clamped_mouse_relative.X_only,
                                position + size.Y_only + clamped_mouse_relative.X_only,
                                Swoop.get_color(this), 1f);
                        } else {
                            Drawing.fill_rect_dither(
                                position  + clamped_mouse_relative.X_only,
                                position + size.Y_only + size_X_scaled,
                                Swoop.get_color(this), Swoop.UI_background_color);

                            //if we're clicking on a point on the bar, draw a little end cap on the 
                            //bar, similar to the above
                            Drawing.line(
                                position + size_X_scaled,
                                position + size.Y_only + size_X_scaled,
                                Swoop.get_color(this), 1f);
                        }

                    //Exactly the same as above, but inverted
                    } else {
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                position  + clamped_mouse_relative.X_only,
                                (position) + size.Y_only + (size.X_only * (1f-draw_value())),
                                Swoop.get_color(this), Swoop.UI_background_color);

                            Drawing.line(
                                position + clamped_mouse_relative.X_only,
                                position + size.Y_only + clamped_mouse_relative.X_only,
                                Swoop.get_color(this), 1f);
                        } else {
                            Drawing.fill_rect_dither(
                                position + (size.X_only * (1f - draw_value())),
                                position + size.Y_only + clamped_mouse_relative.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);

                            Drawing.line(
                                position + (size.X_only * (1f - draw_value())),
                                position + size.Y_only + (size.X_only * (1f - draw_value())),
                                Swoop.get_color(this), 1f);
                        }
                    }                    
                } 

                //Draw header string
                if (!string.IsNullOrEmpty(text))
                    Drawing.text(text, (position + (XYPair.Up * 12f)).ToVector2(), Swoop.get_color(this));

            //Vertical bar
            } else {
                if (!invert) {
                    Drawing.fill_rect(
                        position,
                        (position + (size.Y_only * (draw_value())) + size.X_only), Swoop.get_color(this));
                } else {
                    Drawing.fill_rect(
                        position + (size.Y_only - size_Y_scaled),
                        position + size.X_only + (size.Y_only - size_Y_scaled) + (size.Y_only * (draw_value())), Swoop.get_color(this));
                }

                Drawing.end();

                if (clicking) {
                    if (!invert) {
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                position + size_Y_scaled,
                                position + clamped_mouse_relative.Y_only + size.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);

                            Drawing.line(
                                position + clamped_mouse_relative.Y_only,
                                position + size.X_only + clamped_mouse_relative.Y_only,
                                Swoop.get_color(this), 1f);
                        } else {
                            Drawing.fill_rect_dither(
                                position + clamped_mouse_relative.Y_only,
                                position + size.X_only + size_Y_scaled,
                                Swoop.get_color(this), Swoop.UI_background_color);


                            Drawing.line(
                                position + size_Y_scaled,
                                position + size.X_only + size_Y_scaled,
                                Swoop.get_color(this), 1f);
                        }

                    } else {
                        if (relative_mouse_value > draw_value()) {
                            Drawing.fill_rect_dither(
                                position + clamped_mouse_relative.Y_only,
                                position + size.X_only + (size.Y_only * (1f-draw_value())),
                                Swoop.get_color(this), Swoop.UI_background_color);

                            Drawing.line(
                                position + clamped_mouse_relative.Y_only,
                                position + size.X_only + clamped_mouse_relative.Y_only,
                                Swoop.get_color(this), 1f);
                        } else {
                            Drawing.fill_rect_dither(
                                position + (size.Y_only * (1f - draw_value())),
                                position + clamped_mouse_relative.Y_only + size.X_only,
                                Swoop.get_color(this), Swoop.UI_background_color);

                            Drawing.line(
                                position + size_Y_scaled,
                                position + size.X_only + size_Y_scaled,
                                Swoop.get_color(this), 1f);
                        }
                    }
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

            var clamped_mouse_relative = XYPair.clamp(mouse_relative, XYPair.Zero, size);
            float relative_mouse_value = 0f;
            
            if (!vertical) relative_mouse_value = clamped_mouse_relative.ToVector2().X / size.ToVector2().X;
            else relative_mouse_value = clamped_mouse_relative.ToVector2().Y / size.ToVector2().Y;

            relative_mouse_value = float.Clamp(relative_mouse_value, 0f, 1f);

            if (clickable && mouse_over && mouse_down && !mouse_was_down) clicking = true;
            if (!mouse_down) clicking = false;

            bool mouse_between_sides = false;
            if (!vertical) mouse_between_sides = (mouse_relative.Y >= 0 && mouse_relative.Y <= size.Y);
            else mouse_between_sides = (mouse_relative.X >= 0 && mouse_relative.X <= size.X);

            //mouse just released while mouse was over or off the end of the bar, so apply new value
            if (was_clicking && !clicking && (mouse_over || ((relative_mouse_value == 0f || relative_mouse_value == 1f) && mouse_between_sides) )) {
                if (!invert)
                    _value = relative_mouse_value;
                else
                    _value = 1f - relative_mouse_value;

                if (value_changed != null)
                    value_changed(_value);
            }

            was_clicking = clicking;
        }

        internal override void handle_focused_input() { }
    }
}
