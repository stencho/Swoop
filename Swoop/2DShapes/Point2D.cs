using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SwoopLib;
using SwoopLib.Collision;

namespace SwoopLib.Shapes {
    public class Point2D : Shape2D {
        public Vector2 position { get; set; } = Vector2.Zero;
        public Color debug_color { get; set; } = Color.White;

        public Point2D(Vector2 position) {
            this.position = position;
        }

        public Vector2 support(Vector2 direction) {
            return position;
        }

        public void draw() {
            Drawing.pixel(position, debug_color);
        }
    }
}
