﻿using System;
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

        public enum AnchorPoint : byte {
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

        public AnchorPoint anchor_local = AnchorPoint.TOP_LEFT;

        internal XYPair anchor_offset() {
            XYPair o = XYPair.Zero;
            switch (anchor_local) {
                case AnchorPoint.TOP:
                    o.X -= size.X / 2;
                    break;
                case AnchorPoint.BOTTOM:
                    o.X -= size.X / 2;
                    o.Y -= size.Y;
                    break;
                case AnchorPoint.LEFT:
                    o.Y -= size.Y / 2;
                    break;
                case AnchorPoint.RIGHT:
                    o.X -= size.X;
                    o.Y -= size.Y / 2;
                    break;
                case AnchorPoint.TOP_LEFT:
                    break;
                case AnchorPoint.TOP_RIGHT:
                    o.X -= size.X;
                    break;
                case AnchorPoint.BOTTOM_LEFT:
                    o.Y -= size.Y;
                    break;
                case AnchorPoint.BOTTOM_RIGHT:
                    o.X -= size.X;
                    o.Y -= size.Y;
                    break;
                case AnchorPoint.CENTER:
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
        public XYPair position_actual { get { return _position; } set { _position = value; } }

        XYPair _size;
        public XYPair size { get { return _size; } set {
                _size = value;
                if (has_sub_elements) sub_elements.size = value;
                resize_finish();
            } 
        }

        public float X => position.X;
        public float Y => position.Y;

        public float width => size.X;
        public float height => size.Y;

        public float right => position.X + size.X;
        public float bottom => position.Y + size.Y;

        public XYPair bottom_xy => position + size.Y_only;
        public XYPair right_xy => position + size.X_only;


        public bool mouse_over { get; set; } = false;
        public bool mouse_down { get; set; } = false;
        public bool mouse_was_down { get; set; } = false;
        public bool right_mouse_down { get; set; } = false;
        public bool right_mouse_was_down { get; set; } = false;

        public bool clicking { get; set; } = false;
        public bool was_clicking { get; set; } = false;

        public bool focused => UIElementManager.focused_element == this;

        public bool ignore_dialog { get; set; } = false;
        public bool can_be_focused { get; set; } = true;
        public bool visible { get; set; } = true;
        public bool click_through { get; set; } = false;

        public Anchor? anchor_global { get; set; } = null;
        public Tooltip? tooltip { get; set; } = null;
        public void set_tooltip_text(string text) {
            if (tooltip != null) tooltip.text = text;
        }
        public void set_tooltip_title(string title) {
            if (tooltip != null) tooltip.title = title;            
        }

        public XYPair mouse_relative => Swoop.input_handler.mouse_position.ToXYPair() - this.position;

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
                            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                    } else {
                        draw_target = null;
                    }
                }
            }
        }

        internal RenderTarget2D draw_target;

        internal int disabled_at_index = -1;

        public UIElement(string name, XYPair position, XYPair size) {
            this.name = name;
            this.position = position;
            this.size = size;
        }

        internal abstract void update();
        internal abstract void draw();
        internal abstract void draw_rt();
        internal abstract void handle_focused_input();
        internal abstract void added();

        internal bool click_update(XYPair manager_position, XYPair manager_size, bool mouse_over_hit) {
            if (click_through) return false;

            bool hit_bounds = Collision2D.point_intersects_rect(Input.cursor_pos,
                    manager_position, manager_position + manager_size);

            if (mouse_over_hit) mouse_over = false;
            else mouse_over = hit_bounds && Window.mouse_over_window
                    && Collision2D.point_intersects_rect(Input.cursor_pos, manager_position + position,
                    manager_position + position + (size - XYPair.One));

            mouse_was_down = mouse_down;
            mouse_down = Input.is_pressed(MouseButtons.Left);

            right_mouse_was_down = right_mouse_down;
            right_mouse_down = Input.is_pressed(MouseButtons.Right);
            
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
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                GC.Collect();
            }
        }
    }
}
