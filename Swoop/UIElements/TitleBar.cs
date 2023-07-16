﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SwoopLib.UIElements
{
    public class TitleBar : UIElement
    {
        public string left_text { get; set; } = string.Empty;
        public string right_text { get; set; } = string.Empty;

        bool auto_height = false;
        public TitleBar(string name, Vector2 position, int width) : base(name, position, new Vector2(width, Drawing.measure_string_profont("A").Y + 3)) {
            auto_height = true;
        }
        public TitleBar(string name, Vector2 position, Vector2 size) : base(name, position, size) {
            auto_height = false;
        }

        internal override void update() {
            Window.moving_window = clicking;
        }

        internal override void draw() {
            Drawing.fill_rect_outline(position, position + size, Color.Black, Swoop.UIColor, 1f);

            if (!string.IsNullOrEmpty(left_text))
                Drawing.text(left_text, position + Vector2.One * 3, Swoop.UIColor);
            if (!string.IsNullOrEmpty(right_text)) {
                float right_text_width = Drawing.measure_string_profont(right_text).X;
                Drawing.text(right_text,
                    position + (Vector2.UnitY * 3) + (Vector2.UnitX * size.X) - (Vector2.UnitX * (right_text_width + 3f))
                    , Swoop.UIColor);
            }
        }
        internal override void draw_rt() { }
        internal override void added() { }
    }
}
