using System;
using System.Collections.Generic;
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
