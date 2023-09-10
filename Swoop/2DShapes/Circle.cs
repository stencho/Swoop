using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SwoopLib;
using SwoopLib.Collision;

namespace SwoopLib.Shapes {
    public class Circle : Shape2D {
        public Vector2 position { get; set; } = Vector2.Zero;

        public float radius { get; set; } = 1f;

        public Color debug_color { get; set; } = Color.White;

        public Circle(Vector2 position, float radius) {
            this.position = position;
            this.radius = radius;
        }

        public void draw() {
            Drawing.circle(position, radius, 1f, debug_color);
        }

        public Vector2 support(Vector2 direction) {
            return position + (Vector2.Normalize(direction) * radius);
        }
    }
}
