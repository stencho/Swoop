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
using System.Runtime.CompilerServices;
using SwoopLib.Effects;
using System.Net.WebSockets;

namespace SwoopLib {
    public static class Swoop {
        public static Color get_color(UIElement element) {
            if (element.focused) return UI_highlight_color;
            else return UI_color;
        }
        public static Color get_color_inverse(UIElement element) {
            if (!element.focused) return UI_highlight_color;
            else return UI_color;
        }

        public static Color UI_color = Color.White;
        public static Color UI_highlight_color = Color.FromNonPremultiplied(235, 140, 195, 255);
        public static Color UI_background_color = Color.FromNonPremultiplied(25, 25, 25, 255);
        public static Color UI_disabled_color = Color.FromNonPremultiplied(90, 90, 90, 255);

        public static ContentManager content;

        public static UIElementManager UI;

        public static InputHandler input_handler;

        static XYPair _resolution;
        public static XYPair resolution => _resolution;
        public static RenderTarget2D render_target_output => UI.render_target;
        

        public static bool fill_background { get; set; } = true;
        public static bool draw_UI_border { get; set; } = true;
        public static bool enable_draw { get; set; } = true;
        public static bool show_logo { get; set; } = true;

        public static Action<XYPair>? resize_start;
        public static Action<XYPair>? resize_end;

        static Game parent = null;
        static GameWindow game_window;
        public static GameTime game_time;

        public static void Initialize(Game parent, GraphicsDeviceManager gdm, GameWindow window, XYPair resolution, bool borderless = true) {
            Swoop.parent = parent;

            Input.initialize(parent);
            input_handler = new InputHandler();

            Swoop._resolution = resolution;
            game_window = window;
            window.IsBorderless = borderless;
            window.Title = "Swoop";

            gdm.PreferredBackBufferWidth = resolution.X;
            gdm.PreferredBackBufferHeight = resolution.Y;

            gdm.ApplyChanges();

            window.AllowUserResizing = true;
            window.ClientSizeChanged += Window_ClientSizeChanged;
        }        

        public static void Load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content, GameWindow window, bool default_window_UI = true) {
            Swoop.content = content;
            Drawing.load(gd, gdm, content, resolution);
            SDF.load(content);
            GraphicsDevice.DiscardColor = Color.Transparent;
            UI = new UIElementManager(XYPair.Zero, resolution);

            if (default_window_UI)
                build_default_UI();            
        }
        


        public static void build_default_UI() {
            var text_length = Drawing.measure_string_profont_xy("x") ;

            UI.add_element(new Button("exit_button", "x",
                _resolution.X_only - text_length.X_only - (Vector2.UnitX * 10f)));
            UI.elements["exit_button"].ignore_dialog = true;
            UI.elements["exit_button"].can_be_focused = false;

            UI.add_element(new Button("minimize_button", "_",
                    _resolution.X_only - ((text_length.X_only + (Vector2.UnitX * 9f)) * 2),
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
                XYPair.Zero, (int)(_resolution.X - (UI.elements["exit_button"].width * 2)) + 3));
            UI.elements["title_bar"].ignore_dialog = true;


            UI.add_element(new ResizeHandle("resize_handle", _resolution - (XYPair.One * 15), XYPair.One * 15));


            Window.resize_start = (Point size) => {
                enable_draw = false;
                if (resize_start != null) resize_start(size.ToXYPair());
            };

            Window.resize_end = (Point size) => {
                Swoop._resolution = size.ToXYPair();

                change_resolution(_resolution);

                UI.elements["exit_button"].position = _resolution.X_only - text_length.X_only - (XYPair.UnitX * 10f);
                UI.elements["minimize_button"].position = _resolution.X_only - ((text_length.X_only + (XYPair.UnitX * 9f)) * 2);
                UI.elements["title_bar"].size = new XYPair((int)(_resolution.X - (UI.elements["exit_button"].width * 2)) + 3, UI.elements["title_bar"].size.Y);

                UI.elements["resize_handle"].position = size.ToXYPair() - (XYPair.One * 15);

                Swoop.enable_draw = true;

                if (resize_end != null) resize_end(size.ToXYPair());
            };

        }
        private static void Window_ClientSizeChanged(object? sender, EventArgs e) {
            var s = game_window.ClientBounds.sizeToXYPair();
            if (resize_end != null) resize_end(s);

            _resolution = s;

            change_resolution(_resolution);
        }

        public static void Update(GameTime gt) {
            game_time = gt;

            Window.is_active = parent.IsActive;
            
            UIElementManager.update_UI_input();

            input_handler.update();

            if (enable_draw || Window.resizing_window)
                UI.update();            
        }

        public static void DrawBackground() {
            if (enable_draw) {
                UI.draw_background();

                if (/*fill_background &&*/ show_logo) {
                    Drawing.end();
                    
                    Drawing.image(Drawing.Logo,
                        (Swoop.resolution.ToVector2()) - (Drawing.Logo.Bounds.Size.ToVector2() * 0.5f) - (Vector2.One * 8f),
                        Drawing.Logo.Bounds.Size.ToVector2() * 0.5f,
                        SpriteEffects.FlipHorizontally);
                }
            }
        }

        public static void Draw() {
            if (enable_draw) {
                
                UI.draw();

            } else {
                Drawing.graphics_device.SetRenderTarget(UI.render_target);
                Drawing.graphics_device.Clear(Color.Transparent);                
            }
        }

        public static void End() {
            Input.end();
        }

        public static void change_resolution(XYPair resolution) {
            Swoop._resolution = resolution;
            UI.size = Swoop._resolution;
            GC.Collect();
        }
    }
}
