using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Swoop.UIElements {
    internal class Label : UIElement {
        public enum anchor_point : byte {
            TOP = 1 << 0,
            BOTTOM = 1 << 1,
            LEFT = 1 << 2,
            RIGHT = 1 << 3,
            TOP_LEFT = TOP | LEFT,
            TOP_RIGHT = TOP | RIGHT,
            BOTTOM_LEFT = BOTTOM | LEFT,
            BOTTOM_RIGHT = BOTTOM | RIGHT,
            CENTER = 0
        };
        public anchor_point draw_anchor = anchor_point.TOP_LEFT;

        string text = "";

        public bool draw_outline = false;
        
        Color text_color = Color.White;

        bool _auto_size = true;
        bool auto_size { get { return _auto_size; } set {
                enable_render_target = !value;
                if (value) line_wrap = false;
                _auto_size = value;  
            } }


        bool line_wrap = true;

        public Label(string text, Vector2 position, Vector2 size) 
            : base(position, size) {
            auto_size = false;
            this.text = text;
            this.position = position;
            this.size = size;
        }

        public Label(string text, Vector2 position, Vector2 size, anchor_point anchor)
            : base(position, size) {
            auto_size = false;
            this.text = text;
            this.position = position;
            this.size = size;
            this.draw_anchor = anchor;
        }

        public Label(string text, Vector2 position) 
            : base(position, Vector2.Zero) {
            auto_size = true;
            this.text = text;
            this.position = position;
            this.size = Drawing.measure_string_profont(text);
        }

        public Label(string text, Vector2 position, anchor_point anchor)
            : base(position, Vector2.Zero) {
            auto_size = true;
            this.text = text;
            this.position = position;
            this.size = Drawing.measure_string_profont(text);
            this.draw_anchor = anchor;
        }

        public override void update() {

        }

        public enum alignment { LEFT, CENTER, RIGHT }
        public alignment text_justification = alignment.LEFT;

        private void draw_internal(Vector2 offset, alignment tj) {
            StringReader sr = new StringReader(text);
            int line_num = 0;
            float line_height = Drawing.measure_string_profont("A").Y;
            

            while (sr.Peek() > -1) {
                string line = sr.ReadLine();
                float line_width = Drawing.measure_string_profont(line).X;

                Vector2 line_offset = Vector2.Zero;

                if (tj == alignment.CENTER)
                    line_offset = new Vector2((size.X / 2) - (line_width / 2), 0);
                else if (tj == alignment.RIGHT)
                    line_offset = new Vector2(size.X - line_width , 0);

                if (line_wrap) {
                    if (line_width > size.X) {
                        float build_size = 0;
                        
                        string working_string = "";
                        int last_space = 0;
                        int next_space = 0;

                        still_too_long:

                        if (!line.Contains(' ')) {
                            Drawing.text(line, offset + line_offset + (line_height * line_num * Vector2.UnitY), text_color);
                            line_num++;
                            continue;
                        }

                        while (build_size < this.size.X && line.IndexOf(' ', last_space + 1) < line.LastIndexOf(' ')) {
                            next_space = line.IndexOf(' ', last_space+1);

                            if (Drawing.measure_string_profont(working_string + line.Substring(last_space, next_space - last_space)).X < this.size.X) {
                                working_string += line.Substring(last_space, next_space - last_space);
                                build_size = Drawing.measure_string_profont(working_string).X;
                            } else break;

                            last_space = next_space;
                        }

                        Drawing.text(working_string, offset + line_offset + (line_height * line_num * Vector2.UnitY), text_color);
                        line_num++;

                        line = line.Remove(0, working_string.Length).TrimStart();
                        line_width = Drawing.measure_string_profont(line).X;

                        if (tj == alignment.CENTER)
                            line_offset = new Vector2((size.X / 2) - (line_width / 2), 0);
                        else if (tj == alignment.RIGHT)
                            line_offset = new Vector2(size.X - line_width, 0);

                        if (line_width > size.X) {
                            working_string = "";
                            last_space = 0;
                            build_size = 0;

                            goto still_too_long;
                        } else {
                            Drawing.text(line, offset + line_offset + (line_height * line_num * Vector2.UnitY), text_color);
                            line_num++;
                        }

                    } else {
                        Drawing.text(line, offset + line_offset + (line_height * line_num * Vector2.UnitY), text_color);
                    }

                } else {
                    Drawing.text(line, offset + line_offset + (line_height * line_num * Vector2.UnitY), text_color);
                }

                line_num++;
            }

            if (draw_outline)
                Drawing.rect(offset - (Vector2.UnitX * 2), size.X + 5, size.Y, Color.White, 1f);
        }

        Vector2 draw_offset() {
            Vector2 o = Vector2.Zero;
            switch (draw_anchor) {
                case anchor_point.TOP:
                    o.X -= size.X / 2;
                    break;
                case anchor_point.BOTTOM:
                    o.X -= size.X / 2;
                    o.Y -= size.Y;
                    break;
                case anchor_point.LEFT:
                    o.Y -= size.Y / 2;
                    break;
                case anchor_point.RIGHT:
                    o.X -= size.X;
                    o.Y -= size.Y / 2;
                    break;
                case anchor_point.TOP_LEFT: 
                    break;
                case anchor_point.TOP_RIGHT:
                    o.X -= size.X;
                    break;
                case anchor_point.BOTTOM_LEFT:
                    o.Y -= size.Y;
                    break;
                case anchor_point.BOTTOM_RIGHT:
                    o.X -= size.X;
                    o.Y -= size.Y;
                    break;
                case anchor_point.CENTER:
                    o.X -= size.X / 2;
                    o.Y -= size.Y / 2;
                    break;
            }
            return o;
        }

        public override void draw_rt() {
            draw_internal(Vector2.Zero + draw_offset(), text_justification);
        }

        public override void draw() {
            if (!_auto_size) {
                Drawing.image(draw_target, position, size);
            } else {
                draw_internal(position + draw_offset(), text_justification);
            }
        }
    }
}
