using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SwoopLib;
using SwoopLib.Collision;

namespace SwoopLib.Shapes {
    public class Polygon : Shape2D {
        public Vector2 position { get; set; } = Vector2.Zero;

        public Color debug_color { get; set; } = Color.White;

        List<Vector2> vertices = new List<Vector2>();

        public Polygon(Vector2 position, params Vector2[] vertices) {
            this.position = position;
            this.vertices.AddRange(vertices);
        }
        public Polygon(Vector2 position, int rng_vert_count) {
            this.position = position;
            Random rng =  new Random();
            for (int i = 0; i < rng_vert_count; i++) {
                vertices.Add(rng.Vector2_neg_one_to_one() * 30);
            }

        }

        public Vector2 support(Vector2 direction) {
            return position + Collision2D.highest_dot(direction, vertices.ToArray());
        }
        public void draw() {
            foreach (Vector2 vert in vertices) {
                Drawing.fill_circle(vert + position, 2f, debug_color);
                foreach (Vector2 v in vertices) {
                    Drawing.line(vert + position, v + position, debug_color, 1f);
                }
            }
        }
    }
}
