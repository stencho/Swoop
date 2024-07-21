using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SwoopLib;
using SwoopLib.Collision;

namespace SwoopLib.Shapes {
    public class Square : Shape2D {
        public Vector2 position { get; set; } = Vector2.Zero;

        public Vector2 size { get; set; }

        public Color debug_color { get; set; } = Color.White;
        /*
        float _orientation = 0f;
        public float orientation { 
            get { return _orientation; } 
            set {
                if (value > 360f) _orientation = value % 360f;
                else if (value < 0f) {
                    if (value < -360f) {
                        var below_zero = float.Abs(value) % 360f;
                        _orientation = 360f - below_zero;
                    } else _orientation = 360f - float.Abs(value);
                } else _orientation = value;
            } 
        }
        */
        public Square(Vector2 position, Vector2 size) {
            this.position = position;
            this.size = size;
        }

        public void draw() {
            Drawing.rect(position, position + size, Color.White, 1f);
        }

        public Vector2 support(Vector2 direction) {
            return position + Collision2D.highest_dot(direction, Vector2.Zero, size.X_only(), size.Y_only(), size);
        }
    }
}
