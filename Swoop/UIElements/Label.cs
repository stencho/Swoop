using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class Label : UIElement {
        string text = "";

        public bool draw_outline = false;

        bool _auto_size = true;
        bool auto_size {
            get { return _auto_size; }
            set {
                enable_render_target = !value;
                if (value) line_wrap = false;
                _auto_size = value;
            }
        }


        bool line_wrap = true;

        public Label(string name, string text, XYPair position, XYPair size)
            : base(name, position, size) {
            auto_size = false;
            this.text = text;
            this.position = position;
            this.size = size;
            can_be_focused = false;
        }

        public Label(string name, string text, XYPair position, XYPair size, anchor_point anchor)
            : base(name, position, size) {
            auto_size = false;
            this.text = text;
            this.position = position;
            this.size = size;
            this.anchor = anchor;
            can_be_focused = false;
        }

        public Label(string name, string text, XYPair position)
            : base(name, position, XYPair.Zero) {
            auto_size = true;
            this.text = text;
            this.position = position;
            this.size = Drawing.measure_string_profont_xy(text);
            can_be_focused = false;
        }

        public Label(string name, string text, XYPair position, anchor_point anchor)
            : base(name, position, XYPair.Zero) {
            auto_size = true;
            this.text = text;
            this.position = position;
            this.size = Drawing.measure_string_profont_xy(text);
            this.anchor = anchor;
            can_be_focused = false;
        }

        internal override void update() { }

        public enum alignment { LEFT, CENTER, RIGHT }
        public alignment text_justification = alignment.LEFT;

        private void draw_internal(XYPair offset, alignment tj) {
            StringReader sr = new StringReader(text);
            int line_num = 0;
            float line_height = Drawing.measure_string_profont("A").Y;


            while (sr.Peek() > -1) {
                string line = sr.ReadLine();
                float line_width = Drawing.measure_string_profont(line).X;

                XYPair line_offset = XYPair.Zero;

                if (tj == alignment.CENTER)
                    line_offset = new XYPair((size.X / 2) - (line_width / 2), 0);
                else if (tj == alignment.RIGHT)
                    line_offset = new XYPair(size.X - line_width, 0);

                if (line_wrap) {
                    if (line_width > size.X) {
                        float build_size = 0;

                        string working_string = "";
                        int last_space = 0;
                        int next_space = 0;

                        still_too_long:

                        if (!line.Contains(' ')) {
                            Drawing.text(line, offset + line_offset + (line_height * line_num * XYPair.UnitY), Swoop.get_color(this));
                            line_num++;
                            continue;
                        }

                        while (build_size < this.size.X && line.IndexOf(' ', last_space + 1) < line.LastIndexOf(' ')) {
                            next_space = line.IndexOf(' ', last_space + 1);

                            if (Drawing.measure_string_profont(working_string + line.Substring(last_space, next_space - last_space)).X < this.size.X) {
                                working_string += line.Substring(last_space, next_space - last_space);
                                build_size = Drawing.measure_string_profont(working_string).X;
                            } else break;

                            last_space = next_space;
                        }

                        Drawing.text(working_string, offset + line_offset + (line_height * line_num * Vector2.UnitY), Swoop.get_color(this));
                        line_num++;

                        line = line.Remove(0, working_string.Length).TrimStart();
                        line_width = Drawing.measure_string_profont(line).X;

                        if (tj == alignment.CENTER)
                            line_offset = new XYPair((size.X / 2) - (line_width / 2), 0);
                        else if (tj == alignment.RIGHT)
                            line_offset = new XYPair(size.X - line_width, 0);

                        if (line_width > size.X) {
                            working_string = "";
                            last_space = 0;
                            build_size = 0;

                            goto still_too_long;
                        } else {
                            Drawing.text(line, offset + line_offset + (line_height * line_num * XYPair.UnitY), Swoop.get_color(this));
                            line_num++;
                        }

                    } else {
                        Drawing.text(line, offset + line_offset + (line_height * line_num * XYPair.UnitY), Swoop.get_color(this));
                    }

                } else {
                    Drawing.text(line, offset + line_offset + (line_height * line_num * XYPair.UnitY), Swoop.get_color(this));
                }

                line_num++;
            }

            if (draw_outline)
                Drawing.rect(offset - (XYPair.UnitX * 2), size.X + 5, size.Y, Swoop.get_color(this), 1f);
        }

        internal override void draw_rt() {
            draw_internal(XYPair.Zero, text_justification);
        }

        internal override void draw() {
            if (!_auto_size) {
                Drawing.image(draw_target, position, size);
            } else {
                draw_internal(position, text_justification);
            }
        }
        internal override void added() {}
    }
}
