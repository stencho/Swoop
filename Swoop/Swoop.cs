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
            if (UIElementManager.focused_element == element) return UI_highlight_color;
            else return UI_color;
        }

        public static Color UI_color = Color.White;
        public static Color UI_highlight_color = Color.FromNonPremultiplied(235, 140, 195, 255);
        public static Color UI_background_color = Color.FromNonPremultiplied(25, 25, 25, 255);

        public static UIElementManager UI;

        internal static InputHandler input_handler;

        static XYPair current_resolution;

        public static RenderTarget2D render_target_output => Drawing.main_render_target;
        
        public static bool fill_background { get; set; } = false;
        public static bool draw_UI_border { get; set; } = true;
        public static bool enable_draw { get; set; } = true;

        public static void Initialize(Game parent, XYPair resolution) {
            Input.initialize(parent);
            input_handler = new InputHandler();
            
            current_resolution = resolution;
        }

        public static void Load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content, XYPair resolution) {
            Drawing.load(gd, gdm, content, resolution);
            SDF.load(content);

            UI = new UIElementManager(XYPair.Zero, resolution);
        }

        public static void Update() {
            input_handler.update();

            UI.update();            
        }

        public static void Draw() {
            if (enable_draw) {
                UI.draw();
            } else {
                Drawing.graphics_device.SetRenderTarget(Drawing.main_render_target);
                Drawing.graphics_device.Clear(Color.Transparent);                
            }
        }

        public static void End() {
            Input.end();
        }

        public static void change_resolution(XYPair resolution) {
            current_resolution = resolution;
            UI.size = current_resolution;
            Drawing.main_render_target.Dispose();
            Drawing.main_render_target = new RenderTarget2D(Drawing.graphics_device, current_resolution.X, current_resolution.Y,
                false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            GC.Collect();
        }
    }
}
