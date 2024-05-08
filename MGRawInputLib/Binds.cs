using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MGRawInputLib.Input;

namespace MGRawInputLib {
    class Bind {
        public bool enabled { get; set; } = true;
        
        object trigger;
        
        InputType _bind_type;
        public InputType bind_type => _bind_type;


        public Keys as_key => bind_type == InputType.Key ? (Keys)trigger : Keys.None;
        public MouseButtons as_mouse_button => bind_type == InputType.MouseButton ? (MouseButtons)trigger : MouseButtons.None;


        public string type_string() {
            if (_bind_type == InputType.Key) return "key_";
            else if (_bind_type == InputType.MouseButton) return "mouse_";
            else return "";
        }

        public Bind (Keys trigger) {
            this.trigger = trigger;
            _bind_type = InputType.Key;
        }

        public Bind (MouseButtons trigger) {
            this.trigger = trigger;
            _bind_type = InputType.MouseButton;
        }

        public bool is_pressed(InputHandler input_handler) {            
            if (_bind_type == InputType.Key) return input_handler.is_pressed((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.is_pressed((MouseButtons)trigger);
            else return false;
        }

        public bool was_pressed(InputHandler input_handler) {
            if (_bind_type == InputType.Key) return input_handler.was_pressed((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.was_pressed((MouseButtons)trigger);
            else return false;
        }

        public bool just_pressed(InputHandler input_handler) {
            if (_bind_type == InputType.Key) return input_handler.just_pressed((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.just_pressed((MouseButtons)trigger);
            else return false;
        }
        
        public bool just_released(InputHandler input_handler) {
            if (_bind_type == InputType.Key) return input_handler.just_released((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.just_released((MouseButtons)trigger);
            else return false;
        }

        public bool held(InputHandler input_handler) {
            if (_bind_type == InputType.Key) return input_handler.held((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.held((MouseButtons)trigger);
            else return false;
        }

        public double held_time(InputHandler input_handler) {
            if (_bind_type == InputType.Key) return input_handler.held_time((Keys)trigger);
            else if (_bind_type == InputType.MouseButton) return input_handler.held_time((MouseButtons)trigger);
            else return -1;
        }

        public override string ToString() {
            return trigger.ToString().ToLower();
        }
    }


    public static class Binds {
        static Dictionary<string, Bind> binds = new Dictionary<string, Bind>();

        public static void add(string bind, Keys trigger) {
            if (binds.ContainsKey(bind)) return;
            binds.Add(bind, new Bind(trigger));
        }
        public static void add(string bind, MouseButtons trigger) {
            if (binds.ContainsKey(bind)) return;
            binds.Add(bind, new Bind(trigger));
        }
        public static void remove(string bind) {
            if (!binds.ContainsKey(bind)) return;
            binds.Remove(bind);
        }

        public static void enable_bind(string bind) {
            if (!binds.ContainsKey(bind)) return;
            binds[bind].enabled = true;
        }
        public static void disable_bind(string bind) {
            if (!binds.ContainsKey(bind)) return;
            binds[bind].enabled = false;
        }


        public static void handle(string bind, InputHandler input) {
            if (binds.ContainsKey(bind)) {
                if (binds[bind].bind_type == InputType.Key)
                    input.handle_key(binds[bind].as_key);
                else if (binds[bind].bind_type == InputType.MouseButton)
                    input.handle_key(binds[bind].as_mouse_button);
                
            } else {
                Debug.Print($"Bind does not exist {bind}");
            }
        }


        public static bool is_pressed(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].is_pressed(input) && binds[bind].enabled;
            
            Debug.Print($"Bind does not exist {bind}");
            return false;
        }
        public static bool was_pressed(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].was_pressed(input);

            Debug.Print($"Bind does not exist {bind}");
            return false;
        }

        public static bool just_pressed(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].just_pressed(input) && binds[bind].enabled;

            Debug.Print($"Bind does not exist {bind}");
            return false;
        }
        public static bool just_released(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].just_released(input) && binds[bind].enabled;

            Debug.Print($"Bind does not exist {bind}");
            return false;
        }

        public static bool held(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].held(input) && binds[bind].enabled;

            Debug.Print($"Bind does not exist {bind}");
            return false;
        }
        public static double held_time(string bind, InputHandler input) {
            if (binds.ContainsKey(bind))
                return binds[bind].enabled ? binds[bind].held_time(input) : -1;

            Debug.Print($"Bind does not exist {bind}");
            return -1;
        }

        public static string list_all_binds(InputHandler input_handler) { 
            if (pressed_inputs == null) return "";
            StringBuilder sb = new StringBuilder();
            lock (binds) {
                foreach (string input in binds.Keys) {
                    sb.Append($"[\"{input}\" -> {binds[input].type_string()}{binds[input].ToString()} P{(binds[input].is_pressed(input_handler) ? 1 : 0)} H{(binds[input].held(input_handler) ? 1 : 0)} {(int)binds[input].held_time(input_handler)}]");
                    sb.Append(", ");
                }
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
        public static string list_pressed_binds(InputHandler input_handler) {
            if (pressed_inputs == null) return "";
            StringBuilder sb = new StringBuilder();
            lock (binds) {
                foreach (string input in binds.Keys) {
                    if (!binds[input].is_pressed(input_handler)) continue;
                    sb.Append($"[\"{input}\" -> {binds[input].type_string()}{binds[input].ToString()} P{(binds[input].is_pressed(input_handler) ? 1 : 0)} H{(binds[input].held(input_handler) ? 1 : 0)} {(int)binds[input].held_time(input_handler)}]");
                    sb.Append(", ");
                }
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }
}
