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

        public void add_element(string name, UIElement element) {
            element.parent = this;
            element.name = name;
            elements.Add(name, element);
            elements[name].added();
        }

        public void remove_element(string name) {
            if (dialog_element == name) dialog_element = null;
            elements.Remove(name);
        }

        bool mouse_down_prev = false;
        public void update() {
            bool hit = false;
            bool mouse_down = Input.is_pressed(InputStructs.MouseButtons.Left);
            if (in_dialog) {
                if (elements[dialog_element].click_update(bounds)) {
                    if (!hit) hit = true;

                    focused_element = elements[dialog_element];

                }

                elements[dialog_element].update();

                foreach (string k in elements.Keys) {
                    if (k == dialog_element) continue;
                    if (elements[k].ignore_dialog) {
                        if (elements[k].click_update(bounds)) {
                            if (!hit) hit = true;

                            focused_element = elements[k];
                        }

                        if (!hit && mouse_down && !mouse_down_prev) {
                            focused_element = null;
                        }

                        elements[k].update();
                    }
                }
            } else {
                foreach (string k in elements.Keys) {
                    if (elements[k].click_update(bounds)) {
                        if (!hit) hit = true;

                        focused_element = elements[k];
                    }

                    if (!hit && mouse_down && !mouse_down_prev) {
                        focused_element = null;
                    }

                    elements[k].update();
                }
            }
            mouse_down_prev = mouse_down;
        }

        public void sub_draw(RenderTarget2D return_target) {
            foreach (string k in elements.Keys) {
                if (elements[k].enable_render_target) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    Drawing.fill_rect(Vector2.Zero, elements[k].size, Color.Black);
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
            Drawing.graphics_device.SetRenderTarget(null);
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

            Drawing.graphics_device.SetRenderTarget(null);
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
                Drawing.graphics_device.SetRenderTarget(null);
                Drawing.end();
                elements[dialog_element].draw();
                
            }

        }
    }
}
