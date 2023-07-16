using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib {
    public static class Collision2D {
        public static bool v2_intersects_rect(Vector2 P, Vector2 min, Vector2 max)
            => (P.X >= min.X && P.X <= max.X && P.Y >= min.Y && P.Y <= max.Y);
    }
}
