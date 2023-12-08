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

        public static void focus_element(UIElementManager manager, string element_name) {
            if (manager.elements.ContainsKey(element_name)) {
                focused_manager_index = manager.index;
                focused_element_name = element_name;

            } else {
                throw new Exception("Invalid element name");
            }
        }

        static void find_element_in_direction() {

        }

        public static void update_UI_input() {}


        //PER INSTANCE
        public Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();
        
        internal int index = 0;

        public string dialog_element = "";
        public bool in_dialog => !string.IsNullOrEmpty(dialog_element);

        public XYPair position;
        public XYPair size;

        public UIElementManager(XYPair pos, XYPair size) {
            this.position = pos;
            this.size = size;

            add_manager(this);
        }

        ~UIElementManager() {
            remove_manager(this);
        }

        public void add_element(UIElement element) {
            element.parent = this;

            elements.Add(element.name, element);

            elements[element.name].added();
        }
        public void add_elements(params UIElement[] elements) {
            foreach (var element in elements) {
                add_element(element);
            }
        }

        public void remove_element(string name) {
            if (dialog_element == name) dialog_element = null;
            elements.Remove(name);
        }

        bool mouse_down_prev = false;
        public void update() {
            bool click_hit = false;
            bool mouse_over_hit = false;
            bool mouse_down = Input.is_pressed(InputStructs.MouseButtons.Left);

            if (in_dialog) {
                if (elements[dialog_element].click_update(position, size, mouse_over_hit)) {
                    if (elements[dialog_element].can_be_focused) {
                        focus_element(this, dialog_element);
                        click_hit = true;
                    }
                }

                elements[dialog_element].update();

                foreach (string k in elements.Keys.Reverse()) {
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
                foreach (string k in elements.Keys.Reverse()) {

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

            foreach (string k in elements.Keys.Reverse()) {
                if (elements[k].can_be_focused && elements[k].focused) {
                    elements[k].handle_focused_input();
                }
            }

            if (mouse_down && !mouse_down_prev && !click_hit && UIExterns.in_foreground()) {
                //focused_element = null; 
            }

            mouse_down_prev = mouse_down;
        }


        public void sub_draw(RenderTarget2D return_target) {
            foreach (string k in elements.Keys) {
                if (elements[k].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    Drawing.fill_rect(XYPair.Zero, elements[k].size, Swoop.UI_background_color);
                    elements[k].draw_rt();
                }
            }
            Drawing.graphics_device.SetRenderTarget(return_target);
            foreach (string k in elements.Keys) {
                Drawing.end();
                elements[k].draw();
            }
        }

        public void draw_background() {
            Drawing.graphics_device.SetRenderTarget(Drawing.main_render_target);

            if (Swoop.fill_background) {
                Drawing.graphics_device.Clear(Swoop.UI_background_color);
            } else {
                Drawing.graphics_device.Clear(Color.HotPink);
            }
        }

        public void draw() {

            foreach (string k in elements.Keys) {
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

            Drawing.graphics_device.SetRenderTarget(Drawing.main_render_target);
            foreach (string k in elements.Keys) {
                if (k == dialog_element) continue;
                if (elements[k].ignore_dialog) continue;
                Drawing.end();
                elements[k].draw();
            }

            Drawing.end();

            if (in_dialog)
                Drawing.fill_rect(position, position + size, Color.FromNonPremultiplied(0, 0, 0, 128));

            foreach (string k in elements.Keys) {
                if (k == dialog_element) continue;
                if (!elements[k].ignore_dialog) continue;
                Drawing.end();
                elements[k].draw();
            }

            if (in_dialog) {
                Drawing.graphics_device.SetRenderTarget(Drawing.main_render_target);
                Drawing.end();
                elements[dialog_element].draw();
            }
            
            if (Swoop.draw_UI_border) {
                Drawing.rect(Vector2.Zero, Drawing.main_render_target.Bounds.Size.ToVector2(), Swoop.UI_color, 2f);
            }
        }
    }
}
