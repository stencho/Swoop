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
using SwoopLib.UIElements;

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

        static XYPair resolution;

        public static RenderTarget2D render_target_output => Drawing.main_render_target;
        
        public static bool fill_background { get; set; } = false;
        public static bool draw_UI_border { get; set; } = true;
        public static bool enable_draw { get; set; } = true;

        public static Action<XYPair>? resize_start;
        public static Action<XYPair>? resize_end;

        static Game parent = null;

        public static void Initialize(Game parent, XYPair resolution) {
            Swoop.parent = parent;

            Input.initialize(parent);
            input_handler = new InputHandler();

            Swoop.resolution = resolution;
        }

        public static void Load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content, XYPair resolution, bool default_window_UI = true) {
            Drawing.load(gd, gdm, content, resolution);
            SDF.load(content);

            UI = new UIElementManager(XYPair.Zero, resolution);
            if (default_window_UI)
                build_default_UI();
        }

        public static void build_default_UI() {
            var text_length = Drawing.measure_string_profont_xy("x") ;

            UI.add_element(new Button("exit_button", "x",
                resolution.X_only - text_length.X_only - (Vector2.UnitX * 10f)));
            UI.elements["exit_button"].ignore_dialog = true;
            UI.elements["exit_button"].can_be_focused = false;

            UI.add_element(new Button("minimize_button", "_",
                    resolution.X_only - ((text_length.X_only + (Vector2.UnitX * 9f)) * 2),
                    UI.elements["exit_button"].size));
            UI.elements["minimize_button"].ignore_dialog = true;
            UI.elements["minimize_button"].can_be_focused = false;

            ((Button)UI.elements["exit_button"]).click_action = () => {
                Swoop.End();
                parent.Exit();
            };

            ((Button)UI.elements["minimize_button"]).click_action = () => {
                UIExterns.minimize_window();
            };

            UI.add_element(new TitleBar("title_bar",
                XYPair.Zero, (int)(resolution.X - (UI.elements["exit_button"].width * 2)) + 3));
            UI.elements["title_bar"].ignore_dialog = true;


            UI.add_element(new ResizeHandle("resize_handle", resolution - (XYPair.One * 15), XYPair.One * 15));


            Window.resize_start = (Point size) => {
                enable_draw = false;
                if (resize_start != null) resize_start(size.ToXYPair());
            };

            Window.resize_end = (Point size) => {
                Swoop.resolution = size.ToXYPair();

                change_resolution(resolution);

                UI.elements["exit_button"].position = resolution.X_only - text_length.X_only - (XYPair.UnitX * 10f);
                UI.elements["minimize_button"].position = resolution.X_only - ((text_length.X_only + (XYPair.UnitX * 9f)) * 2);
                UI.elements["title_bar"].size = new XYPair((int)(resolution.X - (UI.elements["exit_button"].width * 2)) + 3, UI.elements["title_bar"].size.Y);

                UI.elements["resize_handle"].position = size.ToXYPair() - (XYPair.One * 15);

                Swoop.enable_draw = true;

                if (resize_end != null) resize_end(size.ToXYPair());
            };

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
            Swoop.resolution = resolution;
            UI.size = Swoop.resolution;
            Drawing.main_render_target.Dispose();
            Drawing.main_render_target = new RenderTarget2D(Drawing.graphics_device, Swoop.resolution.X, Swoop.resolution.Y,
                false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            GC.Collect();
        }
    }
}
