﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public class Panel : UIElement {

        public Action<UIElementManager> build_action;

        public Panel(string name, XYPair position, XYPair size, Action<Panel, UIElementManager>? build_action) : base(name, position, size) {
            this.enable_render_target = true;
            build(position, size, build_action);
        }

        void build(XYPair position, XYPair size, Action<Panel, UIElementManager>? build_action) {
            can_be_focused = false;
            sub_elements = new UIElementManager(position, size);
            if (build_action != null) build_action(this, sub_elements);            
        }

        internal override void update() {
            sub_elements.update();
            sub_elements.size = this.size;
        }

        internal override void draw_rt() {
            Drawing.fill_rect(XYPair.Zero, size.X, size.Y, Swoop.UI_background_color);
            sub_elements.sub_draw(draw_target);
            Drawing.rect(XYPair.One, size, Swoop.get_color(this), 1f);
        }

        internal override void draw() {
            Drawing.image(draw_target, position, size);
        }

        internal override void added() {}
    }
}
