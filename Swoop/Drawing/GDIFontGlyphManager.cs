using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SwoopLib;
using SwoopLib.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwoopLib {
    
    public class GlyphInfo {
        public string character;
        
        public XYPair position;
        public XYPair size;

        public int pixels_start;
        public int pixels_end;
        public int glyph_width;

        public bool needs_adding;

        public GlyphInfo(string character, XYPair position, XYPair size) {
            this.character = character;
            this.position = position;
            this.size = size;
            needs_adding = true;
        }
    }

    public class GlyphRow {        
        public XYPair size = XYPair.Zero;
        public int row_top = 0;
        public float widest_char = float.MinValue;

        string row = "";
        public bool needs_adding;

        public Dictionary<string, GlyphInfo> glyphs = new Dictionary<string, GlyphInfo>();

        public GlyphRow(int row_top) {
            this.row_top = row_top;
        }

        /// <summary>
        /// Returns true if the glyph was able to fit at the end of the row
        /// </summary>
        /// <param name="character"></param>
        /// <param name="font"></param>
        /// <param name="graphics"></param>
        /// <returns></returns>
        public bool add_glyph(string character, FontManager parent) {            
            var char_size = parent.graphics.MeasureString(character.ToString(), parent.gdi_font);
            if (glyphs.ContainsKey(character)) return true;

            //glyph won't fit on X
            if (size.X + (int)char_size.Width > parent.char_map_size.X) return false;            
                      
            if (char_size.Height > size.Y) {
                size.Y = (int)char_size.Height;
            }
            var pos = size.X_only + (XYPair.UnitY * row_top);

            if (char_size.Width > widest_char) widest_char = char_size.Width;
            
            glyphs.Add(character, new GlyphInfo(character, pos, char_size.ToXYPair()));

            parent.clear_canvas();
            parent.draw_glyph_to_canvas(character, XYPair.Zero);

            parent.copy_glyph_to_texture_and_trim_sides(parent, character, ref parent.char_map_texture, pos);

            size.X += (int)glyphs[character].glyph_width;
            needs_adding = true;

            return true;
        }

    }

    public class FontManager {
        string font_family = "Ariel";
        float font_size = 16f;
        internal System.Drawing.FontStyle font_style = System.Drawing.FontStyle.Regular;
        internal System.Drawing.Font gdi_font;

        private float kerning_scale = 1.0f;

        public void alter_line_height(int alter_by) => line_height_alteration = alter_by;
        int line_height_alteration = 0;
        public int line_height => _line_height + line_height_alteration;
        int _line_height = 0;
        float space_size = 0f;
        float average_character_width = 0f;

        internal System.Drawing.Color transparency = System.Drawing.Color.FromArgb(255, 0, 255, 0);
        internal System.Drawing.SolidBrush text_brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 0));

        bool monospace = false;
        int monospace_width = 16;

        public List<GlyphRow> glyph_rows = new List<GlyphRow>();

        public readonly XYPair char_map_size = new XYPair(512, 512);
        public RenderTarget2D char_map_texture;

        public readonly XYPair glyph_canvas_size = new XYPair(100, 100);
        public RenderableGDIBitmap current_glyph_bmp;

        public static DrawGlyph glyph_draw_shader;

        public System.Drawing.Graphics graphics => current_glyph_bmp.graphics;

        public FontManager(string font_family, float font_size, float kerning_scale, bool default_glyphs = true) {
            this.font_family = font_family;
            this.font_size = font_size;
            this.kerning_scale = kerning_scale;

            init();
            if (default_glyphs || monospace) {
                add_glyphs_to_texture(32, 126);
                monospace_width = (int)Math.Ceiling(find_widest_character());
            }
        }

        public FontManager(string font_family, float font_size, bool default_glyphs = true) {
            this.font_family = font_family;
            this.font_size = font_size;

            init();
            if (default_glyphs || monospace) {
                add_glyphs_to_texture(32, 126);
                monospace_width = (int)Math.Ceiling(find_widest_character());
            }
        }
        public FontManager(string font_family, float font_size, System.Drawing.FontStyle font_style, float kerning_scale, bool default_glyphs = true) {
            this.font_family = font_family;
            this.font_size = font_size;
            this.kerning_scale = kerning_scale;
            this.font_style = font_style;

            init();
            if (default_glyphs || monospace) {
                add_glyphs_to_texture(32, 126);
                monospace_width = (int)Math.Ceiling(find_widest_character());
            }
        }

        public FontManager(string font_family, float font_size, System.Drawing.FontStyle font_style, bool default_glyphs = true) {
            this.font_family = font_family;
            this.font_size = font_size;
            this.font_style = font_style;
            init();
            if (default_glyphs || monospace) {
                add_glyphs_to_texture(32, 126);
                monospace_width = (int)Math.Ceiling(find_widest_character());
            }
        }


        public FontManager(bool default_glyphs = true) {
            init();
            if (default_glyphs || monospace) {
                add_glyphs_to_texture(32, 126);
                monospace_width = (int)Math.Ceiling(find_widest_character());
            }
        }

        void init() {
            gdi_font = new System.Drawing.Font(font_family, font_size, font_style);

            char_map_texture = new RenderTarget2D(Drawing.graphics_device, char_map_size.X, char_map_size.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            current_glyph_bmp = new RenderableGDIBitmap(glyph_canvas_size.X, glyph_canvas_size.Y);

            Drawing.graphics_device.SetRenderTarget(char_map_texture);
            Drawing.graphics_device.Clear(Color.FromNonPremultiplied(transparency.R, transparency.G, transparency.B, transparency.A));

            if (glyph_draw_shader == null)
                glyph_draw_shader = new DrawGlyph(Swoop.content);

            //add default glyphs
        }

        internal float find_widest_character() {
            float w = float.MinValue;
            
            foreach (GlyphRow row in glyph_rows) {
                if (row.widest_char > w) w = row.widest_char;
            }

            return w;
        }
        internal float find_average_width() {
            int count = 0;
            float w = 0f;

            foreach (GlyphRow row in glyph_rows) {
                foreach(string k in row.glyphs.Keys) {
                    w += row.glyphs[k].glyph_width;
                    count++;
                }
            }

            return w / (float)count;
        }

        public void alter_glyph_width(string glyph, int alter_by) {
            int ri = -1;
            if (glyph_exists(glyph, out ri)) {
                glyph_rows[ri].glyphs[glyph].glyph_width += alter_by;
            }
        }

        internal void clear_canvas() {
            graphics.Clear(transparency);
        }

        internal void draw_glyph_to_canvas(char character, XYPair pos) {
            graphics.DrawString(character.ToString(), gdi_font, text_brush, pos.ToPointF());
        }
        internal void draw_glyph_to_canvas(string characters, XYPair pos) {
            graphics.DrawString(characters, gdi_font, text_brush, pos.ToPointF());
        }
        internal void copy_glyph_to_texture(XYPair size, XYPair char_map_pos) {
            //oh boy
        }

        public bool glyph_exists(string str, out int row_index) {
            int r = 0;
            for (int i = 0; i < glyph_rows.Count; i++) {
                if (glyph_rows[i].glyphs.ContainsKey(str)) {
                    row_index = r;
                    return true;
                }

                r++;
            }
            row_index = -1;
            return false;
        }

        public void add_glyphs_to_texture(int start_value, int end_value) {
            for (int i = start_value; i <= end_value; i++) {
                if (glyph_rows.Count == 0) {
                    glyph_rows.Add(new GlyphRow(0));
                }

                //glyph didn't fit at end of row
                if (!glyph_rows[^1].add_glyph(((char)i).ToString(), this)) {
                    glyph_rows.Add(new GlyphRow(glyph_rows[^1].row_top + glyph_rows[^1].size.Y));
                    glyph_rows[^1].add_glyph(((char)i).ToString(), this);
                }
            }
            average_character_width = find_average_width();
            space_size = average_character_width / 2f;
            if (_line_height < glyph_rows[^1].size.Y) _line_height = glyph_rows[^1].size.Y;
        }


        public void add_glyphs_to_texture(string characters) {
            foreach (char character in characters) {
                if (glyph_rows.Count == 0) {
                    glyph_rows.Add(new GlyphRow(0));
                }

                //glyph didn't fit at end of row
                if (!glyph_rows[^1].add_glyph(character.ToString(), this)) {
                    glyph_rows.Add(new GlyphRow(glyph_rows[^1].row_top + glyph_rows[^1].size.Y));
                    glyph_rows[^1].add_glyph(character.ToString(), this);
                }
            }
            average_character_width = find_average_width();
            space_size = average_character_width / 2f;
            if (_line_height < glyph_rows[^1].size.Y) _line_height = glyph_rows[^1].size.Y;
        }

        public void add_unicode_glyph_to_texture(string s) {
            if (glyph_rows.Count == 0) {
                glyph_rows.Add(new GlyphRow(0));
            }

            //glyph didn't fit at end of row
            if (!glyph_rows[^1].add_glyph(s, this)) {
                glyph_rows.Add(new GlyphRow(glyph_rows[^1].row_top + glyph_rows[^1].size.Y));

                glyph_rows[^1].add_glyph(s, this);
            }
            average_character_width = find_average_width();
            space_size = average_character_width / 2f;
            if (_line_height < glyph_rows[^1].size.Y) _line_height = glyph_rows[^1].size.Y;
        }


        public void draw_glyph(char character, XYPair position) {
            foreach (GlyphRow row in glyph_rows) {
                if (row.glyphs.ContainsKey(character.ToString())) {
                    var g = row.glyphs[character.ToString()];

                    glyph_draw_shader.configure_shader(this, g);
                    Drawing.begin(glyph_draw_shader.effect);

                    Drawing.image(char_map_texture, position, g.size, g.position, g.size);
                    return;
                }
            }
            Drawing.end();
        }

        public int find_x_position_in_string(string str, int index) {
            string sub = str.Substring(0, index);
            return measure_string(sub).X;
        }

        //this shit is outstandingly fucking stupid why am I maintaining two borderline identical methods
        //do better
        public XYPair measure_string(string s, float scale = 1.0f) {
            if (String.IsNullOrEmpty(s)) return XYPair.Zero;

            int index = 0;
            string current_str = s.Substring(index, 1);
            char current_char = s[index];
            int current_x = 0;
            int current_y = 0;

            int highest_x = int.MinValue;

            while (index < s.Length) {
            start:
                //fancy chars
                switch (current_char) {
                    case '\n':
                        current_x = 0;
                        current_y += _line_height;

                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;

                    case '\r':
                        index++;
                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;

                    case ' ':
                        if (!monospace)
                            current_x += (int)Math.Ceiling(graphics.MeasureString(" ", gdi_font).Width * scale);
                        else
                            current_x += monospace_width;

                        if (current_x > highest_x) highest_x = current_x;

                        index++;
                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;
                }

                if (current_char != ' ')
                    current_str = current_str.TrimEnd();

                int r = 0;
                if (glyph_exists(current_str, out r)) {
                    if (!monospace) {
                        current_x += (int)(((glyph_rows[r].glyphs[current_str].glyph_width + (((font_size / 10f) * kerning_scale))) * (scale)));
                    } else {
                        current_x += monospace_width;
                    }

                    if (current_x > highest_x) highest_x = current_x;

                    if (current_char > 50000) {
                        index++;
                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                        } else break;
                    } else {
                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                        } else break;
                    }


                } else { //add a new glyph
                    if (index < s.Length - 1 && current_char > 50000 && s[index + 1] > 50000) {
                        current_str = current_char.ToString() + s[index + 1].ToString();
                        current_char = s[index];

                        add_unicode_glyph_to_texture(current_str);
                    } else if (index < s.Length) {
                        add_glyphs_to_texture(current_char.ToString());
                    } else break;
                }
            }
            end:
            return new XYPair(highest_x, current_y + _line_height);
        }

        static Rectangle rect_draw = new Rectangle();
        static Rectangle rect_source = new Rectangle();

        public void draw_string(string s, XYPair position, Color color, float scale = 1.0f) {
            if (String.IsNullOrEmpty(s)) return;
            int index = 0;
            string current_str = s.Substring(index, 1);
            char current_char = s[index];
            if (current_char > 50000 && index < s.Length - 1) {
                current_str = current_char.ToString() + s[index + 1].ToString();
            }
            int current_x = 0;
            int current_y = 0;

            if (scale >= 1.0f)
                glyph_draw_shader.begin_spritebatch(Drawing.sb, SamplerState.PointClamp);
            else
                glyph_draw_shader.begin_spritebatch(Drawing.sb, SamplerState.LinearWrap);

            while (index < s.Length) {
            start:
                //fancy chars
                switch (current_char) {
                    case '\n':
                        current_x = 0;
                        current_y += _line_height;
                        index++;
                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;

                    case '\r':
                        index++;
                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;

                    case ' ':
                        if (!monospace)
                            current_x += (int)Math.Ceiling(graphics.MeasureString(" ", gdi_font).Width * scale);
                        else
                            current_x += monospace_width;

                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                            if (current_char > 50000 && index < s.Length - 1) {
                                current_str = current_char.ToString() + s[index + 1].ToString();
                            }
                        } else goto end;
                        goto start;
                }

                if (current_char != ' ')
                    current_str = current_str.TrimEnd();

                int r = 0;
                if (glyph_exists(current_str, out r)) {

                    if (current_char != ' ') {
                        rect_draw.X = position.X + current_x;
                        rect_draw.Y = position.Y + current_y;

                        rect_draw.Size = (glyph_rows[r].glyphs[current_str].size * scale).ToPoint();
                        
                        rect_source.X = glyph_rows[r].glyphs[current_str].position.X;
                        rect_source.Y = glyph_rows[r].glyphs[current_str].position.Y;
                        rect_source.Width = glyph_rows[r].glyphs[current_str].size.X;
                        rect_source.Height = glyph_rows[r].glyphs[current_str].size.Y;

                        Drawing.sb.Draw(char_map_texture, rect_draw, rect_source, color);
                    }
                   // Drawing.image(char_map_texture,
                   //         position + (XYPair.UnitX * current_x) + (XYPair.UnitY * current_y),
                   //         glyph_rows[r].glyphs[current_str].size * scale,
                   //         glyph_rows[r].glyphs[current_str].position, glyph_rows[r].glyphs[current_str].size,
                   //         color);

                    if (!monospace) {
                        current_x += (int)(((glyph_rows[r].glyphs[current_str].glyph_width + (((font_size / 10f) * kerning_scale))) * (scale)));
                    } else {
                        current_x += monospace_width;
                    }

                    if (current_char > 50000) {
                        index++;
                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                        } else break;
                    } else {
                        index++;

                        if (index < s.Length) {
                            current_str = s.Substring(index, 1);
                            current_char = s[index];
                        } else break;
                    }

   
                    
                } else { //add a new glyph
                    if (index < s.Length - 1 && current_char > 50000 && s[index + 1] > 50000) {
                        current_str = current_char.ToString() + s[index + 1].ToString();
                        current_char = s[index];

                        add_unicode_glyph_to_texture(current_str);
                    } else {
                        add_glyphs_to_texture(current_char.ToString());
                    }
                }                           
            }
            end:
            Drawing.end();
        }
        public void draw_string_shadow(string s, XYPair position, Color color, Color shadow_color, XYPair shadow_offset, float scale = 1.0f) {
            draw_string(s, position + shadow_offset, shadow_color, scale);
            draw_string(s, position, color, scale);
        }
        public void draw_string_shadow(string s, XYPair position, Color color, Color shadow_color, float scale = 1.0f) {
            draw_string(s, position + XYPair.One, shadow_color, scale);
            draw_string(s, position, color, scale);
        }
        public void draw_string_rainbow(string s, XYPair position, float scale, XYPair offset_per, params Color[] colors) {
            for (int i = colors.Length-1; i >= 0; i--) {
                draw_string(s, position + (offset_per * i), colors[i], scale);                
            }
        }

        public void draw_map_debug_layer(XYPair position, XYPair size, ContentManager content) {
            
            draw_string("I like elephants and God likes elephants", position, Swoop.UI_disabled_color, .5f);
            draw_string("I like elephants and God likes elephants", position + (XYPair.UnitY * 15), Swoop.UI_color, .75f);
            draw_string("I like elephants and God likes elephants", position + (XYPair.UnitY * 35), Swoop.UI_highlight_color, 1f);


            Drawing.end();
        }



        public void copy_glyph_to_texture(FontManager parent, GlyphInfo glyph, ref RenderTarget2D texture_2d, XYPair position_on_texture) {
            System.Drawing.Imaging.BitmapData data = parent.current_glyph_bmp.bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, glyph.size.X, glyph.size.Y),

                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Color[] cdata = new Color[glyph.size.X * glyph.size.Y];

            int i = 0;
            unsafe {
                uint* ptr = (uint*)data.Scan0;
                int add_index = 0;

                for (i = 0; i < parent.glyph_canvas_size.X * parent.glyph_canvas_size.Y; i++) {
                    //check if the current linear index converted to X/Y coords sits within the bitmap bounds
                    var x = i % parent.glyph_canvas_size.X;
                    var y = Math.Floor((double)(i / parent.glyph_canvas_size.X));

                    if (x >= 0 && x < glyph.size.X &&
                        y >= 0 && y < glyph.size.Y) {

                        cdata[add_index].A = (byte)(*(ptr + (i)) >> 24);

                        cdata[add_index].R = (byte)(*(ptr + (i)) >> 16);

                        cdata[add_index].G = (byte)(*(ptr + (i)) >> 8);
                        cdata[add_index].B = (byte)(*(ptr + (i)));
                        add_index++;
                    }

                }

                lock (texture_2d) texture_2d.SetData(0, 0,
                    new Rectangle(position_on_texture.X, position_on_texture.Y, glyph.size.X, glyph.size.Y),
                    cdata, 0, cdata.Length);

                parent.current_glyph_bmp.bitmap.UnlockBits(data);

                cdata = null;
                ptr = null;
                GC.Collect();
            }
        }
        public void copy_glyph_to_texture_and_trim_sides(FontManager parent, string character, ref RenderTarget2D texture_2d, XYPair position_on_texture) {
            //find row
            int row_index = 0;

            foreach (var row in glyph_rows) {
                if (row.glyphs.ContainsKey(character))
                    break;               
                row_index++;
            }

            int i = 0;
            unsafe {
                int add_index = 0;

                int first_pixel = int.MaxValue;
                int last_pixel = int.MinValue;

                System.Drawing.Imaging.BitmapData data = parent.current_glyph_bmp.bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, glyph_rows[row_index].glyphs[character].size.X, glyph_rows[row_index].glyphs[character].size.Y),

                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                uint* ptr = (uint*)data.Scan0;

                for (i = 0; i < parent.glyph_canvas_size.X * parent.glyph_canvas_size.Y; i++) {
                    //check if the current linear index converted to X/Y coords sits within the bitmap bounds
                    var x = i % parent.glyph_canvas_size.X;
                    var y = Math.Floor((double)(i / parent.glyph_canvas_size.X));

                    //if (x >= 0 && x < glyph_rows[row_index].glyphs[character].size.X &&
                    //    y >= 0 && y < glyph_rows[row_index].glyphs[character].size.Y) {

                        var r = (byte)(*(ptr + (i)) >> 16);

                        if (r > 0 && x < first_pixel)
                            first_pixel = x;

                        if (r > 0 && x > last_pixel)
                            last_pixel = x;

                    //}
                }

                if (first_pixel == int.MaxValue || last_pixel == int.MinValue) goto end;

                glyph_rows[row_index].glyphs[character].pixels_start = first_pixel;
                glyph_rows[row_index].glyphs[character].pixels_end = last_pixel;
                glyph_rows[row_index].glyphs[character].glyph_width = (last_pixel - first_pixel) + 1;
                glyph_rows[row_index].glyphs[character].size.X = glyph_rows[row_index].glyphs[character].glyph_width;

                Color[] cdata = new Color[glyph_rows[row_index].glyphs[character].glyph_width * glyph_rows[row_index].glyphs[character].size.Y];                

                add_index = 0;
                for (i = 0; i < parent.glyph_canvas_size.X * parent.glyph_canvas_size.Y; i++) {
                    //check if the current linear index converted to X/Y coords sits within the bitmap bounds
                    var x = i % parent.glyph_canvas_size.X;
                    var y = Math.Floor((double)(i / parent.glyph_canvas_size.X));

                    if (x >= glyph_rows[row_index].glyphs[character].pixels_start && 
                        x < glyph_rows[row_index].glyphs[character].pixels_start + glyph_rows[row_index].glyphs[character].glyph_width &&
                        y >= 0 && y < glyph_rows[row_index].glyphs[character].size.Y) {

                        cdata[add_index].A = (byte)(*(ptr + (i)) >> 24);
                        cdata[add_index].R = (byte)(*(ptr + (i)) >> 16);
                        cdata[add_index].G = (byte)(*(ptr + (i)) >> 8);
                        cdata[add_index].B = (byte)(*(ptr + (i)));


                        add_index++;
                    }
                }

                lock (texture_2d) texture_2d.SetData(0, 0,
                    new Rectangle(position_on_texture.X, position_on_texture.Y, glyph_rows[row_index].glyphs[character].glyph_width, glyph_rows[row_index].glyphs[character].size.Y),
                    cdata, 0, cdata.Length);
                end:
                parent.current_glyph_bmp.bitmap.UnlockBits(data);

                cdata = null;
                ptr = null;
                GC.Collect();
            }
        }
    }
}
