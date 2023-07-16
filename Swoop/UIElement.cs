using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib {
    public abstract class UIElement {
        internal UIElementManager sub_elements;
        internal bool has_sub_elements => (sub_elements != null && sub_elements.elements.Count > 0);

        public Vector2 position { get; set; }
        public Vector2 size { get; set; }

        public float X => position.X;
        public float Y => position.Y;

        public float width => size.X;
        public float height => size.Y;

        public bool mouse_over { get; set; } = false;
        public bool mouse_down { get; set; } = false;
        public bool mouse_was_down { get; set; } = false;
        public bool right_mouse_down { get; set; } = false;
        public bool right_mouse_was_down { get; set; } = false;

        public bool clicking { get; set; } = false;
        public bool was_clicking { get; set; } = false;

        public bool ignore_dialog { get; set; } = false;

        internal UIElementManager parent { get; set; }

        public readonly string name;

        bool _enable_rt = false;
        public bool enable_render_target {
            get {
                return _enable_rt;
            }
            set {
                if (_enable_rt == value) { 
                    return; 
                } else {
                    _enable_rt = value;

                    if (value) {
                        draw_target = new RenderTarget2D(Drawing.graphics_device, (int)width, (int)height, false, 
                            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
                    } else {
                        draw_target = null;
                    }
                }
            }
        }
        internal RenderTarget2D draw_target;

        public UIElement(string name, Vector2 position, Vector2 size) {
            this.name = name;
            this.position = position;
            this.size = size;
        }

        internal abstract void update();
        internal abstract void added();
        internal abstract void draw();
        internal abstract void draw_rt();

        internal bool click_update(Rectangle bounds) {
            bool hit_bounds = Collision2D.v2_intersects_rect(Input.cursor_pos.ToVector2(),
                    bounds.Location.ToVector2(), bounds.Location.ToVector2() + bounds.Size.ToVector2());

            mouse_over = hit_bounds && Collision2D.v2_intersects_rect(Input.cursor_pos.ToVector2(), bounds.Location.ToVector2() + position, bounds.Location.ToVector2() + position + (size - Vector2.One));

            mouse_was_down = mouse_down;
            mouse_down = Input.is_pressed(InputStructs.MouseButtons.Left);

            right_mouse_was_down = right_mouse_down;
            right_mouse_down = Input.is_pressed(InputStructs.MouseButtons.Right);
            
            was_clicking = clicking;

            if (!UIExterns.in_foreground()) {
                clicking = false;
                return false;
            }

            if (clicking && !mouse_down) clicking = false;
            if (mouse_over && mouse_down && !mouse_was_down) clicking = true;
            
            return (mouse_over && mouse_down && !mouse_was_down);
        }
    }
}
