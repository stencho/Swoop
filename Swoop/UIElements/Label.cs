using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Swoop.UIElements {
    internal class Label : UIElement {
        string text = "";

        public bool draw_outline = false;
        
        Color text_color = Color.White;

        bool _auto_size = true;
        bool auto_size { get { return _auto_size; } set {
                enable_render_target = !value; 
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

        public Label(string text, Vector2 position) 
            : base(position, Drawing.measure_string_profont(text)) {
            auto_size = true;
            this.text = text;
            this.position = position;
            this.size = Drawing.measure_string_profont(text);
        }

        public override void update() {

        }

        private void draw_internal(Vector2 offset) {
            StringReader sr = new StringReader(text);
            int line_num = 0;
            float line_height = Drawing.measure_string_profont("A").Y;
            
            while (sr.Peek() > -1) {
                string line = sr.ReadLine();
                float line_width = Drawing.measure_string_profont(line).X;

                if (line_wrap) {
                    if (line_width > size.X) {
                        float build_size = 0;
                        
                        string working_string = "";
                        int last_space = 0;
                        int next_space = 0;

                        still_too_long:

                        if (!line.Contains(' ')) {
                            Drawing.text(line, offset + (line_height * line_num * Vector2.UnitY), text_color);
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

                        Drawing.text(working_string, offset + (line_height * line_num * Vector2.UnitY), text_color);
                        line_num++;

                        line = line.Remove(0, working_string.Length).TrimStart();
                        line_width = Drawing.measure_string_profont(line).X;

                        if (line_width > size.X) {
                            working_string = "";
                            last_space = 0;
                            build_size = 0;

                            goto still_too_long;
                        } else {
                            Drawing.text(line, offset + (line_height * line_num * Vector2.UnitY), text_color);
                            line_num++;
                        }

                    } else {
                        Drawing.text(line, offset + (line_height * line_num * Vector2.UnitY), text_color);
                    }

                } else {
                    Drawing.text(line, offset + (line_height * line_num * Vector2.UnitY), text_color);
                }

                line_num++;
            }

            if (draw_outline)
                Drawing.rect(offset + Vector2.One, size, Color.White, 1f);
        }

        public override void draw_rt() {
            draw_internal(Vector2.Zero);
        }

        public override void draw() {
            if (!_auto_size) {
                Drawing.image(draw_target, position, size);
            } else {
                draw_internal(position);
            }
        }
    }
}
