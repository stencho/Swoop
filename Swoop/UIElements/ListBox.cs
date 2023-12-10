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
            parent.size.X_only :
            text_size;

        void draw_text(XYPair position, XYPair size) {
            Drawing.text(text, XYPair.Zero, Swoop.get_color(parent));            
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

        public void set_parent (ListBox parent) {
            this.parent = parent;
            if (_custom_draw) {                
                rt = new AutoRenderTarget(size);
                rt.draw = draw_action;
            } else {
                rt = new AutoRenderTarget(text_size);
                rt.draw = draw_text;
            }
        }

        public void size_changed() {
            if (_custom_draw) {
                rt.size = size;
                rt.draw = draw_action;
            } else {
                if (rt == null) rt = new AutoRenderTarget(text_size);
                else rt.size = text_size;
                rt.draw = draw_text;
            }
        }

    }

    public class ListBox : UIElement {
        List<ListBoxItem> items = new List<ListBoxItem>();

        int top_margin = 4;
        int bottom_margin = 4;
        int left_margin = 4;

        public void add_item(ListBoxItem item) { 
            item.set_parent(this);
            items.Add(item); 
        }
        public void remove(ListBoxItem item) => items.Remove(item);
        public void remove_at(int index) => items.RemoveAt(index);

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
            Drawing.line((Vector2.UnitY * running_height), (Vector2.UnitY * running_height) + (Vector2.UnitX * size.X), Swoop.get_color(this), 1f);

            foreach (ListBoxItem item in items) {
                if (item.custom_draw) {
                    if (item.render_target != null)
                        Drawing.image(item.render_target, XYPair.UnitY * running_height, item.size);
                    running_height += item.height;
                    Drawing.line((Vector2.UnitY * running_height), (Vector2.UnitY * running_height) + (Vector2.UnitX * size.X), Swoop.get_color(this), 1f);
                } else {
                    if (item.render_target != null)
                        Drawing.image(item.render_target, XYPair.UnitY * running_height + (Vector2.UnitY * top_margin) + (XYPair.UnitX * left_margin), item.size);

                    running_height += item.height + top_margin + bottom_margin + 1;

                    Drawing.line((Vector2.UnitY * running_height), (Vector2.UnitY * running_height) + (Vector2.UnitX * size.X), Swoop.get_color(this), 1f);
                }
            }
        }

        internal override void handle_focused_input() {

        }

        internal override void update() {

        }
    }
}
