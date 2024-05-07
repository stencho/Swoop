using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.UIElements {
    public class OptionSlider : UIElement {
        string title = "";
        string[] options;
        int selected_option = 2;

        public enum TitlePosition { LEFT, TOP}
        public TitlePosition title_position = TitlePosition.TOP;


        XYPair title_size;
        int bar_height = 16;
        int bar_width => size.X - ((margin + (bar_height / 2)) * 2) - margin;
        int bar_width_no_margin => size.X - (((bar_height / 2)) * 2);

        int margin = 2;
        int tallest_option = int.MinValue;

        XYPair bar_left_no_margin => position + (XYPair.UnitY * (title_size.Y + (bar_height / 2))) + (XYPair.UnitX * ((bar_height / 2f)));

        XYPair bar_left => position + (XYPair.UnitY * (margin + title_size.Y + (bar_height / 2))) + (XYPair.UnitX * (margin + (bar_height / 2f)));
        XYPair bar_right => bar_left + (XYPair.UnitX * bar_width);

        public OptionSlider(string name, XYPair position, XYPair size, string title, params string[] options) : base(name, position, size) {
            this.title = title;
            this.options = options;

            title_size = Drawing.measure_string_profont_xy(title);

            foreach(string option in options) {
                var m = Drawing.measure_string_profont_xy(option);

                if (m.Y > tallest_option) {
                    tallest_option = m.Y;
                }
            }

            this.size = this.size.X_only + (XYPair.UnitY * (margin + title_size.Y + (bar_height*2) + tallest_option + margin));
        }

        internal override void added() {}

        internal override void draw() {
            //debug hitbox
            Drawing.fill_rect_outline(position, position + size, Swoop.UI_background_color, Swoop.get_color(this), 1f);

            //title
            Drawing.text(title, position + (XYPair.UnitY * margin) + (XYPair.UnitX * ((size.X / 2) - (title_size.X / 2))), Swoop.get_color(this));

            //draw lines coming down from slider
            int x_pos = 0; int opt = 0;
            int item_width = bar_width / (options.Length - 1);

            //draw little rounded lines poking down at each option
            foreach (string option in options) {
                Drawing.line_rounded_ends(
                    bar_left + (XYPair.UnitX * (x_pos + 1)),
                    bar_left + (XYPair.UnitX * (x_pos + 1)) + (XYPair.UnitY * (bar_height / 2)),
                    selected_option == opt ? Swoop.UI_highlight_color : Swoop.UI_color, 6f);

                x_pos += item_width;
                opt++;
            }

            //the bar outline
            Drawing.line_rounded_ends(
                bar_left,
                bar_right, 
                Swoop.UI_color, 
                bar_height / 2f);
            //the bar infill
            Drawing.line_rounded_ends(
                bar_left,
                position + (XYPair.UnitY * (margin + title_size.Y + (bar_height / 2))) - (XYPair.UnitX * ((margin*2) + (bar_height / 2f))) + size.X_only,
                Swoop.UI_background_color,
                (bar_height / 2f) - 2f);



            //selection circle background
            Drawing.fill_circle(
                bar_left + (XYPair.UnitX * ((item_width * selected_option) + 1)),
                (bar_height / 2f) - 2f,
                Swoop.UI_background_color);
            
            //selection circle
            Drawing.circle(
                bar_left + (XYPair.UnitX * ((item_width * selected_option) + 1)),
                (bar_height / 2f) - 2f,
                2f, Swoop.UI_highlight_color);
            
            //inner circle, should move when mouse is clicked
            Drawing.fill_circle(
                bar_left + (XYPair.UnitX * ((item_width * (selected_option)) + 1)),
                (bar_height / 2f) - 5f,
                Swoop.UI_highlight_color);
            
            //draw option labels
            x_pos = 0; opt = 0;
            foreach (string option in options) {
                Drawing.text_centered(option,
                    bar_left + (XYPair.UnitX * (x_pos)) + (XYPair.UnitY * (bar_height)),
                    selected_option == opt ? Swoop.UI_highlight_color : Swoop.UI_color);

                x_pos += item_width;
                opt++;
            }

            x_pos = 0; opt = 0;

            //debug boxes to help me brain
            foreach (string option in options) {
                break;
                Drawing.rect(
                    (position.Y_only + (XYPair.UnitY)) + bar_left.X_only + (XYPair.UnitX * x_pos) - (XYPair.UnitX * item_width / 2), 
                    (position.Y_only - (XYPair.UnitY)) + bar_left.X_only + (XYPair.UnitX * x_pos) + (XYPair.UnitX * item_width / 2)
                    + size.Y_only, Swoop.UI_disabled_color, 1f);

                x_pos += item_width;
                opt++;
            }
        }

        internal override void draw_rt() {}

        internal override void handle_focused_input() {}

        internal override void update() {}
    }
}
