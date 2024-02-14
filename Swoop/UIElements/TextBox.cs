using MGRawInputLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
namespace SwoopLib.UIElements {
    internal class TextBox {
        TextInputManager text_manager;
    }

    public class TextEditor : UIElement {
        string status_text = string.Empty;

        public bool line_count = true;

        bool show_menu = false;
        const int menu_height = 17;

        XYPair line_count_width => line_count ? Drawing.measure_string_profont_xy(text_manager.line_count.ToString() + " ").X_only : XYPair.Zero;

        TextInputManager text_manager;
        XYPair single_character_size;

        enum cursor_display_mode { LINE, BOX } 
        cursor_display_mode cursor_mode = cursor_display_mode.LINE;

        public TextEditor(string name, XYPair position, XYPair size) : base(name, position, size) {
            enable_render_target = true;
            text_manager = new TextInputManager("maximum fart dose\n\n\nand a big one in the back");
            single_character_size = Drawing.measure_string_profont_xy("a");
            can_be_focused = false;

            text_manager.external_input_handler = handle_input;
        }

        internal override void update() {
            text_manager.update_input();
            var minmax = text_manager.get_actual_selection_min_max();

            string selection_text = 
                text_manager.has_selection() ? 
                $"[selection start {text_manager.selection_start.ToXString()} end {text_manager.selection_end.ToXString()} size {(minmax.max-minmax.min).ToXString()}]" : "";
            status_text = $"{selection_text} [pos {text_manager.cursor_pos.ToXString()}]";
        }

        void handle_input(InputHandler input_handler) {            
            if (input_handler.just_pressed(Keys.L) && input_handler.ctrl) {
                line_count = !line_count;
                input_handler.handle_key(Keys.L);
            }
            
        }

        internal override void draw() { Drawing.image(draw_target, position, size); }
        internal override void draw_rt() {
            Drawing.fill_rect(XYPair.Zero, size, Swoop.UI_background_color);
            Drawing.fill_rect_dither(XYPair.Zero, (size.Y_only + (XYPair.UnitY * 25)) + (XYPair.UnitX * line_count_width - 3), Swoop.UI_background_color, Swoop.UI_disabled_color);

            if (text_manager.has_selection() && text_manager.select_shape == TextInputManager.selection_shape.BLOCK) {
                var select_min_max = text_manager.get_actual_selection_min_max();

                Drawing.fill_rect_dither(
                    (single_character_size * select_min_max.min) + line_count_width + XYPair.Down, 
                    (single_character_size * (select_min_max.max + XYPair.Down)) + line_count_width + (XYPair.One * 3), 
                    Swoop.UI_background_color, Swoop.UI_disabled_color);
            }

            int ctop = 0;

            //uh oh
            var sel = text_manager.get_actual_selection_min_max();

            var top_line = sel.min.Y;
            var bottom_line = sel.max.Y;

            var selected_lines = bottom_line - top_line;

            var top_line_x = (text_manager.selection_start.Y < text_manager.selection_end.Y) ? text_manager.selection_start.X : text_manager.selection_end.X;
            var bottom_line_x = (text_manager.selection_start.Y > text_manager.selection_end.Y) ? text_manager.selection_start.X : text_manager.selection_end.X;

            var single_line_select = top_line == bottom_line;

            if ((text_manager.has_selection() && text_manager.select_shape == TextInputManager.selection_shape.LINEAR) && single_line_select) {
                Drawing.fill_rect_dither(
                    (single_character_size * sel.min) + line_count_width + XYPair.Down,
                    (single_character_size * (sel.max + XYPair.Down)) + line_count_width + (XYPair.One * 3),
                    Swoop.UI_background_color, Swoop.UI_disabled_color);
            }

            //draw each TextLine
            for (int i = 0; i < text_manager.lines.Count; i++) {
                TextLine tl = text_manager.lines[i];
                bool cursor_on_line = i == text_manager.cursor_pos.Y;
                bool empty_line = (text_manager.lines[i].length == 0);

                if ((text_manager.has_selection() && text_manager.select_shape == TextInputManager.selection_shape.LINEAR) && !single_line_select) {
                    if (i == top_line) {
                        empty_line = text_manager.lines[i].length == top_line_x;
                        Drawing.fill_rect_dither(
                            (single_character_size * ((XYPair.UnitY * top_line) + (XYPair.UnitX * top_line_x))) 
                            + line_count_width + (XYPair.Down * 2),

                            (single_character_size * ((XYPair.UnitY * top_line) + (XYPair.UnitX * (top_line_x + (text_manager.lines[top_line].text.Length - top_line_x))))) 
                            + ((single_character_size / 2) * (XYPair.UnitX * (empty_line ? 1:0)))
                            + (XYPair.Down * 3) + line_count_width + single_character_size.Y_only,

                            Swoop.UI_background_color, Swoop.UI_disabled_color);
                    } else if (i > top_line && i < bottom_line) {
                        Drawing.fill_rect_dither(
                            (single_character_size * (XYPair.UnitY * i)) 
                            + line_count_width + (XYPair.Down * 2),

                            (single_character_size * ((XYPair.UnitX * text_manager.lines[i].text.Length) + (XYPair.UnitY * i)))
                            + ((single_character_size / 2) * (XYPair.UnitX * (empty_line ? 1 : 0)))
                            + (XYPair.Down * 3) + line_count_width + single_character_size.Y_only,

                            Swoop.UI_background_color, Swoop.UI_disabled_color);
                    } else if (i == bottom_line) {
                        Drawing.fill_rect_dither(
                            (single_character_size * (XYPair.UnitY * bottom_line)) 
                            + line_count_width + (XYPair.Down * 2),

                            (single_character_size * ((XYPair.UnitY * bottom_line) + (XYPair.UnitX * bottom_line_x)))
                            + ((single_character_size / 2) * (XYPair.UnitX * (empty_line ? 1 : 0)))
                            + (XYPair.Down * 3) + line_count_width + single_character_size.Y_only,

                            Swoop.UI_background_color, Swoop.UI_disabled_color);
                    }
                }

                if (cursor_mode == cursor_display_mode.BOX) {
                    Drawing.rect(
                        (single_character_size * text_manager.cursor_pos) + line_count_width + (XYPair.One + XYPair.Down),
                        (single_character_size * text_manager.cursor_pos) + line_count_width + (XYPair.One * 3) + single_character_size,
                        Swoop.get_color(this), 1f);                          
                                                                             
                } else if (cursor_mode == cursor_display_mode.LINE) {        
                    Drawing.line(                                            
                        (single_character_size * text_manager.cursor_pos) + line_count_width + (XYPair.One * 2),
                        (single_character_size * text_manager.cursor_pos) + line_count_width + (XYPair.One * 2) + single_character_size.Y_only,
                        Swoop.get_color(this), 1f);
                }


                if (line_count) {
                    
                    Drawing.text(i.ToString(), (XYPair.Down * ctop) + (XYPair.One * 2f), Swoop.UI_highlight_color);
                }

                Drawing.text(tl.text, (XYPair.Down * ctop) + (XYPair.One * 2f) + line_count_width, Swoop.get_color(this));                
                    
                ctop += single_character_size.Y; //tl.size_px.Y;
            }

            if (show_menu) {
                Drawing.fill_rect(XYPair.Zero, Swoop.resolution.X_only + (XYPair.UnitY * menu_height), Swoop.UI_background_color);
                Drawing.line(XYPair.UnitY * menu_height, Swoop.resolution.X_only + (XYPair.UnitY * menu_height), Swoop.UI_color, 1f);
            }

            var ds = Drawing.measure_string_profont_xy(status_text);
            Drawing.text(status_text, size - (ds + (XYPair.One * 3)), Swoop.UI_disabled_color);
        }

        internal override void handle_focused_input() {
        }

        internal override void added() { }
    }
}
