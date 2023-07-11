using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static MGRawInputLib.InputStructs;

namespace MGRawInputLib {
    //This entire class exists to "decouple" the input handling from the polling thread
    //Basically, if the polling thread handles the previous/current state, just_pressed
    //and just_released won't work correctly, as, to put it in some kinda way, the rising
    //edge of the pressed state is happening between frames
    //If the main thread is used for polling, then any threads used for updating will have
    //the same issue if they're running slower than it, or the opposite problem, too much
    //just_pressed, if they're running faster

    //The program will crash if say Keyboard.GetState() gets called in two different threads
    //around the same time, so we have one high rate thread to poll the inputs, then other
    //threads copy from it and handle their own previous state

    //same thing kind of applies for mouse_delta

    public class InputHandler {
        public Point mouse_delta { get; private set; }

        Point _mouse_delta_acc = Point.Zero;
        internal Point mouse_delta_accumulated { 
            get {
                var ret = _mouse_delta_acc;
                _mouse_delta_acc = Point.Zero;
                return ret;
            } 
            set {
                _mouse_delta_acc = value;
            } 
        }

        public Vector2 mouse_position { get; private set; }
        public int scroll_value { get; private set; }
        int scroll_value_previous;

        public int scroll_delta => _scroll_delta;
        int _scroll_delta = 0;
        int scroll_delta_last_frame = 0;

        public InputHandler() {
            Input.handlers.Add(this);
        }
        ~InputHandler() { Input.handlers.Remove(this); }

        public void update() {
            mouse_position = Input.cursor_pos.ToVector2();
            mouse_delta = mouse_delta_accumulated;

            scroll_delta_last_frame = _scroll_delta;
            _scroll_delta = (scroll_value - scroll_value_previous);
            
        }

        public bool is_pressed(Keys key) {
            return false;
        }
        public bool was_pressed(Keys key) {
            return false;
        }
        public bool is_pressed(MouseButtons mouse_button) {
            return false;            
        }
        public bool was_pressed(MouseButtons mouse_button) {
            return false;
        }

        public bool just_pressed(Keys key) => is_pressed(key) && !was_pressed(key);        
        public bool just_released(Keys key) => !is_pressed(key) && was_pressed(key);

        public bool just_pressed(MouseButtons mouse_button) => is_pressed(mouse_button) && !was_pressed(mouse_button);
        public bool just_released(MouseButtons mouse_button) => !is_pressed(mouse_button) && was_pressed(mouse_button);
    }
}
