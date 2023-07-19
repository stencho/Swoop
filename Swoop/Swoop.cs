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
        public static Color get_color(UIElement element) {
            if (UIElementManager.focused_element == element) return UIHighlightColor;
            else return UIColor;
        }

        public static Color UIColor = Color.White;
        public static Color UIHighlightColor = Color.FromNonPremultiplied(235, 140, 195, 255);
        public static Color UIBackgroundColor = Color.FromNonPremultiplied(0x15,0x15,0x15, 255);

        public static UIElementManager UI;

        public static InputHandler input_handler;

        static Point current_resolution;
        
        public static void Initialize(Game parent, Point resolution) {
            Input.initialize(parent);
            input_handler = new InputHandler();

            current_resolution = resolution;
        }

        public static void Load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content, Point resolution) {
            Drawing.load(gd, gdm, content, resolution);
            SDF.load(content);

            UI = new UIElementManager(Vector2.Zero, resolution);
        }

        public static void Update() {
            input_handler.update();

            UI.update();            
        }

        public static void Draw() {
            UI.draw();

            Drawing.graphics_device.SetRenderTarget(null);
            Drawing.image(Drawing.main_render_target, Vector2.Zero, current_resolution);
        }

        public static void End() {
            Input.end();
        }

        static void change_resolution() { }
    }
}
