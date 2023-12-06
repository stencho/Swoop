using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace SwoopLib.UIElements {
    public static class RadioButtons {
        static Dictionary<string, RadioButton> radio_buttons = new Dictionary<string, RadioButton>();

        class radio_button_link {
            int _link_id = 0;

            public Action<string, RadioButton> checked_changed;  

            string _checked_name = "";
            public string checked_name {
                get { return _checked_name; }
                set {
                    if (value != _checked_name) {
                        _checked_name = value;
                        if (checked_changed != null) checked_changed(_checked_name, radio_buttons[_checked_name]);
                    }
                }
            }

            public Dictionary<string, RadioButton> linked_radio_buttons = new Dictionary<string, RadioButton>();

            public bool contains_button(string name) {
                return linked_radio_buttons.ContainsKey(name);
            }

            public int linked_button_count => linked_radio_buttons.Count;

            public radio_button_link(int link_id) {
                _link_id = link_id;
            }


            public radio_button_link(int link_id, params RadioButton[] buttons) {
                _link_id = link_id;
                add_buttons(buttons);
            }


            public void add_button(RadioButton button) {
                linked_radio_buttons.Add(button.name, button);
            }

            public void add_buttons(params RadioButton[] buttons) {
                foreach (var button in buttons) {
                    linked_radio_buttons.Add(button.name, button);
                }
            }


            public void remove_button(RadioButton button) {
                linked_radio_buttons.Remove(button.name);
            }

            public void remove_buttons(params RadioButton[] buttons) { 
                foreach (var button in buttons) {
                    linked_radio_buttons.Remove(button.name);
                }
            }
        }


        static Dictionary<int, radio_button_link> radio_button_links = new Dictionary<int, radio_button_link>();


        static List<int> link_ids = new List<int>();

        public static int random_int() { return RandomNumberGenerator.GetInt32(Int32.MaxValue); }
        static int generate_link_id() {
            int id = random_int();
            while (link_ids.Contains(id)) { id = random_int(); }
            return id;
        }


        public static void check(int id, string name) {
            if (!radio_button_links.ContainsKey(id)) return;
            if (!radio_button_links[id].contains_button(name)) return;

            radio_button_links[id].checked_name = name;

            foreach(var button in radio_button_links[id].linked_radio_buttons.Values) {
                button.Checked = name == button.name;
            }
        }

        public static void link(params RadioButton[] buttons) {
            int id = generate_link_id();            

            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].link_id = id;
            }

            link_ids.Add(id);

            radio_button_link link = new radio_button_link(id, buttons);
            radio_button_links.Add(id, link);

            check(id, buttons[0].name);
        }

        public static void unlink(int id, params RadioButton[] buttons) {
            foreach (RadioButton button in buttons) {
                if (radio_button_links[id].contains_button(button.name))
                    radio_button_links[id].remove_buttons(buttons);
            }

            if (radio_button_links[id].linked_button_count == 0) {
                radio_button_links.Remove(id);
            }
        }

    }

    public class RadioButton : UIElement {
        string _text = "radio_button"; public string text { get { return _text; } set { _text = value; update_size(); } }

        public int link_id = 0;

        public bool Checked { get; set; } = false;
        public Action<RadioButton, bool> checked_changed;

        float check_circle_radius = 5f;
        XYPair text_size = XYPair.Zero;

        const int margin = 4;
        bool text_taller_than_box = false;


        void update_size() {
            text_size = Drawing.measure_string_profont_xy(text);

            if (text_size.Y > check_circle_radius) {
                text_taller_than_box = true;
                size = text_size + (XYPair.UnitX * ((check_circle_radius * 2) + margin));
            } else {
                text_taller_than_box = false;
                size = (XYPair.One * check_circle_radius * 2) + (XYPair.UnitX * (text_size.X + margin));
            }
        }

        public RadioButton(string name, string text, XYPair position) : base(name, position, XYPair.Zero) {
            this.text = text;
        }

        internal override void added() {}

        internal override void draw() {
            if (!visible) return;

            XYPair mid_left = position + (size.Y_only * 0.5f) + (XYPair.Right * check_circle_radius);

            var draw_color = Swoop.get_color(this);

            Drawing.circle(mid_left.ToVector2() , check_circle_radius, 1f, draw_color);


            if (Checked) {
                Drawing.fill_circle(mid_left.ToVector2(), check_circle_radius - 2, draw_color);
            } else if (mouse_down && mouse_over) {
                Drawing.fill_circle(mid_left.ToVector2(), check_circle_radius - 1,draw_color);
            } else if (mouse_over) {
                Drawing.fill_circle(mid_left.ToVector2(), check_circle_radius - 2, draw_color);
            }

            //Drawing.rect(position, position + size, Color.Red, 1f);

            Drawing.text(_text, (mid_left + (XYPair.Right * check_circle_radius)) + (XYPair.UnitX * margin) - (text_size.Y_only * 0.5f),  draw_color);
        }

        internal override void draw_rt() {
        }

        internal override void update() {
            if (!visible) return;
            bool interacted = false;

            if (!clicking && was_clicking && mouse_over) interacted = true;
            if (focused && Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                interacted = true;

            if (interacted) {
                Checked = !Checked;
                update_size();
                RadioButtons.check(link_id, name);
                if (checked_changed != null) checked_changed(this, Checked);
            }
        }
    }
}
