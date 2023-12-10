﻿using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.UIElements {
    public class ListBoxItem {
        ListBox parent;

        AutoRenderTarget rt;
        internal RenderTarget2D render_target => rt.render_target;

        int left_margin = 4;

        bool _custom_draw = false;
        public bool custom_draw => _custom_draw;

        Action<XYPair, XYPair> draw_action;

        string _text = "";
        public string text {
            get { return _text; }
            set {
                if (_text != value) { 
                    _text = value;
                    text_size = Drawing.measure_string_profont_xy(text);
                    _height = text_size.Y;
                    size_changed();
                }
            }
        }

        int _height = 0;
        public int height {
            get => _height;
            set {
                if (_height != value) { 
                    _height = value;
                    size_changed();
                }
            }
        }

        XYPair text_size;
        internal XYPair size => _custom_draw ?
            (XYPair.UnitY * _height) +
            parent.size_minus_scroll_bar.X_only :
            text_size.Y_only + parent.size_minus_scroll_bar.X_only;

        void draw_text(XYPair position, XYPair size) {
            Drawing.text(text, XYPair.Zero + (XYPair.UnitX * left_margin), Swoop.UI_color);            
        }

        public ListBoxItem(string text) {
            this.text = text;
            _height = text_size.Y;
            init();
        }
        
        public ListBoxItem(int height, Action<XYPair, XYPair> draw_action) {
            this.draw_action = draw_action;
            _height = height;
            _custom_draw = true;
            init();
        }

        void init() {
        }

        internal void set_parent (ListBox parent) {
            this.parent = parent;
            if (_custom_draw) {                
                rt = new AutoRenderTarget(size);
                rt.draw = draw_action;
            } else {
                rt = new AutoRenderTarget(size);
                rt.draw = draw_text;
            }
        }

        internal void size_changed() {
            if (rt == null) return;
            rt.size = size;            
        }

    }

    public class ListBox : UIElement {
        List<ListBoxItem> items = new List<ListBoxItem>();

        int top_margin = 5;
        int bottom_margin = 4;

        float lb_height => (float)size.Y;
        int total_height = 0;

        float visible_area_fract_of_height => (float)size.Y / (float)total_height;

        float top_position_fract => scroll_position / (float)total_height;
        float bottom_position_fract => (scroll_position + lb_height) / (float)total_height;

        int _scroll_width = 7;
        int scroll_bar_width { get => _scroll_width;
            set {
                if (_scroll_width != value) {
                    _scroll_width = value;

                    foreach(ListBoxItem item in items) {
                        item.size_changed();
                    }
                }

            }
        }
        float scroll_position = 0f;

        internal XYPair size_minus_scroll_bar => size - (XYPair.UnitX * scroll_bar_width);

        public void add_item(ListBoxItem item) { 
            item.set_parent(this);
            items.Add(item); 

            if (item.custom_draw) total_height += item.height;
            else total_height += item.height + top_margin + bottom_margin + 1;
            
        }
        public void remove(ListBoxItem item) {
            if (item.custom_draw) total_height -= item.height;
            else total_height -= item.height + top_margin + bottom_margin + 1;
            
            items.Remove(item);
            
        }
        public void remove_at(int index) {
            if (items[index].custom_draw) total_height -= items[index].height;
            else total_height -= items[index].height + top_margin + bottom_margin + 1;

            items.RemoveAt(index); 
        }

        public ListBoxItem last_item => items[items.Count - 1];

        public ListBox(string name, XYPair position, XYPair size) : base(name, position, size) {
            enable_render_target = true;
        }

        internal override void added() {}

        internal override void draw() {
            Drawing.fill_rect(position, position + size, Swoop.UI_background_color);
            Drawing.image(draw_target, position, size);
            Drawing.rect(position, position + size, Swoop.get_color(this), 1f);
        }

        internal override void draw_rt() {
            int running_height = 0;


            foreach (ListBoxItem item in items) {
                if (item.custom_draw) {
                    var item_bottom = running_height + item.height;
                    if ((item_bottom > scroll_position && item_bottom < scroll_position + lb_height) ||
                        (running_height > scroll_position && running_height < scroll_position + lb_height)) {

                        if (item.render_target != null)
                            Drawing.image(item.render_target, (XYPair.UnitY * (running_height - scroll_position)), item.size);

                    }

                    running_height += item.height;

                } else {
                    var item_bottom = running_height + item.height;
                    if ((item_bottom > scroll_position && item_bottom < scroll_position + lb_height) ||
                        (running_height > scroll_position && running_height < scroll_position + lb_height)) {

                        if (item.render_target != null)
                            Drawing.image(item.render_target, (XYPair.UnitY * (running_height - scroll_position)) + (Vector2.UnitY * top_margin), item.size);
                    }

                    running_height += item.height + top_margin + bottom_margin + 1;
                }

                Drawing.line((Vector2.UnitY * (running_height - scroll_position)), (Vector2.UnitY * (running_height - scroll_position)) + (Vector2.UnitX * size_minus_scroll_bar.X), Swoop.UI_color, 1f);
            }

            //draw scroll bar            
            Drawing.fill_rect(
                size_minus_scroll_bar.X_only.ToVector2() + (size.Y_only.ToVector2() * top_position_fract),
                size.X_only.ToVector2() + (size.Y_only.ToVector2() * bottom_position_fract),                
                Swoop.UI_color);

            Drawing.line(size_minus_scroll_bar.X_only, size_minus_scroll_bar, Swoop.UI_color, 1f);
        }

        internal override void handle_focused_input() {

        }

        InputHandler handler = new InputHandler();
        internal override void update() {
            handler.update();
            if (mouse_over) {
                var delta = handler.rawinput_mouse_state.ScrollDelta;
                scroll_position -= delta / 6f;

                if (top_position_fract < 0f) {
                    scroll_position = 0f;

                }
                if (bottom_position_fract > 1f) {
                    scroll_position = total_height - lb_height;
                }
            }
        }
    }
}
