using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MGRawInputLib;

namespace SwoopLib {
    public static class Swoop {
        public static UIElementManager UI;

        public static void Initialize(Game parent, Point resolution) {
            Input.initialize(parent);

            UI = new UIElementManager(Vector2.Zero, resolution);
        }

        public static void Load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content) {
            Drawing.load(gd, gdm, content);
            SDF.load(content);
        }

        public static void Update() {
            UI.update();
        }

        public static void Draw() {
            UI.draw();
        }

        public static void End() {
            Input.kill();
        }
    }
}
