using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib {

    public class UIElementManager {
        public RenderTarget2D render_target;

        //STATIC
        static List<UIElementManager> managers = new List<UIElementManager>();

        static void add_manager(UIElementManager manager) { managers.Add(manager); update_manager_indices(); }
        static void remove_manager(UIElementManager manager) { managers.Remove(manager); update_manager_indices(); }

        static void update_manager_indices() {
            for (int i = 0; i < managers.Count; i++) {
                managers[i].index = i;
            }
        }

        static int focused_manager_index = 0;
        public static UIElementManager? focused_manager => managers[focused_manager_index];

        static string focused_element_name = "";
        public static UIElement? focused_element => !string.IsNullOrWhiteSpace(focused_element_name) ? focused_manager?.elements[focused_element_name] : null;

        static bool interacting_with_focused_element = false;

        //public static bool allow_element_defocus { get; set; } = true;

        public static void focus_element(UIElementManager manager, string element_name) {
            if (manager.elements.ContainsKey(element_name)) {
                focused_manager_index = manager.index;
                focused_element_name = element_name;

            } else {
                //focused_manager_index = manager.index;
                //if (allow_element_defocus) focused_element_name = "";
                /*else*/ throw new Exception("Invalid element name");
            }
        }

        static void find_element_in_direction() {

        }

        public static void update_UI_input() { }


        //PER INSTANCE
        public Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();
        public List<string> element_order = new List<string>();
        IEnumerable<string> element_order_inverse_for_draw => element_order.Reverse<string>();

        public void send_to_front(string element_name) {
            lock (element_order) {
                if (element_order.Contains(element_name) && elements.ContainsKey(element_name)) {
                    element_order.Remove(element_name);
                    element_order.Insert(0, element_name);
                }
            }
        }

        public void send_to_back(string element_name) {
            lock (element_order) {
                if (element_order.Contains(element_name) && elements.ContainsKey(element_name)) {
                    element_order.Remove(element_name);
                    element_order.Add(element_name);
                }
            }
        }

        public void disable(string element_name) {
            lock (element_order) {
                if (element_order.Contains(element_name) && elements.ContainsKey(element_name)) {
                    elements[element_name].disabled_at_index = element_order.IndexOf(element_name);
                    element_order.Remove(element_name);
                }
            }
        }
        public void enable(string element_name) {
            lock (element_order) {
                if (!element_order.Contains(element_name) && elements.ContainsKey(element_name)) {
                    if (elements[element_name].disabled_at_index <= element_order.Count) {
                        if (elements[element_name].disabled_at_index == -1)
                            elements[element_name].disabled_at_index = 0;

                        element_order.Insert(elements[element_name].disabled_at_index, element_name);
                        elements[element_name].disabled_at_index = -1;
                    }
                }
            }
        }

        internal int index = 0;

        public string dialog_element = "";
        public bool in_dialog => !string.IsNullOrEmpty(dialog_element);
        public bool draw_border = false;

        public XYPair position;
        XYPair _size;
        public XYPair size {
            get { return _size; }
            set {
                if (_size != value) {
                    _size = value;
                    needs_resize = true;
                }
            }
        }
        bool needs_resize = false;
        void resize_rt() {
            if (render_target != null) render_target.Dispose();
            render_target = new RenderTarget2D(Drawing.graphics_device, _size.X, _size.Y,
                false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            needs_resize = false;
        }

        public UIElementManager(XYPair pos, XYPair size) {
            this.position = pos;
            this.size = size;


            resize_rt();

            add_manager(this);
        }

        ~UIElementManager() {
            remove_manager(this);
        }

        public void add_element(UIElement element) {
            element.parent = this;

            elements.Add(element.name, element);
            lock (element_order) element_order.Insert(0, element.name);

            elements[element.name].added();
        }
        public void add_elements(params UIElement[] elements) {
            foreach (var element in elements) {
                add_element(element);
                lock (element_order) element_order.Insert(0, element.name);
            }
        }

        public void remove_element(string name) {
            if (dialog_element == name) dialog_element = null;
            elements.Remove(name);
            lock (element_order) element_order.Remove(name);
        }

        bool mouse_down_prev = false;
        public void update() {
            bool click_hit = false;
            bool mouse_over_hit = false;
            bool mouse_down = Input.is_pressed(MouseButtons.Left);

            List<string> order = new List<string>(element_order);
            
            if (in_dialog) {
                if (elements[dialog_element].click_update(position, size, mouse_over_hit)) {
                    if (elements[dialog_element].can_be_focused) {
                        focus_element(this, dialog_element);
                        click_hit = true;
                    }
                }

                elements[dialog_element].update();

                foreach (string k in order) {
                    if (!elements.ContainsKey(k)) continue;
                    if (k == dialog_element) continue;
                    if (elements[k].ignore_dialog) {
                        if (elements[k].click_update(position, size, mouse_over_hit)) {
                            if (elements[k].can_be_focused) {
                                focus_element(this, k);
                                click_hit = true;
                            }
                        }

                        if (elements[k].mouse_over) mouse_over_hit = true;

                        elements[k].update();
                    }

                }
            } else {
                foreach (string k in order) {
                    if (!elements.ContainsKey(k)) continue;
                    if (elements[k].click_update(position, size, mouse_over_hit)) {
                        if (elements[k].can_be_focused) {
                            focus_element(this, k);
                            click_hit = true;
                        }
                    }

                    if (elements[k].mouse_over) mouse_over_hit = true;

                    elements[k].update();

                }
            }

            foreach (string k in order) {
                if (!elements.ContainsKey(k)) continue;
                if (elements[k].can_be_focused && elements[k].focused) {
                    elements[k].handle_focused_input();
                }
            }

            
            if (mouse_down && !mouse_down_prev && !click_hit && UIExterns.in_foreground()) {
                //close menus and defocus elements if that is wanted
                //if (allow_element_defocus) focused_element_name = ""; 
            }

            mouse_down_prev = mouse_down;
        }

    

        public void sub_draw(RenderTarget2D return_target) {
            foreach (string k in element_order_inverse_for_draw) {
                if (elements[k].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    Drawing.fill_rect(XYPair.Zero, elements[k].size, Swoop.UI_background_color);
                    elements[k].draw_rt();
                }
            }
            Drawing.graphics_device.SetRenderTarget(return_target);
            foreach (string k in element_order_inverse_for_draw) {
                Drawing.end();
                elements[k].draw();
            }
        }

        public void draw_background() {

            if (Swoop.fill_background) {
                Drawing.graphics_device.Clear(Swoop.UI_background_color);
            } else {
                Drawing.graphics_device.Clear(Color.HotPink);
            }
        }

        public void draw() {
            if (needs_resize)
                resize_rt();

            var inverse_keys = element_order_inverse_for_draw;

            Drawing.graphics_device.SetRenderTarget(render_target);
            Drawing.graphics_device.Clear(Color.Transparent);

            foreach (string k in inverse_keys) {
                if (in_dialog && k == dialog_element) continue;
                if (elements[k].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    elements[k].draw_rt();
                }
            }

            if (in_dialog) {
                if (elements[dialog_element].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[dialog_element].draw_target);
                    elements[dialog_element].draw_rt();

                }
            }
            
            Drawing.end();

            Drawing.graphics_device.SetRenderTarget(render_target);
            foreach (string k in inverse_keys) {
                if (k == dialog_element) continue;
                if (elements[k].ignore_dialog) continue;
                Drawing.end();
                elements[k].draw();
            }

            Drawing.end();

            if (in_dialog)
                Drawing.fill_rect(position, position + size, Color.FromNonPremultiplied(0, 0, 0, 128));

            foreach (string k in inverse_keys) {
                if (k == dialog_element) continue;
                if (!elements[k].ignore_dialog) continue;
                Drawing.end();
                elements[k].draw();
            }

            if (in_dialog) {
                Drawing.graphics_device.SetRenderTarget(render_target);
                Drawing.end();
                elements[dialog_element].draw();
            }
            
            if (draw_border) {
                Drawing.rect(Vector2.Zero, render_target.Bounds.Size.ToVector2(), Swoop.UI_color, 2f);
            }
        }
    }
}
