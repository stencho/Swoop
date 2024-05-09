using MGRawInputLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Runtime.InteropServices;

namespace SwoopLib.UIElements {
    public class TextBox : UIElement {
        TextInputManager text_manager;

        XYPair single_character_size;

        int view_offset = 0;
        int cursor_pixel_pos = 0;

        public TextBox(string name, string text, XYPair position, XYPair size) : base(name, position, size) {
            enable_render_target = true;

            text_manager = new TextInputManager(text);
            text_manager.multiline = false;

            single_character_size = Drawing.measure_string_profont_xy("a");

            this.size = new XYPair(size.X, single_character_size.Y + 6);
        }

        internal override void added() {}

        internal override void draw() {
            Drawing.fill_rect(position, position + size, Swoop.UI_background_color);
            Drawing.image(this.draw_target, position, size);
            Drawing.rect(position, position + size, Swoop.get_color(this), 1f);
        }

        internal override void update() {
            if (focused || !can_be_focused) {
                text_manager.update_input();
            }

            cursor_pixel_pos = Drawing.font_manager_profont.find_x_position_in_string(text_manager.current_line_text, text_manager.cursor);
            
            if (cursor_pixel_pos >= view_offset + size.X - single_character_size.X) {
                view_offset = cursor_pixel_pos - size.X + single_character_size.X;
            }

            if (cursor_pixel_pos < view_offset) {
                view_offset = cursor_pixel_pos;
            }
        }

        internal override void draw_rt() {
            if (text_manager.has_selection()) {
                var minmax = text_manager.get_actual_selection_min_max();

                Drawing.fill_rect_dither(
                    (XYPair.UnitX) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.current_line_text, minmax.min.X)) + (XYPair.UnitY * 2) - (XYPair.UnitX * view_offset),
                    (XYPair.UnitX * (Drawing.font_manager_profont.find_x_position_in_string(text_manager.current_line_text, minmax.max.X)+1)) + size.Y_only - (XYPair.UnitY * 3) - (XYPair.UnitX * view_offset),
                    Swoop.UI_disabled_color,
                    Swoop.UI_background_color);
            }

            Drawing.text(text_manager.lines[0].text, (XYPair.One * 1) + (XYPair.UnitY * 2) - (XYPair.UnitX * view_offset), Swoop.UI_color);

            Drawing.line(
                    (XYPair.UnitX*2) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.current_line_text, text_manager.cursor)) - (XYPair.UnitX * view_offset) + (XYPair.UnitY * 2),
                    (XYPair.UnitX*2) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.current_line_text, text_manager.cursor)) - (XYPair.UnitX * view_offset) + size.Y_only - (XYPair.UnitY * 3),

                    Swoop.get_color(this), 1f
                    );
        }

        internal override void handle_focused_input() {}

    }

    public class TextEditor : UIElement {
        string status_text = string.Empty;

        public bool word_wrap = true;

        XYPair stored_view_offset = XYPair.Zero;
        XYPair view_offset = XYPair.One * 10;
        XYPair view_size => size;
        XYPair view_margin => (XYPair.UnitX * single_character_size * 2) + Drawing.font_manager_profont.line_height_2d;
        
        int cursor_chunk = -1;

        int find_chunk(TextLine tl, int x) {
            var cursor_x = x;
            var line_width = Drawing.font_manager_profont.measure_string(tl.text).X;
            var max_width = size.X;

            float c = (float)line_width / max_width;

            if (c > 1) {
                string chunk = "";
                int chunk_start = 0;
                int chunk_end = 0;
                int chunk_width = 0;
                int chunk_index = 0;
                int move_back = 0;
                bool moved_back = false;


                for (; chunk_end <= tl.text.Length; ++chunk_end) {
                    moved_back = false;
                    move_back = 0;
                    chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);
                    chunk_width = 4 + Drawing.font_manager_profont.measure_string(chunk).X;

                    if (chunk_width > max_width) {
                        chunk_end--;
                        chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                        if (chunk.Contains(' ')) {
                            if (chunk.EndsWith(' ')) {
                                move_back++;
                                chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                            }

                            while (!chunk.EndsWith(' ') && chunk_end - move_back > chunk_start) {
                                move_back++;
                                chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                            }

                            moved_back = move_back > 0;
                            chunk_end -= move_back;
                        }
                    }

                    if (chunk_width > max_width || chunk_end >= tl.text.Length || moved_back) {
                        chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                        if ((x > chunk_start && x < chunk_end)
                            || (!moved_back && (x == chunk_start || x == chunk_end))
                            || (moved_back && x == chunk_start)
                            ) {
                            return chunk_index;
                        }

                        chunk_start = chunk_end;
                        chunk_index++;
                    }
                }


            } else {
                return 0;
            }

            return -1;
        }

        TextInputManager text_manager;
        XYPair single_character_size;

        enum cursor_display_mode { LINE, BOX } 
        cursor_display_mode cursor_mode = cursor_display_mode.LINE;

        public TextEditor(string name, string text, XYPair position, XYPair size) : base(name, position, size) {
            enable_render_target = true;
            text_manager = new TextInputManager(text);
            single_character_size = Drawing.measure_string_profont_xy("A");
            //can_be_focused = false;
            
            text_manager.external_input_handler = handle_input;
        }

        void scroll_view_to_cursor() {
            if (!check_if_cursor_in_view()) {
                var cursor_top = character_pos_to_pixel(text_manager.cursor_pos);
                //(XYPair.UnitY * (Drawing.font_manager_profont.line_height * text_manager.cursor_pos.Y)) +
                //(XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X));
                var cursor_bottom = cursor_top + (XYPair.UnitY * Drawing.font_manager_profont.line_height);

                //up
                if (cursor_bottom.Y < view_offset.Y + (view_margin.Y * 2)) {
                    view_offset.Y = cursor_bottom.Y - (view_margin.Y * 2);
                } else                
                //down
                if (cursor_top.Y > view_offset.Y + view_size.Y - (view_margin.Y * 2)) {
                    view_offset.Y = cursor_top.Y - view_size.Y + (view_margin.Y * 2);
                }


                if (word_wrap) {
                    view_offset.X = 0;
                } else {
                    //left
                    if (cursor_top.X < view_offset.X + view_margin.X) {
                        view_offset.X = cursor_top.X - view_margin.X;
                    } else
                    //right
                    if (cursor_top.X > view_offset.X + view_size.X - (view_margin.X * 2)) {
                        view_offset.X = cursor_top.X - view_size.X + (view_margin.X * 1);
                    }
                }

                //never go negative, keep top corner as the absolute top corner
                if (view_offset.Y < 0) view_offset.Y = 0;
                if (view_offset.X < 0) view_offset.X = 0;
            }
        }

        internal override void update() {
            if (focused || !can_be_focused) {
                text_manager.update_input();
            }
            var minmax = text_manager.get_actual_selection_min_max();

            var cursor_top = character_pos_to_pixel(text_manager.cursor_pos);
            string selection_text = 
                text_manager.has_selection() ? 
                $"[selection start {text_manager.selection_start.ToXString()} end {text_manager.selection_end.ToXString()} size {(minmax.max-minmax.min).ToXString()}]\n" : "\n";
            status_text = $"{selection_text} [pos {text_manager.cursor_pos.ToXString()} [{cursor_top.ToXString()}] vo {(view_offset + view_margin).ToXString()}->{(view_offset + view_size - view_margin).ToXString()} size {text_manager.longest_line_text_length}x{text_manager.line_count}]";

            //TODO MAKE THIS ONLY HAPPEN WHEN MOVING THE CURSOR MANUALLY            
            scroll_view_to_cursor();
        }

        void handle_input(InputHandler input_handler) {            
            
        }

        internal override void draw() { 
            Drawing.image(draw_target, position, size);
            Drawing.rect(
                position, position+size,
                Swoop.get_color(this), 1f);        
        }


        bool AABB(XYPair A_min, XYPair A_max, XYPair B_min, XYPair B_max) {
            return A_max.X > B_min.X && A_min.X < B_max.X && A_max.Y > B_min.Y && A_min.Y < B_max.Y;
        }

        bool check_if_cursor_in_view() {
            var cursor_px = character_pos_to_pixel(text_manager.cursor_pos);

            if (AABB(
                cursor_px, 
                cursor_px + Drawing.font_manager_profont.line_height_2d + (XYPair.UnitX), 
                view_offset + view_margin, 
                view_offset + view_size - view_margin)) {
                return true;
            }

            return false;
        }

        
        internal (int start, int end) find_lines_in_view() {
            int ctop = 0;
            (int start, int end) output = (-1,-1);

            if (!word_wrap) {
                //return (XYPair.UnitY * (Drawing.font_manager_profont.line_height * char_pos.Y)) +
                // (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[char_pos.Y].text, char_pos.X));
                return (0, 0);
            } else {
                for (int i = 0; i < text_manager.line_count; i++) {
                    if (i >= text_manager.line_count) break;
                    TextLine tl = text_manager.lines[i];

                    var line_width = Drawing.font_manager_profont.measure_string(tl.text).X;
                    var max_width = size.X;

                    float c = (float)line_width / max_width;

                    if (c > 1) {
                        string chunk = "";
                        int chunk_start = 0;
                        int chunk_end = 0;
                        int chunk_width = 0;
                        int chunk_index = 0;
                        int move_back = 0;
                        bool moved_back = false;

                        for (; chunk_end <= tl.text.Length; ++chunk_end) {
                            moved_back = false;
                            move_back = 0;

                            chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);
                            chunk_width = 4 + Drawing.font_manager_profont.measure_string(chunk).X;

                            if (chunk_width > max_width) {
                                chunk_end--;
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (chunk.Contains(' ')) {
                                    if (chunk.EndsWith(' ')) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    while (!chunk.EndsWith(' ') && chunk_end - move_back > chunk_start) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    moved_back = move_back > 0;
                                    chunk_end -= move_back;
                                }
                            }

                            if (chunk_width > max_width || chunk_end >= tl.text.Length || moved_back) {
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (output.start != -1 && output.end == -1 && ctop >= view_offset.Y + view_size.Y + view_margin.Y) {
                                    output.end = i;
                                    break;
                                }
                                if (ctop >= view_offset.Y - view_margin.Y && output.start == -1) {
                                    output.start = i;
                                }

                                ctop += Drawing.font_manager_profont.line_height;
                                chunk_start = chunk_end;
                                chunk_index++;
                            }
                        }

                        if (output.end > -1)
                            break;

                    } else {
                        if (output.start != -1 && output.end == -1 && ctop >= view_offset.Y + view_size.Y + view_margin.Y) {
                            output.end = i;
                            break;
                        }
                        if (ctop >= view_offset.Y - view_margin.Y && output.start == -1) {
                            output.start = i;
                        }


                        ctop += Drawing.font_manager_profont.line_height;
                    }
                }
            }

            if (output.end == -1) {
                output.end = text_manager.line_count;
            }

            return output;

        }
        

        internal XYPair character_pos_to_pixel(XYPair char_pos) {
            int ctop = 0;

            if (!word_wrap) {
                return (XYPair.UnitY * (Drawing.font_manager_profont.line_height * char_pos.Y)) +
                    (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[char_pos.Y].text, char_pos.X));

            } else {
                for (int i = 0; i < text_manager.line_count; i++) {
                    if (i >= text_manager.line_count) break;
                    bool on_line = i == char_pos.Y;

                    TextLine tl = text_manager.lines[i];

                    var line_width = Drawing.font_manager_profont.measure_string(tl.text).X;
                    var max_width = size.X;

                    float c = (float)line_width / max_width;

                    if (c > 1) {
                        string chunk = "";
                        int chunk_start = 0;
                        int chunk_end = 0;
                        int chunk_width = 0;
                        int chunk_index = 0;
                        int move_back = 0;
                        bool moved_back = false;


                        for (; chunk_end <= tl.text.Length; ++chunk_end) {
                            moved_back = false;
                            move_back = 0;

                            chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);
                            chunk_width = 4 + Drawing.font_manager_profont.measure_string(chunk).X;

                            if (chunk_width > max_width) {
                                chunk_end--;
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (chunk.Contains(' ')) {
                                    if (chunk.EndsWith(' ')) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    while (!chunk.EndsWith(' ') && chunk_end - move_back > chunk_start) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    moved_back = move_back > 0;
                                    chunk_end -= move_back;
                                }
                            }

                            if (chunk_width > max_width || chunk_end >= tl.text.Length || moved_back) {
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (on_line) {
                                    if ((char_pos.X > chunk_start && char_pos.X < chunk_end)
                                        || (!moved_back && (char_pos.X == chunk_start || char_pos.X == chunk_end))
                                        || (moved_back && char_pos.X == chunk_start)
                                        ) {
                                        return (XYPair.UnitY * ctop) +
                                            (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(chunk, char_pos.X - chunk_start));
                                    }
                                }

                                ctop += Drawing.font_manager_profont.line_height;
                                chunk_start = chunk_end;                                    
                                chunk_index++;
                            }
                        }


                    } else {
                        if (on_line) {
                            return (XYPair.UnitY * ctop) +
                                (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[char_pos.Y].text, char_pos.X));
                        }
                        ctop += Drawing.font_manager_profont.line_height;
                    }
                }
            }
            return XYPair.Zero;
        }

        internal override void draw_rt() {
            Drawing.fill_rect(XYPair.Zero, size, Swoop.UI_background_color);
            Drawing.fill_rect_dither(XYPair.Zero, 
                (size.Y_only + (XYPair.UnitY * 25)), 
                Swoop.UI_background_color, Swoop.UI_disabled_color);

            if (text_manager.has_selection() && text_manager.select_shape == TextInputManager.selection_shape.BLOCK) {
                var select_min_max = text_manager.get_actual_selection_min_max();

                Drawing.fill_rect_dither(
                    view_offset + (single_character_size * select_min_max.min) + XYPair.Down,
                    view_offset + (single_character_size * (select_min_max.max + XYPair.Down)) + (XYPair.One * 3),
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
                var min = Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[top_line].text, sel.min.X);
                var max = Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[top_line].text, sel.max.X);

                Drawing.fill_rect_dither(
                    -view_offset + (XYPair.UnitX * min) + (top_line * Drawing.font_manager_profont.line_height_2d) + (XYPair.One * 2),
                    -view_offset + (XYPair.UnitX * max) + (top_line * Drawing.font_manager_profont.line_height_2d) + single_character_size.Y_only + (XYPair.One * 2),
                    Swoop.UI_background_color, Swoop.UI_disabled_color);
            }

            
            int start_line = view_offset.Y / Drawing.font_manager_profont.line_height;
            int count = view_size.Y / Drawing.font_manager_profont.line_height;

            var lv = find_lines_in_view();

            start_line = lv.start;
            count = lv.end - lv.start;

            //count += 1;
            
            //ctop = start_line * Drawing.font_manager_profont.line_height;

            int drawing_lines = 0;

            //draw each TextLine
            for (int i = 0; i < text_manager.line_count; i++) {
                if (i >= text_manager.line_count) break;

                drawing_lines++;
                TextLine tl = text_manager.lines[i];
                bool cursor_on_line = i == text_manager.cursor_pos.Y;
                bool empty_line = (text_manager.lines[i].length == 0);


                //Draw main text
                if (!word_wrap) {
                    //draw selection background
                    if ((text_manager.has_selection() && text_manager.select_shape == TextInputManager.selection_shape.LINEAR) && !single_line_select) {

                        if (i == top_line) {
                            var min = Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[top_line].text, top_line_x);
                            var max = Drawing.font_manager_profont.measure_string(text_manager.lines[i].text).X;

                            empty_line = text_manager.lines[i].length == top_line_x;

                            Drawing.fill_rect_dither(
                                -view_offset + (top_line * Drawing.font_manager_profont.line_height_2d) + (XYPair.UnitX * min) + (XYPair.One * 2),
                                -view_offset + ((top_line + 1) * Drawing.font_manager_profont.line_height_2d) + (XYPair.UnitX * max) + (XYPair.One * 2),
                                Swoop.UI_background_color, Swoop.UI_disabled_color);

                        } else if (i > top_line && i < bottom_line) {
                            var max = Drawing.font_manager_profont.measure_string(text_manager.lines[i].text).X;

                            Drawing.fill_rect_dither(
                                -view_offset + (i * Drawing.font_manager_profont.line_height_2d) + (XYPair.One * 2),
                                -view_offset + ((i + 1) * Drawing.font_manager_profont.line_height_2d) + (XYPair.UnitX * max) + (XYPair.One * 2),

                                Swoop.UI_background_color, Swoop.UI_disabled_color);
                        } else if (i == bottom_line) {

                            var max = Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[bottom_line].text, bottom_line_x);

                            Drawing.fill_rect_dither(
                                -view_offset + (bottom_line * Drawing.font_manager_profont.line_height_2d) + (XYPair.One * 2),
                                -view_offset + ((bottom_line + 1) * Drawing.font_manager_profont.line_height_2d) + (XYPair.UnitX * max) + (XYPair.One * 2) + (XYPair.Down * 2),


                                Swoop.UI_background_color, Swoop.UI_disabled_color);

                        }
                    }

                    if (i >= lv.start && i <= lv.end) {
                        Drawing.text(tl.text, -view_offset + (XYPair.Down * ctop) + (XYPair.One * 2f), Swoop.UI_color);
                    }
                    //Draw cursor
                    if (cursor_on_line) {
                        Drawing.line(
                            -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X)) + (XYPair.One * 2),

                            -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X)) + (XYPair.One * 2) + single_character_size.Y_only,
                            Swoop.get_color(this), 1f);

                    }

                    //top of line counter
                    ctop += Drawing.font_manager_profont.line_height;

                } else {
                    var cursor_x = text_manager.cursor_pos.X;                    
                    var line_width = Drawing.font_manager_profont.measure_string(tl.text).X;
                    var max_width = size.X;

                    float c = (float)line_width / max_width;                    

                    if (c > 1) {
                        string chunk = "";
                        int chunk_start = 0;
                        int chunk_end = 0;
                        int chunk_width = 0;
                        int chunk_index = 0;
                        int move_back = 0;
                        bool moved_back = false;


                        for (; chunk_end <= tl.text.Length; ++chunk_end) {
                            moved_back = false;
                            move_back = 0;
                            chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);
                            chunk_width = 4 + Drawing.font_manager_profont.measure_string(chunk).X;

                            if (chunk_width > max_width) {
                                chunk_end--;
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (chunk.Contains(' ')) {
                                    if (chunk.EndsWith(' ')) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    while (!chunk.EndsWith(' ') && chunk_end - move_back > chunk_start) {
                                        move_back++;
                                        chunk = tl.text.Substring(chunk_start, chunk_end - move_back - chunk_start);
                                    }

                                    moved_back = move_back > 0;
                                    chunk_end -= move_back;
                                }
                            }
                            
                            if (chunk_width > max_width || chunk_end >= tl.text.Length || moved_back) {
                                chunk = tl.text.Substring(chunk_start, chunk_end - chunk_start);

                                if (cursor_on_line
                                    && ( (cursor_x > chunk_start && cursor_x < chunk_end) 
                                    || (!moved_back && (cursor_x == chunk_start || cursor_x == chunk_end))
                                    || (moved_back && cursor_x == chunk_start)
                                    )) {
                                    int selection_pos = Drawing.font_manager_profont.find_x_position_in_string(chunk, text_manager.cursor_pos.X - chunk_start);
                                    Drawing.line(
                                        -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * selection_pos) + (XYPair.UnitX * 1) + (XYPair.One * 1),
                                        -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * selection_pos) + (XYPair.UnitX * 1) + (XYPair.One * 1) + single_character_size.Y_only,
                                        Swoop.get_color(this), 1f);                                    
                                }

                                if (i >= lv.start && i <= lv.end) {
                                    Drawing.text(chunk, -view_offset + (XYPair.Down * ctop) + (XYPair.One * 1f), Swoop.UI_color);
                                }
                                ctop += Drawing.font_manager_profont.line_height;

                                chunk_start = chunk_end;
                                chunk_index++;
                            }
                        }


                    } else {

                        if (cursor_on_line) {
                            int selection_pos = Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X);
                            Drawing.line(
                                -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * selection_pos) + (XYPair.UnitX * 1) + (XYPair.One * 1),
                                -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * selection_pos) + (XYPair.UnitX * 1) + (XYPair.One * 1) + single_character_size.Y_only,
                                Swoop.get_color(this), 1f);
                        }

                        if (i >= lv.start && i <= lv.end) {
                            Drawing.text(tl.text, -view_offset + (XYPair.Down * ctop) + (XYPair.One * 1f), Swoop.UI_color);
                        }

                        ctop += Drawing.font_manager_profont.line_height;
                    }
                    /*
                    if (cursor_on_line) {
                        Drawing.line(
                            -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X)) + (XYPair.One * 2),

                            -view_offset + (XYPair.UnitY * ctop) + (XYPair.UnitX * Drawing.font_manager_profont.find_x_position_in_string(text_manager.lines[text_manager.cursor_pos.Y].text, text_manager.cursor_pos.X)) + (XYPair.One * 2) + single_character_size.Y_only,
                            Swoop.get_color(this), 1f);
                    }
                    
                    var l = tl.text.Length * single_character_size.X;
                    var max_width = size.X;

                    */
                    if (line_width > max_width) {

                        //Drawing.text(tl.text, -view_offset + (XYPair.Down * ctop) + (XYPair.One * 2f), Swoop.get_color(this));
                    }
                }

                var line_min = new XYPair(0, ctop) + (XYPair.One * 2);
                var line_max = line_min + tl.size_px;

               // ctop += Drawing.font_manager_profont.line_height;


                //debug
                //Drawing.rect(-view_offset + line_min, -view_offset + line_max, i == text_manager.longest_line_index ? Color.Green : Color.Red, 1f);

            }

            Drawing.fill_circle(-view_offset + character_pos_to_pixel(text_manager.cursor_pos), 4f, Color.Red);

            //debug
            /*            
            Drawing.rect(
                XYPair.UnitX * line_count_width + view_margin,
                XYPair.UnitX * line_count_width + view_size - view_margin,
                check_if_cursor_in_view() ? Color.HotPink : Color.Red, 1f);

            Drawing.text_shadow(
                $"{view_offset.ToXString()}", 
                (XYPair.UnitX * line_count_width) + single_character_size + (single_character_size.X_only*2), Color.White, Color.Black);
            */
            //status text
            status_text += $" {drawing_lines}";

            var ds = Drawing.measure_string_profont_xy(status_text);
            Drawing.text(status_text, size - (ds + (XYPair.One * 3)), Swoop.UI_disabled_color);
            
        }

        
        internal override void handle_focused_input() {
        }

        internal override void added() { }
    }
}
