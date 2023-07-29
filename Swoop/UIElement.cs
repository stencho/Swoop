using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SwoopLib.Collision;

namespace SwoopLib {
    public abstract class UIElement {
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

        public anchor_point anchor = anchor_point.TOP_LEFT;

        internal XYPair anchor_offset() {
            XYPair o = XYPair.Zero;
            switch (anchor) {
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

        public readonly string name;

        internal UIElementManager parent { get; set; }

        public UIElementManager sub_elements;
        public bool has_sub_elements => (sub_elements != null && sub_elements.elements.Count > 0);

        XYPair _position = XYPair.Zero;
        public XYPair position { get { return _position + anchor_offset(); } set { _position = value; } }
        public XYPair position_actual => _position;

        XYPair _size;
        public XYPair size { get { return _size; } set {
                _size = value;
                resize_finish();
            } }

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


        public bool is_focused => UIElementManager.focused_element == this;

        public bool ignore_dialog { get; set; } = false;
        public bool can_be_focused { get; set; } = true;

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
                            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    } else {
                        draw_target = null;
                    }
                }
            }
        }
        internal RenderTarget2D draw_target;

        public UIElement(string name, XYPair position, XYPair size) {
            this.name = name;
            this.position = position;
            this.size = size;
        }

        internal abstract void added();
        internal abstract void update();
        internal abstract void draw();
        internal abstract void draw_rt();

        internal bool click_update(XYPair manager_position, XYPair manager_size, bool mouse_over_hit) {

            bool hit_bounds = Collision2D.point_intersects_rect(Input.cursor_pos,
                    manager_position, manager_position + manager_size);

            if (mouse_over_hit) mouse_over = false;
            else mouse_over = hit_bounds && Window.mouse_over_window
                    && Collision2D.point_intersects_rect(Input.cursor_pos, manager_position + position,
                    manager_position + position + (size - XYPair.One));

            mouse_was_down = mouse_down;
            mouse_down = Input.is_pressed(InputStructs.MouseButtons.Left);

            right_mouse_was_down = right_mouse_down;
            right_mouse_down = Input.is_pressed(InputStructs.MouseButtons.Right);
            
            was_clicking = clicking;

            if (!UIExterns.in_foreground()) {
                clicking = false;
                return false;
            }

            if (clicking && !mouse_down) this.clicking = false;
            if (mouse_over && mouse_down && !mouse_was_down) this.clicking = true;
            
            return (mouse_over && mouse_down && !mouse_was_down);
        }

        internal void resize_finish() {
            if (_enable_rt) {
                draw_target.Dispose();
                draw_target = new RenderTarget2D(Drawing.graphics_device, (int)width, (int)height, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
                GC.Collect();
            }
        }
    }
}
