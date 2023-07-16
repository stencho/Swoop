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
        public Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();

        public string dialog_element = "";
        public bool in_dialog => !string.IsNullOrEmpty(dialog_element);

        public UIElement? focused_element = null;

        Rectangle bounds = Rectangle.Empty;
        public UIElementManager(Vector2 pos, Point size) {
            bounds = new Rectangle((int)pos.X, (int)pos.Y, size.X, size.Y);
        }

        public void add_element(UIElement element) {
            element.parent = this;

            elements.Add(element.name, element);

            elements[element.name].added();
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
                if (elements[dialog_element].click_update(bounds, mouse_over_hit)) {
                    if (elements[dialog_element].can_be_focused) {
                        focused_element = elements[dialog_element];
                        click_hit = true;
                    }
                }

                elements[dialog_element].update();

                foreach (string k in elements.Keys.Reverse()) {
                    if (k == dialog_element) continue;
                    if (elements[k].ignore_dialog) {
                        if (elements[k].click_update(bounds, mouse_over_hit)) {
                            if (elements[k].can_be_focused) {
                                focused_element = elements[k];
                                click_hit = true;
                            }
                        }

                        if (elements[k].mouse_over) mouse_over_hit = true;

                        elements[k].update();
                    }
                }

            } else {
                foreach (string k in elements.Keys.Reverse()) {

                    if (elements[k].click_update(bounds, mouse_over_hit)) {
                        if (elements[k].can_be_focused) {
                            focused_element = elements[k];
                            click_hit = true;
                        }
                    }

                    if (elements[k].mouse_over) mouse_over_hit = true;

                    elements[k].update();
                }
            }
            
            if (mouse_down && !mouse_down_prev && !click_hit && UIExterns.in_foreground()) {
                focused_element = null;
            }


            mouse_down_prev = mouse_down;
        }

        public void sub_draw(RenderTarget2D return_target) {
            foreach (string k in elements.Keys) {
                if (elements[k].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    Drawing.fill_rect(Vector2.Zero, elements[k].size, Swoop.UIBackgroundColor);
                    elements[k].draw_rt();
                }
            }
            Drawing.graphics_device.SetRenderTarget(return_target);
            foreach (string k in elements.Keys) {
                Drawing.end();
                elements[k].draw();
            }
        }

        public void draw() {
            Drawing.graphics_device.SetRenderTarget(Drawing.main_render_target);
            Drawing.graphics_device.Clear(Swoop.UIBackgroundColor);

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
                Drawing.fill_rect(bounds.Location.ToVector2(), bounds.Location.ToVector2() + bounds.Size.ToVector2(), Color.FromNonPremultiplied(0, 0, 0, 128));

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

        }
    }
}
